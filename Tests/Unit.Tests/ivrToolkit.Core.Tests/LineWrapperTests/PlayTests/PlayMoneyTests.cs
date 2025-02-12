using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PlayTests;

public class PlayMoneyTests
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
    public async Task Say_10_billion()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(10_000_000_000, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\en-US-JennyNeural\\10.wav",
            "System Recordings\\en-US-JennyNeural\\Billion.wav",
            "System Recordings\\en-US-JennyNeural\\dollars.wav",
            "System Recordings\\en-US-JennyNeural\\and.wav",
            "System Recordings\\en-US-JennyNeural\\0.wav",
            "System Recordings\\en-US-JennyNeural\\cents.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task Say_10_million()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(10_000_000, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\en-US-JennyNeural\\10.wav",
            "System Recordings\\en-US-JennyNeural\\Million.wav",
            "System Recordings\\en-US-JennyNeural\\dollars.wav",
            "System Recordings\\en-US-JennyNeural\\and.wav",
            "System Recordings\\en-US-JennyNeural\\0.wav",
            "System Recordings\\en-US-JennyNeural\\cents.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task Say_negative_10_million()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(-10_000_000, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\en-US-JennyNeural\\negative.wav",
            "System Recordings\\en-US-JennyNeural\\10.wav",
            "System Recordings\\en-US-JennyNeural\\Million.wav",
            "System Recordings\\en-US-JennyNeural\\dollars.wav",
            "System Recordings\\en-US-JennyNeural\\and.wav",
            "System Recordings\\en-US-JennyNeural\\0.wav",
            "System Recordings\\en-US-JennyNeural\\cents.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task Say_1_25()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(1.25, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\en-US-JennyNeural\\1.wav",
            "System Recordings\\en-US-JennyNeural\\dollar.wav",
            "System Recordings\\en-US-JennyNeural\\and.wav",
            "System Recordings\\en-US-JennyNeural\\20.wav",
            "System Recordings\\en-US-JennyNeural\\5.wav",
            "System Recordings\\en-US-JennyNeural\\cents.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task Say_0_14()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(.14, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\en-US-JennyNeural\\0.wav",
            "System Recordings\\en-US-JennyNeural\\dollars.wav",
            "System Recordings\\en-US-JennyNeural\\and.wav",
            "System Recordings\\en-US-JennyNeural\\14.wav",
            "System Recordings\\en-US-JennyNeural\\cents.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task Say_1103_08()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(1103.08, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\en-US-JennyNeural\\1.wav",
            "System Recordings\\en-US-JennyNeural\\Thousand.wav",
            "System Recordings\\en-US-JennyNeural\\100.wav",
            "System Recordings\\en-US-JennyNeural\\3.wav",
            "System Recordings\\en-US-JennyNeural\\dollars.wav",
            "System Recordings\\en-US-JennyNeural\\and.wav",
            "System Recordings\\en-US-JennyNeural\\8.wav",
            "System Recordings\\en-US-JennyNeural\\cents.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task Say_1103_00()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(1103.00, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\en-US-JennyNeural\\1.wav",
            "System Recordings\\en-US-JennyNeural\\Thousand.wav",
            "System Recordings\\en-US-JennyNeural\\100.wav",
            "System Recordings\\en-US-JennyNeural\\3.wav",
            "System Recordings\\en-US-JennyNeural\\dollars.wav",
            "System Recordings\\en-US-JennyNeural\\and.wav",
            "System Recordings\\en-US-JennyNeural\\0.wav",
            "System Recordings\\en-US-JennyNeural\\cents.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task Say_1103_01()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(1103.01, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\en-US-JennyNeural\\1.wav",
            "System Recordings\\en-US-JennyNeural\\Thousand.wav",
            "System Recordings\\en-US-JennyNeural\\100.wav",
            "System Recordings\\en-US-JennyNeural\\3.wav",
            "System Recordings\\en-US-JennyNeural\\dollars.wav",
            "System Recordings\\en-US-JennyNeural\\and.wav",
            "System Recordings\\en-US-JennyNeural\\1.wav",
            "System Recordings\\en-US-JennyNeural\\cent.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task Say_200_00()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(200.00, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\en-US-JennyNeural\\200.wav",
            "System Recordings\\en-US-JennyNeural\\dollars.wav",
            "System Recordings\\en-US-JennyNeural\\and.wav",
            "System Recordings\\en-US-JennyNeural\\0.wav",
            "System Recordings\\en-US-JennyNeural\\cents.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }
    
}