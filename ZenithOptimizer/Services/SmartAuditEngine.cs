using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZenithOptimizer.Models;

namespace ZenithOptimizer.Services
{
    public static class SmartAuditEngine
    {
        public static (string Summary, List<string> RecommendedIds) AnalyzeHardware(SystemMetrics metrics, IEnumerable<OptimizationItem> allTweaks)
        {
            var summary = new StringBuilder();
            var recommended = new List<string>();

            bool isWin11 = metrics.IsWindows11;
            bool isLaptop = metrics.IsUserLaptopMode;
            double ram = metrics.RamTotalGb;

            summary.AppendLine($"[HARDWARE AUDIT REPORT]");
            summary.AppendLine($"• OS Environment: {metrics.OsName} ({metrics.OsBuild})");
            summary.AppendLine($"• Device Profile: {(isLaptop ? "Laptop (Battery Sensitive)" : "Desktop (Maximum Sustained Throughput)")}");
            summary.AppendLine($"• CPU Architecture: {metrics.CpuCores} Logical Cores");
            summary.AppendLine($"• Memory Capacity: {ram:F1} GB Installed RAM");
            summary.AppendLine($"• Storage Medium: {metrics.DiskType}");
            summary.AppendLine();
            summary.AppendLine($"[ALGORITHMIC RECOMMENDATIONS]");

            // 1. Baseline Safe Privacy & Telemetry
            recommended.Add("uni_disable_telemetry");
            recommended.Add("uni_disable_error_reporting");
            recommended.Add("uni_disable_location");
            recommended.Add("uni_disable_advertising_id");
            recommended.Add("uni_disable_delivery_optimization");
            recommended.Add("uni_disable_consumer_bloat");
            recommended.Add("uni_disable_bing_search");
            recommended.Add("uni_optimize_visual_effects");
            recommended.Add("uni_enable_end_task");
            recommended.Add("uni_storage_sense");
            recommended.Add("uni_set_services_manual");
            summary.AppendLine("✔ Base Privacy & Performance: Stripped diagnostic telemetry, CEIP, advertising ID, and background delay.");

            // 2. Storage & Hibernation
            recommended.Add("uni_disable_hibernation");
            recommended.Add("uni_disable_sysmain");
            summary.AppendLine($"✔ High-Speed SSD Profile: Hibernation disabled (reclaims ~{Math.Max(8, (int)ram)} GB SSD capacity); SysMain disabled for NVMe longevity.");

            // 3. OS-Specific Recommendations
            if (isWin11)
            {
                recommended.Add("w11_classic_context_menu");
                recommended.Add("w11_disable_copilot");
                recommended.Add("w11_disable_recall");
                recommended.Add("w11_disable_widgets");
                recommended.Add("w11_disable_teams_chat");
                recommended.Add("w11_disable_start_recommendations");
                recommended.Add("w11_disable_edge_prelaunch");
                summary.AppendLine("✔ Windows 11 Profile: Classic context menu restored; Copilot, Recall AI, and MSN Taskbar feeds disabled.");
            }
            else
            {
                recommended.Add("w10_disable_news_interests");
                recommended.Add("w10_disable_timeline");
                recommended.Add("w10_remove_3d_objects");
                recommended.Add("w10_disable_cortana");
                recommended.Add("w10_disable_lockscreen_ads");
                summary.AppendLine("✔ Windows 10 Profile: News & Interests feed disabled; Timeline cloud logging stopped; 3D Objects namespace purged.");
            }

            // 4. Device Type: Laptop vs Desktop
            if (isLaptop)
            {
                recommended.Add("laptop_wifi_power_save");
                recommended.Add("laptop_modern_standby_drain");
                recommended.Add("laptop_battery_telemetry_reduction");
                recommended.Add("laptop_balanced_power_profile");
                summary.AppendLine("✔ Laptop Power Shield: Modern Standby sleep drain patched; WiFi power droop prevented; energy-efficient CPU throttling active.");
                summary.AppendLine("⚠ Skipped Ultimate ACPI Scheme: Power plan capped to Balanced to prevent thermal throttling and battery degradation.");
            }
            else
            {
                recommended.Add("uni_disable_mouse_acceleration");
                recommended.Add("uni_tcp_tuning");
                recommended.Add("uni_disable_gamedvr");
                if (isWin11) recommended.Add("w11_vbs_hypervisor");
                summary.AppendLine("✔ Desktop Gaming & Throughput: 1:1 Raw mouse tracking enforced; TCP auto-tuning optimized; GameDVR latency killed.");
            }

            // 5. Memory Capacity Check
            if (ram >= 15.5)
            {
                recommended.Add("uni_disable_paging_executive");
                summary.AppendLine("✔ High-Memory Architecture (>= 16GB): Paging Executive disabled; OS kernel locked resident in physical RAM.");
            }
            else
            {
                summary.AppendLine($"ℹ Memory Capacity ({ram:F1} GB): Kernel paging preserved to safeguard system multitasking headroom.");
            }

            return (summary.ToString(), recommended);
        }
    }
}
