﻿using chatbot2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using chatbot2.Embeddings;
using chatbot2.VectorDbs;
using chatbot2.Llms;
using System.Text.Json;
using System.Text;

IConfiguration argsConfig = new ConfigurationBuilder()
       .AddCommandLine(args)
       .Build();

var services = new ServiceCollection();
services.AddSingleton<IEmbedding, AzureOpenAIEmbedding>();
services.AddSingleton<IEmbedding, LocalEmbedding>();
services.AddSingleton<IVectorDb, AzureAISearch>();
services.AddSingleton<IVectorDb, ChromaDbClient>();
services.AddSingleton<ILanguageModel, LocalLLM>();
services.AddSingleton<ILanguageModel, AzureOpenAIClient>();
services.AddSingleton<ILanguageModel, LocalLLM>();

var provider = services.BuildServiceProvider();
var vectorDb = provider.GetServices<IVectorDb>().GetSelectedVectorDb();
await vectorDb.InitAsync();

var deleteSearch = argsConfig["delete-search"];
if (deleteSearch == "true")
{
    await vectorDb.DeleteAsync();
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("database deleted");
    Console.ResetColor();
    return;
}

var ingestData = argsConfig["ingest"];
if (ingestData == "true")
{
    string[] dataSourcePaths = (Environment.GetEnvironmentVariable("DataSourcePaths") ?? throw new Exception("Missing DataSourcePaths!")).Split(',');
    var htmlReader = new HtmlReader();

    foreach (var dataSourcePath in dataSourcePaths)
    {
        Console.WriteLine($"processing data source: {dataSourcePath}...");
        var result = await htmlReader.ReadFilesAsync(dataSourcePath);
        foreach (var page in result.Pages)
        {
            Console.WriteLine($"processing page: {page.Context.PagePath}...");
            foreach (var section in page.Sections)
            {
                Console.WriteLine($"processing section: {section.IdPrefix}...");
                Console.WriteLine(section);
                await vectorDb.ProcessAsync(section);
            }
        }
        foreach (var log in result.Logs)
        {
            Console.WriteLine($"log: {log.Text}, source: {log.Source}");
        }
    }
    return;
}

while (true)
{
    Console.Write("Ask a question.\nUser: ");
    string userInput = Console.ReadLine() ?? "";

    if (userInput == "exit")
    {
        break;
    }

    var intentPrompt = await Util.GetResourceAsync("DetermineIntent.txt");
    intentPrompt = intentPrompt.Replace("{{$previous_intent}}", "");
    intentPrompt = intentPrompt.Replace("{{$query}}", userInput);

    var llm = provider.GetServices<ILanguageModel>().GetSelectedLanguageModel();
    var intentResponse = await llm.GetChatCompletionsAsync(intentPrompt);

    if (intentResponse is null)
    {
        throw new Exception("did not get response from llm");
    }

    const string keywordMarker = "Single intents:";
    var findIndex = intentResponse.IndexOf(keywordMarker, StringComparison.OrdinalIgnoreCase);
    if (findIndex < 0)
    {
        throw new Exception("did not find single intent in response");
    }
    intentResponse = intentResponse.Substring(findIndex + keywordMarker.Length);
    var lastIndex = intentResponse.IndexOf("]", 0, StringComparison.OrdinalIgnoreCase);
    intentResponse = intentResponse.Substring(0, lastIndex + 1);
    var parsedIntents = JsonSerializer.Deserialize<string[]>(intentResponse);
    if (parsedIntents is null)
    {
        throw new Exception("response did not deserialize properly");
    }

    var intent = parsedIntents.Length > 0 ? parsedIntents.Single() : userInput;

    var results = (await vectorDb.SearchAsync(intent)).ToArray();
    var replyPrompt = await Util.GetResourceAsync("DetermineReply.txt");
    replyPrompt = replyPrompt.Replace("{{$conversation}}", "");
    StringBuilder sb = new();
    for (var i = 0; i < results.Length; i++)
    {
        var result = results[i];
        sb.AppendLine($"doc[{i}]\n{result.Text}\n");
    }
    replyPrompt = replyPrompt.Replace("{{$documentation}}", sb.ToString());
    replyPrompt = replyPrompt.Replace("{{$user_query}}", intent);

    var replyResponse = await llm.GetChatCompletionsAsync(replyPrompt);

    if (replyResponse is null)
    {
        throw new Exception("did not get response from llm");
    }

    Console.WriteLine(replyResponse);
}

