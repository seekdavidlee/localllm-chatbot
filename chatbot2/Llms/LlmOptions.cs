﻿namespace chatbot2.Llms;

public class LlmOptions
{
    public string? DeploymentName { get; set; }
    public int? MaxTokens { get; set; }
    public float? Temperature { get; set; }
}
