using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Host
{
    public class Activator
    {        
        private readonly FileSystemWatcher _watcher;
        private readonly ConcurrentDictionary<string, Process> _applicationProcessesDictionary;

        public Activator(string basePath)
        {
            if (basePath == null) throw new ArgumentNullException(nameof(basePath));
            if (!Path.IsPathRooted(basePath)) throw new ArgumentException("The path should be rooted", nameof(basePath));

            _watcher = new FileSystemWatcher()
            {
                Path = basePath,
                Filter = Bootstrapper.ApplicationFileName,
                EnableRaisingEvents = true,
                IncludeSubdirectories = true
            };

            _watcher.Created += Watcher_Created;
            _watcher.Changed += Watcher_Changed;
            _watcher.Deleted += Watcher_Deleted;
            _applicationProcessesDictionary = new ConcurrentDictionary<string, Process>();
        }

        [DllImport("User32.dll")]
        static extern int SetForegroundWindow(IntPtr point);

        private void Watcher_Created(object sender, FileSystemEventArgs e)
        {
            Activate(e.FullPath);
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Deactivate(e.FullPath);
            Activate(e.FullPath);
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            Deactivate(e.FullPath);
        }

        private bool Activate(string applicationPath)
        {
            if (!File.Exists(applicationPath)) return false;

            var process = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = true,
                    FileName = "mhh.exe",
                    Arguments = applicationPath
                }
            };

            if (!process.Start()) return false;
            if (!_applicationProcessesDictionary.TryAdd(applicationPath, process))
            {
                
                return false;
            }

            return true;
        }

        private void Deactivate(string applicationPath)
        {

        }

        private void SendKey(IntPtr handle)
        {
            //SendKeys
        }
    }
}
