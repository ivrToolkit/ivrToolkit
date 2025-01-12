namespace ivrToolkit.Core.Util;

public class PromptOptions
{
    public int MaxLength { get; set; } = 0; // Maximum input length
    public string Terminators { get; set; } = string.Empty; // Termination characters
}