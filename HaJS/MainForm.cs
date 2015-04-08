/* Copyright (C) 2015 haha01haha01

* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HaJS
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void ofdBox_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "XML Files|*.xml" };
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;
            ((TextBox)sender).Text = ofd.FileName;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (serverConfigPathBox.Text == "" || inputXmlBox.Text == "")
                return;
            HaJSCompiler jsc = new HaJSCompiler(serverConfigPathBox.Text);
            string outPath = Path.Combine(Path.GetDirectoryName(inputXmlBox.Text), Path.GetFileNameWithoutExtension(inputXmlBox.Text) + ".js");
            try
            {
                jsc.Compile(inputXmlBox.Text, outPath);
                MessageBox.Show("Finished compiling to " + outPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
