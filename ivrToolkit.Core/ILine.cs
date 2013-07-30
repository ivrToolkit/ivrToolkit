// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
namespace ivrToolkit.Core
{
    /// <summary>
    /// This interface exposes the main methods used to control the selected plugin.
    /// </summary>
    public interface ILine
    {
        /// <summary>
        /// The Status of the line.
        /// </summary>
        LineStatusTypes Status
        {
            get;
        }

        /// <summary>
        /// The last terminator key that was pressed
        /// </summary>
        string LastTerminator
        {
            get;
        }

        /// <summary>
        /// The attached line number.
        /// </summary>
        int LineNumber
        {
            get;
        }

        /// <summary>
        /// Use this within long computational methods to check if the line has hungup or stopped.
        /// </summary>
        void CheckStop();

        /// <summary>
        /// Call this to stop the line and release all its resources.
        /// </summary>
        void Stop();
 
        /// <summary>
        /// The number of rings to wait before answering
        /// </summary>
        /// <param name="rings">The number of rings to wait before answering</param>
        void WaitRings(int rings);

        /// <summary>
        /// Forces a hangup on the line.
        /// </summary>
        void Hangup();

        /// <summary>
        /// Pick up the line.
        /// </summary>
        void TakeOffHook();

        /// <summary>
        /// Dials a phone number using call progress analysis.
        /// </summary>
        /// <param name="number">The phone number to call</param>
        /// <param name="answeringMachineLengthInMilliseconds">A greeting longer than this time indicates a possible answering machine.</param>
        /// <returns>The Call analysis enumeration</returns>
        CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds);

        /// <summary>
        /// Closes the line and releases the resources.
        /// </summary>
        void Close();

        /// <summary>
        /// Plays a wav file which must be in the format of 8000hz 1 channel unsigned 8 bit PCM.
        /// </summary>
        /// <param name="filename">The wav file to play</param>
        void PlayFile(string filename);

        /// <summary>
        /// Records a wav file to the disk in the format of 8000hz 1 channel unsigned 8 bit PCM.
        /// </summary>
        /// <param name="filename">The file name to record to</param>
        void RecordToFile(string filename);

        /// <summary>
        /// Keep prompting for digits until number of digits is pressed or a terminator digit is pressed.
        /// </summary>
        /// <param name="numberOfDigits">Maximum number of digits allowed in the buffer.</param>
        /// <param name="terminators">The terminator keys</param>
        /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
        string GetDigits(int numberOfDigits, string terminators);

        /// <summary>
        /// Returns every character including the terminator
        /// </summary>
        /// <returns>All the digits in the buffer including terminators</returns>
        string FlushDigitBuffer();

    }
}
