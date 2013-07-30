// 
// Copyright 2013 Troy Makaro
// 
// This file is part of ivrToolkit, distributed under the LESSER GNU GPL. For full terms see the included COPYING file and the COPYING.LESSER file.
// 
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace OutTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
