using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PromptTests;

public class SinglePromptSingleDigitTests
{
    private LineWrapper GetLineWrapper(string keysPressed)
    {
        var terminator = keysPressed[^1].ToString();
        var lineMock = new Mock<IIvrBaseLine>();
        lineMock.Setup(x => x.LastTerminator).Returns(terminator);
        lineMock.Setup(x => x.GetDigitsAsync(It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>(),
            0)
        ).ReturnsAsync(keysPressed);
        
        
        var loggerFactory = new NullLoggerFactory();
        
        var properties = new VoiceProperties(loggerFactory)
        {
            PromptAttempts = 99,
            PromptBlankAttempts = 5,
            DigitsTimeoutInMilli = 5000
        };
        
        var testPauser = new TestPause(lineMock.Object);
        return new LineWrapper(loggerFactory, properties, 1, lineMock.Object, testPauser);
    }
    
    [Fact]
    public async Task Allow_any_press_1_returns_1()
    {
        var lineWrapper = GetLineWrapper("1");
        var promptOptions = new PromptOptions
        {
            MaxLength = 1
        };
        
        var actual = await lineWrapper.PromptAsync("Doesn'tMatter", 
            CancellationToken.None, promptOptions);
        actual.ShouldBe("1");
    }
    
    [Fact]
    public async Task Allow_123_press_1_returns_1()
    {
        var lineWrapper = GetLineWrapper("1");
        var promptOptions = new PromptOptions
        {
            MaxLength = 1,
            AllowedDigits = "123"
        };
        
        var actual = await lineWrapper.PromptAsync("Doesn'tMatter", 
            CancellationToken.None, promptOptions);
        actual.ShouldBe("1");
    }
    
    [Fact]
    public void Allow_123_press_4_throws_TooManyAttempts()
    {
        var lineWrapper = GetLineWrapper("4");
        var promptOptions = new PromptOptions
        {
            MaxLength = 1,
            AllowedDigits = "123"
        };
        
        var action = async () => await lineWrapper.PromptAsync("Doesn'tMatter", 
            CancellationToken.None, promptOptions);
        action.ShouldThrow<TooManyAttempts>();
    }
}

