﻿namespace chatbot2.Evals;

public class MetricSummary
{
    public string? Name { get; set; }
    public int TotalRuns { get; set; }
    public int TotalFailedRuns { get; set; }
    public double TotalScore { get; set; }
    public double TotalDurationInMilliseconds { get; set; }
}
