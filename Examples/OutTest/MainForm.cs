// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
using System;
using System.Threading;
using System.Windows.Forms;
using ivrToolkit.Core;
using ivrToolkit.Core.Exceptions;
using SimulatorTest.ScriptBlocks;

namespace OutTest
{
    public partial class MainForm : Form
    {
        private ILine _line;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btnDial_Click(object sender, EventArgs e)
        {
            try
            {
                _line = LineManager.GetLine(int.Parse(txtLine.Text));
                _line.Hangup();
                Thread.Sleep(2000);
                var result = _line.Dial(txtPhoneNumber.Text, 3500);

                if (result == CallAnalysis.Connected)
                {
                    var manager = new ScriptManager(_line, new WelcomeScript());

                    while (manager.HasNext())
                    {
                        // execute the next script
                        manager.Execute();
                    }

                }
                else
                {
                    MessageBox.Show(result.ToString());
                }
                _line.Hangup();
                LineManager.ReleaseAll();

            }
            catch (HangupException)
            {
                MessageBox.Show(@"hungup");                
            }
        }
    }
}
