using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Interfaces;

namespace ivrToolkit.Core.Tests.LineWrapperPlay;

public class FakeLine : IIvrBaseLine
{
    public List<string> PlayList = new();
    
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public IIvrLineManagement Management { get; }
    public string LastTerminator { get; set; }
    public int LineNumber { get; }
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
        throw new NotImplementedException();
    }

    public Task<string> GetDigitsAsync(int numberOfDigits, string terminators, CancellationToken cancellationToken,
        int timeoutMilliseconds = 0)
    {
        throw new NotImplementedException();
    }

    public string FlushDigitBuffer()
    {
        throw new NotImplementedException();
    }

    public int Volume { get; set; }
    public void Reset()
    {
        throw new NotImplementedException();
    }
}