using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PlayTests;

public class PlayPhoneNumberTests
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
    public async Task Null_Does_Nothing()
    {
        var lineWrapper = GetLineWrapper(out _);
        await lineWrapper.PlayPhoneNumberAsync(null, CancellationToken.None);
    }

    [Fact]
    public async Task ValidPhoneNumber_3334444()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayPhoneNumberAsync("3334444", CancellationToken.None);
        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\3.wav",
            "System Recordings\\3.wav",
            "System Recordings\\3.wav",
            "Delay(500)",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
        };
        
        actual.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task ValidPhoneNumber_2223334444()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayPhoneNumberAsync("2223334444", CancellationToken.None);
        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\2.wav",
            "System Recordings\\2.wav",
            "System Recordings\\2.wav",
            "Delay(500)",
            "System Recordings\\3.wav",
            "System Recordings\\3.wav",
            "System Recordings\\3.wav",
            "Delay(500)",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
        };
        
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task ValidPhoneNumber_12223334444()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayPhoneNumberAsync("12223334444", CancellationToken.None);
        var actual = fakeLine.PlayList;

        var expected = new List<string>()
        {
            "System Recordings\\1.wav",
            "Delay(500)",
            "System Recordings\\2.wav",
            "System Recordings\\2.wav",
            "System Recordings\\2.wav",
            "Delay(500)",
            "System Recordings\\3.wav",
            "System Recordings\\3.wav",
            "System Recordings\\3.wav",
            "Delay(500)",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
        };
        
        actual.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task TooLong()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayPhoneNumberAsync("11222333444", CancellationToken.None);
        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\1.wav",
            "Delay(500)",
            "System Recordings\\1.wav",
            "System Recordings\\2.wav",
            "System Recordings\\2.wav",
            "Delay(500)",
            "System Recordings\\2.wav",
            "System Recordings\\3.wav",
            "System Recordings\\3.wav",
            "Delay(500)",
            "System Recordings\\3.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
        };
        
        actual.ShouldBeEquivalentTo(expected);
    }
    
    [Fact]
    public async Task TooShort_444()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayPhoneNumberAsync("444", CancellationToken.None);
        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
        };
        
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task TooShort_34444()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayPhoneNumberAsync("34444", CancellationToken.None);
        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\3.wav",
            "Delay(500)",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
            "System Recordings\\4.wav",
        };
        
        actual.ShouldBeEquivalentTo(expected);
    }

}