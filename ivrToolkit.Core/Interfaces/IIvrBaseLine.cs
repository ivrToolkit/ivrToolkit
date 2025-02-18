// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// This interface provides methods to control the selected IVR plugin.
/// It is primarily used by the <see cref="LineWrapper"/> class.
/// </summary>
public interface IIvrBaseLine : IDisposable
{
    /// <summary>
    /// Provides functionality for managing the line from another thread.
    /// </summary>
    IIvrLineManagement Management { get; }

    /// <summary>
    /// Gets or sets the last terminator key that was pressed.
    /// </summary>
    string LastTerminator { get; set; }

    /// <summary>
    /// Gets the attached line number.
    /// </summary>
    int LineNumber { get; }

    /// <summary>
    /// Waits for a specified number of rings before answering.
    /// </summary>
    /// <param name="rings">The number of rings to wait.</param>
    void WaitRings(int rings);

    /// <summary>
    /// Asynchronously waits for a specified number of rings before answering.
    /// </summary>
    /// <param name="rings">The number of rings to wait.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task WaitRingsAsync(int rings, CancellationToken cancellationToken);

    /// <summary>
    /// Starts an incoming call listener with a callback function.
    /// </summary>
    /// <param name="callback">The function to execute when a call is received.</param>
    /// <param name="line">The line instance.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    protected internal void StartIncomingListener(Func<IIvrLine, CancellationToken, Task> callback, IIvrLine line, CancellationToken cancellationToken);

    /// <summary>
    /// Forces a hangup on the current line.
    /// </summary>
    void Hangup();

    /// <summary>
    /// Takes the line off hook.
    /// </summary>
    void TakeOffHook();

    /// <summary>
    /// Dials a phone number using call progress analysis.
    /// </summary>
    /// <param name="phoneNumber">The phone number to call.</param>
    /// <param name="answeringMachineLengthInMilliseconds">The threshold for determining an answering machine.</param>
    /// <returns>A <see cref="CallAnalysis"/> object containing call results.</returns>
    CallAnalysis Dial(string phoneNumber, int answeringMachineLengthInMilliseconds);

    /// <summary>
    /// Asynchronously dials a phone number using call progress analysis.
    /// </summary>
    /// <param name="phoneNumber">The phone number to call.</param>
    /// <param name="answeringMachineLengthInMilliseconds">The threshold for determining an answering machine.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation, returning a <see cref="CallAnalysis"/> object.</returns>
    Task<CallAnalysis> DialAsync(string phoneNumber, int answeringMachineLengthInMilliseconds, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a WAV file (8000Hz, mono, signed 16-bit PCM).
    /// </summary>
    /// <param name="filename">The path to the WAV file.</param>
    void PlayFile(string filename);

    /// <summary>
    /// Asynchronously plays a WAV file (8000Hz, mono, signed 16-bit PCM).
    /// </summary>
    /// <param name="filename">The path to the WAV file.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayFileAsync(string filename, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a WAV audio stream.
    /// </summary>
    /// <param name="audioStream">The audio stream, including the WAV header.</param>
    protected internal void PlayWavStream(WavStream audioStream);

    /// <summary>
    /// Asynchronously plays a WAV audio stream.
    /// </summary>
    /// <param name="audioStream">The audio stream, including the WAV header.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task PlayWavStreamAsync(WavStream audioStream, CancellationToken cancellationToken);

    /// <summary>
    /// Records a WAV file (8000Hz, mono, signed 16-bit PCM) to disk.
    /// </summary>
    /// <param name="filename">The output file path.</param>
    void RecordToFile(string filename);

    /// <summary>
    /// Asynchronously records a WAV file (8000Hz, mono, signed 16-bit PCM) to disk.
    /// </summary>
    /// <param name="filename">The output file path.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task RecordToFileAsync(string filename, CancellationToken cancellationToken);

    /// <summary>
    /// Records a WAV file with a specified timeout (8000Hz, mono, signed 16-bit PCM) to disk.
    /// </summary>
    /// <param name="filename">The output file path.</param>
    /// <param name="timeoutMilliseconds">The maximum recording duration in milliseconds.</param>
    void RecordToFile(string filename, int timeoutMilliseconds);

    /// <summary>
    /// Asynchronously records a WAV file with a specified timeout (8000Hz, mono, signed 16-bit PCM) to disk.
    /// </summary>
    /// <param name="filename">The output file path.</param>
    /// <param name="timeoutMilliseconds">The maximum recording duration in milliseconds.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    Task RecordToFileAsync(string filename, int timeoutMilliseconds, CancellationToken cancellationToken);

    /// <summary>
    /// Collects digits from the user until the specified number is reached or a terminator is pressed.
    /// </summary>
    /// <param name="numberOfDigits">The maximum number of digits to capture.</param>
    /// <param name="terminators">The valid terminator keys.</param>
    /// <param name="interDigitTimeoutMilliseconds">The timeout duration between keypresses in milliseconds (0 to use default).</param>
    /// <returns>The collected digits, excluding the terminator if used.</returns>
    string GetDigits(int numberOfDigits, string terminators, int interDigitTimeoutMilliseconds = 0);

    /// <summary>
    /// Asynchronously collects digits from the user until the specified number is reached or a terminator is pressed.
    /// </summary>
    /// <param name="numberOfDigits">The maximum number of digits to capture.</param>
    /// <param name="terminators">The valid terminator keys.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <param name="interDigitTimeoutMilliseconds">The timeout duration between keypresses in milliseconds (0 to use default).</param>
    /// <returns>The collected digits, excluding the terminator if used.</returns>
    Task<string> GetDigitsAsync(int numberOfDigits, string terminators, CancellationToken cancellationToken, int interDigitTimeoutMilliseconds = 0);

    /// <summary>
    /// Clears the digit buffer and returns all previously collected digits, including terminators.
    /// </summary>
    /// <returns>All digits in the buffer.</returns>
    string FlushDigitBuffer();

    /// <summary>
    /// Gets or sets the volume level. Valid range: -10 to 10 (0 is default volume).
    /// </summary>
    int Volume { get; set; }

    /// <summary>
    /// Disposes of the line and re-initializes it.
    /// </summary>
    void Reset();
}