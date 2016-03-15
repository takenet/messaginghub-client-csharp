using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Takenet.MessagingHub.Client.Host
{
    public sealed class ApplicationActivator : IDisposable
    {
        private readonly string _basePath;
        private readonly TimeSpan _waitForActivationDelay;
        private readonly FileSystemWatcher _watcher;
        private readonly ConcurrentDictionary<string, Process> _applicationProcessesDictionary;
        private readonly ConcurrentDictionary<string, DateTime> _applicationLastWriteDictionary;
        private readonly string _tempBasePath;
        private readonly Job _job;                
        private readonly object _syncRoot = new object();

        public ApplicationActivator(string basePath, TimeSpan waitForActivationDelay)
        {
            _basePath = basePath;
            _waitForActivationDelay = waitForActivationDelay;
            if (basePath == null) throw new ArgumentNullException(nameof(basePath));
            if (!Path.IsPathRooted(basePath)) throw new ArgumentException("The path should be rooted", nameof(basePath));

            _watcher = new FileSystemWatcher()
            {
                Path = basePath,
                Filter = Bootstrapper.DefaultApplicationFileName,                
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
            };

            _watcher.Created += Watcher_Changed;
            _watcher.Changed += Watcher_Changed;            
            _watcher.Deleted += Watcher_Deleted;
            _watcher.Renamed += Watcher_Renamed; // When moved to the recycle bin
            _watcher.Error += Watcher_Error;

            _applicationProcessesDictionary = new ConcurrentDictionary<string, Process>();
            _applicationLastWriteDictionary = new ConcurrentDictionary<string, DateTime>();
            _tempBasePath = Path.Combine(Path.GetTempPath(), $"{nameof(ApplicationActivator)}-{Guid.NewGuid()}");
            RecreateDirectory(_tempBasePath);
            _job = new Job();
            foreach (var applicationPath in Directory.GetFiles(_basePath, Bootstrapper.DefaultApplicationFileName, SearchOption.AllDirectories))
            {
                Activate(applicationPath);
            }

            _watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            lock (_syncRoot)
            {
                var fileInfo = new FileInfo(e.FullPath);
                while (IsFileLocked(fileInfo))
                {
                    Thread.Sleep(250);
                }

                Thread.Sleep(_waitForActivationDelay);

                // Fix for the multiple raises on the FileSystemWatcher
                var applicationLastWrite = File.GetLastWriteTimeUtc(e.FullPath);
                if (!_applicationLastWriteDictionary.ContainsKey(e.FullPath) ||
                    _applicationLastWriteDictionary[e.FullPath] < applicationLastWrite)
                {
                    _applicationLastWriteDictionary.AddOrUpdate(e.FullPath, applicationLastWrite,
                        (k, v) => applicationLastWrite);
                    Deactivate(e.FullPath);
                    Activate(e.FullPath);
                }
            }
        }

        private void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            lock (_syncRoot)
            {
                Thread.Sleep(_waitForActivationDelay);
                Deactivate(e.FullPath);

                DateTime applicationLastWrite;
                _applicationLastWriteDictionary.TryRemove(e.FullPath, out applicationLastWrite);
            }
        }

        private void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            lock (_syncRoot)
            {
                Thread.Sleep(_waitForActivationDelay);                
                Deactivate(e.OldFullPath);

                DateTime applicationLastWrite;
                _applicationLastWriteDictionary.TryRemove(e.FullPath, out applicationLastWrite);
            }
        }

        private void Watcher_Error(object sender, ErrorEventArgs e)
        {
            Trace.TraceError(e.GetException().ToString());
        }

        private bool Activate(string applicationPath)
        {
            Trace.TraceInformation("Activating application at '{0}'...", applicationPath);
            if (!File.Exists(applicationPath))
            {
                Trace.TraceError("Invalid application path '{0}'", applicationPath);
                return false;
            }

            var applicationDirectory = Path.GetDirectoryName(applicationPath) ?? Environment.CurrentDirectory;
            var tempApplicationDirectory = Path.Combine(
                _tempBasePath, applicationDirectory.Replace(_basePath, "").TrimStart('\\', '/'));

            Trace.TraceInformation("Copying files from path '{0}' to '{1}'...", 
                applicationDirectory, tempApplicationDirectory);

            RecreateDirectory(tempApplicationDirectory);
            CopyDirectory(applicationDirectory, tempApplicationDirectory);                        

            var tempApplicationPath = Path.Combine(
                tempApplicationDirectory, 
                Path.GetFileName(applicationPath) ?? Bootstrapper.DefaultApplicationFileName);

            Trace.TraceInformation("Starting the process for application '{0}'...",
                tempApplicationPath);

            var process = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = false,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    FileName = "mhh.exe",
                    Arguments = tempApplicationPath
                }
            };

            if (!process.Start())
            {
                Trace.TraceError("The process failed to start at '{0}'", tempApplicationPath);
                return false;
            }            
            _job.AddProcess(process.Handle);

            if (!_applicationProcessesDictionary.TryAdd(applicationPath, process))
            {
                StopProcess(process);
                return false;
            }

            Trace.TraceInformation("Application started from file '{0}' in the path '{1}'", 
                applicationPath, tempApplicationPath);
            return true;
        }

        private bool Deactivate(string applicationPath)
        {
            Trace.TraceInformation("Deactivating application at '{0}'...", applicationPath);

            Process process;
            if (!_applicationProcessesDictionary.TryRemove(applicationPath, out process))
            {
                Trace.TraceError("No application found at path '{0}'", applicationPath);
                return false;
            }            
            StopProcess(process);

            Trace.TraceInformation("Application stopped from file '{0}'", applicationPath);
            return true;
        }

        private void StopProcess(Process process)
        {
            if (!process.HasExited)
            {
                process.StandardInput.Write(13);             
                process.WaitForExit();
            }
        }

        private static bool IsFileLocked(FileInfo file)
        {
            FileStream stream = null;
            try
            {
                stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                stream?.Close();
            }
            return false;
        }

        private static void RecreateDirectory(string sourcePath)
        {
            if (Directory.Exists(sourcePath)) Directory.Delete(sourcePath, true);
            Directory.CreateDirectory(sourcePath);
        }

        private static void CopyDirectory(string sourcePath, string destinationPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
        }

        const uint WM_KEYDOWN = 0x100;

        private void SendKey(IntPtr windowHandle, ushort k)
        {
            Win32.SendMessage(windowHandle, WM_KEYDOWN, ((IntPtr)k), (IntPtr)0);
        }

        public void Dispose()
        {
            foreach (var applicationPath in _applicationProcessesDictionary.Keys.ToList())
            {
                Deactivate(applicationPath);
            }
            _job.Close();
            _job.Dispose();
        }
    }
}
