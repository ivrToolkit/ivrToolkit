using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.Util;

internal partial class LineWrapper
{
    private readonly string[] _months = new[] {
        "January","February","March","April","May","June","July","August","September","October",
        "November","December"
    };

    private const string ROOT = "System Recordings\\";

    private bool Is24Hour(string mask) {
        _logger.LogDebug("{method}({mask})", nameof(Is24Hour), mask);
        var parts = mask.Split(new[] { ' ',':', '-' });
        return parts.All(part => part != "a/p");
    }

    /// <summary>
    /// Plays a phone number as long as it is 7, 10 or 11 characters long.
    /// </summary>
    /// <param name="phoneNumber">The phone number must be 7, 10 or 11 characters long with no spaces or dashes. Just numbers.</param>
    public void PlayPhoneNumber(string phoneNumber)
    {
        _logger.LogDebug("{method}({phoneNumber})", phoneNumber, nameof(PlayPhoneNumber));
        phoneNumber = phoneNumber.PadLeft(11);
        phoneNumber = phoneNumber.Substring(0, 1) + " " + phoneNumber.Substring(1, 3) + " " + 
                      phoneNumber.Substring(4, 3) + " " + phoneNumber.Substring(7, 4);
        PlayCharacters(phoneNumber.Trim());
    }

    /// <summary>
    /// Plays a DateTime object given based on the mask parameter.<br/>
    /// </summary>
    /// <param name="dateTime">Can be just the date, just the time or both</param>
    /// <param name="mask">Tells the voice plugin how to speak the datetime.</param>
    /// <remarks>
    ///
    /// Mask parts can be separated by the following characters: `:`, ` `, or `-`.<br/>
    /// <br/>
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
    public void PlayDate(DateTime dateTime, string mask)
    {
        _logger.LogDebug("{method}({dateTime}, {mask})", nameof(PlayDate), dateTime, mask);
        PlayDateInternalAsync(dateTime, mask,
            file => { PlayF(file); return Task.CompletedTask; },
            year => { SpeakYearThousands(year); return Task.CompletedTask; },
            lng => { PlayInteger(lng); return Task.CompletedTask; }
        ).GetAwaiter().GetResult();
    }
        
    public async Task PlayDateAsync(DateTime dateTime, string mask, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({dateTime}, {mask})", nameof(PlayDateAsync), dateTime, mask);
        await PlayDateInternalAsync(dateTime, mask,
            async file => await PlayFAsync(file, cancellationToken),
            async year => await SpeakYearThousandsAsync(year, cancellationToken),
            async l => await PlayIntegerAsync(l, cancellationToken));
    }

    private async Task PlayDateInternalAsync(DateTime dateTime, string mask,
        Func<string, Task> playF,
        Func<string, Task> speakYearThousands,
        Func<long, Task> playInteger)
    {
        _logger.LogDebug("{method}({dateTime}, {mask})", nameof(PlayDateInternalAsync), dateTime, mask);
        mask = mask.ToLower();
        var parts = mask.Split(' ', ':', '-');
        foreach (var part in parts)
        {
            if (part == "m" || part == "mm" || part == "mmm" || part == "mmm")
            {
                var m = dateTime.Month;
                if (m is > 0 and < 13) {
                    var month = _months[m-1];
                    await playF(month);
                }
            }
            else if (part == "d" || part == "dd")
            {
                await playF("ord" + dateTime.Day);
            }
            else if (part == "ddd" || part == "dddd")
            {
                var dow = dateTime.DayOfWeek;
                    
                var day = Enum.Format(typeof(DayOfWeek),dow,"G");
                await playF(day);
            }
            else if (part == "yyy" || part == "yyyy")
            {
                var year = dateTime.Year.ToString(CultureInfo.InvariantCulture);

                // speak years 2010 to 2099 with the word thousand
                await speakYearThousands(year);

                // speak years 2010 to 2099 without the word thousand in it.
                //speakYearBrokenUp(year);


            }
            else if (part == "h" || part == "hh")
            {
                var h = dateTime.Hour;
                if (Is24Hour(mask))
                {
                    if (h < 10)
                    {
                        await playF("o");
                    }
                    if (h > 0)
                    {
                        await playInteger(h);
                    }
                    var m = dateTime.Minute;
                    if (m == 0 || h == 0)
                    {
                        await playF("00 hours");
                    }
                }
                else
                {
                    if (h == 0)
                    {
                        await playF("12");
                    }
                    else if (h > 12)
                    {
                        await playInteger(h - 12);
                    }
                    else
                    {
                        await playInteger(h);
                    }
                }

            }
            else if (part == "n" || part == "nn")
            {
                var m = dateTime.Minute;
                if (m is > 0 and < 10)
                {
                    await playF("o");
                    await playF(m.ToString(CultureInfo.InvariantCulture));
                }
                else if (m >= 10)
                {
                    await playInteger(m);
                }
            }
            else if (part == "a/p")
            {
                var h = dateTime.Hour;
                await playF( h < 12 ? "am" : "pm");
            }
        }
    }

    // speak years 2010 to 2099 with the word thousand
    ///
    private void SpeakYearThousands(string year)
    {
        _logger.LogDebug("{method}({year})", nameof(SpeakYearThousands), year);
        SpeakYearThousandsInternalAsync(year,
            file => { PlayF(file); return Task.CompletedTask; },
            lng => { PlayInteger(lng); return Task.CompletedTask; }).GetAwaiter().GetResult();
    }

    private async Task SpeakYearThousandsAsync(string year, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({year})", nameof(SpeakYearThousandsAsync), year);
        await SpeakYearThousandsInternalAsync(year,
            async file => await PlayFAsync(file, cancellationToken),
            async lng => await PlayIntegerAsync(lng, cancellationToken));
    }

    private async Task SpeakYearThousandsInternalAsync(string year,
        Func<string, Task> playF,
        Func<long, Task> playInteger)
    {
        _logger.LogDebug("{method}({year})", nameof(SpeakYearThousandsInternalAsync), year);
        var y1 = year.Substring(0, 2);
        var y2 = year.Substring(2, 2);
        var y1Int = int.Parse(y1);
        var y2Int = int.Parse(y2);

        if (y1.EndsWith("0"))
        {
            await playF( y1.Substring(0, 1));
            await playF("Thousand");
            if (y2Int > 0)
            {
                await playInteger(y2Int);
            }
        }
        else
        {
            await playInteger(y1Int);
            if (y2Int == 0)
            {
                await playF("o");
                await playF("o");
            }
            else if (y2Int < 10)
            {
                await playF("o");
                await playF(y2Int.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await playInteger(y2Int);
            }
        }
    }

    // speak years 2010 to 2099 without the word thousand in it.
    /// <summary>
    /// Speaks a number in money format. For example 5.23 would be spoken as 'five dollars and twenty three cents'
    /// </summary>
    /// <param name="number">The number you want to speak</param>
    public void PlayMoney(double number)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayMoney), number);
        PlayMoneyInternalAsync(number,
            i =>
            {
                PlayInteger(i);
                return Task.CompletedTask;
            },
            file =>
            {
                PlayF(file);
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
    }
    
    public async Task PlayMoneyAsync(double number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayMoneyAsync), number);
        await PlayMoneyInternalAsync(number,
            async i => await PlayIntegerAsync(i, cancellationToken),
            async file => await PlayFAsync(file, cancellationToken));
    }

    private async Task PlayMoneyInternalAsync(double number,
        Func<long, Task> playInteger,
        Func<string, Task> playF)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayMoneyInternalAsync), number);
        var n = number.ToString("F"); // two decimal places
        var index = n.IndexOf(".", StringComparison.Ordinal);
        string w;
        var f = "";
        if (index == -1)
        {
            w = n;
        }
        else
        {
            w = n.Substring(0, index);
            f = n.Substring(index + 1);
        }
        var whole = long.Parse(w);
        await playInteger(whole);
        await playF(whole == 1 ? "dollar" : "dollars");
        if (f != "")
        {
            await playF("and");
            var cents = long.Parse(f);
            await playInteger(cents);
            await playF( cents == 1 ? "cent" : "cents");
        }
    }

    /// <summary>
    /// Speaks out the digits in the string.
    /// </summary>
    /// <param name="characters">0-9, a-z, # and *</param>
    public void PlayCharacters(string characters)
    {
        _logger.LogDebug("{method}({characters})", nameof(PlayCharacters), characters);
        PlayCharactersInternalAsync(characters,
            chars => { PlayF(chars); return Task.CompletedTask; }
        ).GetAwaiter().GetResult();
    }
    public async Task PlayCharactersAsync(string characters, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({characters})", nameof(PlayCharactersAsync), characters);
        await PlayCharactersInternalAsync(characters,
            async chars => await PlayFAsync(chars, cancellationToken));
    }

    private async Task PlayCharactersInternalAsync(string characters, 
        Func<string, Task> playF)
    {
        if (characters == null) return;
        
        _logger.LogDebug("{method}({characters})", nameof(PlayCharactersInternalAsync), characters);
        var chars = characters.ToCharArray();
        foreach (var c in chars)
        {
            switch (c)
            {
                case ' ':
                    await Task.Delay(500);
                    break;
                case '*':
                    await playF("star");
                    break;
                case '#':
                    await playF("pound");
                    break;
                default:
                    if (c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9')
                    {
                        await playF(c.ToString(CultureInfo.InvariantCulture));
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Speaks out a number. For example 5.23 would be spoken as 'five point two three'
    /// </summary>
    /// <param name="number">The number to speak out</param>
    public void PlayNumber(double number)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayNumber), number);
        PlayNumberInternalAsync(number,
            n => { PlayInteger(n); return Task.CompletedTask; },
            file => { PlayF(file); return Task.CompletedTask; }
        ).GetAwaiter().GetResult();
    }

    public async Task PlayNumberAsync(double number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayNumberAsync), number);
        await PlayNumberInternalAsync(number,
            async n => await PlayIntegerAsync(n, cancellationToken),
            async file => await PlayFAsync(file, cancellationToken));
    }

    private async Task PlayNumberInternalAsync(double number,
        Func<long, Task> playInteger,
        Func<string, Task> playF)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayNumberInternalAsync), number);
        var n =number.ToString("G");
        var index = n.IndexOf(".", StringComparison.Ordinal);
        string w;
        var f = "";
        if (index == -1)
        {
            w = n;
        }
        else
        {
            w = n.Substring(0, index);
            f = n.Substring(index + 1);
        }
        await playInteger(long.Parse(w));
        if (f != "")
        {
            await playF("point");
            var chars = f.ToCharArray();
            foreach (var c in chars)
            {
                await playF(c.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    /// <summary>
    /// Same as PlayNumber but for an integer.
    /// </summary>
    /// <param name="number">The integer to speak out.</param>
    public void PlayInteger(long number)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayInteger), number);
        PlayIntegerInternalAsync(number,
            filename =>
            {
                PlayF(filename);
                return Task.CompletedTask;
            },
            n =>
            {
                SpeakUpTo999(n);
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
    }

    public async Task PlayIntegerAsync(long number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayIntegerAsync), number);
        await PlayIntegerInternalAsync(number,
            async filename => await PlayFAsync(filename, cancellationToken),
            async n => await SpeakUpTo999Async(n, cancellationToken));
    }

    private async Task PlayIntegerInternalAsync(long number,
        Func<string, Task> playF,
        Func<long, Task> speakUpTo999)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayIntegerInternalAsync), number);
        if (number < 0)
        {
            await playF("negative");
            number = number * -1;
        }
        if (number == 0)
        {
            await playF("0");
            return;
        }
        const long billion = 1000000000;
        const long million = 1000000;
        const long thousand = 1000;

        var billions = number / billion;
        var rest = number % billion;
        if (billions > 0)
        {
            await speakUpTo999(billions);
            await playF("Billion");
        }

        var millions = rest / million;
        rest = rest % million;
        if (millions > 0)
        {
            await speakUpTo999(millions);
            await playF("Million");
        }

        var thousands = rest / thousand;
        rest = rest % thousand;
        if (thousands > 0)
        {
            await speakUpTo999(thousands);
            await playF("Thousand");
        }
        if (rest > 0)
        {
            await speakUpTo999(rest);
        }
    }
        
    /// <summary>
    /// Plays a number from 1 to 31. Example 31 would speak 'thirty first'.
    /// </summary>
    /// <param name="number">A number between 1 and 31</param>
    public void PlayOrdinal(int number)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayOrdinal), number);
        PlayF("ord" + number);
    }
        
    public async Task PlayOrdinalAsync(int number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayOrdinalAsync), number);
        await PlayFAsync("ord" + number, cancellationToken);
    }

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
    public void PlayString(string str)
    {
        _logger.LogDebug("{method}({str})", nameof(PlayString), str);
        PlayStringInternalAsync(
            str,
            characters => { PlayCharacters(characters); return Task.CompletedTask; },
            file => { PlayFile(file); return Task.CompletedTask; },
            money => { PlayMoney(money); return Task.CompletedTask; },
            nmbr => { PlayNumber(nmbr); return Task.CompletedTask; },
            ordinal => { PlayOrdinal(ordinal); return Task.CompletedTask; },
            (dt, msk) => { PlayDate(dt, msk); return Task.CompletedTask; }).GetAwaiter().GetResult();
    }
        
    public async Task PlayStringAsync(string str, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({str})", nameof(PlayStringAsync), str);
        await PlayStringInternalAsync(
            str,
            async characters => await PlayCharactersAsync(characters, cancellationToken),
            async file => await PlayFileAsync(file, cancellationToken),
            async money => await PlayMoneyAsync(money, cancellationToken),
            async nmbr => await PlayNumberAsync(nmbr, cancellationToken),
            async ordinal => await PlayOrdinalAsync(ordinal, cancellationToken),
            async (dt, msk) => await PlayDateAsync(dt, msk, cancellationToken));
    }

    private async Task PlayStringInternalAsync(string str,
        Func<string, Task> playCharacters,
        Func<string, Task> playFile,
        Func<double, Task> playMoney,
        Func<double, Task> playNumber,
        Func<int, Task> playOrdinal,
        Func<DateTime, string, Task> playDate)
    {
        _logger.LogDebug("{method}({str})", nameof(PlayStringInternalAsync), str);
        var parts = str.Split(new[]{','});
        foreach (var part in parts)
        {
            var sections = part.Split(new[] { '|' });
            string mask = null;
            var data = sections[0];
            var command = sections[1];
            if (sections.Length > 2)
            {
                mask = sections[2];
            }
            if (command == "C") // character
            {
                await playCharacters(data);
            }
            else if (command == "D") // date
            {
                // US english culture does month/day/year instead of day/month/year
                var dt = DateTime.Parse(data, new CultureInfo("en-US"));
                await playDate(dt, mask);
            }
            else if (command == "F") // file
            {
                // TODO these are not system files - legacy todo statement
                await playFile(data);
            }
            else if (command == "M") // money
            {
                await playMoney(double.Parse(data));
            }
            else if (command == "N") // number
            {
                await playNumber(double.Parse(data));
            }
            else if (command == "O") // ordinal
            {
                await playOrdinal(int.Parse(data));
            }
        }
    }

    private void SpeakUpTo999(long number)
    {
        _logger.LogDebug("{method}({number})", nameof(SpeakUpTo999), number);
        SpeakUpTo999InternalAsync(number,
            n =>
            {
                PlayF(n);
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
    }
    private async Task SpeakUpTo999Async(long number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({number})", nameof(SpeakUpTo999Async), number);
        await SpeakUpTo999InternalAsync(number,
            async n => await PlayFAsync(n, cancellationToken));
    }
        
    private async Task SpeakUpTo999InternalAsync(long number,
        Func<string, Task> playF)
    {
        _logger.LogDebug("{method}({number})", nameof(SpeakUpTo999InternalAsync), number);
        const long hundred = 100;
        var hundreds = number / hundred;
        var rest = number % 100;
        if (hundreds > 0)
        {
            await playF(hundreds + "00");
        }
        if (rest == 0) return;
        if (rest < 20)
        {
            await playF(rest.ToString(CultureInfo.InvariantCulture));
            return;
        }
        var n = rest.ToString(CultureInfo.InvariantCulture);
        await playF(n.Substring(0, 1) + "0");
        if (n.Substring(1, 1) != "0")
        {
            await playF(n.Substring(1, 1));
        }
    }

    private void PlayF(string filename)
    {
        _logger.LogDebug("{method}({fileName})", nameof(PlayF), filename);
        PlayFile(ROOT + filename + ".wav");
    }
        
    private async Task PlayFAsync(string filename, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({fileName})", nameof(PlayFAsync), filename);
        await PlayFileAsync(ROOT + filename + ".wav", cancellationToken);
    }
    
    public void PlayFileOrPhrase(string fileNameOrPhrase)
    {
        _logger.LogDebug("{method}({fileNameOrPhrase})", nameof(PlayFileOrPhrase), fileNameOrPhrase);
        if (fileNameOrPhrase.IndexOf("|", StringComparison.Ordinal) != -1)
        {
            PlayString(fileNameOrPhrase);
        }
        else
        {
            PlayFile(fileNameOrPhrase);
        }
    }
        
    public async Task PlayFileOrPhraseAsync(string fileNameOrPhrase, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({fileNameOrPhrase})", nameof(PlayFileOrPhraseAsync), fileNameOrPhrase);
        if (fileNameOrPhrase.IndexOf("|", StringComparison.Ordinal) != -1)
        {
            await PlayStringAsync(fileNameOrPhrase, cancellationToken);
        }
        else
        {
            await PlayFileAsync(fileNameOrPhrase, cancellationToken);
        }
    }
}