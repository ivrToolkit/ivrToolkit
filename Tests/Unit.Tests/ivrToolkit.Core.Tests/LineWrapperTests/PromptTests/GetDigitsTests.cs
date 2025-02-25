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

namespace ivrToolkit.Core.Tests.LineWrapperTests.PromptTests;

public class GetDigitsTests
{
    private LineWrapper GetLineWrapper()
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory);

        var mock = new Mock<IIvrBaseLine>();
        mock.Setup(x => x.Management.TriggerDispose());
        mock.Setup(x => x.GetDigitsAsync(10, "#", CancellationToken.None, 0)).ReturnsAsync("1234");
        
        var testPauser = new TestPause(mock.Object);
        return new LineWrapper(loggerFactory, properties, 1, mock.Object, testPauser);
    }
    
    [Fact]
    public void TriggerDisposeBeforeGetDigits()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Management.TriggerDispose();
        
        var action = async () => await lineWrapper.GetDigitsAsync(10, "#", CancellationToken.None);
        action.ShouldThrow<DisposingException>();
    }
    
    [Fact]
    public void TriggerDisposeDuringGetDigits()
    {
        var lineWrapper = GetLineWrapper();
        
        var action = async () => await lineWrapper.GetDigitsAsync(10, "#", CancellationToken.None);
        action.ShouldThrow<DisposingException>();
    }
    
    [Fact]
    public void TriggerHangupDuringPlayFile()
    {
        var lineWrapper = GetLineWrapper();
        
        var action = async () => await lineWrapper.GetDigitsAsync(10, "#", CancellationToken.None);
        action.ShouldThrow<HangupException>();
    }
    
    [Fact]
    public void DisposeBeforeGetDigits()
    {
        var lineWrapper = GetLineWrapper();
        lineWrapper.Dispose();
        
        var action = async () => await lineWrapper.GetDigitsAsync(10, "#", CancellationToken.None);
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
    
}