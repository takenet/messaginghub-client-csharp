using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Takenet.MessagingHub.Client.Host
{
    public sealed class ApplicationActivator : IDisposable
    {
        public const string DefaultApplicationFileName = "application.json";
        public const string DefaultHostFileName = "mhh.exe";

        private static readonly TimeSpan PathMutexTimeout = TimeSpan.FromSeconds(30);

        private readonly string _basePath;
        private readonly TimeSpan _waitForActivationDelay;
        private readonly FileSystemWatcher _watcher;
        private readonly ConcurrentDictionary<string, Process> _applicationProcessesDictionary;
        private readonly ConcurrentDictionary<string, DateTime> _applicationLastWriteDictionary;
        private readonly string _tempBasePath;
        private readonly Job _job;

        public ApplicationActivator(string basePath, TimeSpan waitForActivationDelay)
        {
            _basePath = basePath;
            _waitForActivationDelay = waitForActivationDelay;
            if (basePath == null) throw new ArgumentNullException(nameof(basePath));
            if (!Path.IsPathRooted(basePath)) throw new ArgumentException("The path should be rooted", nameof(basePath));


            _watcher = new FileSystemWatcher()
            {
                Path = basePath,
                Filter = DefaultApplicationFileName,
                IncludeSubdirectories = true,
                InternalBufferSize = 1024 * 64,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName
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

            Parallel.ForEach(
                Directory.GetFiles(_basePath, DefaultApplicationFileName, SearchOption.AllDirectories),
                applicationPath => Activate(applicationPath));
            
            _watcher.EnableRaisingEvents = true;
        }

        private async void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            // The FileWatcher raises the Changed event twice
            if (IsDuplicateEvent(e)) return;

            await Task.Delay(_waitForActivationDelay).ConfigureAwait(false);
            RestartApplication(e.FullPath, e.FullPath);           
        }

        private async void Watcher_Deleted(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(_waitForActivationDelay).ConfigureAwait(false);

            Deactivate(e.FullPath);
            DateTime applicationLastWrite;
            _applicationLastWriteDictionary.TryRemove(e.FullPath, out applicationLastWrite);
        }

        private async void Watcher_Renamed(object sender, RenamedEventArgs e)
        {
            await Task.Delay(_waitForActivationDelay).ConfigureAwait(false);
            RestartApplication(e.OldFullPath, e.FullPath);
        }

        private bool IsDuplicateEvent(FileSystemEventArgs e)
        {
            var applicationLastWrite = File.GetLastWriteTimeUtc(e.FullPath);

            if (!_applicationLastWriteDictionary.TryAdd(e.FullPath, applicationLastWrite))
            {
                DateTime existingApplicationLastWrite;

                if (_applicationLastWriteDictionary.TryGetValue(e.FullPath, out existingApplicationLastWrite) &&
                    existingApplicationLastWrite == applicationLastWrite)
                {
                    Trace.TraceWarning(
                        "The file '{0}' was recently changed at '{1}' and the application will NOT be restarted", e.FullPath,
                        applicationLastWrite);
                    return true;
                }

                _applicationLastWriteDictionary.AddOrUpdate(e.FullPath, applicationLastWrite, (k, v) => applicationLastWrite);
            }
            return false;
        }

        private void RestartApplication(string oldPath, string newPath)
        {
            Trace.TraceInformation("The file '{0}' was changed  and the application will be restarted with path '{1}'", oldPath, newPath);

            Deactivate(oldPath);
            if (new FileInfo(newPath).Name == DefaultApplicationFileName)
            {
                if (!Activate(newPath))
                {
                    Trace.TraceError("Could not activate the application on path '{0}'", newPath);
                }
            }
            else
            {
                Trace.TraceError("Invalid application file name on path '{0}'", newPath);
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
                Path.GetFileName(applicationPath) ?? DefaultApplicationFileName);

            Trace.TraceInformation("Starting the process for application '{0}'...",
                tempApplicationPath);

            var hostPath = Path.Combine(tempApplicationDirectory, DefaultHostFileName);
            if (!File.Exists(hostPath))
            {
                hostPath = DefaultHostFileName;
            }

            var process = new Process
            {
                StartInfo =
                {
                    CreateNoWindow = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    FileName = hostPath,
                    Arguments = tempApplicationPath
                }
            };

            var outputPath = Path.Combine(applicationDirectory, "Output.log");
            process.OutputDataReceived += (sender, e) =>
            {
                try
                {
                    using (var writer = File.AppendText(outputPath))
                    {
                        writer.WriteLine("{0} - {1}", DateTime.UtcNow, e.Data);
                    }
                }
                catch (Exception exception)
                {
                    Trace.TraceError($"Could not write to {outputPath}\nData: {e.Data}\nException: {exception}");
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                try
                {
                    using (var writer = File.AppendText(outputPath))
                    {
                        writer.WriteLine("{0} - ERROR: {1}", DateTime.UtcNow, e.Data);
                    }
                }
                catch (Exception exception)
                {
                    Trace.TraceError($"Could not write to {outputPath}\nError data: {e.Data}\nException: {exception}");
                }
            };

            if (!process.Start())
            {
                Trace.TraceError("The process failed to start at '{0}'", tempApplicationPath);
                return false;
            }

            process.BeginOutputReadLine();

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

        public void Dispose()
        {
            foreach (var applicationPath in _applicationProcessesDictionary.Keys.ToList())
            {
                Deactivate(applicationPath);
            }
            _job.Close();
            _job.Dispose();
            Directory.Delete(_tempBasePath, true);
        }
    }
}
