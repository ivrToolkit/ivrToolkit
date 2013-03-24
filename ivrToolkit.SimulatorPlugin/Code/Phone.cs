/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

using System.Threading;
using ivrToolkit.Core;


namespace ivrToolkit.SimulatorPlugin
{
    public class Phone
    {
        private StateObject state;
        private string phoneNumber;
        private SimulatorLine line = null;
        private object dialoutLock = new object();

        private static Dictionary<string, Phone> phones = new Dictionary<string, Phone>();

        public static Phone createPhone(string phoneNumber, StateObject state)
        {
            Phone p = new Phone(phoneNumber, state);
            phones.Add(phoneNumber, p);
            return p;
        }
        public static void removePhone(string phoneNumber)
        {
            phones.Remove(phoneNumber);
        }

        public static Phone getPhone(string phoneNumber)
        {
            try
            {
                return phones[phoneNumber];
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }
        private CallAnalysis dialAnalysis = CallAnalysis.stopped;
        public CallAnalysis dial(SimulatorLine line)
        {
            if (this.line != null)
            {
                return CallAnalysis.busy;
            }

            lock (dialoutLock)
            {
                Send("dialout");
                Monitor.Wait(dialoutLock);
            }
            if (dialAnalysis == CallAnalysis.connected)
            {
                this.line = line;
            }
            return dialAnalysis;
        }

        private Phone(string phoneNumber, StateObject state)
        {
            this.state = state;
            this.phoneNumber = phoneNumber;

            state.workSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        
        }
        private void clearLine()
        {
            if (line != null)
            {
                line.unsubscribeToHangup(new HangupDelegate(handleHangup));
                line.sendHangup();
                line = null;
            }
        }

        private void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = 0;
            try
            {
                // Read data from the client socket. 
                bytesRead = handler.EndReceive(ar);
            }
            catch (SocketException)
            {
                clearLine();
                Phone.removePhone(this.phoneNumber);
                return;
            }
            catch (ObjectDisposedException)
            {
                clearLine();
                Phone.removePhone(this.phoneNumber);
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
                    if (command == "disconnect")
                    {
                        clearLine();
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Disconnect(false);
                        Phone.removePhone(this.phoneNumber);
                        return;
                    }
                    else if (command.StartsWith("dial,"))
                    {
                        string[] parts = command.Split(new char[] { ',' });
                        int lineNumber = int.Parse(parts[1]);
                        // line should equal null the first dial
                        if (line == null)
                        {
                            line = (SimulatorLine)(new Simulator().GetLine(lineNumber));
                            line.subscribeToHangup(new HangupDelegate(handleHangup));
                        }
                        switch (line.Status)
                        {
                            case LineStatusTypes.OnHook:
                                Send("idle");
                                line.unsubscribeToHangup(new HangupDelegate(handleHangup));
                                line = null;
                                break;
                            case LineStatusTypes.Connected:
                                Send("busy");
                                line.unsubscribeToHangup(new HangupDelegate(handleHangup));
                                line = null;
                                break;
                            case LineStatusTypes.AcceptingCalls:
                                LineStatusTypes status = line.sendRing();
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
                        clearLine();
                    }
                    else if (command.StartsWith("dialoutResponse,"))
                    {
                        string[] parts = command.Split(new char[] { ',' });
                        string subCommand = parts[1];
                        if (subCommand == "Busy")
                        {
                            dialAnalysis = CallAnalysis.busy;
                        }
                        else if (subCommand == "Ignore")
                        {
                            dialAnalysis = CallAnalysis.noAnswer;
                        }
                        else if (subCommand == "Answering Machine")
                        {
                            dialAnalysis = CallAnalysis.answeringMachine;
                        }
                        else if (subCommand == "No Dial Tone")
                        {
                            dialAnalysis = CallAnalysis.noDialTone;
                        }
                        else if (subCommand == "Answer")
                        {
                            dialAnalysis = CallAnalysis.connected;
                        }
                        else
                        {
                            dialAnalysis = CallAnalysis.error;
                        }
                        lock (dialoutLock)
                        {
                            Monitor.Pulse(dialoutLock);
                        }                    
                    }
                    else if (command.Length == 1)
                    {
                        line.sendDigit(char.Parse(command));
                    }
                    else
                    {
                        // echo the command back
                        Send(command);
                    }
                }
                // read more or wait for the next command to come in
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
        }

        private void handleHangup(object sender, EventArgs args)
        {
            Send("hangup");
            clearLine();
        }

        private void Send(String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data+"<EOF>");

            // Begin sending the data to the remote device.
            state.workSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), state.workSocket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
