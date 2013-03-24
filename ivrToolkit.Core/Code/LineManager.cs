/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ivrToolkit.Core.Properties;
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
        leading,
        /// <summary>
        /// use trailing edge
        /// </summary>
        trailing
    }

    /// <summary>
    /// Call analysis description of enum goes here
    /// </summary>
    public enum CallAnalysis
    {
        /// <summary>
        /// The line is currently busy.
        /// </summary>
        busy,
        /// <summary>
        /// no answer.
        /// </summary>
        noAnswer,
        /// <summary>
        /// No ringback (not sure what this is).
        /// </summary>
        noRingback,
        /// <summary>
        /// Call is connected.
        /// </summary>
        connected,
        /// <summary>
        /// Operator Intercept(not used. This would come across as an answering machine instead).
        /// </summary>
        operatorIntercept,
        /// <summary>
        /// line has been stopped.
        /// </summary>
        stopped,
        /// <summary>
        /// No dial tone on the line.
        /// </summary>
        noDialTone,
        /// <summary>
        /// Fax Tone.
        /// </summary>
        faxTone,
        /// <summary>
        /// An error happened on the voice card.
        /// </summary>
        error,
        /// <summary>
        /// An answering machine has been detected. based on salutation length. See voice.properties
        /// </summary>
        answeringMachine,
        /// <summary>
        /// No free line available. This is characterized by a fast busy signal. It happens if all the switchboard lines are used up. For example you dial 9 to get an out line
        /// and there are none available.
        /// </summary>
        noFreeLine
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
        private static Logger logger = MyLogManager.Instance.GetCurrentClassLogger();

        private static Dictionary<int,ILine> lines = new Dictionary<int,ILine>();

        /// <summary>
        /// Gets the line class that will do the line manipulation. This method relies on the following entries in voice.properties:
        /// 
        /// voice.classname = The name of the class that implements the IVoice interface
        /// voice.assemblyName = The assembly name that contains the above class
        /// </summary>
        /// 
        /// <param name="lineNumber">The line number to connect to</param>
        /// <returns>A class that represents the phone line</returns>
        public static ILine GetLine(int lineNumber)
        {
            logger.Debug("Getting line number: "+lineNumber);

            string className = VoiceProperties.Current.ClassName;
            string assemblyName = VoiceProperties.Current.AssemblyName;
            
            // create an instance of the class from the assembly
            Assembly assembly = Assembly.LoadFrom(assemblyName);
            object o = assembly.CreateInstance(className);

            // check if this class implements IVoice interface
            if (!(o is IVoice)) {
                throw new VoiceException("class must implement the IVoice interface");
            }
            IVoice voiceDriver = (IVoice)o;
            ILine line = voiceDriver.GetLine(lineNumber);
            lines[lineNumber] = line;
            return line;
        }

        /// <summary>
        /// Gets the count of lines retrieved by using the getLine method.
        /// </summary>
        public static int GetLineCount()
        {
            return lines.Count;
        }

        /// <summary>
        /// Releases a voice line and removes it from the list of used lines.
        /// </summary>
        /// <param name="lineNumber">The line number to release</param>
        public static void ReleaseLine(int lineNumber)
        {
            try
            {
                ILine line = lines[lineNumber];
                lines.Remove(lineNumber);
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
            List<KeyValuePair<int,ILine>> linesList = lines.ToList();
            foreach (KeyValuePair<int,ILine> keyValue in linesList)
            {
                ILine line = keyValue.Value;
                ReleaseLine(line.LineNumber);
            }
        }

    } // class
}
