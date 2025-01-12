namespace ivrToolkit.Core.Util;

public class MultiTryPromptOptions : PromptOptions
{
    /// <summary>
    /// The number of attempts before throwing the TooManyAttemptsException
    /// </summary>
    public int MaxAttempts { get; set; }

    /// <summary>
    /// The number of blank attempts before throwing the TooManyAttemptsException
    /// </summary>
    public int BlankMaxAttempts { get; set; }
}