﻿namespace AIOChatbot.Inferences;

public class InferenceStepData
{
    public InferenceStepData(string name)
    {
        Name = name;
        Outputs = [];
        Inputs = [];
    }

    public string? Name { get; }

    public Dictionary<string, string> Inputs { get; set; }
    public Dictionary<string, object> Outputs { get; set; }

    public int TryGetIntInputValue(string key, int defaultValue)
    {
        if (Inputs.TryGetValue(key, out string? value) && int.TryParse(value, out int result))
        {
            return result;
        }

        return defaultValue;
    }

    public double TryGetDoubleInputValue(string key, double defaultValue)
    {
        if (Inputs.TryGetValue(key, out string? value) && double.TryParse(value, out double result))
        {
            return result;
        }

        return defaultValue;
    }

    public string TryGetStringInputValue(string key, string defaultValue)
    {
        if (Inputs.TryGetValue(key, out string? value) && value is not null)
        {
            return value;
        }

        return defaultValue;
    }

    public bool TryGetBoolInputValue(string key, bool defaultValue)
    {
        if (Inputs.TryGetValue(key, out string? value) && bool.TryParse(value, out bool result))
        {
            return result;
        }

        return defaultValue;
    }

    public T GetOutputValue<T>(string key)
    {
        if (Outputs.TryGetValue(key, out object? value))
        {
            return (T)value;
        }

        throw new Exception($"key: {key} is missing");
    }

    public void AddStepOutput(string name, object value)
    {
        Outputs[name] = value;
    }
}
