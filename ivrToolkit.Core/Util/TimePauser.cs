using System.Threading;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Util;

internal class TimePauser : IPauser
{
    public async Task PauseAsync(int delayInMilli, CancellationToken cancellationToken)
    {
        await Task.Delay(delayInMilli, cancellationToken);
    }
}