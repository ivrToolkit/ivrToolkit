using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Tests.LineWrapperTests;

public class FakeLine : IIvrBaseLine, IIvrLineManagement
{
    public List<string> PlayList = new();
    private readonly Queue<string> _digitsList = new();
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public IIvrLineManagement Management => this;
    public string LastTerminator { get; set; }
    public int LineNumber { get; set; }

    public void WaitRings(int rings)
    {
        throw new NotImplementedException();
    }

    public Task WaitRingsAsync(int rings, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void StartIncomingListener(Func<IIvrLine, CancellationToken, Task> callback, IIvrLine line, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void Hangup()
    {
        throw new NotImplementedException();
    }

    public void TakeOffHook()
    {
        throw new NotImplementedException();
    }

    public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
    {
        throw new NotImplementedException();
    }

    public Task<CallAnalysis> DialAsync(string phoneNumber, int answeringMachineLengthInMilliseconds, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void PlayFile(string filename)
    {
        PlayFileAsync(filename, CancellationToken.None).GetAwaiter().GetResult();
    }
    
    void IIvrBaseLine.PlayWavStream(WavStream wavStream)
    {
        throw new NotImplementedException();
    }

    Task IIvrBaseLine.PlayWavStreamAsync(WavStream wavStream, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public async Task PlayFileAsync(string filename, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        PlayList.Add(filename);
    }

    public void RecordToFile(string filename)
    {
        throw new NotImplementedException();
    }

    public Task RecordToFileAsync(string filename, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public void RecordToFile(string filename, int timeoutMilliseconds)
    {
        throw new NotImplementedException();
    }

    public Task RecordToFileAsync(string filename, int timeoutMilliseconds, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public string GetDigits(int numberOfDigits, string terminators, int timeoutMilliseconds = 0)
    {
        return ""; // todo read from _pushDigits
    }

    public async Task<string> GetDigitsAsync(int numberOfDigits, string terminators, CancellationToken cancellationToken,
        int timeoutMilliseconds = 0)
    {
        await Task.CompletedTask; 
        var result = _digitsList.Dequeue();
        return result;
    }

    public string FlushDigitBuffer()
    {
        // todo need to look into this
        return "";
    }

    public int Volume { get; set; }
    public void Reset()
    {
        throw new NotImplementedException();
    }

    public void TriggerDispose()
    {
    }

    public void PushDigits(string digits)
    {
        _digitsList.Enqueue(digits);
    }
}