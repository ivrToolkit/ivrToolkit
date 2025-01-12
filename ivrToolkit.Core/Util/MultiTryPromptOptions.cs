namespace ivrToolkit.Core.Util;

public class MultiTryPromptOptions : PromptOptions
{
    public int MaxRepeat { get; set; } = 0;
    public int BlankMaxRepeat { get; set; } = 0;
    public string AllowDigits { get; set; } = string.Empty;
}