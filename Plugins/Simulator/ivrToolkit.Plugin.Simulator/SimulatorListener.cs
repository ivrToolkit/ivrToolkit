// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ivrToolkit.Plugin.Simulator
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket WorkSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder Sb = new StringBuilder();
    }

    public class SimulatorListener
    {
        private SimulatorListener() {}

// ReSharper disable InconsistentNaming
        private static readonly SimulatorListener _singleton = new SimulatorListener();
// ReSharper restore InconsistentNaming
        public static SimulatorListener Singleton
        {
            get
            {
                return _singleton;
            }
        }

        private bool _started;
        private Thread _listenerThread;
        private readonly object _lockObject = new object();

        //Thread signal.
        private readonly ManualResetEvent _allDone = new ManualResetEvent(false);

        private Socket _listenerSocket;
        private readonly List<Socket> _connectedSockets = new List<Socket>();

        public void Start()
        {
            lock (_lockObject)
            {
                // start up the thread if it is not already started
                if (!_started)
                {
                    // Create the thread object, passing in the SimulatorListener.run method
                    // via a ThreadStart delegate. This does not start the thread.
                    _listenerThread = new Thread(Run);

                    // Start the thread
                    _listenerThread.Start();
                }
            }
        }

        public void Stop()
        {
            if (_started)
            {
                // close the sockets will end the threads
                _listenerSocket.Close();

                foreach (var s in _connectedSockets)
                {
                    s.Close();
                }
                _connectedSockets.Clear();
            }
        }

        public void Run()
        {
            _started = true;
            // Data buffer for incoming data.


            var ipAddress = IPAddress.Parse("127.0.0.1");
            var localEndPoint = new IPEndPoint(ipAddress, 6050);

            // Create a TCP/IP socket.
            _listenerSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                _listenerSocket.Bind(localEndPoint);
                _listenerSocket.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    _allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    _listenerSocket.BeginAccept(
                        AcceptCallback,
                        _listenerSocket);

                    // Wait until a connection is made before continuing.
                    _allDone.WaitOne();
                }

            }
            catch (ObjectDisposedException)
            {
                // socket was closed
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            _started = false;
        }


        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.
                _allDone.Set();

                // Get the socket that handles the client request.
                var listener = (Socket)ar.AsyncState;
                var handler = listener.EndAccept(ar);
                _connectedSockets.Add(handler);

                // Create the state object.
                var state = new StateObject {WorkSocket = handler};
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    ReadCallback, state);
            }
            catch (ObjectDisposedException)
            {
                // socket was closed
            }
        }
        public void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject)ar.AsyncState;
            var handler = state.WorkSocket;

            // Read data from the client socket. 
            int bytesRead;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    handler.Close();
                    _connectedSockets.Remove(handler);
                    return;
                }
                handler.Close();
                _connectedSockets.Remove(handler);
                throw;
            }
            catch (ObjectDisposedException)
            {
                // the socket has been closed
                return;
            }

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.Sb.Append(Encoding.ASCII.GetString(
                    state.Buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                var content = state.Sb.ToString();
                var index = content.IndexOf("<EOF>", StringComparison.Ordinal);
                if (index != -1)
                {
                    state.Sb = new StringBuilder();
                    var command = content.Substring(0, index);
                    if (command.StartsWith("connect,"))
                    {
                        var parts = command.Split(new[]{','});
                        Phone.CreatePhone(parts[1], state);
                        return;
                    }
                    // echo the command back
                    Send(handler, content);
                }
                try
                {
                    // read more or wait for the next command to come in
                    handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    ReadCallback, state);
                }
                catch (ObjectDisposedException)
                {
                    // socket was closed
                }
            } // if
        }

        private void Send(Socket handler, String data)
        {
            try
            {
                // Convert the string data to byte data using ASCII encoding.
                var byteData = Encoding.ASCII.GetBytes(data);

                // Begin sending the data to the remote device.
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    SendCallback, handler);
            }
            catch (ObjectDisposedException)
            {
                // socket was closed
            }
        }

        private void SendCallback(IAsyncResult ar)
        {
            Socket handler = null;
            try
            {
                // Retrieve the socket from the state object.
                handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                handler.EndSend(ar);
            }
            catch (ObjectDisposedException)
            {
                // socket was closed
            }
            catch (Exception)
            {
                _connectedSockets.Remove(handler);
                throw;
            }
        }
    } // class
}
