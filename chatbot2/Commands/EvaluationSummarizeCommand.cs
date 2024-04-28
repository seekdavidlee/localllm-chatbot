﻿using chatbot2.Evals;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace chatbot2.Commands;

public class EvaluationSummarizeCommand : ICommandAction
{
    private readonly EvaluationSummarizeWorkflow evaluationSummarizeWorkflow;
    private readonly ILogger<EvaluationSummarizeCommand> logger;

    public EvaluationSummarizeCommand(EvaluationSummarizeWorkflow evaluationSummarizeWorkflow, ILogger<EvaluationSummarizeCommand> logger)
    {
        this.evaluationSummarizeWorkflow = evaluationSummarizeWorkflow;
        this.logger = logger;
    }
    public string Name => "summarize";

    public async Task ExecuteAsync(IConfiguration argsConfiguration)
    {
        var path = argsConfiguration["blob-path"];
        if (path is null)
        {
            logger.LogWarning("blob-path argument is missing");
            return;
        }

        await this.evaluationSummarizeWorkflow.CreateAsync(path);
    }
}