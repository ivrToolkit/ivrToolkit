using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PlayTests;

public class PlayFileOrPhraseTests
{
    private LineWrapper GetLineWrapper(out FakeLine fakeLine)
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);
        fakeLine = new FakeLine();
        var testPauser = new TestPause(fakeLine);
        return new LineWrapper(loggerFactory, properties, 1, fakeLine, testPauser);
    }

    [Theory]
    [InlineData("1.25|N", new[] {"1","point","2","5"})]
    [InlineData("SomeFile.wav", new[] {">SomeFile.wav"})]
    public async Task Strings(string myString, string[] expect)
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayFileOrPhraseAsync(myString, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var min = Math.Min(actual.Count, expect.Length);

        for (var index = 0; index < min; index++)
        {
            actual[index].ShouldBeEquivalentTo(expect[index].StartsWith(">")
                ? expect[index].Substring(1)
                : $"System Recordings\\{expect[index]}.wav");
        }
        actual.Count.ShouldBe(expect.Length);
    }
}