using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PromptTests;

public class MultiPromptMultiDigitMultiDigitTests
{
    private LineWrapper GetLineWrapper(out FakeLine fakeLine)
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);
        fakeLine = new FakeLine();
        var testPauser = new TestPause(fakeLine);
        return new LineWrapper(loggerFactory, properties, 1, fakeLine, testPauser);
    }
    
    [Fact]
    public async Task Enter_1234P_Returns_1234()
    {
        var lineWrapper = GetLineWrapper(out var fakeLine);
        
        fakeLine.PushDigits("123#"); // todo need to make this better
        fakeLine.PushDigits("1234#");

        var actual = await lineWrapper.MultiTryPromptAsync("Doesn'tMatter",
            value => value == "1234",
            CancellationToken.None);
        actual.ShouldBe("1234");
    }
}

