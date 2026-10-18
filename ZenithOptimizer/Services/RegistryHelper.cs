using System;
using Microsoft.Win32;

namespace ZenithOptimizer.Services
{
    public static class RegistryHelper
    {
        public static bool SetDword(RegistryHive hive, string subKey, string valueName, int value)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using var key = baseKey.CreateSubKey(subKey, true);
                if (key != null)
                {
                    key.SetValue(valueName, value, RegistryValueKind.DWord);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RegistryHelper] Error setting DWORD {subKey}\\{valueName}: {ex.Message}");
            }
            return false;
        }

        public static int? GetDword(RegistryHive hive, string subKey, string valueName)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using var key = baseKey.OpenSubKey(subKey, false);
                if (key != null)
                {
                    var val = key.GetValue(valueName);
                    if (val is int intVal) return intVal;
                    if (val != null && int.TryParse(val.ToString(), out var parsed)) return parsed;
                }
            }
            catch
            {
                // Key may not exist
            }
            return null;
        }

        public static bool SetString(RegistryHive hive, string subKey, string valueName, string value)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using var key = baseKey.CreateSubKey(subKey, true);
                if (key != null)
                {
                    key.SetValue(valueName, value, RegistryValueKind.String);
                    return true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RegistryHelper] Error setting String {subKey}\\{valueName}: {ex.Message}");
            }
            return false;
        }

        public static string? GetString(RegistryHive hive, string subKey, string valueName)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using var key = baseKey.OpenSubKey(subKey, false);
                if (key != null)
                {
                    return key.GetValue(valueName)?.ToString();
                }
            }
            catch
            {
                // Key may not exist
            }
            return null;
        }

        public static bool DeleteValue(RegistryHive hive, string subKey, string valueName)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using var key = baseKey.OpenSubKey(subKey, true);
                if (key != null)
                {
                    key.DeleteValue(valueName, false);
                    return true;
                }
            }
            catch
            {
                // Ignored
            }
            return false;
        }

        public static bool DeleteSubKeyTree(RegistryHive hive, string subKey)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                baseKey.DeleteSubKeyTree(subKey, false);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool KeyExists(RegistryHive hive, string subKey)
        {
            try
            {
                using var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Default);
                using var key = baseKey.OpenSubKey(subKey, false);
                return key != null;
            }
            catch
            {
                return false;
            }
        }
    }
}
