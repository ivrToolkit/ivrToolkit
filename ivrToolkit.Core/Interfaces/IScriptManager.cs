using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Interfaces;

public interface IScriptManager
{
    /// <summary>
    /// The next script block to be executed.
    /// </summary>
    IScript NextScript { get; set; }

    /// <summary>
    /// Executes the next script block.
    /// </summary>
    void Execute();

    /// <summary>
    /// Checks to see if there is another script block to execute.
    /// </summary>
    /// <returns>Returns the next script block to execute or null if there are no more.</returns>
    bool HasNext();

    Task ExecuteScriptAsync(CancellationToken cancellationToken);
    Task ExecuteAsync(CancellationToken cancellationToken);
}