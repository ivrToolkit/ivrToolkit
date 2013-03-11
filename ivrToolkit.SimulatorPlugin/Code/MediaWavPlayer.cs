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
    public class MediaWavPlayer : IDisposable
    {
        private string fileName;
        private Microsoft.DirectX.AudioVideoPlayback.Audio soundFile;
        SoundPlayer player = new SoundPlayer();

        private event EventHandler onFinished;

        public MediaWavPlayer()
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
            player.SoundLocation = fileName;
            player.Play();

            try
            {
                player.SoundLocation = fileName;
                player.Play();
            }
            catch (Exception e)
            {
            }

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
            player.Stop();
        }
    }

}
