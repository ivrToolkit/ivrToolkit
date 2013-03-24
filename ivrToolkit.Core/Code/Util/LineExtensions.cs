/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Globalization;

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// This is an extension class for the ILine interface. It give you extended functionality for handling money, dates, times, ordinals etc.
    /// </summary>
    public static class LineExtensions
    {
        private static string[] months = new string[]{ "January","February","March","April","May","June","July","August","September","October",
            "November","December"};

        private const string ROOT = "System Recordings\\";

        private static bool is24Hour(string mask) {
            string[] parts = mask.Split(new char[] { ' ',':', '-' });
            foreach (string part in parts)
            {
                if (part == "a/p") return false;
            }
            return true;
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
            string[] parts = mask.Split(new char[] { ' ',':', '-' });
            foreach (string part in parts)
            {
                if (part == "m" || part == "mm" || part == "mmm" || part == "mmm")
                {
                    int m = dateTime.Month;
                    if (m > 0 && m < 13) {
                        string month = months[m-1];
                        PlayF(line,month);
                    }
                }
                else if (part == "d" || part == "dd")
                {
                    PlayF(line,"ord" + dateTime.Day);
                }
                else if (part == "ddd" || part == "dddd")
                {
                    DayOfWeek dow = dateTime.DayOfWeek;
                    
                    string day = Enum.Format(typeof(DayOfWeek),dow,"G");
                    PlayF(line,day);
                }
                else if (part == "yyy" || part == "yyyy")
                {
                    string year = dateTime.Year.ToString();

                    // speak years 2010 to 2099 with the word thousand
                    SpeakYearThousands(line,year);

                    // speak years 2010 to 2099 without the word thousand in it.
                    //speakYearBrokenUp(year);


                }
                else if (part == "h" || part == "hh")
                {
                    int h = dateTime.Hour;
                    if (is24Hour(mask))
                    {
                        if (h < 10)
                        {
                            PlayF(line,"o");
                        }
                        if (h > 0)
                        {
                            line.PlayInteger(h);
                        }
                        int m = dateTime.Minute;
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
                    int m = dateTime.Minute;
                    if (m > 0 && m < 10)
                    {
                        PlayF(line,"o");
                        PlayF(line,m.ToString());
                    }
                    else if (m >= 10)
                    {
                        line.PlayInteger(m);
                    }
                }
                else if (part == "a/p")
                {
                    int h = dateTime.Hour;
                    if (h < 12)
                    {
                        PlayF(line,"am");
                    }
                    else
                    {
                        PlayF(line,"pm");
                    }
                }
            }
        }

        // speak years 2010 to 2099 with the word thousand
        ///
        private static void SpeakYearThousands(ILine line, string year)
        {
            string y1 = year.Substring(0, 2);
            string y2 = year.Substring(2, 2);
            int y1Int = int.Parse(y1);
            int y2Int = int.Parse(y2);

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
                    PlayF(line,y2Int.ToString());
                }
                else
                {
                    line.PlayInteger(y2Int);
                }
            }
        }

        // speak years 2010 to 2099 without the word thousand in it.
        private static void SpeakYearBrokenUp(ILine line, string year)
        {
            string y1 = year.Substring(0, 2);
            string y2 = year.Substring(2, 2);
            int y1Int = int.Parse(y1);
            int y2Int = int.Parse(y2);

            if (year.Substring(1, 2) == "00")
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
                    PlayF(line,y2Int.ToString());
                }
                else
                {
                    line.PlayInteger(y2Int);
                }
            }
        }
        /// <summary>
        /// Speaks a number in money format. For example 5.23 would be spoken as 'five dollars and twenty three cents'
        /// </summary>
        /// <param name="line">The voice line object</param>
        /// <param name="number">The number you want to speak</param>
        public static void PlayMoney(this ILine line, double number)
        {
            string n = number.ToString("F"); // two decimal places
            int index = n.IndexOf(".");
            string w = "";
            string f = "";
            if (index == -1)
            {
                w = n;
            }
            else
            {
                w = n.Substring(0, index);
                f = n.Substring(index + 1);
            }
            long whole = long.Parse(w);
            line.PlayInteger(whole);
            if (whole == 1) {
                PlayF(line,"dollar");
            } else {
                PlayF(line,"dollars");
            }
            if (f != "")
            {
                PlayF(line,"and");
                long cents = long.Parse(f);
                line.PlayInteger(cents);
                if (cents == 1)
                {
                    PlayF(line,"cent");
                }
                else
                {
                    PlayF(line,"cents");
                }
            }
        }
        /// <summary>
        /// Speaks out the digits in the string.
        /// </summary>
        /// <param name="line">The voice line object</param>
        /// <param name="characters">0-9, a-z, # and *</param>
        public static void PlayCharacters(this ILine line, string characters)
        {
            char[] chars = characters.ToCharArray();
            foreach (char c in chars)
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
                    PlayF(line,c.ToString());
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
            string n =number.ToString("G");
            int index = n.IndexOf(".");
            string w = "";
            string f = "";
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
                char[] chars = f.ToCharArray();
                foreach (char c in chars)
                {
                    PlayF(line,c.ToString());
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
            const long BILLION = 1000000000;
            const long MILLION = 1000000;
            const long THOUSAND = 1000;
            const long HUNDRED = 100;

            long billions = number / BILLION;
            long rest = number % BILLION;
            if (billions > 0)
            {
                SpeakUpTo999(line, billions);
                PlayF(line,"Billion");
            }

            long millions = rest / MILLION;
            rest = rest % MILLION;
            if (millions > 0)
            {
                SpeakUpTo999(line, millions);
                PlayF(line,"Million");
            }

            long thousands = rest / THOUSAND;
            rest = rest % THOUSAND;
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
            string[] parts = str.Split(new char[]{','});
            foreach (string part in parts)
            {
                string[] sections = part.Split(new char[] { '|' });
                string mask = null;
                string data = sections[0];
                string command = sections[1];
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
                    DateTime dt = DateTime.Parse(data, new CultureInfo("en-US"));
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
            const long HUNDRED = 100;
            long hundreds = number / HUNDRED;
            long rest = number % 100;
            if (hundreds > 0)
            {
                PlayF(line,hundreds + "00");
            }
            if (rest == 0) return;
            if (rest < 20)
            {
                PlayF(line,rest.ToString());
                return;
            }
            string n = rest.ToString();
            PlayF(line,n.Substring(0, 1) + "0");
            if (n.Substring(1, 1) != "0")
            {
                PlayF(line,n.Substring(1, 1));
            }
        }

        private static void PlayF(ILine line, string filename)
        {
            line.PlayFile(ROOT + filename + ".wav");
        }
    }
}
