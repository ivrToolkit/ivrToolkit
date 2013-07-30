// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
namespace ivrToolkit.Core
{
    /// <summary>
    /// The script manager manages IScript blocks. A script block is a unit of code that does one particular job. It is used by the script manager to navigate through your menu options. 
    /// </summary>
    /// <remarks>
    /// <code>
    /// An example of a simple IVR program usingscript blocks would be:
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
        private readonly ILine _line;
        private IScript _nextScript;

        /// <summary>
        /// The next script block to be executed.
        /// </summary>
        public IScript NextScript
        {
            get { return _nextScript; }
            set { _nextScript = value; }
        }
        /// <summary>
        /// Initializes the script manager with the current voice line and a starting script.
        /// </summary>
        /// <param name="line">The voice line to use</param>
        /// <param name="startingScript">The first script</param>
        public ScriptManager(ILine line, IScript startingScript)
        {
            NextScript = startingScript;
            _line = line;
        }

        /// <summary>
        /// Executes the next script block.
        /// </summary>
        public void Execute()
        {
            _nextScript.Line = _line;
            _nextScript = _nextScript.Execute();
        }

        /// <summary>
        /// Checks to see if there is another script block to execute.
        /// </summary>
        /// <returns>Returns the next script block to execute or null if there are no more.</returns>
        public bool HasNext()
        {
            return _nextScript != null;
        }
    } // class
}
