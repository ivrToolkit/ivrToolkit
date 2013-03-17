/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Media;
using System.IO;
using NAudio.Wave;

namespace ivrToolkit.SimulatorPlugin
{
    public class WavPlayer : IDisposable
    {

        private string fileName;
        private static bool stopped;

        private event EventHandler onFinished;

        private WaveOut woCall;

        public WavPlayer()
        {
        }

        public void play(string fileName)
        {
            this.fileName = fileName;
            Thread thread = new Thread(new ThreadStart(run));
            thread.Start();
        }

        public void run()
        {
            // pcm 8000 hz, 64kb/s 1 channel
            WaveFormat wfOKI = new WaveFormat(8000, 8, 1);

            WaveStream wsRaw = new WaveFileReader(fileName);

            using (wsRaw = WaveFormatConversionStream.CreatePcmStream(wsRaw))
            using (WaveStream wsOKI = new RawSourceWaveStream(wsRaw, wfOKI))
            using (woCall = new WaveOut())
            {
                woCall.PlaybackStopped += woCall_PlaybackStopped;
                woCall.Init(wsOKI);

                stopped = false;
                woCall.Play();

                while (!stopped)
                {
                    System.Threading.Thread.Sleep(100);
                }

                if (onFinished != null)
                {
                    onFinished(this, null);
                }
            } // using
        }

        void woCall_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            stopped = true;
        }
        public void Dispose()
        {
            onFinished = null;
        }

        public void subscribeFinished(EventHandler handler)
        {
            onFinished += handler;
        }

        public void unsubscribeFinished(EventHandler handler)
        {
            onFinished -= handler;
        }
        public void stop()
        {
            woCall.Stop();
        }
    }

}
