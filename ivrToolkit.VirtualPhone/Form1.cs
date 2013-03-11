/*
 * Copyright 2013 Troy Makaro
 *
 * This file is part of ivrToolkit, distributed under the GNU GPL. For full terms see the included COPYING file.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Media;

namespace ivrToolkit.VirtualPhone
{
    public partial class Form1 : Form
    {
        private enum connType {
            connected,
            notconnected
        }
        private Socket mySocket;
        private connType conn = connType.notconnected;
        private bool disconnecting = false;
        private SoundPlayer myPlayer = new SoundPlayer();
        Timer timer;

        public Form1()
        {

            playFileAsync("silence");
            InitializeComponent();
            disableAll();

            // TODO remove line once virtual phone is completed
            //ivrToolkit.voice.Manager.getLine(1);
            timer = new Timer();
            timer.Interval = 2000;
            timer.Tick += new EventHandler(timer_Tick);
        }

        void timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            Debug.WriteLine("tick");
            Send(mySocket, "dial," + txtNumberToDial.Text);
        }

        private void disableAll()
        {
            btnReciever.Enabled = false;
            btnDial.Enabled = false;
            txtNumberToDial.Enabled = false;
            disableDigits();
        }

        private void enableDigits()
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
        private void disableDigits()
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
            if (conn == connType.notconnected)
            {
                // Create a TCP/IP socket.
                Socket socket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                socket.BeginConnect("localhost", 6050, new AsyncCallback(connectCallback), socket);
                mySocket = socket;
            }
            else
            {
                disconnecting = true;
                Send(mySocket, "disconnect");
            }
        }

        public delegate void InvokeDelegate();
        private void sayDisconnected()
        {
            conn = connType.notconnected;
            lblRegisterStatus.Text = "Not Connected";
            btnConnect.Text = "Connect";
            lblStatus.Text = "On Hook";
            btnReciever.Text = "Lift Reciever";
            lblDisplay.Text = "";

            recieverLifted = false;
            disableAll();
            timer.Stop();

            mySocket.Shutdown(SocketShutdown.Both);
            mySocket.Close();
            mySocket = null;
        }

        private void sayConnected()
        {
            conn = connType.connected;
            lblRegisterStatus.Text = "Connected";
            btnConnect.Text = "Disconnect";
            btnReciever.Enabled = true;
        }

        private Exception myException;
        private void sayError()
        {
            lblRegisterStatus.Text = "Target Refused Connection";
        }

        private void connectCallback(IAsyncResult ar)
        {
            // Retrieve the socket from the state object.
            Socket socket = (Socket)ar.AsyncState;
            try
            {
                socket.EndConnect(ar);
            }
            catch (SocketException e)
            {
                myException = e;
                BeginInvoke(new InvokeDelegate(sayError));
                return;
            }
            Send(socket, "connect," + txtVirtualNumber.Text);

            BeginInvoke(new InvokeDelegate(sayConnected));

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = socket;
            socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(readCallback), state);
        }

        private void idle()
        {
            lblStatus.Text = "Not Accepting Calls";
            lblStatus.Refresh();
            playFile("phone2");
            timer.Start();
        }
        private void busy()
        {
            lblStatus.Text = "Busy";
            playFileAsync("busyTone");
        }
        private void answered()
        {
            lblStatus.Text = "Answered";
            playFileAsync("phone2");
            enableDigits();
            
        }
        private void ringing()
        {
            lblStatus.Text = "Ringing";
            lblStatus.Refresh();
            playFile("phone2");
            timer.Start();
        }
        private void doIncomming()
        {
            Incomming i = new Incomming();
            i.ShowDialog(this);
            if (i.selected == true)
            {
                Send(mySocket, "dialoutResponse," + i.cboReply.SelectedItem.ToString());
                if (i.cboReply.SelectedItem.ToString() == "Answer")
                {


                    // pick up the receiver
                    recieverLifted = true;
                    btnReciever.Text = "Hangup";
                    btnDial.Enabled = false;
                    txtNumberToDial.Enabled = false;

                    lblStatus.Text = "Answered";
                    enableDigits();
                }
            }
            else
            {
                Send(mySocket, "dialoutResponse,Ignore");
            }
        }

        private  void readCallback(IAsyncResult ar)
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
                BeginInvoke(new InvokeDelegate(sayDisconnected));
                return;
            }
            catch (ObjectDisposedException)
            {
                BeginInvoke(new InvokeDelegate(sayDisconnected));
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
                    if (command == "idle")
                    {
                        BeginInvoke(new InvokeDelegate(idle));
                    }
                    else if (command == "busy")
                    {
                        BeginInvoke(new InvokeDelegate(busy));
                    }
                    else if (command == "answered")
                    {
                        BeginInvoke(new InvokeDelegate(answered));
                    }
                    else if (command == "ring")
                    {
                        BeginInvoke(new InvokeDelegate(ringing));
                    }
                    else if (command == "hangup")
                    {
                        BeginInvoke(new InvokeDelegate(doHangup));
                    }
                    else if (command == "dialout")
                    { // an incomming line
                        BeginInvoke(new InvokeDelegate(doIncomming));
                    }

                    // All the data has been read from the 
                    // client. Display it on the console.
                    Debug.WriteLine("received command: " + command);
                }

                // get more or wait for the next response
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(readCallback), state);
            }
        }
        private  void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data+"<EOF>");

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private  void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);

                if (disconnecting == true)
                {
                    disconnecting = false;
                    BeginInvoke(new InvokeDelegate(sayDisconnected));
                    return;
                }

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
        }

        private void playFileAsync(string filename)
        {
            myPlayer.SoundLocation = @"tones\" + filename + ".wav";
            myPlayer.Play();
        }
        private void playFile(string filename)
        {
            myPlayer.SoundLocation = @"tones\" + filename + ".wav";
            myPlayer.PlaySync();
        }

        private void btn1_Click(object sender, EventArgs e)
        {
            playFile("1");
            lblDisplay.Text += "1";
            Send(mySocket, "1");
        }

        private void btn2_Click(object sender, EventArgs e)
        {
            playFile("2");
            lblDisplay.Text += "2";
            Send(mySocket, "2");
        }

        private void btn3_Click(object sender, EventArgs e)
        {
            playFile("3");
            lblDisplay.Text += "3";
            Send(mySocket, "3");

        }

        private void btn4_Click(object sender, EventArgs e)
        {
            playFile("4");
            lblDisplay.Text += "4";
            Send(mySocket, "4");

        }

        private void btn5_Click(object sender, EventArgs e)
        {
            playFile("5");
            lblDisplay.Text += "5";
            Send(mySocket, "5");

        }

        private void btn6_Click(object sender, EventArgs e)
        {
            playFile("6");
            lblDisplay.Text += "6";
            Send(mySocket, "6");

        }

        private void btn7_Click(object sender, EventArgs e)
        {
            playFile("7");
            lblDisplay.Text += "7";
            Send(mySocket, "7");

        }

        private void btn8_Click(object sender, EventArgs e)
        {
            playFile("8");
            lblDisplay.Text += "8";
            Send(mySocket, "8");

        }

        private void btn9_Click(object sender, EventArgs e)
        {
            playFile("9");
            lblDisplay.Text += "9";
            Send(mySocket, "9");

        }

        private void btnPound_Click(object sender, EventArgs e)
        {
            playFile("hash");
            lblDisplay.Text += "#";
            Send(mySocket, "#");

        }

        private void btn0_Click(object sender, EventArgs e)
        {
            playFile("0");
            lblDisplay.Text += "0";
            Send(mySocket, "0");

        }

        private void btnStar_Click(object sender, EventArgs e)
        {
            playFile("star");
            lblDisplay.Text += "*";
            Send(mySocket, "*");

        }
        private void doHangup()
        {
            playFileAsync("dialtone");
            setHangup();
            lblStatus.Text = "Server Hangup";
        }

        private void setHangup()
        {
            // do the hangup
            recieverLifted = false;
            btnReciever.Text = "Lift Reciever";
            lblStatus.Text = "On Hook";
            btnDial.Enabled = false;
            txtNumberToDial.Enabled = false;
            disableDigits();
            lblDisplay.Text = "";
        }

        private void btnDial_Click(object sender, EventArgs e)
        {
            lblStatus.Text = "Dialing";
            lblStatus.Refresh();
            btnDial.Enabled = false;
            txtNumberToDial.Enabled = false;
            Send(mySocket, "dial," + txtNumberToDial.Text);
        }
        private bool recieverLifted = false;
        private void btnReciever_Click(object sender, EventArgs e)
        {
            if (recieverLifted)
            {
                // do the hangup
                setHangup();
                timer.Stop();
                Send(mySocket, "hangup");
            }
            else
            {
                // pick up the receiver
                recieverLifted = true;
                btnReciever.Text = "Hangup";
                lblStatus.Text = "Off Hook";
                btnReciever.Refresh();
                lblStatus.Refresh();
                playFile("dialtone");
                btnDial.Enabled = true;
                txtNumberToDial.Enabled = true;
            }
        }
    }
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
}
