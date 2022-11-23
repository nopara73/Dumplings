using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Dumplings.Helpers
{
    public static class EnvironmentHelpers
    {
        private const int ProcessorCountRefreshIntervalMs = 30000;

        private static volatile int InternalProcessorCount;
        private static volatile int LastProcessorCountRefreshTicks;

        /// <summary>
        /// https://github.com/i3arnon/ConcurrentHashSet/blob/master/src/ConcurrentHashSet/PlatformHelper.cs
        /// </summary>
        internal static int ProcessorCount
        {
            get
            {
                var now = Environment.TickCount;
                if (InternalProcessorCount == 0 || now - LastProcessorCountRefreshTicks >= ProcessorCountRefreshIntervalMs)
                {
                    InternalProcessorCount = Environment.ProcessorCount;
                    LastProcessorCountRefreshTicks = now;
                }

                return InternalProcessorCount;
            }
        }

        // appName, dataDir
        private static ConcurrentDictionary<string, string> DataDirDict { get; } = new ConcurrentDictionary<string, string>();

        // Do not change the output of this function. Backwards compatibility depends on it.
        public static string GetDataDir(string appName)
        {
            if (DataDirDict.TryGetValue(appName, out string dataDir))
            {
                return dataDir;
            }

            string directory;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var home = Environment.GetEnvironmentVariable("HOME");
                if (!string.IsNullOrEmpty(home))
                {
                    directory = Path.Combine(home, "." + appName.ToLowerInvariant());
                    Logger.LogInfo($"Using HOME environment variable for initializing application data at `{directory}`.");
                }
                else
                {
                    throw new DirectoryNotFoundException("Could not find suitable datadir.");
                }
            }
            else
            {
                var localAppData = Environment.GetEnvironmentVariable("APPDATA");
                if (!string.IsNullOrEmpty(localAppData))
                {
                    directory = Path.Combine(localAppData, appName);
                    Logger.LogInfo($"Using APPDATA environment variable for initializing application data at `{directory}`.");
                }
                else
                {
                    throw new DirectoryNotFoundException("Could not find suitable datadir.");
                }
            }

            if (Directory.Exists(directory))
            {
                DataDirDict.TryAdd(appName, directory);
                return directory;
            }

            Logger.LogInfo($"Creating data directory at `{directory}`.");
            Directory.CreateDirectory(directory);

            DataDirDict.TryAdd(appName, directory);
            return directory;
        }

        // This method removes the path and file extension.
        //
        // Given Wasabi releases are currently built using Windows, the generated assemblies contain
        // the hardcoded "C:\Users\User\Desktop\WalletWasabi\.......\FileName.cs" string because that
        // is the real path of the file, it doesn't matter what OS was targeted.
        // In Windows and Linux that string is a valid path and that means Path.GetFileNameWithoutExtension
        // can extract the file name but in the case of OSX the same string is not a valid path so, it assumes
        // the whole string is the file name.
        public static string ExtractFileName(string callerFilePath)
        {
            var lastSeparatorIndex = callerFilePath.LastIndexOf("\\");
            if (lastSeparatorIndex == -1)
            {
                lastSeparatorIndex = callerFilePath.LastIndexOf("/");
            }

            lastSeparatorIndex++;
            var fileNameWithoutExtension = callerFilePath.Substring(lastSeparatorIndex, callerFilePath.Length - lastSeparatorIndex - ".cs".Length);
            return fileNameWithoutExtension;
        }

        /// <summary>
        /// Executes a command with bash.
        /// https://stackoverflow.com/a/47918132/2061103
        /// </summary>
        /// <param name="cmd"></param>
        public static void ShellExec(string cmd, bool waitForExit = true)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            using var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = "/bin/sh",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                }
            );
            if (waitForExit)
            {
                process.WaitForExit();
                if (process.ExitCode != 0)
                {
                    Logger.LogError($"{nameof(ShellExec)} command: {cmd} exited with exit code: {process.ExitCode}, instead of 0.");
                }
            }
        }

        /// <summary>
        /// Gets the name of the current method.
        /// </summary>
        public static string GetMethodName([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

        public static string GetFullBaseDirectory()
        {
            var fullBaseDirectory = Path.GetFullPath(AppContext.BaseDirectory);

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (!fullBaseDirectory.StartsWith('/'))
                {
                    fullBaseDirectory = fullBaseDirectory.Insert(0, "/");
                }
            }

            return fullBaseDirectory;
        }

        /// <summary>
        /// Accessibility of binaries must follow the convention: {binariesContainingFolderRelativePath}/Binaries/{binaryNameWithoutExtension}-{os}64/binaryNameWithoutExtension(.exe)
        /// </summary>
        public static string GetBinaryPath(string binariesContainingFolderRelativePath, string binaryNameWithoutExtension)
        {
            var fullBaseDirectory = GetFullBaseDirectory();

            string commonPartialPath = Path.Combine(fullBaseDirectory, binariesContainingFolderRelativePath, "Binaries");
            string path;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                path = Path.Combine(commonPartialPath, $"{binaryNameWithoutExtension}-win64", $"{binaryNameWithoutExtension}.exe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                path = Path.Combine(commonPartialPath, $"{binaryNameWithoutExtension}-lin64", binaryNameWithoutExtension);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                path = Path.Combine(commonPartialPath, $"{binaryNameWithoutExtension}-osx64", binaryNameWithoutExtension);
            }
            else
            {
                throw new NotSupportedException("Operating system is not supported.");
            }

            return path;
        }
    }
}
