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

        public static void playPhoneNumber(this ILine line, string phoneNumber)
        {
            phoneNumber = phoneNumber.PadLeft(11);
            phoneNumber = phoneNumber.Substring(0, 1) + " " + phoneNumber.Substring(1, 3) + " " + 
                phoneNumber.Substring(4, 3) + " " + phoneNumber.Substring(7, 4);
            playCharacters(line, phoneNumber.Trim());
        }

        public static void playDate(this ILine line, DateTime dateTime, string mask)
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
                        playF(line,month);
                    }
                }
                else if (part == "d" || part == "dd")
                {
                    playF(line,"ord" + dateTime.Day);
                }
                else if (part == "ddd" || part == "dddd")
                {
                    DayOfWeek dow = dateTime.DayOfWeek;
                    
                    string day = Enum.Format(typeof(DayOfWeek),dow,"G");
                    playF(line,day);
                }
                else if (part == "yyy" || part == "yyyy")
                {
                    string year = dateTime.Year.ToString();

                    // speak years 2010 to 2099 with the word thousand
                    speakYearThousands(line,year);

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
                            playF(line,"o");
                        }
                        if (h > 0)
                        {
                            line.playInteger(h);
                        }
                        int m = dateTime.Minute;
                        if (m == 0 || h == 0)
                        {
                            playF(line,"00 hours");
                        }
                    }
                    else
                    {
                        if (h == 0)
                        {
                            playF(line,"12");
                        }
                        else if (h > 12)
                        {
                            line.playInteger(h - 12);
                        }
                        else
                        {
                            line.playInteger(h);
                        }
                    }

                }
                else if (part == "n" || part == "nn")
                {
                    int m = dateTime.Minute;
                    if (m > 0 && m < 10)
                    {
                        playF(line,"o");
                        playF(line,m.ToString());
                    }
                    else if (m >= 10)
                    {
                        line.playInteger(m);
                    }
                }
                else if (part == "a/p")
                {
                    int h = dateTime.Hour;
                    if (h < 12)
                    {
                        playF(line,"am");
                    }
                    else
                    {
                        playF(line,"pm");
                    }
                }
            }
        }

        // speak years 2010 to 2099 with the word thousand
        private static void speakYearThousands(ILine line, string year)
        {
            string y1 = year.Substring(0, 2);
            string y2 = year.Substring(2, 2);
            int y1Int = int.Parse(y1);
            int y2Int = int.Parse(y2);

            if (y1.EndsWith("0"))
            {
                playF(line, y1.Substring(0, 1));
                playF(line,"Thousand");
                if (y2Int > 0)
                {
                    line.playInteger(y2Int);
                }
            }
            else
            {
                line.playInteger(y1Int);
                if (y2Int == 0)
                {
                    playF(line,"o");
                    playF(line,"o");
                }
                else if (y2Int < 10)
                {
                    playF(line,"o");
                    playF(line,y2Int.ToString());
                }
                else
                {
                    line.playInteger(y2Int);
                }
            }
        }

        // speak years 2010 to 2099 without the word thousand in it.
        private static void speakYearBrokenUp(ILine line, string year)
        {
            string y1 = year.Substring(0, 2);
            string y2 = year.Substring(2, 2);
            int y1Int = int.Parse(y1);
            int y2Int = int.Parse(y2);

            if (year.Substring(1, 2) == "00")
            {
                playF(line, y1.Substring(0, 1));
                playF(line,"Thousand");
                if (y2Int > 0)
                {
                    line.playInteger(y2Int);
                }
            }
            else
            {
                line.playInteger(y1Int);
                if (y2Int == 0)
                {
                    playF(line,"o");
                    playF(line,"o");
                }
                else if (y2Int < 10)
                {
                    playF(line,"o");
                    playF(line,y2Int.ToString());
                }
                else
                {
                    line.playInteger(y2Int);
                }
            }
        }
        public static void playMoney(this ILine line, double number)
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
            line.playInteger(whole);
            if (whole == 1) {
                playF(line,"dollar");
            } else {
                playF(line,"dollars");
            }
            if (f != "")
            {
                playF(line,"and");
                long cents = long.Parse(f);
                line.playInteger(cents);
                if (cents == 1)
                {
                    playF(line,"cent");
                }
                else
                {
                    playF(line,"cents");
                }
            }
        }
        public static void playCharacters(this ILine line, string characters)
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
                    playF(line,"star");
                }
                else if (c == '#')
                {
                    playF(line,"pound");
                }
                else
                {
                    playF(line,c.ToString());
                }
            }
        }

        public static void playNumber(this ILine line, double number)
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
            line.playInteger(long.Parse(w));
            if (f != "")
            {
                playF(line,"point");
                char[] chars = f.ToCharArray();
                foreach (char c in chars)
                {
                    playF(line,c.ToString());
                }
            }
        }

        public static void playInteger(this ILine line, long number)
        {
            if (number < 0)
            {
                playF(line,"negative");
                number = number * -1;
            }
            if (number == 0)
            {
                playF(line,"0");
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
                speakUpTo999(line, billions);
                playF(line,"Billion");
            }

            long millions = rest / MILLION;
            rest = rest % MILLION;
            if (millions > 0)
            {
                speakUpTo999(line, millions);
                playF(line,"Million");
            }

            long thousands = rest / THOUSAND;
            rest = rest % THOUSAND;
            if (thousands > 0)
            {
                speakUpTo999(line, thousands);
                playF(line,"Thousand");
            }
            if (rest > 0)
            {
                speakUpTo999(line, rest);
            }
        }
        public static void playOrdinal(this ILine line, int number)
        {
            playF(line,"ord" + number);
        }
        public static void playString(this ILine line, string str)
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
                    line.playCharacters(data);
                }
                else if (command == "D") // date
                {
                    // US english culture does month/day/year instead of day/month/year
                    DateTime dt = DateTime.Parse(data, new CultureInfo("en-US"));
                    line.playDate(dt, mask);
                }
                else if (command == "F") // file
                {
                    // TODO these are not system files
                    line.playFile(data);
                }
                else if (command == "M") // money
                {
                    line.playMoney(double.Parse(data));
                }
                else if (command == "N") // number
                {
                    line.playNumber(double.Parse(data));
                }
                else if (command == "O") // ordinal
                {
                    line.playOrdinal(int.Parse(data));
                }
            }
        }

        private static void speakUpTo999(ILine line, long number)
        {
            const long HUNDRED = 100;
            long hundreds = number / HUNDRED;
            long rest = number % 100;
            if (hundreds > 0)
            {
                playF(line,hundreds + "00");
            }
            if (rest == 0) return;
            if (rest < 20)
            {
                playF(line,rest.ToString());
                return;
            }
            string n = rest.ToString();
            playF(line,n.Substring(0, 1) + "0");
            if (n.Substring(1, 1) != "0")
            {
                playF(line,n.Substring(1, 1));
            }
        }

        private static void playF(ILine line, string filename)
        {
            line.playFile(ROOT + filename + ".wav");
        }
    }
}
