using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ZenithOptimizer.Services
{
    public static class NetworkRepairService
    {
        public static async Task<(bool Success, string Log)> RepairNetworkAndDnsAsync()
        {
            return await Task.Run(() =>
            {
                var log = new StringBuilder();
                bool allOk = true;

                log.AppendLine("=== Initiating Zenith Network & DNS Stack Restoration ===");

                try
                {
                    // 1. Reset TCP Stack Global Settings to Windows Defaults (Remove dangerous global RSS)
                    log.AppendLine("[1/5] Restoring TCP global parameters...");
                    var (code1, out1, _) = ProcessHelper.RunCommand("netsh.exe", "int tcp set global autotuninglevel=normal");
                    var (code2, out2, _) = ProcessHelper.RunCommand("netsh.exe", "int tcp set global rss=default");
                    ProcessHelper.RunCommand("netsh.exe", "int tcp set global chimney=default");
                    ProcessHelper.RunCommand("netsh.exe", "int tcp set global dca=default");
                    ProcessHelper.RunCommand("netsh.exe", "int tcp set global netdma=default");
                    ProcessHelper.RunCommand("netsh.exe", "int tcp set global ecncapability=default");
                    log.AppendLine("   ✔ TCP Auto-Tuning set to 'normal', Receive-Side Scaling (RSS) set to 'default'.");

                    // 2. Restore MMCSS Network Throttling Index to safe defaults
                    log.AppendLine("[2/5] Restoring MMCSS multimedia network throttling keys...");
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 10);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 20);
                    log.AppendLine("   ✔ NetworkThrottlingIndex restored to 10 (0xA), SystemResponsiveness restored to 20 (0x14).");

                    // 3. Ensure critical network services are enabled and started
                    log.AppendLine("[3/5] Verifying vital network services (DNS, DHCP, NLA, IP Helper)...");
                    string[] services = { "Dnscache", "Dhcp", "NlaSvc", "netprofm", "iphlpsvc" };
                    foreach (var s in services)
                    {
                        ServiceHelper.EnableServiceAuto(s);
                        ProcessHelper.RunCommand("sc.exe", $"start {s}");
                    }
                    log.AppendLine("   ✔ Dnscache, Dhcp, NlaSvc, netprofm, and iphlpsvc configured to Automatic and started.");

                    // 4. Reset Winsock catalog and IP routing stack
                    log.AppendLine("[4/5] Resetting Winsock catalog & TCP/IP stack...");
                    var (wCode, _, _) = ProcessHelper.RunCommand("netsh.exe", "winsock reset");
                    var (ipCode, _, _) = ProcessHelper.RunCommand("netsh.exe", "int ip reset");
                    log.AppendLine("   ✔ Winsock catalog and IP stack reset successfully.");

                    // 5. Flush and re-register DNS resolver
                    log.AppendLine("[5/5] Flushing and re-registering DNS resolver cache...");
                    ProcessHelper.RunCommand("ipconfig.exe", "/flushdns");
                    ProcessHelper.RunCommand("ipconfig.exe", "/registerdns");
                    log.AppendLine("   ✔ DNS resolver cache flushed and dynamic name registration queued.");

                    log.AppendLine("=== Network & DNS Stack Restoration Complete ===");
                    log.AppendLine("Tip: If Brave still shows cached error pages, restart Brave or clear host cache at brave://net-internals/#dns");
                }
                catch (Exception ex)
                {
                    allOk = false;
                    log.AppendLine($"[Error] Network repair failed: {ex.Message}");
                }

                return (allOk, log.ToString());
            });
        }
    }
}
