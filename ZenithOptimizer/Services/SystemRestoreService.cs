using System;
using System.Threading.Tasks;

namespace ZenithOptimizer.Services
{
    public static class SystemRestoreService
    {
        public static async Task<(bool Success, string Message)> CreateRestorePointAsync(string description = "Zenith Optimizer Pre-Tweak Restore Point")
        {
            return await Task.Run(() =>
            {
                try
                {
                    // Ensure System Restore is enabled on C: drive
                    string enableScript = "Enable-ComputerRestore -Drive 'C:\\' -ErrorAction SilentlyContinue;";
                    ProcessHelper.RunPowerShell(enableScript, 10000);

                    // Check if a restore point was recently created (Windows has a 24-hr frequency limit by default in Registry SystemRestorePointCreationFrequency)
                    // We temporarily set SystemRestorePointCreationFrequency to 0 to permit immediate point creation
                    RegistryHelper.SetDword(Microsoft.Win32.RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SystemRestore", "SystemRestorePointCreationFrequency", 0);

                    string script = $"Checkpoint-Computer -Description '{description}' -RestorePointType 'MODIFY_SETTINGS' -ErrorAction Stop";
                    var (code, output, error) = ProcessHelper.RunPowerShell(script, 45000);

                    if (code == 0)
                    {
                        return (true, "System Restore Point created successfully.");
                    }
                    else
                    {
                        // Fallback using WMI if Checkpoint-Computer failed
                        string wmiScript = $"([wmiclass]'\\\\localhost\\root\\default:systemrestore').CreateRestorePoint('{description}', 0, 100)";
                        var (wmiCode, wmiOut, wmiErr) = ProcessHelper.RunPowerShell(wmiScript, 45000);
                        if (wmiCode == 0 && (wmiOut.Contains("ReturnValue = 0") || !wmiErr.Contains("Error")))
                        {
                            return (true, "System Restore Point created successfully via WMI.");
                        }

                        return (false, string.IsNullOrWhiteSpace(error) ? (string.IsNullOrWhiteSpace(wmiErr) ? "Unable to create restore point. Ensure System Protection is enabled on drive C:." : wmiErr) : error);
                    }
                }
                catch (Exception ex)
                {
                    return (false, $"Error creating restore point: {ex.Message}");
                }
            });
        }
    }
}
