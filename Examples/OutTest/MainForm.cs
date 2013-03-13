using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ivrToolkit.Core;

namespace OutTest
{
    public partial class MainForm : Form
    {
        private ILine line;

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            line = LineManager.getLine(1);
        }

        private void btnDial_Click(object sender, EventArgs e)
        {
            CallAnalysis result = line.dial(txtPhoneNumber.Text, 3500);
            Console.WriteLine(result);
        }
    }
}
