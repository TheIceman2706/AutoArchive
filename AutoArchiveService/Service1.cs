using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Timers;
using Microsoft.Win32;

namespace AutoArchiveService
{
    public partial class AutoArchiveService : ServiceBase
    {
        Timer timer1;
        public AutoArchiveService()
        {
            InitializeComponent();
            timer1 = new Timer();
        }

        protected override void OnStart(string[] args)
        {
            RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Friedrich May").OpenSubKey("AutoArchiv");

            Properties.Settings.Default.OlderThan = TimeSpan.Parse((string)rk.GetValue("OlderThen"));
            Properties.Settings.Default.ToDirectory = (string)rk.GetValue("ToDirectory");
            Properties.Settings.Default.FromDirectory = (string)rk.GetValue("FromDirectory");
            Properties.Settings.Default.CheckInterval = TimeSpan.Parse((string)rk.GetValue("CheckInterval"));
            Properties.Settings.Default.IncludeHidden = Boolean.Parse((string)rk.GetValue("IncludeHidden"));
            Properties.Settings.Default.MoveOldFiles = Boolean.Parse((string)rk.GetValue("MoveOldFiles"));
            Properties.Settings.Default.Save();

            timer1.Interval = Convert.ToInt32(Properties.Settings.Default.CheckInterval.TotalMilliseconds);
            timer1.Enabled = true;
            timer1.Elapsed += timer1_Tick;
            timer1.AutoReset = true;
            timer1.Start();
        }

        protected override void OnStop()
        {
            Properties.Settings.Default.Save();
        }

        protected override void OnCustomCommand(int command)
        {

            base.OnCustomCommand(command);
            switch (command)
            {
                case Messages.RELOAD_SETTINGS:
                    RegistryKey rk = Registry.LocalMachine.OpenSubKey("SOFTWARE").OpenSubKey("Friedrich May").OpenSubKey("AutoArchiv");

                    Properties.Settings.Default.OlderThan = TimeSpan.Parse((string)rk.GetValue("OlderThen"));
                    Properties.Settings.Default.ToDirectory = (string)rk.GetValue("ToDirectory");
                    Properties.Settings.Default.FromDirectory = (string)rk.GetValue("FromDirectory");
                    Properties.Settings.Default.CheckInterval = TimeSpan.Parse((string)rk.GetValue("CheckInterval"));
                    Properties.Settings.Default.IncludeHidden = Boolean.Parse((string)rk.GetValue("IncludeHidden"));
                    Properties.Settings.Default.MoveOldFiles = Boolean.Parse((string)rk.GetValue("MoveOldFiles"));
                    Properties.Settings.Default.Save();
                    timer1.Interval = Convert.ToInt32(Properties.Settings.Default.CheckInterval.TotalMilliseconds);
                    break;
                default:break;
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Stack<string> dirsToCheck = new Stack<string>();
            string fromPath = Path.GetFullPath(Properties.Settings.Default.FromDirectory);
            if (!fromPath.EndsWith("\\"))
                fromPath += "\\";
            string toPath = Path.GetFullPath(Properties.Settings.Default.ToDirectory);
            if (!toPath.EndsWith("\\"))
            {
                toPath += "\\";
            }
            if(Directory.Exists(fromPath))
                dirsToCheck.Push(fromPath);

            while(dirsToCheck.Count != 0)
            {
                string cd = dirsToCheck.Pop();
                foreach (string d in Directory.GetDirectories(cd))
                {
                    DirectoryInfo info = new DirectoryInfo(d);
                      if ((Properties.Settings.Default.IncludeHidden || (info.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden) && (info.Attributes & FileAttributes.System) != FileAttributes.System)
                          dirsToCheck.Push(d.EndsWith("\\") ? d : d + "\\");
                }

                string dir = cd.Replace(fromPath, "");
                string to = Path.Combine(toPath, dir);
                if (!to.EndsWith("\\"))
                    to += "\\";
                if (!Directory.Exists(to))
                {
                    Directory.CreateDirectory(to);
                }
                foreach (string f in Directory.EnumerateFiles(cd))
                {
                    to = Path.Combine(toPath, dir, f.Replace(cd, ""));
                    if ((!File.Exists(to)) || (File.GetLastWriteTime(f).Add(Properties.Settings.Default.OlderThan) <= DateTime.Now && File.GetLastWriteTime(f) >= File.GetLastWriteTime(to)))
                    {
                        if (Properties.Settings.Default.MoveOldFiles)
                        {
                            File.Move(f, to);
                        }
                        else
                        {
                            File.Copy(f, to, true);
                        }
                    }
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!backgroundWorker1.IsBusy)
            {
                backgroundWorker1.RunWorkerAsync();
            }
        }
    }
}
