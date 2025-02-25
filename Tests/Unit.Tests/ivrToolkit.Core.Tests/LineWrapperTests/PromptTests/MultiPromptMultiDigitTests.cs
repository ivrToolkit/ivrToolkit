using System.Collections.Generic;
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

public class MultiPromptMultiDigitMultiDigitTests
{
    private LineWrapper GetLineWrapper(out Mock<IIvrLine> mockLine)
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);

        
        mockLine = new Mock<IIvrLine>();
        
        var testPauser = new TestPause(mockLine.Object);
        
        return new LineWrapper(loggerFactory, properties, 1, mockLine.Object, testPauser);
    }
    
    [Fact]
    public async Task Enter_123P_123t_1234P_Returns_1234()
    {
        var queue = new Queue<string>();
        queue.Enqueue("123#");
        queue.Enqueue("123t");
        queue.Enqueue("1234#");
        
        var lineWrapper = GetLineWrapper(out var mockLine);

        mockLine.Setup(x => x.GetDigitsAsync(It.IsAny<int>(),
            "#t",
            It.IsAny<CancellationToken>(),
            It.IsAny<int>())).ReturnsAsync(queue.Dequeue);
        
        var actual = await lineWrapper.MultiTryPromptAsync("Test.wav",
            value => value == "1234",
            CancellationToken.None);
        actual.ShouldBe("1234");
        lineWrapper.LastTerminator.ShouldBe("#");

        mockLine.Verify(x => x.PlayFileAsync("Test.wav", 
            It.IsAny<CancellationToken>()), Times.Exactly(3));
        mockLine.Verify(x => x.GetDigitsAsync(30, "#t",
            It.IsAny<CancellationToken>(), 0), Times.Exactly(3));
    }
    
    [Fact]
    public async Task Enter_1234P_Uses_PlayFileAsync()
    {
        var queue = new Queue<string>();
        queue.Enqueue("1234#");
        
        var lineWrapper = GetLineWrapper(out var mockLine);

        mockLine.Setup(x => x.GetDigitsAsync(It.IsAny<int>(),
            "#t",
            It.IsAny<CancellationToken>(),
            It.IsAny<int>())).ReturnsAsync(queue.Dequeue);

        var mockTextToSpeechCache = new Mock<ITextToSpeechCache>();
        mockTextToSpeechCache.Setup(x => x.GetCacheFileName()).Returns("Test.wav");
        
        
        var actual = await lineWrapper.MultiTryPromptAsync(mockTextToSpeechCache.Object,
            value => value == "1234",
            CancellationToken.None);
        actual.ShouldBe("1234");
        lineWrapper.LastTerminator.ShouldBe("#");

        mockLine.Verify(x => x.PlayFileAsync("Test.wav", It.IsAny<CancellationToken>()), Times.Exactly(1));
        mockLine.Verify(x => x.GetDigitsAsync(30, "#t",
            It.IsAny<CancellationToken>(), 0), Times.Exactly(1));
    }
    
    [Fact]
    public async Task Enter_1234P_Uses_PlayWavStreamAsync()
    {
        var queue = new Queue<string>();
        queue.Enqueue("1234#");
        
        var lineWrapper = GetLineWrapper(out var mockLine);

        mockLine.Setup(x => x.GetDigitsAsync(It.IsAny<int>(),
            "#t",
            It.IsAny<CancellationToken>(),
            It.IsAny<int>())).ReturnsAsync(queue.Dequeue);

        var mockTextToSpeechCache = new Mock<ITextToSpeechCache>();
        mockTextToSpeechCache.Setup(x => x.GetCacheFileName()).Returns((string)null);
        
        
        var actual = await lineWrapper.MultiTryPromptAsync(mockTextToSpeechCache.Object,
            value => value == "1234",
            CancellationToken.None);
        actual.ShouldBe("1234");
        lineWrapper.LastTerminator.ShouldBe("#");

        mockLine.Verify(x => x.PlayWavStreamAsync(It.IsAny<WavStream>(), It.IsAny<CancellationToken>()), Times.Exactly(1));
        mockLine.Verify(x => x.GetDigitsAsync(30, "#t",
            It.IsAny<CancellationToken>(), 0), Times.Exactly(1));
    }
    
    [Fact]
    public async Task SayInvalidAnswer()
    {
        var queue = new Queue<string>();
        queue.Enqueue("123#");
        queue.Enqueue("1234#");
        
        var lineWrapper = GetLineWrapper(out var mockLine);

        mockLine.Setup(x => x.GetDigitsAsync(It.IsAny<int>(),
            "#t",
            It.IsAny<CancellationToken>(),
            It.IsAny<int>())).ReturnsAsync(queue.Dequeue);

        var mockTextToSpeechCache = new Mock<ITextToSpeechCache>();
        mockTextToSpeechCache.Setup(x => x.GetCacheFileName()).Returns((string)null);

        var options = new MultiTryPromptOptions
        {
            InvalidAnswerMessage = "invalidAnswer.wav"
        };
        
        var actual = await lineWrapper.MultiTryPromptAsync(mockTextToSpeechCache.Object,
            value => value == "1234",
            options,
            CancellationToken.None);
        actual.ShouldBe("1234");
        lineWrapper.LastTerminator.ShouldBe("#");

        mockLine.Verify(x => x.PlayFileAsync("invalidAnswer.wav", It.IsAny<CancellationToken>()), Times.Once);
        mockLine.Verify(x => x.PlayWavStreamAsync(It.IsAny<WavStream>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockLine.Verify(x => x.GetDigitsAsync(30, "#t",
            It.IsAny<CancellationToken>(), 0), Times.Exactly(2));
    }
    
    [Fact]
    public async Task PlayFile_with_options()
    {
        var queue = new Queue<string>();
        queue.Enqueue("123#");
        queue.Enqueue("1234#");
        
        var lineWrapper = GetLineWrapper(out var mockLine);

        mockLine.Setup(x => x.GetDigitsAsync(It.IsAny<int>(),
            "#t",
            It.IsAny<CancellationToken>(),
            It.IsAny<int>())).ReturnsAsync(queue.Dequeue);

        var mockTextToSpeechCache = new Mock<ITextToSpeechCache>();
        mockTextToSpeechCache.Setup(x => x.GetCacheFileName()).Returns((string)null);

        var options = new MultiTryPromptOptions
        {
            InvalidAnswerMessage = "invalidAnswer.wav"
        };
        
        var actual = await lineWrapper.MultiTryPromptAsync("test.wav",
            value => value == "1234",
            options,
            CancellationToken.None);
        actual.ShouldBe("1234");
        lineWrapper.LastTerminator.ShouldBe("#");

        mockLine.Verify(x => x.PlayFileAsync("invalidAnswer.wav", It.IsAny<CancellationToken>()), Times.Once);
        mockLine.Verify(x => x.PlayFileAsync("test.wav", It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockLine.Verify(x => x.GetDigitsAsync(30, "#t",
            It.IsAny<CancellationToken>(), 0), Times.Exactly(2));
    }
    
    [Fact]
    public async Task SpecialTerminator()
    {
        var queue = new Queue<string>();
        queue.Enqueue("12*");
        queue.Enqueue("1234#");
        
        var lineWrapper = GetLineWrapper(out var mockLine);

        mockLine.Setup(x => x.GetDigitsAsync(It.IsAny<int>(),
            "#*t",
            It.IsAny<CancellationToken>(),
            It.IsAny<int>())).ReturnsAsync(queue.Dequeue);

        var mockTextToSpeechCache = new Mock<ITextToSpeechCache>();
        mockTextToSpeechCache.Setup(x => x.GetCacheFileName()).Returns((string)null);

        var specialTerminatorPressed = false;
        
        var options = new MultiTryPromptOptions
        {
            SpecialTerminator = "*",
            OnSpecialTerminator = () =>
            {
                specialTerminatorPressed = true;
            }
        };
        
        var actual = await lineWrapper.MultiTryPromptAsync(mockTextToSpeechCache.Object,
            value => value == "1234",
            options,
            CancellationToken.None);
        actual.ShouldBe("1234");
        specialTerminatorPressed.ShouldBeTrue();
        lineWrapper.LastTerminator.ShouldBe("#");

        mockLine.Verify(x => x.PlayWavStreamAsync(It.IsAny<WavStream>(), 
            It.IsAny<CancellationToken>()), Times.Exactly(2));
        mockLine.Verify(x => x.GetDigitsAsync(30, "#*t",
            It.IsAny<CancellationToken>(), 0), Times.Exactly(2));
    }
    
    [Fact]
    public void TooManyBlankTries()
    {
        var queue = new Queue<string>();
        queue.Enqueue("#");
        queue.Enqueue("#");
        queue.Enqueue("#");
        queue.Enqueue("#");
        queue.Enqueue("#");
        
        var lineWrapper = GetLineWrapper(out var mockLine);

        mockLine.Setup(x => x.GetDigitsAsync(It.IsAny<int>(),
            "#t",
            It.IsAny<CancellationToken>(),
            It.IsAny<int>())).ReturnsAsync(queue.Dequeue);

        var mockTextToSpeechCache = new Mock<ITextToSpeechCache>();
        mockTextToSpeechCache.Setup(x => x.GetCacheFileName()).Returns((string)null);
        
        var action = async () => await lineWrapper.MultiTryPromptAsync(mockTextToSpeechCache.Object,
            value => value == "1234",
            CancellationToken.None);
        action.ShouldThrow<TooManyAttempts>();
    }
}

