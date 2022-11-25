using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dumplings.Helpers
{
    public static class IoHelpers
    {
        public static void EnsureContainingDirectoryExists(string fileNameOrPath)
        {
            string fullPath = Path.GetFullPath(fileNameOrPath); // No matter if relative or absolute path is given to this.
            string dir = Path.GetDirectoryName(fullPath);
            EnsureDirectoryExists(dir);
        }

        /// <summary>
        /// It's like Directory.CreateDirectory, but does not fail when root is given.
        /// </summary>
        public static void EnsureDirectoryExists(string dir)
        {
            if (!string.IsNullOrWhiteSpace(dir)) // If root is given, then do not worry.
            {
                Directory.CreateDirectory(dir); // It does not fail if it exists.
            }
        }

        public static void EnsureFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                EnsureContainingDirectoryExists(filePath);

                File.Create(filePath)?.Dispose();
            }
        }

        public static void OpenFolderInFileExplorer(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                using Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                        ? "explorer.exe"
                        : RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                            ? "open"
                            : "xdg-open",
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? $"\"{dirPath}\"" : dirPath,
                    CreateNoWindow = true
                });
            }
        }

        public static void OpenBrowser(string url)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // If no associated application/json MimeType is found xdg-open opens retrun error
                // but it tries to open it anyway using the console editor (nano, vim, other..)
                EnvironmentHelpers.ShellExec($"xdg-open {url}", waitForExit: false);
            }
            else
            {
                using Process process = Process.Start(new ProcessStartInfo
                {
                    FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                    Arguments = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? $"-e {url}" : "",
                    CreateNoWindow = true,
                    UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                });
            }
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
            {
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name));
            }

            foreach (FileInfo file in source.GetFiles())
            {
                file.CopyTo(Path.Combine(target.FullName, file.Name));
            }
        }
    }
}
