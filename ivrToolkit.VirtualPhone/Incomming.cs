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

namespace ivrToolkit.VirtualPhone
{
    public partial class Incomming : Form
    {
        public bool selected = false;

        public Incomming()
        {
            InitializeComponent();
        }

        private void Incomming_Load(object sender, EventArgs e)
        {
            cboReply.SelectedIndex = 0;
        }

        private void btnReply_Click(object sender, EventArgs e)
        {
            selected = true;
            this.Close();
        }
    }
}
