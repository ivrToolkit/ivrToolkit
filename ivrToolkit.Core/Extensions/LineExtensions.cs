// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Interfaces;

// ReSharper disable MemberCanBePrivate.Global

namespace ivrToolkit.Core.Extensions;

/// <summary>
/// This is an extension class for the IIvrLine interface. It give you extended functionality for handling money, dates, times, ordinals etc.
/// </summary>
public static class LineExtensions
{
    private static readonly string[] Months = new[]{ "January","February","March","April","May","June","July","August","September","October",
        "November","December"};

    private const string ROOT = "System Recordings\\";

    private static bool Is24Hour(string mask) {
        var parts = mask.Split(new[] { ' ',':', '-' });
        return parts.All(part => part != "a/p");
    }

    /// <summary>
    /// Plays a phone number as long as it is 7, 10 or 11 characters long.
    /// </summary>
    /// <param name="line">The voice line object</param>
    /// <param name="phoneNumber">The phone number must be 7, 10 or 11 characters long with no spaces or dashes. Just numbers.</param>
    public static void PlayPhoneNumber(this IIvrLine line, string phoneNumber)
    {
        phoneNumber = phoneNumber.PadLeft(11);
        phoneNumber = phoneNumber.Substring(0, 1) + " " + phoneNumber.Substring(1, 3) + " " + 
                      phoneNumber.Substring(4, 3) + " " + phoneNumber.Substring(7, 4);
        PlayCharacters(line, phoneNumber.Trim());
    }

    /// <summary>
    /// Plays a DateTime object given based on the mask parameter.<br/>
    /// </summary>
    /// <param name="line">The voice line object</param>
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
    ///     line.PlayDate(myDateTime,"m-d-yyy h:m a/p");
    /// </code>
    /// </example>
    public static void PlayDate(this IIvrLine line, DateTime dateTime, string mask)
    {
        PlayDateInternalAsync(dateTime, mask,
            file => { PlayF(line, file); return Task.CompletedTask; },
            year => { SpeakYearThousands(line, year); return Task.CompletedTask; },
            lng => { line.PlayInteger(lng); return Task.CompletedTask; }
        ).GetAwaiter().GetResult();
    }
        
    public static async Task PlayDateAsync(this IIvrLine line, DateTime dateTime, string mask, CancellationToken cancellationToken)
    {
        await PlayDateInternalAsync(dateTime, mask,
            async file => await PlayFAsync(line, file, cancellationToken),
            async year => await SpeakYearThousandsAsync(line, year, cancellationToken),
            async l => await line.PlayIntegerAsync(l, cancellationToken));
    }

    private static async Task PlayDateInternalAsync(DateTime dateTime, string mask,
        Func<string, Task> playF,
        Func<string, Task> speakYearThousands,
        Func<long, Task> playInteger)
    {
        mask = mask.ToLower();
        var parts = mask.Split(' ', ':', '-');
        foreach (var part in parts)
        {
            if (part == "m" || part == "mm" || part == "mmm" || part == "mmm")
            {
                var m = dateTime.Month;
                if (m is > 0 and < 13) {
                    var month = Months[m-1];
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
    private static void SpeakYearThousands(IIvrLine line, string year)
    {
        SpeakYearThousandsInternalAsync(year,
            file => { PlayF(line, file); return Task.CompletedTask; },
            lng => { line.PlayInteger(lng); return Task.CompletedTask; }).GetAwaiter().GetResult();
    }

    private static async Task SpeakYearThousandsAsync(IIvrLine line, string year, CancellationToken cancellationToken)
    {
        await SpeakYearThousandsInternalAsync(year,
            async file => await PlayFAsync(line, file, cancellationToken),
            async lng => await line.PlayIntegerAsync(lng, cancellationToken));
    }

    private static async Task SpeakYearThousandsInternalAsync(string year,
        Func<string, Task> playF,
        Func<long, Task> playInteger)
    {
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
    /// <param name="line">The voice line object</param>
    /// <param name="number">The number you want to speak</param>
    public static void PlayMoney(this IIvrLine line, double number)
    {
        PlayMoneyInternalAsync(number,
            i =>
            {
                line.PlayInteger(i);
                return Task.CompletedTask;
            },
            file =>
            {
                PlayF(line, file);
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
    }
    public static async Task PlayMoneyAsync(this IIvrLine line, double number, CancellationToken cancellationToken)
    {
        await PlayMoneyInternalAsync(number,
            async i => await line.PlayIntegerAsync(i, cancellationToken),
            async file => await PlayFAsync(line, file, cancellationToken));
    }

    private static async Task PlayMoneyInternalAsync(double number,
        Func<long, Task> playInteger,
        Func<string, Task> playF)
    {
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
    /// <param name="line">The voice line object</param>
    /// <param name="characters">0-9, a-z, # and *</param>
    public static void PlayCharacters(this IIvrLine line, string characters)
    {
        PlayCharactersInternalAsync(characters,
            chars => { PlayF(line, chars); return Task.CompletedTask; }
        ).GetAwaiter().GetResult();
    }
    public static async Task PlayCharactersAsync(this IIvrLine line, string characters, CancellationToken cancellationToken)
    {
        await PlayCharactersInternalAsync(characters,
            async chars => await PlayFAsync(line, chars, cancellationToken));
    }

    private static async Task PlayCharactersInternalAsync(string characters, 
        Func<string, Task> playF)
    {
        var chars = characters.ToCharArray();
        foreach (var c in chars)
        {
            if (c == ' ')
            {
                Thread.Sleep(500);
            }
            else if (c == '*')
            {
                await playF("star");
            }
            else if (c == '#')
            {
                await playF("pound");
            }
            else
            {
                await playF(c.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    /// <summary>
    /// Speaks out a number. For example 5.23 would be spoken as 'five point two three'
    /// </summary>
    /// <param name="line">The voice line object</param>
    /// <param name="number">The number to speak out</param>
    public static void PlayNumber(this IIvrLine line, double number)
    {
        PlayNumberInternalAsync(number,
            n => { line.PlayInteger(n); return Task.CompletedTask; },
            file => { PlayF(line, file); return Task.CompletedTask; }
        ).GetAwaiter().GetResult();
    }

    public static async Task PlayNumberAsync(this IIvrLine line, double number, CancellationToken cancellationToken)
    {
        await PlayNumberInternalAsync(number,
            async n => await line.PlayIntegerAsync(n, cancellationToken),
            async file => await PlayFAsync(line, file, cancellationToken));
    }

    private static async Task PlayNumberInternalAsync(double number,
        Func<long, Task> playInteger,
        Func<string, Task> playF)
    {
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
    /// <param name="line">The voice line object</param>
    /// <param name="number">The integer to speak out.</param>
    public static void PlayInteger(this IIvrLine line, long number)
    {
        PlayIntegerInternalAsync(number,
            filename =>
            {
                PlayF(line, filename);
                return Task.CompletedTask;
            },
            n =>
            {
                SpeakUpTo999(line, n);
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
    }

    public static async Task PlayIntegerAsync(this IIvrLine line, long number, CancellationToken cancellationToken)
    {
        await PlayIntegerInternalAsync(number,
            async filename => await PlayFAsync(line, filename, cancellationToken),
            async n => await SpeakUpTo999Async(line, n, cancellationToken));
    }

    private static async Task PlayIntegerInternalAsync(long number,
        Func<string, Task> playF,
        Func<long, Task> speakUpTo999)
    {
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
    /// <param name="line">The voice line object</param>
    /// <param name="number">A number between 1 and 31</param>
    public static void PlayOrdinal(this IIvrLine line, int number)
    {
        PlayF(line,"ord" + number);
    }
        
    public static async Task PlayOrdinalAsync(this IIvrLine line, int number, CancellationToken cancellationToken)
    {
        await PlayFAsync(line,"ord" + number, cancellationToken);
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
    /// <param name="line">The voice line object</param>
    /// <param name="str">The string to interpret in the format of 'data|code,data|code,data|code'...</param>
    public static void PlayString(this IIvrLine line, string str)
    {
        PlayStringInternalAsync(
            str,
            characters => { line.PlayCharacters(characters); return Task.CompletedTask; },
            file => { line.PlayFile(file); return Task.CompletedTask; },
            money => { line.PlayMoney(money); return Task.CompletedTask; },
            nmbr => { line.PlayNumber(nmbr); return Task.CompletedTask; },
            ordinal => { line.PlayOrdinal(ordinal); return Task.CompletedTask; },
            (dt, msk) => { line.PlayDate(dt, msk); return Task.CompletedTask; }).GetAwaiter().GetResult();
    }
        
    public static async Task PlayStringAsync(this IIvrLine line, string str, CancellationToken cancellationToken)
    {
        await PlayStringInternalAsync(
            str,
            async characters => await line.PlayCharactersAsync(characters, cancellationToken),
            async file => await line.PlayFileAsync(file, cancellationToken),
            async money => await line.PlayMoneyAsync(money, cancellationToken),
            async nmbr => await line.PlayNumberAsync(nmbr, cancellationToken),
            async ordinal => await line.PlayOrdinalAsync(ordinal, cancellationToken),
            async (dt, msk) => await line.PlayDateAsync(dt, msk, cancellationToken));
    }

    private static async Task PlayStringInternalAsync(string str,
        Func<string, Task> playCharacters,
        Func<string, Task> playFile,
        Func<double, Task> playMoney,
        Func<double, Task> playNumber,
        Func<int, Task> playOrdinal,
        Func<DateTime, string, Task> playDate)
    {
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

    private static void SpeakUpTo999(IIvrLine line, long number)
    {
        SpeakUpTo999InternalAsync(number,
            n =>
            {
                PlayF(line, n);
                return Task.CompletedTask;
            }).GetAwaiter().GetResult();
    }
    private static async Task SpeakUpTo999Async(IIvrLine line, long number, CancellationToken cancellationToken)
    {
        await SpeakUpTo999InternalAsync(number,
            async n => await PlayFAsync(line, n, cancellationToken));
    }
        
    private static async Task SpeakUpTo999InternalAsync(long number,
        Func<string, Task> playF)
    {
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

    private static void PlayF(IIvrLine line, string filename)
    {
        line.PlayFile(ROOT + filename + ".wav");
    }
        
    private static async Task PlayFAsync(IIvrLine line, string filename, CancellationToken cancellationToken)
    {
        await line.PlayFileAsync(ROOT + filename + ".wav", cancellationToken);
    }
    
    public static void PlayFileOrPhrase(this IIvrLine line, string fileNameOrPhrase)
    {
        if (fileNameOrPhrase.IndexOf("|", StringComparison.Ordinal) != -1)
        {
            line.PlayString(fileNameOrPhrase);
        }
        else
        {
            line.PlayFile(fileNameOrPhrase);
        }
    }
        
    public static async Task PlayFileOrPhraseAsync(this IIvrLine line, string fileNameOrPhrase, CancellationToken cancellationToken)
    {
        if (fileNameOrPhrase.IndexOf("|", StringComparison.Ordinal) != -1)
        {
            await line.PlayStringAsync(fileNameOrPhrase, cancellationToken);
        }
        else
        {
            await line.PlayFileAsync(fileNameOrPhrase, cancellationToken);
        }
    }
}