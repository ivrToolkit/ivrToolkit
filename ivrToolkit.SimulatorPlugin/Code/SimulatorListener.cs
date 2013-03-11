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
using System.Net;
using System.Net.Sockets;

namespace ivrToolkit.SimulatorPlugin
{
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class SimulatorListener
    {
        private SimulatorListener() {}

        private static SimulatorListener _singleton = new SimulatorListener();
        public static SimulatorListener singleton
        {
            get
            {
                return _singleton;
            }
        }

        private bool started;
        private Thread listenerThread;
        private object lockObject = new object();

        //Thread signal.
        private ManualResetEvent allDone = new ManualResetEvent(false);

        private Socket listenerSocket;
        private List<Socket> connectedSockets = new List<Socket>();

        public void start()
        {
            lock (lockObject)
            {
                // start up the thread if it is not already started
                if (!started)
                {
                    // Create the thread object, passing in the SimulatorListener.run method
                    // via a ThreadStart delegate. This does not start the thread.
                    listenerThread = new Thread(new ThreadStart(run));

                    // Start the thread
                    listenerThread.Start();
                }
            }
        }

        public void stop()
        {
            if (started)
            {
                // close the sockets will end the threads
                listenerSocket.Close();

                foreach (Socket s in connectedSockets)
                {
                    s.Close();
                }
                connectedSockets.Clear();
            }
        }

        public void run()
        {
            started = true;
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];


            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 6050);

            // Create a TCP/IP socket.
            listenerSocket = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listenerSocket.Bind(localEndPoint);
                listenerSocket.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    listenerSocket.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listenerSocket);

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
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
            started = false;
        }


        public void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                // Signal the main thread to continue.
                allDone.Set();

                // Get the socket that handles the client request.
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);
                connectedSockets.Add(handler);

                // Create the state object.
                StateObject state = new StateObject();
                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
            }
            catch (ObjectDisposedException)
            {
                // socket was closed
            }
        }
        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = 0;
            try
            {
                bytesRead = handler.EndReceive(ar);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode == SocketError.ConnectionReset)
                {
                    handler.Close();
                    connectedSockets.Remove(handler);
                    return;
                }
                handler.Close();
                connectedSockets.Remove(handler);
                throw e;
            }
            catch (ObjectDisposedException)
            {
                // the socket has been closed
                return;
            }

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                int index = content.IndexOf("<EOF>");
                if (index != -1)
                {
                    state.sb = new StringBuilder();
                    string command = content.Substring(0, index);
                    if (command.StartsWith("connect,"))
                    {
                        string[] parts = command.Split(new char[]{','});
                        Phone.createPhone(parts[1], state);
                        return;
                    }
                    else
                    {
                        // echo the command back
                        Send(handler, content);
                    }
                }
                try
                {
                    // read more or wait for the next command to come in
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReadCallback), state);
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
                byte[] byteData = Encoding.ASCII.GetBytes(data);

                // Begin sending the data to the remote device.
                handler.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), handler);
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
                int bytesSent = handler.EndSend(ar);
            }
            catch (ObjectDisposedException)
            {
                // socket was closed
            }
            catch (Exception e)
            {
                connectedSockets.Remove(handler);
                throw e;
            }
        }
    } // class
}
