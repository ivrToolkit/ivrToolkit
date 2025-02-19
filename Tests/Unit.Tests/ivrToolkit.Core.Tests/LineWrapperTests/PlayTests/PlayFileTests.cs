

using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.TTS;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PlayTests;

public class PlayFileTests
{
    private LineWrapper GetLineWrapper()
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);

        var mock = new Mock<IIvrBaseLine>();
        mock.Setup(x => x.PlayFileAsync("Test.wav", It.IsAny<CancellationToken>()));
        mock.Setup(x => x.Management.TriggerDispose());
        mock.Setup(x => x.PlayFileAsync("Test2.wav", It.IsAny<CancellationToken>())).ThrowsAsync(new DisposingException());
        mock.Setup(x => x.PlayFileAsync("Test3.wav", It.IsAny<CancellationToken>())).ThrowsAsync(new HangupException());
        
        var testPauser = new TestPause(mock.Object);
        return new LineWrapper(loggerFactory, properties, 1, mock.Object, testPauser);
    }
    
    [Fact]
    public void TriggerDisposeBeforePlayFile()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Management.TriggerDispose();
        
        var action = async () => await lineWrapper.PlayFileAsync("Test1.wav", CancellationToken.None);
        action.ShouldThrow<DisposingException>();
    }
    
    [Fact]
    public void TriggerDisposeDuringPlayFile()
    {
        var lineWrapper = GetLineWrapper();
        
        var action = async () => await lineWrapper.PlayFileAsync("Test2.wav", CancellationToken.None);
        action.ShouldThrow<DisposingException>();
    }
    
    [Fact]
    public void TriggerHangupDuringPlayFile()
    {
        var lineWrapper = GetLineWrapper();
        
        var action = async () => await lineWrapper.PlayFileAsync("Test3.wav", CancellationToken.None);
        action.ShouldThrow<HangupException>();
    }
    
    [Fact]
    public void DisposeBeforePlayFile()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Dispose();
        
        var action = async () => await lineWrapper.PlayFileAsync("Test3.wav", CancellationToken.None);
        action.ShouldThrow<DisposedException>();
    }
    
    [Fact]
    public void AccidentallyDisposedTwice()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Dispose();
        lineWrapper.Dispose();
        
        var action = async () => await lineWrapper.PlayFileAsync("Test3.wav", CancellationToken.None);
        action.ShouldThrow<DisposedException>();
    }
    
    [Fact]
    public void TriggerDisposeAfterDisposed()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Dispose();
        lineWrapper.Management.TriggerDispose();
        
        var action = async () => await lineWrapper.PlayFileAsync("Test1.wav", CancellationToken.None);
        action.ShouldThrow<DisposedException>();
    }
    
    [Fact]
    public void TextToSpeechBuilder_Null_ThrowsArgumentNullException()
    {
        var lineWrapper = GetLineWrapper();

        var action = async () => await lineWrapper.PlayFileAsync((ITextToSpeechCache)null, CancellationToken.None);
        action.ShouldThrow<ArgumentNullException>()
            .Message.ShouldBe("Value cannot be null. (Parameter 'textToSpeechBuilder')");
    }
    
    [Fact]
    public void GetTextToSpeechBuilder_null_ThrowsArgumentNullException()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Dispose();
        lineWrapper.Management.TriggerDispose();

        var textToSpeechGenerator = new TextToSpeechCacheFactory(new NullLoggerFactory(), null, null!);

        var action = () => textToSpeechGenerator.Create(null!);
        action.ShouldThrow<ArgumentNullException>()
            .Message.ShouldBe("Value cannot be null. (Parameter 'text')");
    }

    [Fact]
    public void Null_TextToSpeechEngine_RequiresWavFileName()
    {
        var lineWrapper = GetLineWrapper();

        var textToSpeechGenerator = new TextToSpeechCacheFactory(new NullLoggerFactory(), null, new RegularFileHandler());

        var textToSpeechBuilder = textToSpeechGenerator.Create("say Something");
        
        var action = async () => await lineWrapper.PlayFileAsync(textToSpeechBuilder, CancellationToken.None);
        action.ShouldThrow<VoiceException>()
            .Message.ShouldBe("Requires fileName.");
    }


    [Fact]
    public void Null_TextToSpeech_fileExistsButDifferent()
    {
        var lineWrapper = GetLineWrapper();
        
        var mockFileHandler = new Mock<IFileHandler>(MockBehavior.Strict);
        mockFileHandler.Setup(x => x.Exists("test.wav")).Returns(true);
        mockFileHandler.Setup(x => x.Exists("test.txt")).Returns(true);
        mockFileHandler.Setup(x => x.ReadAllTextAsync("test.txt", 
            It.IsAny<CancellationToken>())).ReturnsAsync("something different");

        var textToSpeechGenerator = new TextToSpeechCacheFactory(new NullLoggerFactory(), null, mockFileHandler.Object);

        var textToSpeechBuilder = textToSpeechGenerator.Create("say Something", "test.wav");
        
        var action = async () => await lineWrapper.PlayFileAsync(textToSpeechBuilder, CancellationToken.None);
        action.ShouldThrow<VoiceException>()
            .Message.ShouldBe("Missing text to speech engine. Cannot convert text to Speech.");
    }
    
    [Fact]
    public void Null_TextToSpeech_wavFileDoesNotExist()
    {
        var lineWrapper = GetLineWrapper();
        
        var mockFileHandler = new Mock<IFileHandler>(MockBehavior.Strict);
        mockFileHandler.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);

        var textToSpeechGenerator = new TextToSpeechCacheFactory(new NullLoggerFactory(), null, mockFileHandler.Object);

        var textToSpeechBuilder = textToSpeechGenerator.Create("say Something", "test.wav");
        
        var action = async () => await lineWrapper.PlayFileAsync(textToSpeechBuilder, CancellationToken.None);
        action.ShouldThrow<VoiceException>()
            .Message.ShouldBe("Missing text to speech engine. Cannot convert text to Speech.");
    }
    
    [Fact]
    public async Task WavFileExistsNotChanged_executionPathTest()
    {
        var lineWrapper = GetLineWrapper();
        
        var mockTextToSpeechBuilder = new Mock<ITextToSpeechCache>();
        
        var mockTextToSpeechGenerator = new Mock<ITextToSpeechCacheFactory>(MockBehavior.Strict);
        mockTextToSpeechGenerator.Setup(x => x.Create(It.IsAny<string>(),
            It.IsAny<string>())).Returns(mockTextToSpeechBuilder.Object);

        var textToSpeechGenerator = mockTextToSpeechGenerator.Object;
        var textToSpeechBuilder = textToSpeechGenerator.Create("say something", "Test.wav");
        
        //act
        await lineWrapper.PlayFileAsync(textToSpeechBuilder, CancellationToken.None);
        
        // assert
        mockTextToSpeechBuilder.Verify(x => x.GenerateCacheAsync(
            It.IsAny<CancellationToken>()), Times.Once);
        mockTextToSpeechBuilder.Verify(x => x.GetCacheFileName(), Times.Once);
        mockTextToSpeechBuilder.Verify(x => x.GetOrGenerateCacheAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }
    
    [Fact]
    public async Task WavFileExistsNotChanged()
    {
        var lineWrapper = GetLineWrapper();
        
        var message = "say something";

        var mockTextToSpeech = new Mock<ITextToSpeech>(MockBehavior.Strict);
        
        var mockFileHandler = new Mock<IFileHandler>(MockBehavior.Strict);
        mockFileHandler.Setup(x => x.Exists("Test.wav")).Returns(true);
        mockFileHandler.Setup(x => x.Exists("Test.txt")).Returns(true);
        mockFileHandler.Setup(x => x.ReadAllTextAsync("Test.txt", 
            It.IsAny<CancellationToken>())).ReturnsAsync(message);

        var textToSpeechGenerator = new TextToSpeechCacheFactory(new NullLoggerFactory(), 
            mockTextToSpeech.Object, 
            mockFileHandler.Object);

        var textToSpeechBuilder = textToSpeechGenerator.Create(message, "Test.wav");
        
        await lineWrapper.PlayFileAsync(textToSpeechBuilder, CancellationToken.None);
        // once for test.wav and once for test.txt
        mockFileHandler.Verify(x => x.Exists(It.IsAny<string>()), Times.Exactly(2));
    }
}