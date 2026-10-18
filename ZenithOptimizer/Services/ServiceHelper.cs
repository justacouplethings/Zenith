using System;
using System.Diagnostics;
using Microsoft.Win32;

namespace ZenithOptimizer.Services
{
    public static class ServiceHelper
    {
        public static bool DoesServiceExist(string serviceName)
        {
            return RegistryHelper.KeyExists(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Services\{serviceName}");
        }

        public static int? GetServiceStartType(string serviceName)
        {
            // Windows Service Start Values:
            // 2 = Automatic
            // 3 = Manual (Demand)
            // 4 = Disabled
            return RegistryHelper.GetDword(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Services\{serviceName}", "Start");
        }

        public static bool IsServiceDisabled(string serviceName)
        {
            var start = GetServiceStartType(serviceName);
            return start == 4;
        }

        public static bool IsServiceManualOrDisabled(string serviceName)
        {
            var start = GetServiceStartType(serviceName);
            return start == 3 || start == 4;
        }

        public static bool SetServiceStartType(string serviceName, string startType, bool stopService = true)
        {
            try
            {
                // startType can be: "disabled", "demand", "auto"
                var result = ProcessHelper.RunCommand("sc.exe", $"config \"{serviceName}\" start= {startType}");
                if (stopService)
                {
                    ProcessHelper.RunCommand("sc.exe", $"stop \"{serviceName}\"");
                }

                // Double check / enforce via registry directly
                int dwordVal = startType switch
                {
                    "disabled" => 4,
                    "demand" => 3,
                    "auto" => 2,
                    _ => 3
                };
                RegistryHelper.SetDword(RegistryHive.LocalMachine, $@"SYSTEM\CurrentControlSet\Services\{serviceName}", "Start", dwordVal);

                return result.ExitCode == 0 || IsServiceDisabled(serviceName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ServiceHelper] Failed to set service {serviceName} to {startType}: {ex.Message}");
                return false;
            }
        }

        public static bool DisableService(string serviceName)
        {
            return SetServiceStartType(serviceName, "disabled", true);
        }

        public static bool SetServiceManual(string serviceName)
        {
            return SetServiceStartType(serviceName, "demand", false);
        }

        public static bool EnableServiceAuto(string serviceName)
        {
            return SetServiceStartType(serviceName, "auto", false);
        }
    }
}
