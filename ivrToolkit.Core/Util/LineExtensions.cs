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

// ReSharper disable MemberCanBePrivate.Global

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// This is an extension class for the ILine interface. It give you extended functionality for handling money, dates, times, ordinals etc.
    /// </summary>
    public static class LineExtensions
    {
        private static readonly string[] Months = new[]{ "January","February","March","April","May","June","July","August","September","October",
            "November","December"};

        private const string Root = "System Recordings\\";

        private static bool Is24Hour(string mask) {
            var parts = mask.Split(new[] { ' ',':', '-' });
            return parts.All(part => part != "a/p");
        }

        /// <summary>
        /// Plays a phone number as long as it is 7, 10 or 11 characters long.
        /// </summary>
        /// <param name="line">The voice line object</param>
        /// <param name="phoneNumber">The phone number must be 7, 10 or 11 characters long with no spaces or dashes. Just numbers.</param>
        public static void PlayPhoneNumber(this ILine line, string phoneNumber)
        {
            phoneNumber = phoneNumber.PadLeft(11);
            phoneNumber = phoneNumber.Substring(0, 1) + " " + phoneNumber.Substring(1, 3) + " " + 
                phoneNumber.Substring(4, 3) + " " + phoneNumber.Substring(7, 4);
            PlayCharacters(line, phoneNumber.Trim());
        }

        /// <summary>
        /// Plays a DateTime object given based on the mask parameter.
        /// </summary>
        /// <param name="line">The voice line object</param>
        /// <param name="dateTime">Can be just the date, just the time or both</param>
        /// <param name="mask">Tells the voice plugin how to speak the datetime</param>
        /// <remarks>
        /// Mask Definition: (mask parts can be separated by the following characters: ':',' ' or '-'.
        ///     <table>
        ///         <tr><th>Mask Part</th><th>Description</th></tr>
        ///         <tr><td>m, mm or mmm   </td><td>Speaks the month. Ex: December   </td></tr>
        ///         <tr><td>d or dd   </td><td>Speaks the day of the month. ex: 3rd   </td></tr>
        ///         <tr><td>ddd or dddd   </td><td>Speaks the day of the week. ex: Saturday   </td></tr>
        ///         <tr><td>yyy or yyyy   </td><td>Speaks the year. speak years 2010 to 2099 with the word thousand   </td></tr>
        ///         <tr><td>h or hh   </td><td>Speaks the hours. If you mask contains 'a/p' then it is 12 hour time else 24 hour time  </td></tr>
        ///         <tr><td>n or nn   </td><td>Speaks the minutes   </td></tr>
        ///         <tr><td>a/p   </td><td>Speaks either am or pm   </td></tr>
        ///     </table>
        ///     <example>
        ///     line.playDate(myDateTime,"m-d-yyy h:m a/p");
        ///     </example>
        /// </remarks>
        public static void PlayDate(this ILine line, DateTime dateTime, string mask)
        {
            mask = mask.ToLower();
            var parts = mask.Split(new[] { ' ',':', '-' });
            foreach (var part in parts)
            {
                if (part == "m" || part == "mm" || part == "mmm" || part == "mmm")
                {
                    var m = dateTime.Month;
                    if (m > 0 && m < 13) {
                        var month = Months[m-1];
                        PlayF(line,month);
                    }
                }
                else if (part == "d" || part == "dd")
                {
                    PlayF(line,"ord" + dateTime.Day);
                }
                else if (part == "ddd" || part == "dddd")
                {
                    var dow = dateTime.DayOfWeek;
                    
                    var day = Enum.Format(typeof(DayOfWeek),dow,"G");
                    PlayF(line,day);
                }
                else if (part == "yyy" || part == "yyyy")
                {
                    var year = dateTime.Year.ToString(CultureInfo.InvariantCulture);

                    // speak years 2010 to 2099 with the word thousand
                    SpeakYearThousands(line,year);

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
                            PlayF(line,"o");
                        }
                        if (h > 0)
                        {
                            line.PlayInteger(h);
                        }
                        var m = dateTime.Minute;
                        if (m == 0 || h == 0)
                        {
                            PlayF(line,"00 hours");
                        }
                    }
                    else
                    {
                        if (h == 0)
                        {
                            PlayF(line,"12");
                        }
                        else if (h > 12)
                        {
                            line.PlayInteger(h - 12);
                        }
                        else
                        {
                            line.PlayInteger(h);
                        }
                    }

                }
                else if (part == "n" || part == "nn")
                {
                    var m = dateTime.Minute;
                    if (m > 0 && m < 10)
                    {
                        PlayF(line,"o");
                        PlayF(line,m.ToString(CultureInfo.InvariantCulture));
                    }
                    else if (m >= 10)
                    {
                        line.PlayInteger(m);
                    }
                }
                else if (part == "a/p")
                {
                    var h = dateTime.Hour;
                    PlayF(line, h < 12 ? "am" : "pm");
                }
            }
        }

        // speak years 2010 to 2099 with the word thousand
        ///
        private static void SpeakYearThousands(ILine line, string year)
        {
            var y1 = year.Substring(0, 2);
            var y2 = year.Substring(2, 2);
            var y1Int = int.Parse(y1);
            var y2Int = int.Parse(y2);

            if (y1.EndsWith("0"))
            {
                PlayF(line, y1.Substring(0, 1));
                PlayF(line,"Thousand");
                if (y2Int > 0)
                {
                    line.PlayInteger(y2Int);
                }
            }
            else
            {
                line.PlayInteger(y1Int);
                if (y2Int == 0)
                {
                    PlayF(line,"o");
                    PlayF(line,"o");
                }
                else if (y2Int < 10)
                {
                    PlayF(line,"o");
                    PlayF(line,y2Int.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    line.PlayInteger(y2Int);
                }
            }
        }

        // speak years 2010 to 2099 without the word thousand in it.
        /// <summary>
        /// Speaks a number in money format. For example 5.23 would be spoken as 'five dollars and twenty three cents'
        /// </summary>
        /// <param name="line">The voice line object</param>
        /// <param name="number">The number you want to speak</param>
        public static void PlayMoney(this ILine line, double number)
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
            line.PlayInteger(whole);
            PlayF(line, whole == 1 ? "dollar" : "dollars");
            if (f != "")
            {
                PlayF(line,"and");
                var cents = long.Parse(f);
                line.PlayInteger(cents);
                PlayF(line, cents == 1 ? "cent" : "cents");
            }
        }
        /// <summary>
        /// Speaks out the digits in the string.
        /// </summary>
        /// <param name="line">The voice line object</param>
        /// <param name="characters">0-9, a-z, # and *</param>
        public static void PlayCharacters(this ILine line, string characters)
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
                    PlayF(line,"star");
                }
                else if (c == '#')
                {
                    PlayF(line,"pound");
                }
                else
                {
                    PlayF(line,c.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
        /// <summary>
        /// Speaks out a number. For example 5.23 would be spoken as 'five point two three'
        /// </summary>
        /// <param name="line">The voice line object</param>
        /// <param name="number">The number to speak out</param>
        public static void PlayNumber(this ILine line, double number)
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
            line.PlayInteger(long.Parse(w));
            if (f != "")
            {
                PlayF(line,"point");
                var chars = f.ToCharArray();
                foreach (var c in chars)
                {
                    PlayF(line,c.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
        /// <summary>
        /// Same as PlayNumber but for an integer.
        /// </summary>
        /// <param name="line">The voice line object</param>
        /// <param name="number">The integer to speak out.</param>
        public static void PlayInteger(this ILine line, long number)
        {
            if (number < 0)
            {
                PlayF(line,"negative");
                number = number * -1;
            }
            if (number == 0)
            {
                PlayF(line,"0");
                return;
            }
            const long billion = 1000000000;
            const long million = 1000000;
            const long thousand = 1000;

            var billions = number / billion;
            var rest = number % billion;
            if (billions > 0)
            {
                SpeakUpTo999(line, billions);
                PlayF(line,"Billion");
            }

            var millions = rest / million;
            rest = rest % million;
            if (millions > 0)
            {
                SpeakUpTo999(line, millions);
                PlayF(line,"Million");
            }

            var thousands = rest / thousand;
            rest = rest % thousand;
            if (thousands > 0)
            {
                SpeakUpTo999(line, thousands);
                PlayF(line,"Thousand");
            }
            if (rest > 0)
            {
                SpeakUpTo999(line, rest);
            }
        }
        /// <summary>
        /// Plays a number from 1 to 31. Example 31 would speak 'thirty first'.
        /// </summary>
        /// <param name="line">The voice line object</param>
        /// <param name="number">A number between 1 and 31</param>
        public static void PlayOrdinal(this ILine line, int number)

        {
            PlayF(line,"ord" + number);
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
        public static void PlayString(this ILine line, string str)
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
                    line.PlayCharacters(data);
                }
                else if (command == "D") // date
                {
                    // US english culture does month/day/year instead of day/month/year
                    var dt = DateTime.Parse(data, new CultureInfo("en-US"));
                    line.PlayDate(dt, mask);
                }
                else if (command == "F") // file
                {
                    // TODO these are not system files
                    line.PlayFile(data);
                }
                else if (command == "M") // money
                {
                    line.PlayMoney(double.Parse(data));
                }
                else if (command == "N") // number
                {
                    line.PlayNumber(double.Parse(data));
                }
                else if (command == "O") // ordinal
                {
                    line.PlayOrdinal(int.Parse(data));
                }
            }
        }

        private static void SpeakUpTo999(ILine line, long number)
        {
            const long hundred = 100;
            var hundreds = number / hundred;
            var rest = number % 100;
            if (hundreds > 0)
            {
                PlayF(line,hundreds + "00");
            }
            if (rest == 0) return;
            if (rest < 20)
            {
                PlayF(line,rest.ToString(CultureInfo.InvariantCulture));
                return;
            }
            var n = rest.ToString(CultureInfo.InvariantCulture);
            PlayF(line,n.Substring(0, 1) + "0");
            if (n.Substring(1, 1) != "0")
            {
                PlayF(line,n.Substring(1, 1));
            }
        }

        private static void PlayF(ILine line, string filename)
        {
            line.PlayFile(Root + filename + ".wav");
        }
    }
}
