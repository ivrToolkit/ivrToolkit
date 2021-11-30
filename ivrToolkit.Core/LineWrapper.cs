using System;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Exceptions;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using Microsoft.Extensions.Logging;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

namespace ivrToolkit.Core
{
    /// <summary>
    /// This wrapper handles common functionality in all plugin lines so that the implementation doesn't have to handle it.
    /// </summary>
    public class LineWrapper : IIvrLine, IIvrLineManagement
    {

        private readonly int _lineNumber;
        private readonly IIvrBaseLine _lineImplementation;

        private readonly ILogger<LineWrapper> _logger;


        private LineStatusTypes _status = LineStatusTypes.OnHook;

        private bool _disposeTriggerActivated;
        private int _volume;
        private bool _disposed;


        public LineWrapper(ILoggerFactory loggerFactory, int lineNumber, IIvrBaseLine lineImplementation)
        {
            _lineNumber = lineNumber;
            _lineImplementation = lineImplementation;
            _logger = loggerFactory.CreateLogger<LineWrapper>();
            _logger.LogDebug("ctr(ILoggerFactory, VoiceProperties, {0})", lineNumber);

        }

        public IIvrLineManagement Management => this;

        public LineStatusTypes Status => _status;

        public string LastTerminator { get; set; }

        public int LineNumber => _lineNumber;

        public void CheckDispose()
        {
            _logger.LogTrace("CheckDispose()");
            CheckDisposed();
            CheckDisposing();
        }

        public void WaitRings(int rings)
        {
            _logger.LogDebug("WaitRings({0})", rings);
            CheckDispose();

            _status = LineStatusTypes.AcceptingCalls;

            _lineImplementation.WaitRings(rings);

            _status = LineStatusTypes.Connected;
            CheckDisposing();
        }

        public void Hangup()
        {
            _logger.LogDebug("Hangup()");
            _status = LineStatusTypes.OnHook;
            CheckDispose();

            _lineImplementation.Hangup();
        }

        public void TakeOffHook()
        {
            _logger.LogDebug("TakeOffHook()");
            _status = LineStatusTypes.OffHook;
            CheckDispose();

            _lineImplementation.TakeOffHook();
        }


        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            answeringMachineLengthInMilliseconds.ThrowIfLessThanOrEqualTo(999, nameof(answeringMachineLengthInMilliseconds));
            number.ThrowIfNullOrWhiteSpace(nameof(number));

            _logger.LogDebug("Dial({0}, {1})", number, answeringMachineLengthInMilliseconds);
            CheckDispose();

            TakeOffHook();
            _logger.LogDebug("Line is now off hook");

            _logger.LogDebug("about to dial: {0}", number);
            var result = _lineImplementation.Dial(number, answeringMachineLengthInMilliseconds);

            _logger.LogDebug("CallAnalysis is: {0}", result.ToString());

            if (result == CallAnalysis.Stopped) ThrowDisposingException();

            if (result == CallAnalysis.AnsweringMachine || result == CallAnalysis.Connected)
            {
                _status = LineStatusTypes.Connected;
            }
            else
            {
                Hangup();
            }

            return result;
        }

        #region ILineManagement region

        void IIvrLineManagement.TriggerDispose()
        {
            _logger.LogDebug("ILineManagement.TriggerDispose() for line: {0}", _lineNumber);
            if (_disposed)
            {
                _logger.LogDebug("Line {0} has already been disposed", _lineNumber);
                return;
            }

            _lineImplementation.Management.TriggerDispose();
            _disposeTriggerActivated = true;
        }

        #endregion

        public void Dispose()
        {
            if (_disposed)
            {
                _logger.LogDebug("Dispose() - Line is already disposed");
                return;
            }
            _logger.LogDebug("Dispose() - Disposing of the line");

            try
            {
                _status = LineStatusTypes.OnHook;
                _lineImplementation.Hangup();
                _lineImplementation.Dispose();
            }
            finally
            {
                _disposed = true;
                _disposeTriggerActivated = false;
            }
        }

        public void PlayFile(string filename)
        {
            _logger.LogDebug("PlayFile({0})", filename);
            CheckDispose();
            try
            {
                _lineImplementation.PlayFile(filename);
            }
            catch (DisposingException)
            {
                ThrowDisposingException();
            }
            catch (HangupException)
            {
                _status = LineStatusTypes.OnHook;
                throw;
            }
        }

        public void RecordToFile(string filename)
        {
            RecordToFile(filename, 60000 * 5); // default timeout of 5 minutes
        }

        public void RecordToFile(string filename, int timeoutMilliseconds)
        {
            _logger.LogDebug("RecordToFile({0}, {1})", filename, timeoutMilliseconds);

            CheckDispose();

            _lineImplementation.RecordToFile(filename, timeoutMilliseconds);
        }


        public string GetDigits(int numberOfDigits, string terminators)
        {
            _logger.LogDebug("GetDigits({0}, {1})", numberOfDigits, terminators);
            CheckDispose();
            try
            {
                var answer = _lineImplementation.GetDigits(numberOfDigits, terminators);
                return StripOffTerminator(answer, terminators);
            }
            catch (DisposingException)
            {
                ThrowDisposingException();
            }
            catch (HangupException)
            {
                _status = LineStatusTypes.OnHook;
                throw;
            }

            return null; // will never get here
        }

        public string FlushDigitBuffer()
        {
            _logger.LogDebug("FlushDigitBuffer()");
            CheckDispose();

            var all = "";
            try
            {
                _lineImplementation.FlushDigitBuffer();
            }
            catch (GetDigitsTimeoutException)
            {
            }

            return all;
        }

        public int Volume
        {
            get
            {
                CheckDispose();
                return _volume;
            }
            set
            {
                if (value < -10 || value > 10)
                {
                    throw new VoiceException("size must be between -10 to 10");
                }

                CheckDispose();

                _lineImplementation.Volume = value;
                _volume = value;
            }
        }

        private string StripOffTerminator(string answer, string terminators)
        {
            _logger.LogDebug("StripOffTerminator({0}, {1})", answer, terminators);

            LastTerminator = "";
            if (answer.Length >= 1)
            {
                var lastDigit = answer.Substring(answer.Length - 1, 1);
                if (terminators != null & terminators != "")
                {
                    if (terminators.IndexOf(lastDigit, StringComparison.Ordinal) != -1)
                    {
                        LastTerminator = lastDigit;
                        answer = answer.Substring(0, answer.Length - 1);
                    }
                }
            }

            return answer;
        }

        private void CheckDisposed()
        {
            if (_disposed) ThrowDisposedException();
        }

        private void CheckDisposing()
        {
            if (_disposeTriggerActivated) ThrowDisposingException();
        }

        private void ThrowDisposingException()
        {
            _logger.LogDebug("ThrowDisposingException()");
            _disposeTriggerActivated = false;
            throw new DisposingException();
        }

        private void ThrowDisposedException()
        {
            _logger.LogDebug("ThrowDisposedException()");
            throw new DisposedException($"Line {_lineNumber} has already been disposed");
        }

    }
}