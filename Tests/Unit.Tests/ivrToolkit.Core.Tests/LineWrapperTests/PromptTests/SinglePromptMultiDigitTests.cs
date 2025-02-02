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

public class SinglePromptMultiDigitMultiDigitTests
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
    public async Task Enter_1234P_Returns_1234()
    {
        var lineWrapper = GetLineWrapper("1234#");
        
        var actual = await lineWrapper.PromptAsync("Doesn'tMatter", CancellationToken.None);
        actual.ShouldBe("1234");
    }

    [Fact]
    public void Enter_1234ThenLetTimeout_ShouldThrowTooManyAttempts()
    {
        var lineWrapper = GetLineWrapper("1234t");
        
        var action = async () => await lineWrapper.PromptAsync("Doesn'tMatter", CancellationToken.None);
        action.ShouldThrow<TooManyAttempts>();
    }
    
    [Fact]
    public void Timeout_ShouldThrowTooManyAttempts()
    {
        var lineWrapper = GetLineWrapper("t");
        
        var action = async () => await lineWrapper.PromptAsync("Doesn'tMatter", CancellationToken.None);
        action.ShouldThrow<TooManyAttempts>();
    }
    
    [Fact]
    public async Task AllowTimeout_Enter_12t_Returns_12()
    {
        var lineWrapper = GetLineWrapper("12t");

        var promptOptions = new PromptOptions
        {
            Terminators = "#t"
        };
        
        var actual = await lineWrapper.PromptAsync("Doesn'tMatter", CancellationToken.None,
            promptOptions);
        actual.ShouldBe("12");
    }
    
    [Fact]
    public async Task AllowEmpty_Press_Pound_Returns_Empty()
    {
        var lineWrapper = GetLineWrapper("#");

        var promptOptions = new PromptOptions
        {
            AllowEmpty = true
        };
        
        var actual = await lineWrapper.PromptAsync("Doesn'tMatter", CancellationToken.None,
            promptOptions);
        actual.ShouldBe("");
    }
    
    [Fact]
    public async Task AllowEmptyAndTimeout_PressNothing_Returns_Empty()
    {
        var lineWrapper = GetLineWrapper("t");

        // not sure why you would ever do this
        var promptOptions = new PromptOptions
        {
            Terminators = "#t",
            AllowEmpty = true
        };
        
        var actual = await lineWrapper.PromptAsync("Doesn'tMatter", CancellationToken.None,
            promptOptions);
        actual.ShouldBe("");
    }
    
}

