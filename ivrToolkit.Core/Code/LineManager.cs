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
    public enum ToneDetection
    {
        leading,
        trailing
    }
    /// <summary>
    /// Call analysis description of enum goes here
    /// </summary>
    public enum CallAnalysis
    {
        /// <summary>
        /// The line is currently busy
        /// </summary>
        busy,
        /// <summary>
        /// no answer
        /// </summary>
        noAnswer,
        /// <summary>
        /// 
        /// </summary>
        noRingback,
        /// <summary>
        /// 
        /// </summary>
        connected,
        /// <summary>
        /// 
        /// </summary>
        operatorIntercept,
        /// <summary>
        /// 
        /// </summary>
        stopped,
        /// <summary>
        /// 
        /// </summary>
        noDialTone,
        /// <summary>
        /// 
        /// </summary>
        faxTone,
        /// <summary>
        /// 
        /// </summary>
        error,
        /// <summary>
        /// 
        /// </summary>
        answeringMachine,
        /// <summary>
        /// 
        /// </summary>
        noFreeLine
    }

    /// <summary>
    /// Line Manager class description goes here.
    /// </summary>
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
        public static ILine getLine(int lineNumber)
        {
            logger.Debug("Getting line number: "+lineNumber);

            string className = VoiceProperties.current.className;
            string assemblyName = VoiceProperties.current.assemblyName;
            
            // create an instance of the class from the assembly
            Assembly assembly = Assembly.LoadFrom(assemblyName);
            object o = assembly.CreateInstance(className);

            // check if this class implements IVoice interface
            if (!(o is IVoice)) {
                throw new VoiceException("class must implement the IVoice interface");
            }
            IVoice voiceDriver = (IVoice)o;
            ILine line = voiceDriver.getLine(lineNumber);
            lines[lineNumber] = line;
            return line;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int getLineCount()
        {
            return lines.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineNumber"></param>
        public static void releaseLine(int lineNumber)
        {
            try
            {
                ILine line = lines[lineNumber];
                lines.Remove(lineNumber);
                line.stop();
            }
            catch (KeyNotFoundException)
            {
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static void releaseAll()
        {
            List<KeyValuePair<int,ILine>> linesList = lines.ToList();
            foreach (KeyValuePair<int,ILine> keyValue in linesList)
            {
                ILine line = keyValue.Value;
                releaseLine(line.lineNumber);
            }
        }

    }
}
