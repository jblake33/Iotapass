using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// Two dependencies for copying
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Iotapass.Services
{
    // Another supporting class for copying
    internal static class OperatingSystem
    {
        internal static bool IsWindows() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        internal static bool IsMacOS() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        internal static bool IsLinux() =>
            RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
    }
    // Supporting class for copying
    internal static class Shell
    {
        internal static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            string result = Run("/bin/bash", $"-c \"{escapedArgs}\"");
            return result;
        }

        internal static string Bat(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            string result = Run("cmd.exe", $"/c \"{escapedArgs}\"");
            return result;
        }

        private static string Run(string filename, string arguments)
        {
            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = filename,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = false,
                }
            };
            process.Start();
            string result = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return result;
        }
    }
    // Copy to clipboard using Copy.
    internal static class Clipboard
    {
        /// <summary>
        /// Copies the string value to the user's clipboard.
        /// </summary>
        /// <param name="val"></param>
        internal static void Copy(string val)
        {
            if (OperatingSystem.IsWindows())
            {
                // Instead of a command in powershell, this uses a Windows API
                System.Windows.Clipboard.SetText(val);
                //$"echo {val} | clip".Bat();
            }

            if (OperatingSystem.IsMacOS())
            {
                $"echo \"{val}\" | pbcopy".Bash();
            }
            
        }
    }
}
