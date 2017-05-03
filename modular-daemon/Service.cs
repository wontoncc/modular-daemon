using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.ComponentModel;
using System.Management;
using System.IO;

namespace modular_daemon {
    public class Service : INotifyPropertyChanged {
        public string Name { get; set; }
        private string command;
        private string arguments;
        private string dir;
        private List<KeyValuePair<string, string>> env = new List<KeyValuePair<string, string>>();
        public string Log { get; set; }
        private StreamWriter logWriter = null;
        private FileStream logFile = null;
        Process process;

        int outputBufferSize = 5000;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Output { get; set; }

        public Service(
            string name,
            string command,
            string arguments = null,
            string dir = null,
            string log = null,
            List<KeyValuePair<string, string>> env = null,
            bool started = true) {
            this.Name = name;
            this.Output = "";
            this.command = command;
            this.dir = dir;
            this.arguments = arguments;
            if (env != null) {
                this.env = this.env.Concat(env).ToList();
            }
            if (log != null) {
                this.Log = log;
                this.logFile = new FileStream(log, FileMode.Append);
                this.logWriter = new StreamWriter(this.logFile);
            }
            if (started) {
                Start();
            }
        }

        public static Service ServiceBuilder(string name, string command, string arguments) {
            return new Service(name, command, arguments, started: false);
        }

        public void SerEnvironmentVariable (string key, string value) {
            this.process.StartInfo.EnvironmentVariables.Add(key, value);
        }

        public void SetWorkingDirectory(string d) {
            this.process.StartInfo.WorkingDirectory = d;
        }

        public void SetArguments(string a) {
            this.process.StartInfo.Arguments = a;
        }

        public void SetOutputBufferSize (int size) {
            this.outputBufferSize = size;
        }

        public void Start () {
            this.process = new Process();
            this.process.StartInfo.CreateNoWindow = true;
            this.process.StartInfo.UseShellExecute = false;
            this.process.StartInfo.RedirectStandardError = true;
            this.process.StartInfo.RedirectStandardOutput = true;
            this.process.EnableRaisingEvents = true;

            this.process.StartInfo.FileName = this.command;
            this.process.StartInfo.Arguments = this.arguments;
            this.process.StartInfo.WorkingDirectory = this.dir;
            foreach (var p in this.env) {
                this.process.StartInfo.EnvironmentVariables.Add(p.Key, p.Value);
            }

            this.process.ErrorDataReceived += new DataReceivedEventHandler((s, e) => {
                this.PushOutput(e.Data + Environment.NewLine);
            });
            this.process.OutputDataReceived += new DataReceivedEventHandler((s, e) => {
                this.PushOutput(e.Data + Environment.NewLine);
            });

            this.process.Start();
            this.process.BeginOutputReadLine();
            this.process.BeginErrorReadLine();
        }
        public void Restart () {
            this.EndProcess();
            this.PushOutput(Environment.NewLine + Environment.NewLine);
            this.Start();
        }

        public void PushOutput (string s) {
            this.Output += s;
            if (this.Output.Length > this.outputBufferSize) {
                this.Output = this.Output.Remove(0, this.Output.Length - this.outputBufferSize);
            }
            this.logWriter?.Write(s);
            OnPropertyChanged("Output");
        }

        public void EndProcess () {
            if (!this.process.HasExited) {
                this.process.CancelErrorRead();
                this.process.CancelOutputRead();
                KillProcessAndChildren(this.process.Id);
            }
        }

        public void Close () {
            this.PushOutput(Environment.NewLine + Environment.NewLine);
            this.logWriter?.Close();
            this.logFile?.Close();
        }

        protected void OnPropertyChanged (string name) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        /// <summary>
        /// Kill a process, and all of its children, grandchildren, etc.
        /// from http://stackoverflow.com/questions/5901679/kill-process-tree-programmatically-in-c-sharp
        /// </summary>
        /// <param name="pid">Process ID.</param>
        private static void KillProcessAndChildren(int pid) {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher
                ("Select * From Win32_Process Where ParentProcessID=" + pid);
            ManagementObjectCollection moc = searcher.Get();
            foreach (ManagementObject mo in moc) {
                KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
            }
            try {
                Process proc = Process.GetProcessById(pid);
                if (!proc.HasExited) {
                    proc.Kill();
                }
            } catch (ArgumentException) {
                // the process is not running
            }
        }
    }
}
