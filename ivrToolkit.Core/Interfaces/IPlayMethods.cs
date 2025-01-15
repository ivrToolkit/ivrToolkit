using System;
using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Interfaces;

public interface IPlayMethods
{
    /// <summary>
    /// Plays a phone number as long as it is 7, 10 or 11 characters long.
    /// </summary>
    /// <param name="phoneNumber">The phone number must be 7, 10 or 11 characters long with no spaces or dashes. Just numbers.</param>
    void PlayPhoneNumber(string phoneNumber);

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
    ///     PlayDate(myDateTime,"m-d-yyy h:m a/p");
    /// </code>
    /// </example>
    void PlayDate(DateTime dateTime, string mask);
    
    /// <summary>
    /// Plays a DateTime object given based on the mask parameter asynchronously.
    /// </summary>
    /// <param name="dateTime">Can be just the date, just the time or both</param>
    /// <param name="mask">Tells the voice plugin how to speak the datetime.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task PlayDateAsync(DateTime dateTime, string mask, CancellationToken cancellationToken);

    /// <summary>
    /// Speaks a number in money format. For example 5.23 would be spoken as 'five dollars and twenty three cents'
    /// </summary>
    /// <param name="number">The number you want to speak</param>
    void PlayMoney(double number);

    /// <summary>
    /// Speaks a number in money format asynchronously. For example 5.23 would be spoken as 'five dollars and twenty three cents'
    /// </summary>
    /// <param name="number">The number you want to speak</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task PlayMoneyAsync(double number, CancellationToken cancellationToken);

    /// <summary>
    /// Speaks out the digits in the string.
    /// </summary>
    /// <param name="characters">0-9, a-z, # and *</param>
    void PlayCharacters(string characters);

    /// <summary>
    /// Speaks out the digits in the string asynchronously.
    /// </summary>
    /// <param name="characters">0-9, a-z, # and *</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task PlayCharactersAsync(string characters, CancellationToken cancellationToken);

    /// <summary>
    /// Speaks out a number. For example 5.23 would be spoken as 'five point two three'
    /// </summary>
    /// <param name="number">The number to speak out</param>
    void PlayNumber(double number);

    /// <summary>
    /// Speaks out a number asynchronously. For example 5.23 would be spoken as 'five point two three'
    /// </summary>
    /// <param name="number">The number to speak out</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task PlayNumberAsync(double number, CancellationToken cancellationToken);

    /// <summary>
    /// Same as PlayNumber but for an integer.
    /// </summary>
    /// <param name="number">The integer to speak out.</param>
    void PlayInteger(long number);

    /// <summary>
    /// Same as PlayNumber but for an integer asynchronously.
    /// </summary>
    /// <param name="number">The integer to speak out.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task PlayIntegerAsync(long number, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a number from 1 to 31. Example 31 would speak 'thirty first'.
    /// </summary>
    /// <param name="number">A number between 1 and 31</param>
    void PlayOrdinal(int number);

    /// <summary>
    /// Plays a number from 1 to 31 asynchronously. Example 31 would speak 'thirty first'.
    /// </summary>
    /// <param name="number">A number between 1 and 31</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task PlayOrdinalAsync(int number, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a collection of one or more string parts which are separated with a comma. Each string part is in the format of 'data|code'. You would string them together like: 'data|code,data|code,data|code' etc.
    /// </summary>
    /// <remarks>
    /// Each string part is in the format of 'data|code'. You would string them together like: 'data|code,data|code,data|code' etc.
    ///     <table>
    ///         <tr><th>Codes</th><th>Data</th></tr>
    ///         <tr><td> C </td><td> Characters that will be played with the PlayCharater method </td></tr>
    ///         <tr><td> D </td><td> Expects the data to be a date in month/day/year format. Can be a date, time or both date and time. Uses DateTime.Parse(data, new CultureInfo("en-US"));</td></tr>
    ///         <tr><td> F </td><td> A file name </td></tr>
    ///         <tr><td> M </td><td> A string that can convert to a double value and spoken with the PlayMoney method </td></tr>
    ///         <tr><td> N </td><td> A string that can convert to a double value and spoken with the PlayNumber method </td></tr>
    ///         <tr><td> O </td><td> A string number between 1 and 31 that will be spoken with the PlayOrdinal method </td></tr>
    ///     </table>
    /// </remarks>
    /// <param name="str">The string to interpret in the format of 'data|code,data|code,data|code'...</param>
    void PlayString(string str);

    /// <summary>
    /// Plays a collection of one or more string parts asynchronously which are separated with a comma. Each string part is in the format of 'data|code'. You would string them together like: 'data|code,data|code,data|code' etc.
    /// </summary>
    /// <param name="str">The string to interpret in the format of 'data|code,data|code,data|code'...</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task PlayStringAsync(string str, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a file or phrase based on the provided string.
    /// </summary>
    /// <param name="fileNameOrPhrase">The name of the file or the phrase to be played.</param>
    void PlayFileOrPhrase(string fileNameOrPhrase);

    /// <summary>
    /// Plays a file or phrase asynchronously based on the provided string.
    /// </summary>
    /// <param name="fileNameOrPhrase">The name of the file or the phrase to be played.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task PlayFileOrPhraseAsync(string fileNameOrPhrase, CancellationToken cancellationToken);
}