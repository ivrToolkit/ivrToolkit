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
using SharpDX.MediaFoundation;
using SharpDX.XAudio2;
using SharpDX.IO;
using AudioPlayerApp;

namespace ivrToolkit.SimulatorPlugin
{
    public class WavPlayer : IDisposable
    {
        private XAudio2 xaudio2;
        private MasteringVoice masteringVoice;
        private Stream fileStream;
        private AudioPlayer audioPlayer;

        private string fileName;

        private event EventHandler onFinished;

        public WavPlayer()
        {
            InitializeXAudio2();
        }

        private void InitializeXAudio2()
        {
            // This is mandatory when using any of SharpDX.MediaFoundation classes
            MediaManager.Startup();

            // Starts The XAudio2 engine
            xaudio2 = new XAudio2();
            xaudio2.StartEngine();
            masteringVoice = new MasteringVoice(xaudio2);
        }

        public void play(string fileName)
        {
            this.fileName = fileName;
            Thread thread = new Thread(new ThreadStart(run));
            thread.Start();
        }

        public void run()
        {
            fileStream = new NativeFileStream(fileName, NativeFileMode.Open, NativeFileAccess.Read);

            audioPlayer = new AudioPlayer(xaudio2, fileStream);

            // Auto-play
            audioPlayer.Play();

            while (audioPlayer.State == AudioPlayerState.Playing /* && audioPlayer.Position < audioPlayer.Duration */)
            {
                Thread.Sleep(100);
            }

            audioPlayer.Close();
            audioPlayer = null;

            if (fileStream != null)
            {
                fileStream.Close();
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
            audioPlayer.Stop();
        }
    }

}
