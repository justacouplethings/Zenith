using System;
using System.Diagnostics;
using System.Text;

namespace ZenithOptimizer.Services
{
    public static class ProcessHelper
    {
        public static (int ExitCode, string Output, string Error) RunCommand(string fileName, string arguments, int timeoutMs = 30000)
        {
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8
                };

                using var process = new Process { StartInfo = psi };
                process.OutputDataReceived += (s, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
                process.ErrorDataReceived += (s, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                if (!process.WaitForExit(timeoutMs))
                {
                    try { process.Kill(); } catch { }
                    return (-1, outputBuilder.ToString(), "Process timed out.");
                }

                return (process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
            }
            catch (Exception ex)
            {
                return (-1, string.Empty, ex.Message);
            }
        }

        public static (int ExitCode, string Output, string Error) RunPowerShell(string script, int timeoutMs = 45000)
        {
            // Escape double quotes in script for execution
            string encodedCommand = Convert.ToBase64String(Encoding.Unicode.GetBytes(script));
            return RunCommand("powershell.exe", $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -EncodedCommand {encodedCommand}", timeoutMs);
        }

        public static void RestartExplorer()
        {
            try
            {
                var processes = Process.GetProcessesByName("explorer");
                foreach (var p in processes)
                {
                    try { p.Kill(); } catch { }
                }
                // Windows will automatically restart explorer, but if not, launch it
                System.Threading.Thread.Sleep(500);
                if (Process.GetProcessesByName("explorer").Length == 0)
                {
                    Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
                }
            }
            catch { }
        }
    }
}
