﻿// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Enums;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// This interface exposes the main methods used to control the selected plugin.
/// It is meant to be used by the LineWrapper class only.
/// </summary>
public interface IIvrBaseLine : IDisposable
{
    /// <summary>
    /// Functionality for managing the line from another thread.
    /// </summary>
    IIvrLineManagement Management { get; }

    /// <summary>
    /// The last terminator key that was pressed
    /// </summary>
    string LastTerminator { get; set; }

    /// <summary>
    /// The attached line number.
    /// </summary>
    int LineNumber
    {
        get;
    }

    /// <summary>
    /// The number of rings to wait before answering
    /// </summary>
    /// <param name="rings">The number of rings to wait before answering</param>
    void WaitRings(int rings);
    Task WaitRingsAsync(int rings, CancellationToken cancellationToken);

    protected internal void StartIncomingListener(Func<IIvrLine, CancellationToken, Task> callback, IIvrLine line, CancellationToken cancellationToken);
    
    /// <summary>
    /// Forces a hangup on the line.
    /// </summary>
    void Hangup();

    /// <summary>
    /// Pick up the line.
    /// </summary>
    void TakeOffHook();

    /// <summary>
    /// Dials a phone number using call progress analysis.
    /// </summary>
    /// <param name="number">The phone number to call</param>
    /// <param name="answeringMachineLengthInMilliseconds">A greeting longer than this time indicates a possible answering machine.</param>
    /// <returns>The Call analysis enumeration</returns>
    CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds);

    Task<CallAnalysis> DialAsync(string phoneNumber, int answeringMachineLengthInMilliseconds, CancellationToken cancellationToken);

    /// <summary>
    /// Plays a wav file which must be in the format of 8000hz 1 channel signed 16 bit PCM.
    /// </summary>
    /// <param name="filename">The wav file to play</param>
    void PlayFile(string filename);
    
    /// <summary>
    /// Plays a wav stream.
    /// </summary>
    /// <param name="audioStream">including the wav header</param>
    protected internal void PlayWavStream(MemoryStream audioStream);
    
    /// <summary>
    /// Plays a wav stream.
    /// </summary>
    /// <param name="audioStream">including the wav header</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected internal Task PlayWavStreamAsync(MemoryStream audioStream, CancellationToken cancellationToken);

    Task PlayFileAsync(string filename, CancellationToken cancellationToken);

    /// <summary>
    /// Records a wav file to the disk in the format of 8000hz 1 channel signed 16 bit PCM. Has a default timeout of 5 minutes.
    /// </summary>
    /// <param name="filename">The file name to record to</param>
    void RecordToFile(string filename);
    
    Task RecordToFileAsync(string filename, CancellationToken cancellationToken);

    /// <summary>
    /// Records a wav file to the disk in the format of 8000hz 1 channel signed 16 bit PCM.
    /// </summary>
    /// <param name="filename">The file name to record to</param>
    /// <param name="timeoutMilliseconds">Maximum time to record in milliseconds</param>
    void RecordToFile(string filename, int timeoutMilliseconds);
    Task RecordToFileAsync(string filename, int timeoutMilliseconds, CancellationToken cancellationToken);

    /// <summary>
    /// Keep prompting for digits until number of digits is pressed or a terminator digit is pressed.
    /// </summary>
    /// <param name="numberOfDigits">Maximum number of digits allowed in the buffer.</param>
    /// <param name="terminators">The terminator keys</param>
    /// <param name="timeoutMilliseconds">Override the default timeout if not zero.</param>
    /// <returns>Returns the digits pressed not including the terminator if there was one</returns>
    string GetDigits(int numberOfDigits, string terminators, int timeoutMilliseconds = 0);
    Task<string> GetDigitsAsync(int numberOfDigits, string terminators, CancellationToken cancellationToken, int timeoutMilliseconds = 0);

    /// <summary>
    /// Returns every character including the terminator
    /// </summary>
    /// <returns>All the digits in the buffer including terminators</returns>
    string FlushDigitBuffer();

    /// <summary>
    /// Gets or sets the Volume. Value can be in the range of -10 to 10. Zero being the regular volume.
    /// </summary>
    int Volume { get; set; }

    /// <summary>
    /// Disposes of the line and then recreates it.
    /// </summary>
    void Reset();

}