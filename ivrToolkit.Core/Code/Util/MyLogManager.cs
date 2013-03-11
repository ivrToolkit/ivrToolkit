/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Config;
using System.Reflection;
using System.IO;

namespace ivrToolkit.Core.Util
{
    internal class MyLogManager
    {
        // A Logger dispenser for the current assembly 
        public static readonly LogFactory Instance = new LogFactory(new XmlLoggingConfiguration(GetNLogConfigFilePath()));

        // 
        // Use a config file located next to our current assembly dll 
        // eg, if the running assembly is c:\path\to\MyComponent.dll 
        // the config filepath will be c:\path\to\MyComponent.nlog 
        // 
        // WARNING: This will not be appropriate for assemblies in the GAC 
        // 
        private static string GetNLogConfigFilePath()
        {
            // Use name of current assembly to construct NLog config filename 

            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            return Path.ChangeExtension(thisAssembly.Location, ".nlog");
        }
    }




} // namespace
