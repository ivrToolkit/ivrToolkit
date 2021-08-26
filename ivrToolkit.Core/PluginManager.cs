// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 

using System;
using System.Collections.Generic;
using System.Reflection;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core
{
    /// <summary>
    /// The line manager is the main class that loads the plugin and execute getLine to get a line object.
    /// </summary>
    /// <example>
    /// <code>
    ///        // pick the line number you want
    ///        line = PluginManager.getLine(1);
    /// </code>
    /// </example>
    public class PluginManager : IDisposable
    {
        private readonly Dictionary<int, ILine> _lines = new();
        private readonly object _lockObject = new();

        private readonly IIvrPlugin _ivrPlugin;
        private readonly ILogger<PluginManager> _logger;

        public PluginManager(ILoggerFactory loggerFactory, IIvrPlugin ivrPlugin)
        {
            loggerFactory.ThrowIfNull(nameof(loggerFactory));
            ivrPlugin.ThrowIfNull(nameof(ivrPlugin));

            _logger = loggerFactory.CreateLogger<PluginManager>();
            _logger.LogDebug("ctr()");
            _ivrPlugin = ivrPlugin;
        }

        public PluginManager(ILoggerFactory loggerFactory, VoiceProperties voiceProperties)
        {
            loggerFactory.ThrowIfNull(nameof(loggerFactory));
            voiceProperties.ThrowIfNull(nameof(voiceProperties));

            _logger = loggerFactory.CreateLogger<PluginManager>();
            _logger.LogDebug("ctr() - generates plugin from voice.properties");

            var className = voiceProperties.ClassName;
            var assemblyName = voiceProperties.AssemblyName;

            // create an instance of the class from the assembly
            var assembly = Assembly.LoadFrom(assemblyName);
            var obj = assembly.CreateInstance(className);

            // check if this class implements IIvrPlugin interface
            if (obj is not IIvrPlugin plugin)
            {
                throw new VoiceException("class must implement the IIvrPlugin interface");
            }
            _ivrPlugin = plugin;
        }

        /// <summary>
        /// Gets the line class that will do the line manipulation. This method relies on the following entries in voice.properties:
        /// 
        /// voice.classname = The name of the class that implements the IVoice interface
        /// voice.assemblyName = The assembly name that contains the above class
        /// </summary>
        /// 
        /// <param name="lineNumber">The line number to connect to starting at 1</param>
        /// <returns>A class that represents the phone line</returns>
        public ILine GetLine(int lineNumber)
        {
            lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

            _logger.LogDebug("PluginManager.GetLine({0})", lineNumber);
            lock (_lockObject)
            {
                var line = _ivrPlugin.GetLine(lineNumber);
                _lines.Add(line.LineNumber, line);
                return line;
            }
        }

        /// <summary>
        /// Releases a voice line and removes it from the list of used lines.
        /// </summary>
        /// <param name="lineNumber">The line number to release</param>
        public void ReleaseLine(int lineNumber)
        {
            lineNumber.ThrowIfLessThanOrEqualTo(0, nameof(lineNumber));

            _logger.LogDebug("ReleaseLine({0})", lineNumber);
            lock (_lockObject)
            {
                try
                {
                    var line = _lines[lineNumber];
                    _lines.Remove(lineNumber);
                    // some other thread is handling this line and it will get the DisposingException and handle the stopping of the line.

                    line.Management.Dispose();
                }
                catch (KeyNotFoundException)
                {
                    _logger.LogDebug("line not found");
                }
            }
        }

        /// <summary>
        /// Releases all the voice lines.
        /// </summary>
        public void ReleaseAll()
        {
            _logger.LogDebug("ReleaseAll()");
            lock (_lockObject)
            {
                foreach (var keyValue in _lines)
                {
                    var line = keyValue.Value;
                    ReleaseLine(line.LineNumber);
                }
            }
        }

        public void Dispose()
        {
            _ivrPlugin.Dispose();
        }
    }
}
