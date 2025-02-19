using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Interfaces;

namespace ivrToolkit.Core.Util;

internal class TimePause : IPause
{
    public async Task PauseAsync(int delayInMilli, CancellationToken cancellationToken)
    {
        await Task.Delay(delayInMilli, cancellationToken);
    }
}