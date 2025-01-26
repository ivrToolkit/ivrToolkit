using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests;

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
    public async Task Say_1_25()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayMoneyAsync(1.25, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\1.wav",
            "System Recordings\\dollar.wav",
            "System Recordings\\and.wav",
            "System Recordings\\20.wav",
            "System Recordings\\5.wav",
            "System Recordings\\cents.wav",
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
            "System Recordings\\0.wav",
            "System Recordings\\dollars.wav",
            "System Recordings\\and.wav",
            "System Recordings\\14.wav",
            "System Recordings\\cents.wav",
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
            "System Recordings\\1.wav",
            "System Recordings\\Thousand.wav",
            "System Recordings\\100.wav",
            "System Recordings\\3.wav",
            "System Recordings\\dollars.wav",
            "System Recordings\\and.wav",
            "System Recordings\\8.wav",
            "System Recordings\\cents.wav",
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
            "System Recordings\\1.wav",
            "System Recordings\\Thousand.wav",
            "System Recordings\\100.wav",
            "System Recordings\\3.wav",
            "System Recordings\\dollars.wav",
            "System Recordings\\and.wav",
            "System Recordings\\0.wav",
            "System Recordings\\cents.wav",
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
            "System Recordings\\1.wav",
            "System Recordings\\Thousand.wav",
            "System Recordings\\100.wav",
            "System Recordings\\3.wav",
            "System Recordings\\dollars.wav",
            "System Recordings\\and.wav",
            "System Recordings\\1.wav",
            "System Recordings\\cent.wav",
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
            "System Recordings\\200.wav",
            "System Recordings\\dollars.wav",
            "System Recordings\\and.wav",
            "System Recordings\\0.wav",
            "System Recordings\\cents.wav",
        };
        actual.ShouldBeEquivalentTo(expected);
    }
    
}