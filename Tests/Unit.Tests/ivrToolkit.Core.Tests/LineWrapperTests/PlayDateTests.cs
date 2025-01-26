using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests;

public class PlayDateTests
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
    public async Task Date_2025_1_25_16_20_0_Mask_mDdDyyyShCnSap()
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        var dateTime = new DateTime(2025, 1, 25, 16, 20, 0);
        await lineWrapper.PlayDateAsync(dateTime, "m-d-yyy h:n a/p", CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var expected = new List<string>()
        {
            "System Recordings\\January.wav",
            "System Recordings\\ord25.wav",
            "System Recordings\\2.wav",
            "System Recordings\\Thousand.wav",
            "System Recordings\\20.wav",
            "System Recordings\\5.wav",
            "System Recordings\\4.wav",
            "System Recordings\\20.wav",
            "System Recordings\\pm.wav"
        };
        actual.ShouldBeEquivalentTo(expected);
    }

    [Fact]
    public async Task Valid_Date()
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
    
}