using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Util;

/// <summary>
/// Used to inject the delay into the LineWrapper. This is to facility unit testing
/// </summary>
internal interface IPauser 
{
    Task PauseAsync(int delayInMilli, CancellationToken cancellationToken);
}