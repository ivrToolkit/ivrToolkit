using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Util;

internal class TimePause : IPause
{
    public async Task PauseAsync(int delayInMilli, CancellationToken cancellationToken)
    {
        await Task.Delay(delayInMilli, cancellationToken);
    }
}