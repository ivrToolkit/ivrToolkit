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

    [Theory]
    [InlineData("2025-01-18", "ddd", new[] {"Saturday"})]
    [InlineData("2025-01-19", "ddd", new[] {"Sunday"})]
    [InlineData("2025-01-20", "ddd",new[] {"Monday"})]
    [InlineData("2025-01-21", "ddd",new[] {"Tuesday"})]
    [InlineData("2025-01-22", "ddd",new[] {"Wednesday"})]
    [InlineData("2025-01-23", "ddd",new[] {"Thursday"})]
    [InlineData("2025-01-24", "ddd",new[] {"Friday"})]
    [InlineData("2025-01-24 14:00", "h",new[] {"14", "00 hours"})]
    [InlineData("2025-01-24 23:00", "h",new[] {"20", "3", "00 hours"})]
    [InlineData("2025-01-24 7:00", "h",new[] {"o", "7", "00 hours"})]
    [InlineData("2025-01-24 7:02", "h",new[] {"o", "7"})]
    [InlineData("2025-01-24 7:02", "h:n",new[] {"o", "7", "o", "2"})]
    [InlineData("2025-01-24 14:02", "h:n",new[] { "14", "o", "2"})]
    [InlineData("2025-01-24 14:02", "h:n a/p",new[] { "2", "o", "2", "pm"})]
    [InlineData("2025-01-24 00:02", "h:n a/p",new[] { "12", "o", "2", "am"})]
    [InlineData("2025-01-24 02:02", "h:n a/p",new[] { "2", "o", "2", "am"})]
    [InlineData("2025-01-24", "yyy",new[] { "2", "Thousand", "20","5"})]
    [InlineData("2125-01-24", "yyy",new[] { "20", "1", "20","5"})]
    [InlineData("2100-01-24", "yyy",new[] { "20", "1", "o","o"})]
    [InlineData("2105-01-24", "yyy",new[] { "20", "1", "o","5"})]
    public async Task Date_Mask_ddd(string dateTimeString, string mask, string[] expect)
    {
        DateTime dateTime = DateTime.Parse(dateTimeString);
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayDateAsync(dateTime, mask, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var min = Math.Min(actual.Count, expect.Length);

        for (var index = 0; index < min; index++)
        {
            actual[index].ShouldBeEquivalentTo($"System Recordings\\{expect[index]}.wav");
        }
        actual.Count.ShouldBe(expect.Length);
    }
}