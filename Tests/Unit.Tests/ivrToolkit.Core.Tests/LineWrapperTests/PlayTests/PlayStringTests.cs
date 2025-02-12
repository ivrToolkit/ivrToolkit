using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PlayTests;

public class PlayStringTests
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
    public void Abc_throws_FormatException()
    {
        var lineWrapper = GetLineWrapper(out _);
        var action = async () => await lineWrapper.PlayStringAsync("abc", CancellationToken.None);
        action.ShouldThrow<FormatException>().Message.ShouldBe("Requires 1 or 2 pipes: abc.");
    }
    
    [Fact]
    public void AbcPipeM_throws_FormatException()
    {
        var lineWrapper = GetLineWrapper(out _);
        var action = async () => await lineWrapper.PlayStringAsync("abc|m", CancellationToken.None);
        action.ShouldThrow<FormatException>().Message.ShouldBe("The input string 'abc' was not in a correct format.");
    }

    [Fact]
    public void AbcPipeO_throws_FormatException()
    {
        var lineWrapper = GetLineWrapper(out _);
        var action = async () => await lineWrapper.PlayStringAsync("abc|o", CancellationToken.None);
        action.ShouldThrow<FormatException>().Message.ShouldBe("The input string 'abc' was not in a correct format.");
    }
    
    [Fact]
    public void AbcPipeDef_throws_FormatException()
    {
        var lineWrapper = GetLineWrapper(out _);
        var action = async () => await lineWrapper.PlayStringAsync("abc|def", CancellationToken.None);
        action.ShouldThrow<FormatException>().Message.ShouldBe("Invalid command: def");
    }
    [Fact]
    public void AbcPipeN_throws_FormatException()
    {
        var lineWrapper = GetLineWrapper(out _);
        var action = async () => await lineWrapper.PlayStringAsync("abc|N", CancellationToken.None);
        action.ShouldThrow<FormatException>().Message.ShouldBe("The input string 'abc' was not in a correct format.");
    }
    
    [Theory]
    [InlineData("1.25|N", new[] {"1","point","2","5"})]
    [InlineData("1.25|n", new[] {"1","point","2","5"})]
    [InlineData("1.25|m", new[] { "1","dollar","and", "20","5", "cents" })]
    [InlineData("abc|c", new[] {"a","b","c"})]
    [InlineData("2025-01-26|d|m", new[] {"January"})]
    [InlineData("23|o", new[] { "ord23"})]
    [InlineData("somefile.wav|f", new[] { ">somefile.wav"})]
    [InlineData("abc|c,23|o", new[] {"a","b","c", "ord23"})]
    [InlineData("abc|c,", new[] {"a","b","c"})]
    [InlineData(",,abc||c,,", new[] {"a","b","c"})]
    public async Task Strings(string myString, string[] expect)
    {
        var lineWrapper = GetLineWrapper(out FakeLine fakeLine);
        await lineWrapper.PlayStringAsync(myString, CancellationToken.None);

        var actual = fakeLine.PlayList;
        
        var min = Math.Min(actual.Count, expect.Length);

        for (var index = 0; index < min; index++)
        {
            actual[index].ShouldBeEquivalentTo(expect[index].StartsWith(">")
                ? expect[index].Substring(1)
                : $"System Recordings\\en-US-JennyNeural\\{expect[index]}.wav");
        }
        actual.Count.ShouldBe(expect.Length);
    }
}