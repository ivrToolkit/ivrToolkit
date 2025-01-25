using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Tests.LineWrapperTests;

internal class TestPauser : IPauser
{
    private readonly IIvrBaseLine _line;

    public TestPauser(IIvrBaseLine line)
    {
        _line = line;
    }
    
    public async Task PauseAsync(int delayInMilli, CancellationToken cancellationToken)
    {
        await _line.PlayFileAsync($"Delay({delayInMilli})", cancellationToken);
    }
}