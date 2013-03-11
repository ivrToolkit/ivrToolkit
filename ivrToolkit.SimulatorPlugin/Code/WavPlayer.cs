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
using Microsoft.DirectX.AudioVideoPlayback;
using System.IO;

namespace ivrToolkit.SimulatorPlugin
{
    public class WavPlayer : IDisposable
    {
        private string fileName;
        private Microsoft.DirectX.AudioVideoPlayback.Audio soundFile;

        private event EventHandler onFinished;

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
            soundFile = new Microsoft.DirectX.AudioVideoPlayback.Audio(fileName, false);
            soundFile.Play();

            while (soundFile.State == StateFlags.Running && soundFile.CurrentPosition < soundFile.Duration)
            {
                Thread.Sleep(100);
            }
            soundFile.Dispose();
            soundFile = null;
            if (onFinished != null)
            {
                onFinished(this, null);
            }
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
            soundFile.Stop();
        }
    }

}
