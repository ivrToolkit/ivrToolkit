using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// Used to inject the delay into the LineWrapper. This is to facility unit testing
/// </summary>
internal interface IPause 
{
    /// <summary>
    /// Pauses the thread for a certain amount of time
    /// </summary>
    /// <param name="delayInMilli">The amount of time in milliseconds to pause the thread</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PauseAsync(int delayInMilli, CancellationToken cancellationToken);
}