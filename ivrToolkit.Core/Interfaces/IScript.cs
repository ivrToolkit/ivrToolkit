﻿// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// A script block is a unit of code that does one particular job. It is used by the script manager to navigate through your menu options. 
/// </summary>
/// 
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
///        var scriptManager = new ScriptManager(line, startingScript);
///
///        // run the script blocks
///        while (scriptManager.hasNext())
///        {
///            scriptManager.execute();
///        }
///       line.hangup();
/// </code>
/// </example>
public interface IScript
{
    /// <summary>
    /// Gets the voice line used by the script block
    /// </summary>
    IIvrLine Line
    {
        get;
    }
    /// <summary>
    /// Gets the description of the script block.
    /// </summary>
    string Description
    {
        get;
    }
    /// <summary>
    /// Executes the script block
    /// </summary>
    /// <returns>The next script block to run or null indicating the scripts are done</returns>
    IScript Execute();

    /// <summary>
    /// Asynchronously executes the script block
    /// </summary>
    /// <returns>The next script block to run or null indicating the scripts are done</returns>
    Task<IScript> ExecuteAsync(CancellationToken cancellationToken);
}