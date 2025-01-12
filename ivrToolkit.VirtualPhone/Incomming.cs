// 
// Copyright 2021 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the Apache-2.0 license.
// 
// 
using System;
using System.Windows.Forms;

namespace ivrToolkit.VirtualPhone;

public partial class Incomming : Form
{
    public bool Selected = false;

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
        Selected = true;
        Close();
    }
}