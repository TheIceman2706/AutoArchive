using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Configuration
{
    public partial class Form1 : Form
    {
        RegistryKey rk;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            rk = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE", true);
            rk= rk.CreateSubKey("Friedrich May", true).CreateSubKey("AutoArchiv", true);
            rk.Flush();
            if(rk.GetValue("FromDirectory") == null)
            {
                rk.SetValue("FromDirectory", "D:\\archTest\\");
                rk.SetValue("ToDirectory", "E:\\archTest");
                rk.SetValue("CheckInterval", new TimeSpan(0, 0, 10));
                rk.SetValue("OlderThen", new TimeSpan(0, 0, 1));
                rk.SetValue("MoveOldFiles", false);
                rk.SetValue("IncludeHidden", false);
            }
            rk.Flush();

            moveCheckBox.Checked = Boolean.Parse((string)rk.GetValue("MoveOldFiles"));
            treatHiddenCheckBox.Checked = Boolean.Parse((string)rk.GetValue("IncludeHidden"));

            label1.Text = "Von: "+(string)rk.GetValue("FromDirectory");
            label2.Text = "Nach: "+(string)rk.GetValue("ToDirectory");
            maskedTextBox2.Text = TimeSpan.Parse((string)rk.GetValue("CheckInterval")).ToString("hh\\:mm\\:ss");
            maskedTextBox1.Text = TimeSpan.Parse((string)rk.GetValue("OlderThen")).ToString("dd\\ hh\\:mm\\:ss");

            serviceController1.MachineName = Environment.MachineName;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

            rk.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                label1.Text = "Von: " + folderBrowserDialog1.SelectedPath;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                label2.Text = "Nach :" + folderBrowserDialog1.SelectedPath;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            rk.SetValue("MoveOldFiles", moveCheckBox.Checked);
            rk.SetValue("IncludeHidden", treatHiddenCheckBox.Checked);
            rk.SetValue("FromDirectory", label1.Text.Substring(5));
            rk.SetValue("ToDirectory", label2.Text.Substring(6));
            rk.SetValue("CheckInterval", TimeSpan.Parse(maskedTextBox2.Text));
            rk.SetValue("OlderThen", TimeSpan.Parse(maskedTextBox1.Text.Substring(3)).Add(TimeSpan.FromDays(Convert.ToInt32(maskedTextBox1.Text.Remove(2)))));

            rk.Flush();

            if (serviceController1.Status == System.ServiceProcess.ServiceControllerStatus.Running)
            {
                serviceController1.Refresh();
                serviceController1.ExecuteCommand(AutoArchiveService.Messages.RELOAD_SETTINGS);
            }
        }
    }
}
