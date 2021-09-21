using System;
using FluentAssertions;
using FluentAssertions.Execution;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace ivrToolkit.Core.Tests
{
    public class LineWrapperTests
    {

        private Mock<ILineBase> GetMockedLineBase()
        {
            var line = new Mock<ILineBase>();
            line.Setup(x => x.Management.Dispose());
            //line.Setup(x => x.Dial("1", 1000)).Returns(CallAnalysis.AnsweringMachine);
            //line.Setup(x => x.Dial("1", 1000)).Returns(CallAnalysis.Connected);
            //line.Setup(x => x.Dial("1", 1000)).Returns(CallAnalysis.Stopped);
            //line.Setup(x => x.Dial("1", 1000)).Returns(CallAnalysis.NoAnswer);
            return line;
        }

        #region Dial
        [Fact]
        public void LineWrapper_Dial_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.Dial("1", 1000);
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_Dial_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.Dial("1", 1000);
            act.Should().Throw<DisposedException>();
        }

        [Fact]
        public void LineWrapper_Dial_anseringMachineLengthInMs_999_throws_ArgumentOutOfRangeException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.Dial("1", 999);
            act.Should().Throw<ArgumentOutOfRangeException>();
        }

        [Fact]
        public void LineWrapper_Dial_numberIsNull_throws_ArgumentNullException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.Dial(null, 1000);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void LineWrapper_Dial_numberIsEmpty_throws_ArgumentException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.Dial("", 1000);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void LineWrapper_Dial_GetAnsweringMachine_lineStatus_connected()
        {
            var mock = GetMockedLineBase();
            mock.Setup(x => x.Dial("12223334444", 1000)).Returns(CallAnalysis.AnsweringMachine);

            var test = new LineWrapper(new NullLoggerFactory(), 1, mock.Object);
            var result = test.Dial("12223334444", 1000);
            test.Status.Should().Be(LineStatusTypes.Connected);
        }

        [Fact]
        public void LineWrapper_Dial_GetConnected_lineStatus_connected()
        {
            var mock = GetMockedLineBase();
            mock.Setup(x => x.Dial("12223334444", 1000)).Returns(CallAnalysis.Connected);

            var test = new LineWrapper(new NullLoggerFactory(), 1, mock.Object);
            var result = test.Dial("12223334444", 1000);
            test.Status.Should().Be(LineStatusTypes.Connected);
        }

        [Fact]
        public void LineWrapper_Dial_GetBusy_lineStatus_onHook()
        {
            var mock = GetMockedLineBase();
            mock.Setup(x => x.Dial("12223334444", 1000)).Returns(CallAnalysis.Busy);

            var test = new LineWrapper(new NullLoggerFactory(), 1, mock.Object);
            var result = test.Dial("12223334444", 1000);
            test.Status.Should().Be(LineStatusTypes.OnHook);
        }

        [Fact]
        public void LineWrapper_Dial_GetStopped_throws_DisposingException()
        {
            var mock = GetMockedLineBase();
            mock.Setup(x => x.Dial("12223334444", 1000)).Returns(CallAnalysis.Stopped);

            var test = new LineWrapper(new NullLoggerFactory(), 1, mock.Object);

            using (new AssertionScope())
            {
                Action act = () => test.Dial("12223334444", 1000);
                act.Should().Throw<DisposingException>();
                test.Status.Should().Be(LineStatusTypes.OffHook);
            }
        }
        #endregion

        #region CheckDispose
        [Fact]
        public void LineWrapper_CheckDispose_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.CheckDispose();
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_CheckDispose_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.CheckDispose();
            act.Should().Throw<DisposedException>();
        }
        #endregion

        #region WaitRings
        [Fact]
        public void LineWrapper_WaitRings_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.WaitRings(1);
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_WaitRings_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.WaitRings(1);
            act.Should().Throw<DisposedException>();
        }
        #endregion

        #region Hangup
        [Fact]
        public void LineWrapper_Hangup_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.Hangup();
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_Hangup_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.Hangup();
            act.Should().Throw<DisposedException>();
        }
        #endregion

        #region TakeOffHook
        [Fact]
        public void LineWrapper_TakeOffHook_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.TakeOffHook();
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_TakeOffHook_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.TakeOffHook();
            act.Should().Throw<DisposedException>();
        }
        #endregion

        #region PlayFile
        [Fact]
        public void LineWrapper_PlayFile_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.PlayFile("");
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_PlayFile_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.PlayFile("");
            act.Should().Throw<DisposedException>();
        }
        #endregion

        #region RecordToFile
        [Fact]
        public void LineWrapper_RecordToFile_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.RecordToFile("");
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_RecordToFile_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.RecordToFile("");
            act.Should().Throw<DisposedException>();
        }
        #endregion

        #region GetDigits
        [Fact]
        public void LineWrapper_GetDigits_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.GetDigits(1, "");
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_GetDigits_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.GetDigits(1, "");
            act.Should().Throw<DisposedException>();
        }
        #endregion

        #region FlushDigitBuffer
        [Fact]
        public void LineWrapper_FlushDigitBuffer_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.FlushDigitBuffer();
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_FlushDigitBuffer_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.FlushDigitBuffer();
            act.Should().Throw<DisposedException>();
        }
        #endregion

        #region Volume
        [Fact]
        public void LineWrapper_Volume_throws_DisposingException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Management.Dispose();

            Action act = () => test.Volume = 1;
            act.Should().Throw<DisposingException>();
        }

        [Fact]
        public void LineWrapper_Volume_throws_DisposedException()
        {
            var test = new LineWrapper(new NullLoggerFactory(), 1, GetMockedLineBase().Object);
            test.Dispose();

            Action act = () => test.Volume = 1;
            act.Should().Throw<DisposedException>();
        }
        #endregion
    }
}
