// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using System.Text;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Diagnostics;
using System.Media;

namespace ivrToolkit.VirtualPhone
{
    // ReSharper disable LocalizableElement
    public partial class Form1 : Form
    {
        private enum ConnType {
            Connected,
            Notconnected
        }
        private Socket _mySocket;
        private ConnType _conn = ConnType.Notconnected;
        private bool _disconnecting;
        private readonly SoundPlayer _myPlayer = new SoundPlayer();
        readonly Timer _timer;

        public Form1()
        {

            PlayFileAsync("silence");
            InitializeComponent();
            DisableAll();

            // TODO remove line once virtual phone is completed
            //ivrToolkit.voice.Manager.getLine(1);
            _timer = new Timer {Interval = 2000};
            _timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            _timer.Stop();
            Debug.WriteLine("tick");
            Send(_mySocket, "dial," + txtNumberToDial.Text);
        }

        private void DisableAll()
        {
            btnReciever.Enabled = false;
            btnDial.Enabled = false;
            txtNumberToDial.Enabled = false;
            DisableDigits();
        }

        private void EnableDigits()
        {
            btn0.Enabled = true;
            btn1.Enabled = true;
            btn2.Enabled = true;
            btn3.Enabled = true;
            btn4.Enabled = true;
            btn5.Enabled = true;
            btn6.Enabled = true;
            btn7.Enabled = true;
            btn8.Enabled = true;
            btn9.Enabled = true;
            btnStar.Enabled = true;
            btnPound.Enabled = true;
        }
        private void DisableDigits()
        {
            btn0.Enabled = false;
            btn1.Enabled = false;
            btn2.Enabled = false;
            btn3.Enabled = false;
            btn4.Enabled = false;
            btn5.Enabled = false;
            btn6.Enabled = false;
            btn7.Enabled = false;
            btn8.Enabled = false;
            btn9.Enabled = false;
            btnStar.Enabled = false;
            btnPound.Enabled = false;
        }

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (_conn == ConnType.Notconnected)
            {
                // Create a TCP/IP socket.
                var socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                socket.BeginConnect("localhost", 6050, ConnectCallback, socket);
                _mySocket = socket;
            }
            else
            {
                _disconnecting = true;
                Send(_mySocket, "disconnect");
            }
        }

        public delegate void InvokeDelegate();
        private void SayDisconnected()
        {
            _conn = ConnType.Notconnected;
            lblRegisterStatus.Text = "Not Connected";
            btnConnect.Text = "Connect";
            lblStatus.Text = "On Hook";
            btnReciever.Text = "Lift Reciever";
            lblDisplay.Text = "";

            _recieverLifted = false;
            DisableAll();
            _timer.Stop();

            _mySocket.Shutdown(SocketShutdown.Both);
            _mySocket.Close();
            _mySocket = null;
        }

        private void SayConnected()
        {
            _conn = ConnType.Connected;
            lblRegisterStatus.Text = "Connected";
            btnConnect.Text = "Disconnect";
            btnReciever.Enabled = true;
        }

        private void SayError()
        {
            lblRegisterStatus.Text = "Target Refused Connection";
        }

        private void ConnectCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            var socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndConnect(ar);
            }
            catch (SocketException)
            {
                BeginInvoke(new InvokeDelegate(SayError));
                return;
            }
            Send(socket, "connect," + txtVirtualNumber.Text);

            BeginInvoke(new InvokeDelegate(SayConnected));

            // Create the state object.
            var state = new StateObject {WorkSocket = socket};
            socket.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
        }

        private void Idle()
        {
            lblStatus.Text = "Not Accepting Calls";
            lblStatus.Refresh();
            PlayFile("phone2");
            _timer.Start();
        }
        private void Busy()
        {
            lblStatus.Text = "Busy";
            PlayFileAsync("busyTone");
        }
        private void Answered()
        {
            lblStatus.Text = "Answered";
            PlayFileAsync("phone2");
            EnableDigits();
            
        }
        private void Ringing()
        {
            lblStatus.Text = "Ringing";
            lblStatus.Refresh();
            PlayFile("phone2");
            _timer.Start();
        }
        private void DoIncomming()
        {
            var i = new Incomming();
            i.ShowDialog(this);
            if (i.Selected)
            {
                Send(_mySocket, "dialoutResponse," + i.cboReply.SelectedItem);
                if (i.cboReply.SelectedItem.ToString() == "Answer")
                {


                    // pick up the receiver
                    _recieverLifted = true;
                    btnReciever.Text = "Hangup";
                    btnDial.Enabled = false;
                    txtNumberToDial.Enabled = false;

                    lblStatus.Text = "Answered";
                    EnableDigits();
                }
            }
            else
            {
                Send(_mySocket, "dialoutResponse,Ignore");
            }
        }

        private  void ReadCallback(IAsyncResult ar)
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
                BeginInvoke(new InvokeDelegate(SayDisconnected));
                return;
            }
            catch (ObjectDisposedException)
            {
                BeginInvoke(new InvokeDelegate(SayDisconnected));
                return;
            }

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.Sb.Append(Encoding.ASCII.GetString(
                    state.Buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                String content = state.Sb.ToString();
                int index = content.IndexOf("<EOF>", StringComparison.Ordinal);
                if (index != -1)
                {
                    state.Sb = new StringBuilder();
                    var command = content.Substring(0, index);
                    switch (command)
                    {
                        case "idle":
                            BeginInvoke(new InvokeDelegate(Idle));
                            break;
                        case "busy":
                            BeginInvoke(new InvokeDelegate(Busy));
                            break;
                        case "answered":
                            BeginInvoke(new InvokeDelegate(Answered));
                            break;
                        case "ring":
                            BeginInvoke(new InvokeDelegate(Ringing));
                            break;
                        case "hangup":
                            BeginInvoke(new InvokeDelegate(DoHangup));
                            break;
                        case "dialout":
                            BeginInvoke(new InvokeDelegate(DoIncomming));
                            break;
                    }

                    // All the data has been read from the 
                    // client. Display it on the console.
                    Debug.WriteLine("received command: " + command);
                }

                // get more or wait for the next response
                handler.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                ReadCallback, state);
            }
        }
        private  void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data+"<EOF>");

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                SendCallback, handler);
        }

        private  void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.
                handler.EndSend(ar);

                if (_disconnecting)
                {
                    _disconnecting = false;
                    BeginInvoke(new InvokeDelegate(SayDisconnected));
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }

        private void PlayFileAsync(string filename)
        {
            _myPlayer.SoundLocation = @"tones\" + filename + ".wav";
            _myPlayer.Play();
        }
        private void PlayFile(string filename)
        {
            _myPlayer.SoundLocation = @"tones\" + filename + ".wav";
            _myPlayer.PlaySync();
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            PlayFile("1");
            lblDisplay.Text += "1";
            Send(_mySocket, "1");
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            PlayFile("2");
            lblDisplay.Text += "2";
            Send(_mySocket, "2");
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            PlayFile("3");
            lblDisplay.Text += "3";
            Send(_mySocket, "3");

        }

        private void btn4_Click(object sender, EventArgs e)
        {
            PlayFile("4");
            lblDisplay.Text += "4";
            Send(_mySocket, "4");

        }

        private void btn5_Click(object sender, EventArgs e)
        {
            PlayFile("5");
            lblDisplay.Text += "5";
            Send(_mySocket, "5");

        }

        private void btn6_Click(object sender, EventArgs e)
        {
            PlayFile("6");
            lblDisplay.Text += "6";
            Send(_mySocket, "6");

        }

        private void btn7_Click(object sender, EventArgs e)
        {
            PlayFile("7");
            lblDisplay.Text += "7";
            Send(_mySocket, "7");

        }

        private void btn8_Click(object sender, EventArgs e)
        {
            PlayFile("8");
            lblDisplay.Text += "8";
            Send(_mySocket, "8");

        }

        private void btn9_Click(object sender, EventArgs e)
        {
            PlayFile("9");
            lblDisplay.Text += "9";
            Send(_mySocket, "9");

        }

        private void btnPound_Click(object sender, EventArgs e)
        {
            PlayFile("hash");
            lblDisplay.Text += "#";
            Send(_mySocket, "#");

        }

        private void btn0_Click(object sender, EventArgs e)
        {
            PlayFile("0");
            lblDisplay.Text += "0";
            Send(_mySocket, "0");

        }

        private void btnStar_Click(object sender, EventArgs e)
        {
            PlayFile("star");
            lblDisplay.Text += "*";
            Send(_mySocket, "*");

        }
        private void DoHangup()
        {
            PlayFileAsync("dialtone");
            SetHangup();
            lblStatus.Text = "Server Hangup";
        }

        private void SetHangup()
        {
            // do the hangup
            _recieverLifted = false;
            btnReciever.Text = "Lift Reciever";
            lblStatus.Text = "On Hook";
            btnDial.Enabled = false;
            txtNumberToDial.Enabled = false;
            DisableDigits();
            lblDisplay.Text = "";
        }

        private void btnDial_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Dialing";

            lblStatus.Refresh();
            btnDial.Enabled = false;
            txtNumberToDial.Enabled = false;
            Send(_mySocket, "dial," + txtNumberToDial.Text);
        }
        private bool _recieverLifted;
        private void btnReciever_Click(object sender, EventArgs e)
        {
            if (_recieverLifted)
            {
                // do the hangup
                SetHangup();
                _timer.Stop();
                Send(_mySocket, "hangup");
            }
            else
            {
                // pick up the receiver
                _recieverLifted = true;
                btnReciever.Text = "Hangup";
                lblStatus.Text = "Off Hook";
                btnReciever.Refresh();
                lblStatus.Refresh();
                PlayFile("dialtone");
                btnDial.Enabled = true;
                txtNumberToDial.Enabled = true;
            }
        }
    }
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
}
