using System;
using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Interfaces;

namespace ivrToolkit.Core.Tests
{
    public class MockLine : ILineBase
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ILineManagement Management { get; }
        public string LastTerminator { get; }
        public int LineNumber { get; }

        public void WaitRings(int rings)
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

        public void PlayFile(string filename)
        {
            throw new NotImplementedException();
        }

        public void RecordToFile(string filename)
        {
            throw new NotImplementedException();
        }

        public void RecordToFile(string filename, int timeoutMillisconds)
        {
            throw new NotImplementedException();
        }

        public string GetDigits(int numberOfDigits, string terminators)
        {
            throw new NotImplementedException();
        }

        public string FlushDigitBuffer()
        {
            throw new NotImplementedException();
        }

        public int Volume { get; set; }
    }
}
