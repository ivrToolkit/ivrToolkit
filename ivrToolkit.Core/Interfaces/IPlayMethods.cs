using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// Defines a collection of methods for playing audio to the person on the call.
/// </summary>
public interface IPlayMethods
{
    /// <summary>
    /// Plays a phone number as long as it is 7, 10 or 11 characters long.
    /// </summary>
    /// <param name="phoneNumber">The phone number must be 7, 10 or 11 characters long with no spaces or dashes. Just numbers.</param>
    void PlayPhoneNumber(string phoneNumber);

    /// <summary>
    /// Asynchronously plays a phone number as long as it is 7, 10 or 11 characters long.
    /// </summary>
    /// <param name="phoneNumber">The phone number must be 7, 10 or 11 characters long with no spaces or dashes. Just numbers.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a DateTime object given based on the mask parameter.<br/>
    /// </summary>
    /// <param name="dateTime">Can be just the date, just the time or both</param>
    /// <param name="mask">Tells the voice plugin how to speak the datetime.</param>
    /// <remarks>
    /// Mask parts can be separated by the following characters: `:`, ` `, or `-`.<br/>
    /// <b>"m" "mm", or "mmm"</b>  - Speaks the month. Example: December<br/>
    /// <b>"d" or "dd"</b>     - Speaks the day of the month. Example: 3rd<br/>
    /// <b>"ddd" or "dddd"</b> - Speaks the day of the week. Example: Saturday<br/>
    /// <b>"yyy" or "yyyy"</b> - Speaks the year. Speak years 2010 to 2099 with the word "thousand".<br/>
    /// <b>"h" or "hh"</b>     - Speaks the hours. If your mask contains `a/p`, it is 12-hour time; otherwise, it is 24-hour time.<br/>
    /// <b>"n" or "nn"</b>     - Speaks the minutes.<br/>
    /// <b>"a/p"</b>           - Speaks either "am" or "pm".<br/>
    /// </remarks>
    /// <example>
    /// <code>
    ///     line.PlayDate(myDateTime,"m-d-yyy h:n a/p");
    /// </code>
    /// </example>
    void PlayDate(DateTime dateTime, string mask);

    /// <summary>
    /// Asynchronously plays a <see cref="DateTime"/> object given based on the mask parameter.<br/>
    /// </summary>
    /// <param name="dateTime">Can be just the date, just the time or both</param>
    /// <param name="mask">Tells the voice plugin how to speak the datetime.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <remarks>
    /// Mask parts can be separated by the following characters: `:`, ` `, or `-`.<br/>
    /// <b>"m" "mm", or "mmm"</b>  - Speaks the month. Example: December<br/>
    /// <b>"d" or "dd"</b>     - Speaks the day of the month. Example: 3rd<br/>
    /// <b>"ddd" or "dddd"</b> - Speaks the day of the week. Example: Saturday<br/>
    /// <b>"yyy" or "yyyy"</b> - Speaks the year. Speak years 2010 to 2099 with the word "thousand".<br/>
    /// <b>"h" or "hh"</b>     - Speaks the hours. If your mask contains `a/p`, it is 12-hour time; otherwise, it is 24-hour time.<br/>
    /// <b>"n" or "nn"</b>     - Speaks the minutes.<br/>
    /// <b>"a/p"</b>           - Speaks either "am" or "pm".<br/>
    /// </remarks>
    /// <example>
    /// <code>
    ///     await line.PlayDateAsync(myDateTime,"m-d-yyy h:n a/p", cancellationToken);
    /// </code>
    /// </example>
    Task PlayDateAsync(DateTime dateTime, string mask, CancellationToken cancellationToken);

    /// <summary>
    /// Speaks a double in money format. For example 5.23 would be spoken as 'five dollars and twenty three cents'
    /// </summary>
    /// <param name="number">The number you want to speak</param>
    void PlayMoney(double number);

    /// <summary>
    /// Asynchronously speaks a double in money format. For example 5.23 would be spoken as 'five dollars and twenty three cents'
    /// </summary>
    /// <param name="number">The number you want to speak</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayMoneyAsync(double number, CancellationToken cancellationToken);

    /// <summary>
    /// Speaks out the digits in the string.
    /// </summary>
    /// <param name="characters">0-9, a-z, # and *</param>
    void PlayCharacters(string characters);

    /// <summary>
    /// Asynchronously speaks out the digits in the string.
    /// </summary>
    /// <param name="characters">0-9, a-z, # and *</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayCharactersAsync(string characters, CancellationToken cancellationToken);

    /// <summary>
    /// Speaks out a double. For example 5.23 would be spoken as 'five point two three'
    /// </summary>
    /// <param name="number">The number to speak out</param>
    void PlayNumber(double number);

    /// <summary>
    /// Asynchronously speaks out a double. For example 5.23 would be spoken as 'five point two three'
    /// </summary>
    /// <param name="number">The number to speak out</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayNumberAsync(double number, CancellationToken cancellationToken);

    /// <summary>
    /// Speaks out a long. For example 25 would be spoken as 'twenty-five'
    /// </summary>
    /// <param name="number">The long to speak out.</param>
    void PlayInteger(long number);

    /// <summary>
    /// Asynchronously speaks out a long. For example 25 would be spoken as 'twenty-five'
    /// </summary>
    /// <param name="number">The long to speak out.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayIntegerAsync(long number, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a number from 1 to 31. Example 31 would speak 'thirty-first'.
    /// </summary>
    /// <param name="number">A number between 1 and 31</param>
    void PlayOrdinal(int number);

    /// <summary>
    /// Asychronously plays a number from 1 to 31. Example 31 would speak 'thirty-first'.
    /// </summary>
    /// <param name="number">A number between 1 and 31</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayOrdinalAsync(int number, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a collection of one or more string parts which are separated with a comma.
    /// Each string part is in the format of 'data|code'.
    /// You would string them together like: 'data|code,data|code,data|code' etc.
    /// </summary>
    /// <remarks>
    /// Each string part is in the format of 'data|code'. You would string them together like: 'data|code,data|code,data|code' etc.
    /// <para>
    /// The following codes are supported:
    /// </para>
    /// <list type="bullet">
    ///     <item>
    ///         <term><c>C</c></term>
    ///         <description>- Characters that will be played with the <c>PlayCharacter</c> method.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>D</c></term>
    ///         <description>- Expects the data to be a date in month/day/year format. Can be a date, time, or both.
    ///         Uses <c>DateTime.Parse(data, new CultureInfo("en-US"));</c>.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>F</c></term>
    ///         <description>- A file name.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>M</c></term>
    ///         <description>- A string that can convert to a double value and be spoken with the <c>PlayMoney</c> method.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>N</c></term>
    ///         <description>- A string that can convert to a double value and be spoken with the <c>PlayNumber</c> method.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>O</c></term>
    ///         <description>- A string number between 1 and 31 that will be spoken with the <c>PlayOrdinal</c> method.</description>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <param name="str">The string to interpret in the format of 'data|code,data|code,data|code'...</param>
    void PlayString(string str);

    /// <summary>
    /// Asychronously plays a collection of one or more string parts which are separated with a comma.
    /// Each string part is in the format of 'data|code'.
    /// You would string them together like: 'data|code,data|code,data|code' etc.
    /// </summary>
    /// <remarks>
    /// Each string part is in the format of 'data|code'. You would string them together like: 'data|code,data|code,data|code' etc.
    /// <para>
    /// The following codes are supported:
    /// </para>
    /// <list type="bullet">
    ///     <item>
    ///         <term><c>C</c></term>
    ///         <description>- Characters that will be played with the <c>PlayCharacter</c> method.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>D</c></term>
    ///         <description>- Expects the data to be a date in month/day/year format. Can be a date, time, or both.
    ///         Uses <c>DateTime.Parse(data, new CultureInfo("en-US"));</c>.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>F</c></term>
    ///         <description>- A file name.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>M</c></term>
    ///         <description>- A string that can convert to a double value and be spoken with the <c>PlayMoney</c> method.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>N</c></term>
    ///         <description>- A string that can convert to a double value and be spoken with the <c>PlayNumber</c> method.</description>
    ///     </item>
    ///     <item>
    ///         <term><c>O</c></term>
    ///         <description>- A string number between 1 and 31 that will be spoken with the <c>PlayOrdinal</c> method.</description>
    ///     </item>
    /// </list>
    /// </remarks>
    /// <param name="str">The string to interpret in the format of 'data|code,data|code,data|code'...</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayStringAsync(string str, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a file or phrase based on the provided string. If the fileNameOrPhrase has a "|" then
    /// it is considered a phrase. <see cref="PlayString"/>
    /// </summary>
    /// <param name="fileNameOrPhrase">The name of the file or the phrase to be played.</param>
    void PlayFileOrPhrase(string fileNameOrPhrase);

    /// <summary>
    /// Asynchronously plays a file or phrase based on the provided string. If the fileNameOrPhrase has a "|" then
    /// it is considered a phrase. <see cref="PlayString"/>
    /// </summary>
    /// <param name="fileNameOrPhrase">The name of the file or the phrase to be played.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayFileOrPhraseAsync(string fileNameOrPhrase, CancellationToken cancellationToken);
    
    /// <summary>
    /// Converts the specified text into a wav stream and plays it.
    /// </summary>
    /// <exception cref="VoiceException">If <see cref="ITextToSpeechFactory"/> wasn't passed to <see cref="LineManager"/> </exception>
    /// <param name="textToSpeech">The text to convert to speech.</param>
    void PlayTextToSpeech(string textToSpeech);
    
    /// <summary>
    /// Converts the specified text into a wav stream and plays it using <see cref="ITextToSpeechCache"/>
    /// </summary>
    /// <param name="textToSpeechCache">Used to generated TTS and possibly save it to a wav file for later use.
    /// Will only generate TTS if a wav file is not specified, missing or changed.</param>
    void PlayTextToSpeech(ITextToSpeechCache textToSpeechCache);
    
    /// <summary>
    /// Asynchronously converts the specified text into a wav stream and plays it.
    /// </summary>
    /// <param name="textToSpeech">The text to convert to speech</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayTextToSpeechAsync(string textToSpeech, CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously converts the specified text into a wav stream and plays it using <see cref="ITextToSpeechCache"/>
    /// </summary>
    /// <param name="textToSpeechCache">Used to generated TTS and possibly save it to a wav file for later use.
    /// Will only generate TTS if a wav file is not specified, missing or changed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayTextToSpeechAsync(ITextToSpeechCache textToSpeechCache, CancellationToken cancellationToken);

    /// <summary>
    /// Converts the specified text into a wav stream and plays it using <see cref="ITextToSpeechCache"/>
    /// </summary>
    /// <param name="textToSpeechCache">Used to generated TTS and possibly save it to a wav file for later use.
    /// Will only generate TTS if a wav file is not specified, missing or changed.</param>
    /// <exception cref="VoiceException">If ITextToSpeechCache doesn't have a wav file name.</exception>
    void PlayFile(ITextToSpeechCache textToSpeechCache);
    
    /// <summary>
    /// Asynchronously converts the specified text into a wav stream and plays it using <see cref="ITextToSpeechCache"/>
    /// </summary>
    /// <param name="textToSpeechCache">Used to generated TTS and possibly save it to a wav file for later use.
    /// Will only generate TTS if a wav file is not specified, missing or changed.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <exception cref="VoiceException">If ITextToSpeechCache doesn't have a wav file name.</exception>
    Task PlayFileAsync(ITextToSpeechCache textToSpeechCache, CancellationToken cancellationToken);
}