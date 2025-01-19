using System;
using Shouldly;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ivrToolkit.Core.Tests;

public class LineWrapperTests
{

    private Mock<IIvrBaseLine> GetMockedLineBase()
    {
        var line = new Mock<IIvrBaseLine>();
        line.Setup(x => x.Management.TriggerDispose());
        return line;
    }

    private LineWrapper CreateLineWrapper()
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory)
        {
            PromptAttempts = 99,
            PromptBlankAttempts = 5,
            DigitsTimeoutInMilli = 5000
        };
        
        return new LineWrapper(new NullLoggerFactory(), properties, 1, GetMockedLineBase().Object);
    }
    private LineWrapper CreateLineWrapper(Mock<IIvrBaseLine> mockedLineBase)
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory)
        {
            PromptAttempts = 99,
            PromptBlankAttempts = 5,
            DigitsTimeoutInMilli = 5000
        };
        
        return new LineWrapper(new NullLoggerFactory(), properties, 1, mockedLineBase.Object);
    }

    #region Dial
    [Fact]
    public void LineWrapper_Dial_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.Dial("1", 1000);
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_Dial_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.Dial("1", 1000);
        act.ShouldThrow<DisposedException>();
    }

    [Fact]
    public void LineWrapper_Dial_anseringMachineLengthInMs_999_throws_ArgumentOutOfRangeException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.Dial("1", 999);
        act.ShouldThrow<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void LineWrapper_Dial_numberIsNull_throws_ArgumentNullException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.Dial(null, 1000);
        act.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void LineWrapper_Dial_numberIsEmpty_throws_ArgumentException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.Dial("", 1000);
        act.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void LineWrapper_Dial_GetAnsweringMachine_lineStatus_connected()
    {
        var mock = GetMockedLineBase();
        mock.Setup(x => x.Dial(It.IsAny<string>(), It.IsAny<int>())).Returns(CallAnalysis.AnsweringMachine);
        var test = CreateLineWrapper(mock);
        
        test.Dial("12223334444", 1000);
        test.Status.ShouldBe(LineStatusTypes.Connected);
    }

    [Fact]
    public void LineWrapper_Dial_GetConnected_lineStatus_connected()
    {
        var mock = GetMockedLineBase();
        mock.Setup(x => x.Dial(It.IsAny<string>(), It.IsAny<int>())).Returns(CallAnalysis.Connected);
        var test = CreateLineWrapper(mock);
        
        test.Dial("12223334444", 1000);
        test.Status.ShouldBe(LineStatusTypes.Connected);
    }

    [Fact]
    public void LineWrapper_Dial_GetBusy_lineStatus_onHook()
    {
        var mock = GetMockedLineBase();
        mock.Setup(x => x.Dial(It.IsAny<string>(), It.IsAny<int>())).Returns(CallAnalysis.Busy);

        var test = CreateLineWrapper(mock);
        test.Dial("12223334444", 1000);
        test.Status.ShouldBe(LineStatusTypes.OnHook);
    }

    [Fact]
    public void LineWrapper_Dial_GetStopped_throws_DisposingException()
    {
        var mock = GetMockedLineBase();
        mock.Setup(x => x.Dial(It.IsAny<string>(), It.IsAny<int>())).Returns(CallAnalysis.Stopped);
        var test = CreateLineWrapper(mock);

        Action act = () => test.Dial("12223334444", 1000);
        act.ShouldThrow<DisposingException>();
        test.Status.ShouldBe(LineStatusTypes.OffHook);
    }
    #endregion

    #region CheckDispose
    [Fact]
    public void LineWrapper_CheckDispose_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.CheckDispose();
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_CheckDispose_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.CheckDispose();
        act.ShouldThrow<DisposedException>();
    }
    #endregion

    #region WaitRings
    [Fact]
    public void LineWrapper_WaitRings_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.WaitRings(1);
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_WaitRings_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.WaitRings(1);
        act.ShouldThrow<DisposedException>();
    }
    #endregion

    #region Hangup
    [Fact]
    public void LineWrapper_Hangup_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.Hangup();
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_Hangup_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.Hangup();
        act.ShouldThrow<DisposedException>();
    }
    #endregion

    #region TakeOffHook
    [Fact]
    public void LineWrapper_TakeOffHook_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.TakeOffHook();
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_TakeOffHook_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.TakeOffHook();
        act.ShouldThrow<DisposedException>();
    }
    #endregion

    #region PlayFile
    [Fact]
    public void LineWrapper_PlayFile_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.PlayFile("");
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_PlayFile_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.PlayFile("");
        act.ShouldThrow<DisposedException>();
    }
    #endregion

    #region RecordToFile
    [Fact]
    public void LineWrapper_RecordToFile_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.RecordToFile("");
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_RecordToFile_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.RecordToFile("");
        act.ShouldThrow<DisposedException>();
    }
    #endregion

    #region GetDigits
    [Fact]
    public void LineWrapper_GetDigits_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.GetDigits(1, "");
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_GetDigits_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.GetDigits(1, "");
        act.ShouldThrow<DisposedException>();
    }
    #endregion

    #region FlushDigitBuffer
    [Fact]
    public void LineWrapper_FlushDigitBuffer_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.FlushDigitBuffer();
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_FlushDigitBuffer_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.FlushDigitBuffer();
        act.ShouldThrow<DisposedException>();
    }
    #endregion

    #region Volume
    [Fact]
    public void LineWrapper_Volume_throws_DisposingException()
    {
        var test = CreateLineWrapper();
        test.Management.TriggerDispose();

        Action act = () => test.Volume = 1;
        act.ShouldThrow<DisposingException>();
    }

    [Fact]
    public void LineWrapper_Volume_throws_DisposedException()
    {
        var test = CreateLineWrapper();
        test.Dispose();

        Action act = () => test.Volume = 1;
        act.ShouldThrow<DisposedException>();
    }
    #endregion
}
