/*
    <FileBurn, The simple file shredder>
    Copyright (C) <2014> <Jacopo De Luca>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Collections;
using System.IO;


namespace FileBurn
{
    public partial class Form1 : Form
    {
        #region Variable
        private List<string> FileList = new List<string>();
        bool rndType = false;
        Thread BurnTh;
        RndByte rndB;
        Shredder shd;
        #endregion

        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();

        internal static class UnsafeNativeMethods
        {
            [DllImport("CpuVendor.dll")]
            public static extern int GetCpuVendor();

            [DllImport("IntelRnd.dll")]
            public static extern bool RndSupported();
        }


        public Form1()
        {
            InitializeComponent();
            shd = new Shredder();
            shd.Publishp += this.PublishProgress;
            shd.PublishFi += this.PublishFinfo;
            shd.CicleN += this.CicleN;
            shd.Finish += this.Finish;
            shd.WorkE += this.WorkException;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            Console.Title = "FileBurn - Console";
            Console.Write("Copyright (C) 2014 Jacopo De Luca\nThis program is free software.\n\nThis program is distributed in the hope that it will be useful,but WITHOUT ANY WARRANTY!\nsee <http://www.gnu.org/licenses/>.\n\n");
            this.ProcessIdentifier();
            rndB = new RndByte(this.rndType ? RndByte.RndType.TRNG : RndByte.RndType.PRNG);
        }

        #region UI

        #region menu
        private void clearFileListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox1.Items.Count > 0)
            {
                listBox1.BeginUpdate();
                listBox1.Items.Clear();
                this.FileList.Clear();
                listBox1.EndUpdate();
            }
        }

        private void removeSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listBox1.BeginUpdate();
            ArrayList vSelectedItems = new ArrayList(listBox1.SelectedItems);
            foreach (string item in vSelectedItems)
            {
                this.FileList.Remove(item);
                listBox1.Items.Remove(item);
            }
            listBox1.EndUpdate();
        }

        private void cleanConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.Clear();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            About ab = new About();
            ab.ShowDialog();
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                string path = folderBrowserDialog1.SelectedPath;
                this.LoadFileFromDirectory(path);
                textBox1.Text = path;
                Console.WriteLine("Added directory: " + path);
                this.UpdateListView();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                foreach (string file in openFileDialog1.FileNames)
                {
                    if (!this.FileList.Contains(file))
                        this.FileList.Add(file);
                    textBox1.Text = file;
                    Console.WriteLine("Added file: " + file);  
                }
                this.UpdateListView();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.FileList.Count > 0)
            {
                DialogResult dr = MessageBox.Show("Warning:\nAll data will be lost, Continue?", "FileBurn", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);
                if (dr == DialogResult.OK)
                {
                    progressBar2.Value = 0;
                    progressBar2.Maximum = this.FileList.Count;
                    progressBar2.Step = 1;
                    button3.Enabled = false;
                    button4.Enabled = true;
                    this.BurnTh = new Thread(new ThreadStart(this.Execute));
                    this.BurnTh.Start();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.shd.stopProcess();
            this.stopTh = true;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = !radioButton1.Checked;
            this.SecurityInfo();
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            numericUpDown1.Enabled = radioButton2.Checked;
            this.SecurityInfo();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            this.SecurityInfo();
        }

        #endregion

        #region method

        private void ProcessIdentifier()
        {
            Console.Write("Checking CPU Vendor... ");
            int tmp=UnsafeNativeMethods.GetCpuVendor();
            if (tmp == 0)
            {
                Console.WriteLine("Intel");
                Console.Write("Checking Rdrand support... ");
                bool rdtmp = UnsafeNativeMethods.RndSupported();
                if (rdtmp)
                {
                    Console.WriteLine("OK");
                    this.rndType = true;
                }
                else
                {
                    Console.WriteLine("Not Supported");
                    this.rndType = false;
                }
            }
            else if (tmp == 1)
            {
                Console.WriteLine("AMD");
                this.rndType = false;
            }
            else
            {
                Console.WriteLine("Other");
                this.rndType = false;
            }
            Console.WriteLine();
        }

        private void SecurityInfo()
        {
            if (radioButton1.Checked)
                pictureBox1.Image = FileBurn.Properties.Resources.security_low;
            else if (radioButton2.Checked && numericUpDown1.Value <= 2)
                pictureBox1.Image = FileBurn.Properties.Resources.security_medium;
            else if (radioButton2.Checked && numericUpDown1.Value > 2)
                pictureBox1.Image = FileBurn.Properties.Resources.security_high;
        }

        private void LoadFileFromDirectory(string directory)
        {
            string[] files = System.IO.Directory.GetFiles(directory);
            foreach (string file in files)
                if(!this.FileList.Contains(file))
                    this.FileList.Add(file);
        }

        private void UpdateListView()
        {
            listBox1.Items.Clear();
            listBox1.BeginUpdate();
            foreach (string file in this.FileList)
                listBox1.Items.Add(file);
            listBox1.EndUpdate();
        }

        #region Shredder
        private bool stopTh = false;
        public void Execute()
        {
            this.stopTh = false;
            foreach (string File in this.FileList)
            {
                // Cicle
                int cicle=radioButton1.Checked?1:(int)numericUpDown1.Value;
                shd.setNext(File, cicle, this.rndB);
                shd.Execute();

                progressBar2.Invoke((MethodInvoker)delegate
                {
                    progressBar2.PerformStep();
                    progressBar1.Value = 0;
                });

                if (stopTh)
                {
                    Console.WriteLine("\n\nOperation aborted by user!");
                    break;
                }
            }
            this.Invoke((MethodInvoker)delegate
            {
                button4.Enabled = false;
                button3.Enabled = true; 
            });
        }

        private void PublishFinfo(object sender, string file, long len)
        {
            Console.WriteLine("\nNow: " + file + " - " + len + " byte");
        }

        private void PublishProgress(object sender, int progress)
        {
            this.Invoke((MethodInvoker)delegate
            {
                progressBar1.Value = progress;
            });
        }

        private void CicleN(object sender,int now,int total)
        {
            Console.WriteLine("Cicle " + (now + 1) + " of " + total);
        }

        private void Finish(object source, string file, long len)
        {
            Console.WriteLine("Cleaned " + len + " Byte"); 
        }

        private void WorkException(object source, string message)
        {
            Console.WriteLine(message);
        }
        #endregion

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        #endregion
    }    
}
