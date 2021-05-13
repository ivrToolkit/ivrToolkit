// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using System.Threading;
using NAudio.Wave;

namespace ivrToolkit.SimulatorPlugin
{
    public class WavPlayer : IDisposable
    {

        private string _fileName;
        private static bool _stopped;

        private event EventHandler OnFinished;

        private WaveOut _woCall;

        public void Play(string fileName)
        {
            _fileName = fileName;
            var thread = new Thread(Run);
            thread.Start();
        }

        public void Run()
        {
            // pcm 8000 hz, 64kb/s 1 channel
            var wfOKI = new WaveFormat(8000, 8, 1);

            WaveStream wsRaw = new WaveFileReader(_fileName);

            using (wsRaw = WaveFormatConversionStream.CreatePcmStream(wsRaw))
            using (WaveStream wsOKI = new RawSourceWaveStream(wsRaw, wfOKI))
            using (_woCall = new WaveOut())
            {
                _woCall.PlaybackStopped += woCall_PlaybackStopped;
                _woCall.Init(wsOKI);

                _stopped = false;
                _woCall.Play();

                while (!_stopped)
                {
                    Thread.Sleep(100);
                }

                if (OnFinished != null)
                {
                    OnFinished(this, null);
                }
            } // using
        }

        void woCall_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            _stopped = true;
        }
        public void Dispose()
        {
            OnFinished = null;
        }

        public void SubscribeFinished(EventHandler handler)
        {
            OnFinished += handler;
        }

        public void UnsubscribeFinished(EventHandler handler)
        {
            OnFinished -= handler;
        }
        public void Stop()
        {
            _woCall.Stop();
        }
    }

}
