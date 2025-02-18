using System;
using System.IO;
using System.Linq;
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

public class PlayTextToSpeechTests
{
    private static byte[] _wavFile =
    [
        82, 73, 70, 70, 100, 31, 0, 0, 87, 65, 86, 69, 102, 109, 116, 32,
        16, 0, 0, 0, 1, 0, 1, 0, 64, 31, 0, 0, 128, 62, 0, 0, 2, 0, 16, 0,
        100, 97, 116, 97, 64, 31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    ];
    private readonly WavStream _wavStream = new WavStream(_wavFile);

    private LineWrapper GetLineWrapper()
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);

        var mock = new Mock<IIvrBaseLine>();
        mock.Setup(x => x.Management.TriggerDispose());
        
        mock
            .Setup(x => x.PlayWavStreamAsync(It.IsAny<WavStream>(), It.IsAny<CancellationToken>()))
            .Callback<WavStream, CancellationToken>((stream, token) =>
            {
                if (stream == null) throw new HangupException();
            })
            .Returns(Task.CompletedTask);
        
        
        var testPauser = new TestPause(mock.Object);
        
        var mockTextToSpeech = new Mock<ITextToSpeech>();
        
        var textToSpeechGenerator = new TextToSpeechGenerator(loggerFactory, mockTextToSpeech.Object, new RegularFileHandler());
        return new LineWrapper(loggerFactory, properties, 1, mock.Object, testPauser, textToSpeechGenerator);
    }
    
    [Fact]
    public void TriggerDisposeBeforePlayTextToSpeech()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Management.TriggerDispose();
        
        var action = async () => await lineWrapper.PlayTextToSpeechAsync("Say something", CancellationToken.None);
        action.ShouldThrow<DisposingException>();
    }
    
    [Fact]
    public void TriggerDisposeDuringPlayFile()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Management.TriggerDispose();
        
        var action = async () => await lineWrapper.PlayTextToSpeechAsync("Test2.wav", CancellationToken.None);
        action.ShouldThrow<DisposingException>();
    }
    
    [Fact]
    public void TriggerHangupDuringPlayFile()
    {
        var lineWrapper = GetLineWrapper(); 
        
        var action = async () => await lineWrapper.PlayTextToSpeechAsync("Test3.wav", CancellationToken.None);
        action.ShouldThrow<HangupException>();
    }
    
    [Fact]
    public void DisposeBeforePlayFile()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Dispose();
        
        var action = async () => await lineWrapper.PlayTextToSpeechAsync("Test3.wav", CancellationToken.None);
        action.ShouldThrow<DisposedException>();
    }
    
    [Fact]
    public void AccidentallyDisposedTwice()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Dispose();
        lineWrapper.Dispose();
        
        var action = async () => await lineWrapper.PlayTextToSpeechAsync("Test3.wav", CancellationToken.None);
        action.ShouldThrow<DisposedException>();
    }
    
    [Fact]
    public void TriggerDisposeAfterDisposed()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Dispose();
        lineWrapper.Management.TriggerDispose();
        
        var action = async () => await lineWrapper.PlayTextToSpeechAsync("Test1.wav", CancellationToken.None);
        action.ShouldThrow<DisposedException>();
    }
    
    [Fact]
    public void TextToSpeechBuilder_Null_ThrowsArgumentNullException()
    {
        var lineWrapper = GetLineWrapper();

        var action = async () => await lineWrapper.PlayTextToSpeechAsync((ITextToSpeechBuilder)null, CancellationToken.None);
        action.ShouldThrow<ArgumentNullException>()
            .Message.ShouldBe("Value cannot be null. (Parameter 'textToSpeechBuilder')");
    }
    
    [Fact]
    public void GetTextToSpeechBuilder_null_ThrowsArgumentNullException()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Dispose();
        lineWrapper.Management.TriggerDispose();

        var textToSpeechGenerator = new TextToSpeechGenerator(new NullLoggerFactory(), null, null!);

        var action = () => textToSpeechGenerator.GetTextToSpeechBuilder(null!);
        action.ShouldThrow<ArgumentNullException>()
            .Message.ShouldBe("Value cannot be null. (Parameter 'text')");
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

        var textToSpeechGenerator = new TextToSpeechGenerator(new NullLoggerFactory(), null, mockFileHandler.Object);

        var textToSpeechBuilder = textToSpeechGenerator.GetTextToSpeechBuilder("say Something", "test.wav");
        
        var action = async () => await lineWrapper.PlayTextToSpeechAsync(textToSpeechBuilder, CancellationToken.None);
        action.ShouldThrow<VoiceException>()
            .Message.ShouldBe("Missing text to speech engine. Cannot convert text to Speech.");
    }
    
    [Fact]
    public void Null_TextToSpeech_wavFileDoesNotExist()
    {
        var lineWrapper = GetLineWrapper();
        
        var mockFileHandler = new Mock<IFileHandler>(MockBehavior.Strict);
        mockFileHandler.Setup(x => x.Exists(It.IsAny<string>())).Returns(false);

        var textToSpeechGenerator = new TextToSpeechGenerator(new NullLoggerFactory(), null, mockFileHandler.Object);

        var textToSpeechBuilder = textToSpeechGenerator.GetTextToSpeechBuilder("say Something", "test.wav");
        
        var action = async () => await lineWrapper.PlayTextToSpeechAsync(textToSpeechBuilder, CancellationToken.None);
        action.ShouldThrow<VoiceException>()
            .Message.ShouldBe("Missing text to speech engine. Cannot convert text to Speech.");
    }
    
    
    
    [Fact]
    public async Task WavFileMissing()
    {
        var lineWrapper = GetLineWrapper();
        
        const string wavFileName = "Test.wav";
        const string txtFileName = "Test.txt";
        const string message = "say something";
        
        var mockTextToSpeech = new Mock<ITextToSpeech>(MockBehavior.Strict);
        mockTextToSpeech.Setup(x => x.TextToSpeechAsync(message, It.IsAny<CancellationToken>())).ReturnsAsync(_wavStream);
        
        var mockFileHandler = new Mock<IFileHandler>(MockBehavior.Strict);
        mockFileHandler.Setup(x => x.Exists(wavFileName)).Returns(false);

        mockFileHandler.Setup(x => x.WriteAllBytesAsync(wavFileName, _wavStream.ToArray(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        mockFileHandler.Setup(x => x.WriteAllTextAsync(txtFileName, message,
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var textToSpeechGenerator = new TextToSpeechGenerator(new NullLoggerFactory(), 
            mockTextToSpeech.Object, 
            mockFileHandler.Object);

        var textToSpeechBuilder = textToSpeechGenerator.GetTextToSpeechBuilder(message, wavFileName);

        // act
        await lineWrapper.PlayTextToSpeechAsync(textToSpeechBuilder, CancellationToken.None);

        // assert

        mockFileHandler.Verify(x => x.Exists(wavFileName), Times.Once);
        mockFileHandler.Verify(x => x.WriteAllBytesAsync(wavFileName, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
        mockFileHandler.Verify(x => x.WriteAllTextAsync(txtFileName, message, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    
    
    
    [Fact]
    public async Task WavFileExistsNotChanged()
    {
        var lineWrapper = GetLineWrapper();
        
        const string wavFileName = "Test.wav";
        const string txtFileName = "Test.txt";
        const string message = "say something";
        const string messageFromTxtFile = message;
        
        var mockTextToSpeech = new Mock<ITextToSpeech>(MockBehavior.Strict);
        mockTextToSpeech.Setup(x => x.TextToSpeechAsync(message, It.IsAny<CancellationToken>())).ReturnsAsync(_wavStream);
        
        var mockFileHandler = new Mock<IFileHandler>(MockBehavior.Strict);
        mockFileHandler.Setup(x => x.Exists(wavFileName)).Returns(true);
        mockFileHandler.Setup(x => x.Exists(txtFileName)).Returns(true);
        mockFileHandler.Setup(x => x.ReadAllTextAsync(txtFileName, 
            It.IsAny<CancellationToken>())).ReturnsAsync(messageFromTxtFile);
        mockFileHandler.Setup(x => x.GetFileStream(wavFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            .Returns(_wavStream);

        mockFileHandler.Setup(x => x.WriteAllBytesAsync(wavFileName, _wavStream.ToArray(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        mockFileHandler.Setup(x => x.WriteAllTextAsync(txtFileName, message,
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var textToSpeechGenerator = new TextToSpeechGenerator(new NullLoggerFactory(), 
            mockTextToSpeech.Object, 
            mockFileHandler.Object);

        var textToSpeechBuilder = textToSpeechGenerator.GetTextToSpeechBuilder(message, wavFileName);

        // act
        await lineWrapper.PlayTextToSpeechAsync(textToSpeechBuilder, CancellationToken.None);

        // assert

        mockFileHandler.Verify(x => x.Exists(wavFileName), Times.Exactly(2));
        mockFileHandler.Verify(x => x.Exists(txtFileName), Times.Once);
        mockFileHandler.Verify(x => x.ReadAllTextAsync(txtFileName, It.IsAny<CancellationToken>()), Times.Once);
        mockFileHandler.Verify(x => x.GetFileStream(wavFileName, FileMode.Open, FileAccess.Read, FileShare.Read), Times.Once);
    }
    
    [Fact]
    public async Task WavFileExistsButChanged()
    {
        var lineWrapper = GetLineWrapper();
        
        const string wavFileName = "Test.wav";
        const string txtFileName = "Test.txt";
        const string message = "say something";
        const string messageFromTxtFile = "Something else";
        
        var mockTextToSpeech = new Mock<ITextToSpeech>(MockBehavior.Strict);
        mockTextToSpeech.Setup(x => x.TextToSpeechAsync(message, It.IsAny<CancellationToken>())).ReturnsAsync(_wavStream);
        
        var mockFileHandler = new Mock<IFileHandler>(MockBehavior.Strict);
        mockFileHandler.Setup(x => x.Exists(wavFileName)).Returns(true);
        mockFileHandler.Setup(x => x.Exists(txtFileName)).Returns(true);
        mockFileHandler.Setup(x => x.ReadAllTextAsync(txtFileName, 
            It.IsAny<CancellationToken>())).ReturnsAsync(messageFromTxtFile);
        mockFileHandler.Setup(x => x.GetFileStream(wavFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            .Returns(_wavStream);

        mockFileHandler.Setup(x => x.WriteAllBytesAsync(wavFileName, _wavStream.ToArray(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        mockFileHandler.Setup(x => x.WriteAllTextAsync(txtFileName, message,
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var textToSpeechGenerator = new TextToSpeechGenerator(new NullLoggerFactory(), 
            mockTextToSpeech.Object, 
            mockFileHandler.Object);

        var textToSpeechBuilder = textToSpeechGenerator.GetTextToSpeechBuilder(message, wavFileName);

        // act
        await lineWrapper.PlayTextToSpeechAsync(textToSpeechBuilder, CancellationToken.None);

        // assert

        mockFileHandler.Verify(x => x.Exists(wavFileName), Times.Once);
        mockFileHandler.Verify(x => x.Exists(txtFileName), Times.Once);
        mockFileHandler.Verify(x => x.ReadAllTextAsync(txtFileName, It.IsAny<CancellationToken>()), Times.Once);
        mockFileHandler.Verify(x => x.WriteAllBytesAsync(wavFileName, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
        mockFileHandler.Verify(x => x.WriteAllTextAsync(txtFileName, message, It.IsAny<CancellationToken>()), Times.Once);
    }
    
    [Fact]
    public async Task WavFileExistsButTxtFileMissing()
    {
        var lineWrapper = GetLineWrapper();
        
        const string wavFileName = "Test.wav";
        const string txtFileName = "Test.txt";
        const string message = "say something";
        const string messageFromTxtFile = "Something else";
        
        var mockTextToSpeech = new Mock<ITextToSpeech>(MockBehavior.Strict);
        mockTextToSpeech.Setup(x => x.TextToSpeechAsync(message, It.IsAny<CancellationToken>())).ReturnsAsync(_wavStream);
        
        var mockFileHandler = new Mock<IFileHandler>(MockBehavior.Strict);
        mockFileHandler.Setup(x => x.Exists(wavFileName)).Returns(true);
        mockFileHandler.Setup(x => x.Exists(txtFileName)).Returns(false);
        mockFileHandler.Setup(x => x.ReadAllTextAsync(txtFileName, 
            It.IsAny<CancellationToken>())).ReturnsAsync(messageFromTxtFile);
        mockFileHandler.Setup(x => x.GetFileStream(wavFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            .Returns(_wavStream);

        mockFileHandler.Setup(x => x.WriteAllBytesAsync(wavFileName, _wavStream.ToArray(),
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        
        mockFileHandler.Setup(x => x.WriteAllTextAsync(txtFileName, message,
            It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var textToSpeechGenerator = new TextToSpeechGenerator(new NullLoggerFactory(), 
            mockTextToSpeech.Object, 
            mockFileHandler.Object);

        var textToSpeechBuilder = textToSpeechGenerator.GetTextToSpeechBuilder(message, wavFileName);

        // act
        await lineWrapper.PlayTextToSpeechAsync(textToSpeechBuilder, CancellationToken.None);

        // assert

        mockFileHandler.Verify(x => x.Exists(wavFileName), Times.Once);
        mockFileHandler.Verify(x => x.Exists(txtFileName), Times.Once);
        mockFileHandler.Verify(x => x.WriteAllBytesAsync(wavFileName, It.IsAny<byte[]>(), It.IsAny<CancellationToken>()), Times.Once);
        mockFileHandler.Verify(x => x.WriteAllTextAsync(txtFileName, message, It.IsAny<CancellationToken>()), Times.Once);
    }
}