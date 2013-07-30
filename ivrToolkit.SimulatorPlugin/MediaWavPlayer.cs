// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
using System;
using System.Threading;
using System.Media;

namespace ivrToolkit.SimulatorPlugin
{
    public class MediaWavPlayer : IDisposable
    {
        private string _fileName;
        readonly SoundPlayer _player = new SoundPlayer();

        private event EventHandler OnFinished;

        public void Play(string fileName)
        {
            _fileName = fileName;
            var thread = new Thread(Run);
            thread.Start();
        }

        public void Run()
        {
            _player.SoundLocation = _fileName;
            _player.Play();

            try
            {
                _player.SoundLocation = _fileName;
                _player.Play();
            }
// ReSharper disable EmptyGeneralCatchClause
            catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
            {
            }

            if (OnFinished != null)
            {
                OnFinished(this, null);
            }
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
            _player.Stop();
        }
    }

}
