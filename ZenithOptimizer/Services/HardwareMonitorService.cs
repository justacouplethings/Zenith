using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using ZenithOptimizer.Models;

namespace ZenithOptimizer.Services
{
    public class HardwareMonitorService : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private class MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;

            public MEMORYSTATUSEX()
            {
                dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SYSTEM_POWER_STATUS
        {
            public byte ACLineStatus;
            public byte BatteryFlag;
            public byte BatteryLifePercent;
            public byte SystemStatusFlag;
            public int BatteryLifeTime;
            public int BatteryFullLifeTime;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalMemoryStatusEx([In, Out] MEMORYSTATUSEX lpBuffer);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetSystemTimes(out long lpIdleTime, out long lpKernelTime, out long lpUserTime);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetSystemPowerStatus(out SYSTEM_POWER_STATUS lpSystemPowerStatus);

        private readonly SystemMetrics _metrics;
        private readonly Timer _timer;
        private long _prevIdleTime;
        private long _prevKernelTime;
        private long _prevUserTime;
        private bool _isFirstCpuSample = true;

        public HardwareMonitorService(SystemMetrics metrics)
        {
            _metrics = metrics;
            DetectHardwareBaseline();
            _timer = new Timer(UpdateMetrics, null, 0, 1000);
        }

        private void DetectHardwareBaseline()
        {
            try
            {
                // 1. Detect OS
                var os = Environment.OSVersion;
                int build = os.Version.Build;
                _metrics.OsBuildNumber = build;
                _metrics.OsBuild = $"Build {build}";
                _metrics.OsName = build >= 22000 ? "Windows 11" : "Windows 10";

                // 2. CPU Cores
                _metrics.CpuCores = Environment.ProcessorCount;

                // 3. Power status / Laptop Detection
                if (GetSystemPowerStatus(out var powerStatus))
                {
                    bool hasBattery = powerStatus.BatteryFlag != 128 && powerStatus.BatteryFlag != 255;
                    _metrics.IsLaptop = hasBattery;
                    _metrics.IsUserLaptopMode = hasBattery; // Set default profile based on detection
                    _metrics.IsCharging = powerStatus.ACLineStatus == 1;
                    _metrics.BatteryPercent = powerStatus.BatteryLifePercent <= 100 ? powerStatus.BatteryLifePercent : 100;
                }

                // 4. Initial RAM
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    double totalGb = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    _metrics.RamTotalGb = Math.Round(totalGb, 1);
                }

                // 5. Drive Type
                _metrics.IsSsd = true;
                _metrics.DiskType = "SSD Storage";
            }
            catch { }
        }

        private void UpdateMetrics(object? state)
        {
            try
            {
                // CPU
                if (GetSystemTimes(out long idleTime, out long kernelTime, out long userTime))
                {
                    if (!_isFirstCpuSample)
                    {
                        long usr = userTime - _prevUserTime;
                        long ker = kernelTime - _prevKernelTime;
                        long idl = idleTime - _prevIdleTime;
                        long sys = ker + usr;

                        if (sys > 0)
                        {
                            float cpu = (float)((sys - idl) * 100.0 / sys);
                            _metrics.CpuUsage = Math.Clamp((float)Math.Round(cpu, 1), 0f, 100f);
                        }
                    }
                    _prevIdleTime = idleTime;
                    _prevKernelTime = kernelTime;
                    _prevUserTime = userTime;
                    _isFirstCpuSample = false;
                }

                // RAM
                var memStatus = new MEMORYSTATUSEX();
                if (GlobalMemoryStatusEx(memStatus))
                {
                    double totalGb = memStatus.ullTotalPhys / (1024.0 * 1024.0 * 1024.0);
                    double availGb = memStatus.ullAvailPhys / (1024.0 * 1024.0 * 1024.0);
                    double usedGb = totalGb - availGb;

                    _metrics.RamTotalGb = Math.Round(totalGb, 1);
                    _metrics.RamUsedGb = Math.Round(usedGb, 1);
                    _metrics.RamUsagePercent = (float)Math.Round((usedGb / totalGb) * 100.0, 1);
                }

                // Process Count
                _metrics.ProcessCount = Process.GetProcesses().Length;

                // Uptime
                var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
                if (uptime.TotalDays >= 1)
                {
                    _metrics.UptimeString = $"{(int)uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m";
                }
                else
                {
                    _metrics.UptimeString = $"{uptime.Hours:D2}h {uptime.Minutes:D2}m {uptime.Seconds:D2}s";
                }

                // Power Status
                if (GetSystemPowerStatus(out var powerStatus))
                {
                    bool hasBattery = powerStatus.BatteryFlag != 128 && powerStatus.BatteryFlag != 255;
                    _metrics.IsLaptop = hasBattery;
                    _metrics.IsCharging = powerStatus.ACLineStatus == 1;
                    _metrics.BatteryPercent = powerStatus.BatteryLifePercent <= 100 ? powerStatus.BatteryLifePercent : 100;
                }

                // Disk Space
                try
                {
                    var systemDrive = new DriveInfo(Path.GetPathRoot(Environment.SystemDirectory) ?? "C:\\");
                    if (systemDrive.IsReady)
                    {
                        double totalSpace = systemDrive.TotalSize;
                        double freeSpace = systemDrive.TotalFreeSpace;
                        double usedPercent = ((totalSpace - freeSpace) / totalSpace) * 100.0;
                        _metrics.DiskUsagePercent = (float)Math.Round(usedPercent, 1);
                    }
                }
                catch { }
            }
            catch { }
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
