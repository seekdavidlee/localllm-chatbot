﻿using chatbot2.VectorDbs;

namespace chatbot2.Inferences;

public class InferenceOutput
{
    public string? Text { get; set; }
    public double? DurationInMilliseconds { get; set; }
    public IndexedDocument[]? Documents { get; set; }
    public int? CompletionTokens { get; set; }
    public int? PromptTokens { get; set; }
}