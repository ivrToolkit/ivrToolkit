using System;

namespace ivrToolkit.Core.Util;

/// <summary>
/// For setting up extra options when using line.Prompt
/// </summary>
public class PromptOptions
{
    /// <summary>
    /// The maximum number of digits that can be intered in the prompt. Default is 30.
    /// </summary>
    public int MaxLength { get; set; } = 30;

    /// <summary>
    /// List of one or more valid termination digits. The default is '#'.
    /// You can also use 'T' which will allow a timeout to be a valid termination key
    /// </summary>
    public string Terminators { get; set; } = "#";
    
    /// <summary>
    /// The digits you are allowed to press in single digit mode.
    /// When you are set to single digit mode(MaxDigits=1) and you are not using an evaluator parameter,
    /// then this will a list of digits that you are allowed to press.
    /// </summary>
    public string AllowedDigits { get; set; }

    /// <summary>
    /// You can define special terminator digits that will fire the OnSpecialTerminator event.
    /// For example a '*' could take you to a special option to control volume.
    /// </summary>
    public string SpecialTerminator { get; set; }
    
    /// <summary>
    /// The method to handle your special terminator digit.
    /// </summary>
    public Action OnSpecialTerminator { get; set; }

    /// <summary>
    /// A message to be played if the answer is incorrect. If null then no message will be played.
    /// </summary>
    public string InvalidAnswerMessage { get; set; }

    /// <summary>
    /// Lets blank be a valid answer. Only works if the evaluator parameter is null. If you supply the evaluator parameter then you are in total
    /// control of the validation. Default is true.
    /// </summary>
    public bool AllowEmpty { get; set; } = true;
    
    /// <summary>
    /// Set to false and the prompt will return a value of "". Set to true and TooManyAttemptsException will be thrown.
    /// Default is true.
    /// </summary>
    public bool CatchTooManyAttempts { get; set; } = true;
}