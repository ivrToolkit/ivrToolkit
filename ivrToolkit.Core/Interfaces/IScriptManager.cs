using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// A collection of common methods used by the implementations.
/// </summary>
public interface IScriptManager
{
    /// <summary>
    /// The next script block to be executed.
    /// </summary>
    IScript NextScript { get; set; }

    /// <summary>
    /// Executes the next script block. Used with <see cref="HasNext"/>
    /// </summary>
    void Execute();

    /// <summary>
    /// Checks to see if there is another script block to execute.
    /// </summary>
    /// <returns>Returns the next script block to execute or null if there are no more.</returns>
    bool HasNext();

    /// <summary>
    /// Asynchronously executes all the script blocks.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteScriptAsync(CancellationToken cancellationToken);
    
    /// <summary>
    /// Asynchronously executes the next script block. Used with <see cref="HasNext"/>
    /// </summary>
    /// <param name="cancellationToken"></param>
    Task ExecuteAsync(CancellationToken cancellationToken);
}