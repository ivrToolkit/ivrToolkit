// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// Reads in a java style property file. format is key = value.
    /// A # key in the first character of the line denotes a comment
    /// </summary>
    public class Properties : IDisposable
    {
        private readonly Dictionary<string, string> _stuff = new();
        private readonly ILogger<Properties> _logger;
        private readonly string _fileName;
        private FileSystemWatcher _watcher;
        private DateTime _lastRead;

        /// <summary>
        /// Opens up a java style property file.
        /// </summary>
        /// <param name="loggerFactory">Used for debugging</param>
        /// <param name="fullFileName">The name of the file to open.</param>
        public Properties(ILoggerFactory loggerFactory, string fullFileName)
        {
            _fileName = fullFileName;
            _logger = loggerFactory.CreateLogger<Properties>();
            _logger.LogDebug("ctr(ILoggerFactory, {0})", fullFileName);

            var directory = Path.GetDirectoryName(fullFileName);
            var fileName = Path.GetFileName(fullFileName);
            if (string.IsNullOrWhiteSpace(directory)) directory = ".";

            _watcher = new FileSystemWatcher(directory, fileName);

            _watcher.NotifyFilter = NotifyFilters.LastWrite;

            _watcher.Changed += OnChanged;
            _watcher.EnableRaisingEvents = true;

            Load();
        }

        /// <summary>
        /// Gets the string value of a property. Case insensitive.
        /// </summary>
        /// <param name="key">The key name to search for</param>
        /// <returns>The value store for the key</returns>
        public string GetProperty(string key)
        {
            lock (this)
            {
                _logger.LogTrace("GetProperty({0})", key);
                try
                {
                    return _stuff[key.ToLower()];
                }
                catch (KeyNotFoundException)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets the string value of a property. Case insensitive.
        /// </summary>
        /// <param name="key">The key name to search for. Case insensitve.</param>
        /// <param name="def">The default to be returned if the key was not found.</param>
        /// <returns>The value of the property or the default if the property was not found.</returns>
        public string GetProperty(string key, string def)
        {
            lock (this)
            {
                try
                {
                    return _stuff[key.ToLower()];
                }
                catch (KeyNotFoundException)
                {
                    return def;
                }
            }
        }

        /// <summary>
        /// Gets a list of property names that matches the prefix. Strips off the prefix from the property names
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of parameter names(less the prefix) where the property name begins with the prefix</returns>
        public string[] GetKeyPrefixMatch(string prefix)
        {
            lock (this)
            {
                return (from a in _stuff
                        where a.Key.StartsWith(prefix)
                        select a.Key.Substring(prefix.Length).Trim()).ToArray();
            }
        }

        /// <summary>
        /// Gets a list of property values where the key name matches the prefix.
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of values where the property name begins with the prefix</returns>
        public string[] GetValuePrefixMatch(string prefix)
        {
            lock (this)
            {
                return (from a in _stuff
                        where a.Key.StartsWith(prefix)
                        select a.Value).ToArray();
            }
        }

        /// <summary>
        /// Gets a list of key/value pairs where the property name matches the prefix. Strips off the prefix from the keys
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of key/value pairs where the property name begins with the prefix</returns>
        public KeyValuePair<string, string>[] GetPairPrefixMatch(string prefix)
        {
            lock (this)
            {
                return (from a in _stuff
                        where a.Key.StartsWith(prefix)
                        select new KeyValuePair<string, string>(a.Key.Substring(prefix.Length).Trim(), a.Value)).ToArray();
            }
        }

        public void Dispose()
        {
            _logger.LogDebug("Dispose()");
            _watcher?.Dispose();
        }

        /// <summary>
        /// converts value to boolean
        /// </summary>
        /// <param name="value">the value to convert to bool</param>
        /// <returns>true if value equals 'true','on' or 'yes'. Otherwise returns false. case insensitive.</returns>
        protected bool ToBool(string value)
        {
            _logger.LogDebug("ToBool({0})", value);
            return value.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                value.Equals("on", StringComparison.OrdinalIgnoreCase) || 
                value.Equals("yes", StringComparison.OrdinalIgnoreCase);
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            // stop known issue of event firing twice
            if ((DateTime.Now-_lastRead).TotalMilliseconds > 1000)
            {
                Thread.Sleep(1000);
                Load();
                _lastRead = DateTime.Now;
            }

        }

        private void Load()
        {
            lock(this)
            {
                try
                {
                    _logger.LogDebug("Loading profile: {0}", _fileName);

                    _stuff.Clear();
                    var lines = File.ReadAllLines(_fileName);
                    foreach (var line in lines)
                    {
                        if (!line.Trim().StartsWith("#"))
                        {
                            var index = line.IndexOf("=", StringComparison.Ordinal);
                            if (index != -1)
                            {
                                var key = line.Substring(0, index).Trim().ToLower();
                                var value = line.Substring(index + 1).Trim();

                                index = value.IndexOf("#", StringComparison.Ordinal);
                                if (index != -1)
                                {
                                    value = value.Substring(0, index).Trim();
                                }
                                _stuff.Add(key, value);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, e.Message);
                }
            }
        }

    }
}
