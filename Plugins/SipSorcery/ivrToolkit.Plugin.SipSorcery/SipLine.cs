using ivrToolkit.Core.Enums;
using ivrToolkit.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ivrToolkit.Plugin.SipSorcery
{
    internal class SipLine : IIvrBaseLine, IIvrLineManagement
    {
        public IIvrLineManagement Management => throw new NotImplementedException();

        public string LastTerminator => throw new NotImplementedException();

        public int LineNumber => throw new NotImplementedException();

        public int Volume { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public CallAnalysis Dial(string number, int answeringMachineLengthInMilliseconds)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string FlushDigitBuffer()
        {
            throw new NotImplementedException();
        }

        public string GetDigits(int numberOfDigits, string terminators)
        {
            throw new NotImplementedException();
        }

        public void Hangup()
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

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void TakeOffHook()
        {
            throw new NotImplementedException();
        }

        public void TriggerDispose()
        {
            throw new NotImplementedException();
        }

        public void WaitRings(int rings)
        {
            throw new NotImplementedException();
        }
    }
}
