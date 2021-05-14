// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 

using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using ivrToolkit.Core;

namespace ivrToolkit.Plugin.Simulator
{
    public class Phone
    {
        private readonly StateObject _state;
        private readonly string _phoneNumber;
        private SimulatorLine _line;
        private readonly object _dialoutLock = new object();

        private static readonly Dictionary<string, Phone> Phones = new Dictionary<string, Phone>();

        public static Phone CreatePhone(string phoneNumber, StateObject state)
        {
            var p = new Phone(phoneNumber, state);
            Phones.Add(phoneNumber, p);
            return p;
        }
        public static void RemovePhone(string phoneNumber)
        {
            Phones.Remove(phoneNumber);
        }

        public static Phone GetPhone(string phoneNumber)
        {
            try
            {
                return Phones[phoneNumber];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
        private CallAnalysis _dialAnalysis = CallAnalysis.Stopped;
        public CallAnalysis Dial(SimulatorLine line)
        {
            if (_line != null)
            {
                return CallAnalysis.Busy;
            }

            lock (_dialoutLock)
            {
                Send("dialout");
                Monitor.Wait(_dialoutLock);
            }
            if (_dialAnalysis == CallAnalysis.Connected)
            {
                _line = line;
            }
            return _dialAnalysis;
        }

        private Phone(string phoneNumber, StateObject state)
        {
            _state = state;
            _phoneNumber = phoneNumber;

            state.WorkSocket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
        
        }
        private void ClearLine()
        {
            if (_line != null)
            {
                _line.UnsubscribeToHangup(HandleHangup);
                _line.SendHangup();
                _line = null;
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            var state = (StateObject)ar.AsyncState;
            var handler = state.WorkSocket;

            int bytesRead;
            try
            {
                // Read data from the client socket. 
                bytesRead = handler.EndReceive(ar);
            }
            catch (SocketException)
            {
                ClearLine();
                RemovePhone(_phoneNumber);
                return;
            }
            catch (ObjectDisposedException)
            {
                ClearLine();
                RemovePhone(_phoneNumber);
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
                    string command = content.Substring(0, index);
                    if (command == "disconnect")
                    {
                        ClearLine();
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Disconnect(false);
                        RemovePhone(_phoneNumber);
                        return;
                    }
                    if (command.StartsWith("dial,"))
                    {
                        var parts = command.Split(new[] { ',' });
                        var lineNumber = int.Parse(parts[1]);
                        // line should equal null the first dial
                        if (_line == null)
                        {
                            _line = (SimulatorLine)(new Simulator().GetLine(lineNumber));
                            _line.SubscribeToHangup(HandleHangup);
                        }
                        switch (_line.Status)
                        {
                            case LineStatusTypes.OnHook:
                                Send("idle");
                                _line.UnsubscribeToHangup(HandleHangup);
                                _line = null;
                                break;
                            case LineStatusTypes.Connected:
                                Send("busy");
                                _line.UnsubscribeToHangup(HandleHangup);
                                _line = null;
                                break;
                            case LineStatusTypes.AcceptingCalls:
                                var status = _line.SendRing();
                                switch (status)
                                {
                                    case LineStatusTypes.AcceptingCalls:
                                        Send("ring");
                                        break;
                                    case LineStatusTypes.Connected:
                                        Send("answered");
                                        break;
                                }
                                break;
                        }
                    }
                    else if (command == "hangup")
                    {
                        ClearLine();
                    }
                    else if (command.StartsWith("dialoutResponse,"))
                    {
                        var parts = command.Split(new[] { ',' });
                        var subCommand = parts[1];
                        switch (subCommand)
                        {
                            case "Busy":
                                _dialAnalysis = CallAnalysis.Busy;
                                break;
                            case "Ignore":
                                _dialAnalysis = CallAnalysis.NoAnswer;
                                break;
                            case "Answering Machine":
                                _dialAnalysis = CallAnalysis.AnsweringMachine;
                                break;
                            case "No Dial Tone":
                                _dialAnalysis = CallAnalysis.NoDialTone;
                                break;
                            case "Answer":
                                _dialAnalysis = CallAnalysis.Connected;
                                break;
                            default:
                                _dialAnalysis = CallAnalysis.Error;
                                break;
                        }
                        lock (_dialoutLock)
                        {
                            Monitor.Pulse(_dialoutLock);
                        }                    
                    }
                    else if (command.Length == 1)
                    {
                        _line.SendDigit(char.Parse(command));
                    }
                    else
                    {
                        // echo the command back
                        Send(command);
                    }
                }
                // read more or wait for the next command to come in
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
            }
        }

        private void HandleHangup(object sender, EventArgs args)
        {
            Send("hangup");
            ClearLine();
        }

        private void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            var byteData = Encoding.ASCII.GetBytes(data+"<EOF>");

            // Begin sending the data to the remote device.
            _state.WorkSocket.BeginSend(byteData, 0, byteData.Length, 0,
                SendCallback, _state.WorkSocket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                handler.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
