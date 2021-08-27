// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core
{
    /// <summary>
    /// Holds the voice.properties in a static Properties class.
    /// </summary>
    public class VoiceProperties
    {
        protected readonly Properties TheProperties;
        private readonly ILogger<VoiceProperties> _logger;

        public VoiceProperties(ILoggerFactory loggerFactory, string fileName)
        {
            _logger = loggerFactory.CreateLogger<VoiceProperties>();
            _logger.LogDebug("ctr(ILoggerFactory, {0})", fileName);
            TheProperties = new Properties(loggerFactory, fileName);
        }

        /// <summary>
        /// converts value to boolean
        /// </summary>
        /// <param name="value">the value to convert to bool</param>
        /// <returns>true if value equals 'true' or 'on'. Otherwise returns false. case insensitive.</returns>
        protected bool ToBool(string value)
        {
            _logger.LogDebug("ToBool({0})", value);
            if (value.ToLower() == "true" || value.ToLower() == "on")
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Total number of attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '99'.
        /// </summary>
        public int PromptAttempts => int.Parse(TheProperties.GetProperty("prompt.attempts", "99"));

        /// <summary>
        /// Number of blank entry attempts to try at a prompt before 'TooManyAttemptsException' is thrown. Default is '5'.
        /// </summary>
        public int PromptBlankAttempts => int.Parse(TheProperties.GetProperty("prompt.blankAttempts", "5"));




        /// <summary>
        /// Gets a property given the name parameter. Only use this method if there is no VoiceProperties helper method.
        /// </summary>
        /// <param name="name">The name of the property to look up.</param>
        /// <returns>The value of the property.</returns>
        public string GetProperty(string name)
        {
            return TheProperties.GetProperty(name);
        }
        /// <summary>
        /// Gets a property given the name parameter. Only use this method if there is no VoiceProperties helper method.
        /// </summary>
        /// <param name="name">The name of the property to look up.</param>
        /// <param name="def">The default value if the property name is not found</param>
        /// <returns>The value of the property.</returns>
        public string GetProperty(string name, string def)
        {
            return TheProperties.GetProperty(name, def);
        }
        /// <summary>
        /// Gets a list of property names that matches the prefix.
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of parameter names where the property name begins with the prefix</returns>
        public string[] GetKeyPrefixMatch(string prefix)
        {
            return TheProperties.GetKeyPrefixMatch(prefix);
        }
        /// <summary>
        /// Gets a list of property values where the property name matches the prefix.
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of values where the property name begins with the prefix</returns>
        public string[] GetPrefixMatch(string prefix)
        {
            return TheProperties.GetPrefixMatch(prefix);
        }


    } // class

    // class

} // namespace
