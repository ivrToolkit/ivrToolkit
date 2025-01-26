using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests;

public class PlayNumberTests
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
    [InlineData(1.25, new[] {"1","point","2","5"})]
    [InlineData(-1.25, new[] {"negative","1","point","2","5"})]
    [InlineData(1, new[] {"1"})]
    public async Task Numbers(double number, string[] expect)
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayNumberAsync(number, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var min = Math.Min(actual.Count, expect.Length);

        for (var index = 0; index < min; index++)
        {
            actual[index].ShouldBeEquivalentTo($"System Recordings\\{expect[index]}.wav");
        }
        actual.Count.ShouldBe(expect.Length);
    }
}