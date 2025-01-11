// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core
{
    /// <summary>
    /// The script manager manages IScript blocks. A script block is a unit of code that does one particular job. It is used by the script manager to navigate through your menu options. 
    /// </summary>
    /// <remarks>
    /// <code>
    /// An example of a simple IVR program using script blocks would be:
    /// 
    /// ScriptBlocks\Welcome.cs
    /// ScriptBlocks\MainMenu.cs
    /// ScriptBlocks\Option1.cs
    /// ScriptBlocks\Option2.cs
    /// </code>
    /// </remarks>
    ///
    /// <example>
    /// <code language="C#">
    ///        // create the script manager
    ///        ScriptManager scriptManager = new ScriptManager(line, startingScript);
    ///
    ///        // run the script blocks
    ///        while (scriptManager.hasNext())
    ///        {
    ///            scriptManager.execute();
    ///        }
    ///       line.hangup();
    /// </code>
    /// </example>
    public class ScriptManager
    {
        private IScript _nextScript;
        private readonly ILogger<ScriptManager> _logger;

        /// <summary>
        /// The next script block to be executed.
        /// </summary>
        public IScript NextScript
        {
            get => _nextScript;
            set => _nextScript = value;
        }

        /// <summary>
        /// Initializes the script manager with the current voice line and a starting script.
        /// </summary>
        /// <param name="loggerFactory"></param>
        /// <param name="startingScript">The first script</param>
        public ScriptManager(ILoggerFactory loggerFactory, IScript startingScript)
        {
            _logger = loggerFactory.CreateLogger<ScriptManager>();
            _logger.LogDebug("Ctr()");
            NextScript = startingScript;
        }

        /// <summary>
        /// Executes the next script block.
        /// </summary>
        public void Execute()
        {
            _logger.LogDebug("Execute()");
            _nextScript = _nextScript.Execute();
        }
        
        /// <summary>
        /// Checks to see if there is another script block to execute.
        /// </summary>
        /// <returns>Returns the next script block to execute or null if there are no more.</returns>
        public bool HasNext()
        {
            _logger.LogDebug("HasNext()");
            return _nextScript != null;
        }

        public async Task ExecuteScriptAsync(CancellationToken cancellationToken)
        {
            while (HasNext())
            {
                // execute the next script
                await ExecuteAsync(cancellationToken);
            }
        }

        private async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Execute()");
            _nextScript = await _nextScript.ExecuteAsync(cancellationToken);
        }
    } // class
}
