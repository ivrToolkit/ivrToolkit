/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ivrToolkit.Core.Util
{
    /// <summary>
    /// Reads in a java style property file. format is key = value.
    /// A # key in the first character of the line denotes a comment
    /// </summary>
    public class Properties
    {
        Dictionary<string, string> stuff = new Dictionary<string, string>();

        /// <summary>
        /// Opens up a java style property file.
        /// </summary>
        /// <param name="fileName">The name of the file to open.</param>
        public Properties(string fileName)
        {
            string[] lines = File.ReadAllLines(fileName);
            foreach (string line in lines) {
                if (!line.Trim().StartsWith("#")) {
                    int index = line.IndexOf("=");
                    if (index != -1) {
                        string key = line.Substring(0, index).Trim().ToLower();
                        string value = line.Substring(index + 1).Trim();
                        stuff.Add(key, value);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the string value of a property. Case insensitive.
        /// </summary>
        /// <param name="key">The key name to search for</param>
        /// <returns>The value store for the key</returns>
        public string GetProperty(string key)
        {
            try {
                return stuff[key.ToLower()];
            } catch (KeyNotFoundException) {
                return null;
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
            try
            {
                return stuff[key.ToLower()];
            }
            catch (KeyNotFoundException)
            {
                return def;
            }
        }
        /// <summary>
        /// Gets a list of property names that matches the prefix.
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of parameter names where the property name begins with the prefix</returns>
        public string[] GetKeyPrefixMatch(string prefix)
        {
            return (from a in stuff
                    where a.Key.StartsWith(prefix)
                    select a.Key.Substring(prefix.Length).Trim()).ToArray();
        }
        /// <summary>
        /// Gets a list of property values where the property name matches the prefix.
        /// </summary>
        /// <param name="prefix">The string to match on.</param>
        /// <returns>A string array of values where the property name begins with the prefix</returns>
        public string[] GetPrefixMatch(string prefix)
        {
            return (from a in stuff
                    where a.Key.StartsWith(prefix)
                    select a.Value).ToArray();
        }
    }
}
