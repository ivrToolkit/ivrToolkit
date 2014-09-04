// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Util;
using NLog;

namespace ivrToolkit.Core
{
    /// <summary>
    /// Detects tones using either leading edge or trailing edge
    /// </summary>
    public enum ToneDetection
    {
        /// <summary>
        /// use leading edge
        /// </summary>
        Leading,
        /// <summary>
        /// use trailing edge
        /// </summary>
        Trailing
    }

    /// <summary>
    /// Outgoing call analysis.
    /// </summary>
    public enum CallAnalysis
    {
        /// <summary>
        /// The line is currently busy.
        /// </summary>
        Busy,
        /// <summary>
        /// no answer.
        /// </summary>
        NoAnswer,
        /// <summary>
        /// No ringback (not sure what this is).
        /// </summary>
        NoRingback,
        /// <summary>
        /// Call is connected.
        /// </summary>
        Connected,
        /// <summary>
        /// Operator Intercept(not used. This would come across as an answering machine instead).
        /// </summary>
        OperatorIntercept,
        /// <summary>
        /// line has been stopped.
        /// </summary>
        Stopped,
        /// <summary>
        /// No dial tone on the line.
        /// </summary>
        NoDialTone,
        /// <summary>
        /// Fax Tone.
        /// </summary>
        FaxTone,
        /// <summary>
        /// An error happened on the voice card.
        /// </summary>
        Error,
        /// <summary>
        /// An answering machine has been detected. based on salutation length. See voice.properties
        /// </summary>
        AnsweringMachine,
        /// <summary>
        /// No free line available. This is characterized by a fast busy signal. It happens if all the switchboard lines are used up. For example you dial 9 to get an out line
        /// and there are none available.
        /// </summary>
        NoFreeLine
    }

    /// <summary>
    /// The line manager is the main class that loads the plugin and execute getLine to get a line object.
    /// </summary>
    /// <example>
    /// <code>
    ///        // pick the line number you want
    ///        line = LineManager.getLine(1);
    /// </code>
    /// </example>
    public class LineManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly Dictionary<int,ILine> Lines = new Dictionary<int,ILine>();

        private static readonly object LockObject = new object();

        /// <summary>
        /// Gets the line class that will do the line manipulation. This method relies on the following entries in voice.properties:
        /// 
        /// voice.classname = The name of the class that implements the IVoice interface
        /// voice.assemblyName = The assembly name that contains the above class
        /// </summary>
        /// 
        /// <param name="lineNumber">The line number to connect to starting at 1</param>
        /// <param name="data">Optional parameter to pass in data. The dialogic driver can take a string that represents the device name.</param>
        /// <returns>A class that represents the phone line</returns>
        public static ILine GetLine(int lineNumber, object data = null)
        {
            lock (LockObject)
            {
                Logger.Debug("Getting line number: " + lineNumber);

                var className = VoiceProperties.Current.ClassName;
                var assemblyName = VoiceProperties.Current.AssemblyName;

                // create an instance of the class from the assembly
                var assembly = Assembly.LoadFrom(assemblyName);
                var o = assembly.CreateInstance(className);

                // check if this class implements IVoice interface
                if (!(o is IVoice))
                {
                    throw new VoiceException("class must implement the IVoice interface");
                }
                var voiceDriver = (IVoice) o;


                var line = voiceDriver.GetLine(lineNumber, data);
                Lines[lineNumber] = line;
                return line;
            } // lock
        }

        /// <summary>
        /// Gets the count of lines retrieved by using the getLine method.
        /// </summary>
        public static int GetLineCount()
        {
            return Lines.Count;
        }

        /// <summary>
        /// Releases a voice line and removes it from the list of used lines.
        /// </summary>
        /// <param name="lineNumber">The line number to release</param>
        public static void ReleaseLine(int lineNumber)
        {
            try
            {
                var line = Lines[lineNumber];
                Lines.Remove(lineNumber);
                line.Stop();
            }
            catch (KeyNotFoundException)
            {
            }
        }

        /// <summary>
        /// Releases all the voice lines.
        /// </summary>
        public static void ReleaseAll()
        {
            var linesList = Lines.ToList();
            foreach (var keyValue in linesList)
            {
                var line = keyValue.Value;
                ReleaseLine(line.LineNumber);
            }
        }

    } // class
}
