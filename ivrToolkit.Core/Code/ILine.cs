/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ivrToolkit.Core
{
    /// <summary>
    /// 
    /// </summary>
    public interface ILine
    {
        /// <summary>
        /// 
        /// </summary>
        LineStatusTypes status
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        string lastTerminator
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        int lineNumber
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        void checkStop();

        /// <summary>
        /// 
        /// </summary>
        void stop();
 
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rings"></param>
        void waitRings(int rings);

        /// <summary>
        /// 
        /// </summary>
        void hangup();

        /// <summary>
        /// 
        /// </summary>
        void takeOffHook();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="number"></param>
        /// <param name="answeringMachineLengthInMilliseconds"></param>
        /// <returns></returns>
        CallAnalysis dial(string number, int answeringMachineLengthInMilliseconds);

        /// <summary>
        /// 
        /// </summary>
        void close();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        void playFile(string filename);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filename"></param>
        void recordToFile(string filename);

        /// <summary>
        /// Keep prompting for digits until number of digits is pressed or a terminator digit is pressed.
        /// </summary>
        /// <param name="numberOfDigits">Maximum number of digits allowed in the buffer.</param>
        /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
        string getDigits(int numberOfDigits, string terminators);

        /// <summary>
        /// Returns every character including the terminator
        /// </summary>
        /// <returns>All the digits in the buffer including terminators</returns>
        string flushDigitBuffer();

    }
}
