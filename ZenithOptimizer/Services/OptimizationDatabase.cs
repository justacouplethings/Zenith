using System;
using System.Collections.Generic;
using Microsoft.Win32;
using ZenithOptimizer.Models;

namespace ZenithOptimizer.Services
{
    public static class OptimizationDatabase
    {
        public static List<OptimizationItem> GetAllOptimizations()
        {
            var list = new List<OptimizationItem>();

            #region Windows 11 Dedicated Tweaks
            list.Add(new OptimizationItem
            {
                Id = "w11_classic_context_menu",
                Title = "Classic Right-Click Context Menu",
                Category = OptimizationCategory.InterfaceErgonomics,
                TargetOs = OsTarget.Windows11,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Restores the instant full classic right-click menu, eliminating the Windows 11 'Show more options' delay.",
                Tradeoff = "Replaces the modern rounded right-click menu with the classic instant Windows 10 menu.",
                RequiresRestart = true,
                CheckAction = () => RegistryHelper.KeyExists(RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"),
                ApplyAction = () =>
                {
                    bool ok = RegistryHelper.SetString(RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32", "", "");
                    ProcessHelper.RestartExplorer();
                    return ok;
                },
                RevertAction = () =>
                {
                    bool ok = RegistryHelper.DeleteSubKeyTree(RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}");
                    ProcessHelper.RestartExplorer();
                    return ok;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_disable_copilot",
                Title = "Disable Windows Copilot AI",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Windows11,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Completely disables the Windows Copilot AI integration and removes the sidebar icon.",
                Tradeoff = "Disables Microsoft Copilot assistant across the operating system.",
                RequiresRestart = true,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot") == 1,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 0);
                    ProcessHelper.RestartExplorer();
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot");
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot");
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 1);
                    ProcessHelper.RestartExplorer();
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_disable_recall",
                Title = "Disable Windows Recall & AI Snapshots",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Windows11,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables Windows Recall AI background screen analysis, continuous snapshots, and telemetry.",
                Tradeoff = "Blocks continuous activity snapshots. Protects privacy and saves disk/CPU overhead.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows AI", "DisableAIDataAnalysis") == 1,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows AI", "DisableAIDataAnalysis", 1);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Windows AI", "DisableAIDataAnalysis", 1);
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows AI", "DisableAIDataAnalysis");
                    RegistryHelper.DeleteValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Windows AI", "DisableAIDataAnalysis");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_disable_widgets",
                Title = "Disable Taskbar Widgets Feed",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Windows11,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Removes the MSN News and Weather widget from the taskbar, saving background memory and network bandwidth.",
                Tradeoff = "Disables the taskbar widgets panel.",
                RequiresRestart = true,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests", 0);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 0);
                    ProcessHelper.RestartExplorer();
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests");
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarDa", 1);
                    ProcessHelper.RestartExplorer();
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_taskbar_align_left",
                Title = "Align Taskbar Icons to Left",
                Category = OptimizationCategory.InterfaceErgonomics,
                TargetOs = OsTarget.Windows11,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Positions Start button and taskbar icons on the left side, matching traditional Windows ergonomics.",
                Tradeoff = "Changes taskbar alignment from Center to Left.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl") == 0,
                ApplyAction = () => RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 0),
                RevertAction = () => RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 1)
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_disable_teams_chat",
                Title = "Remove Teams Chat Icon from Taskbar",
                Category = OptimizationCategory.InterfaceErgonomics,
                TargetOs = OsTarget.Windows11,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Removes the integrated consumer Microsoft Teams Chat icon from the Windows 11 taskbar.",
                Tradeoff = "Hides the built-in personal Teams Chat icon.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn") == 0,
                ApplyAction = () => RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0),
                RevertAction = () => RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 1)
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_disable_start_recommendations",
                Title = "Disable Start Menu Recommended Feed",
                Category = OptimizationCategory.InterfaceErgonomics,
                TargetOs = OsTarget.Windows11,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Cleans the Start Menu by hiding recent files, recommended apps, and sponsored tips.",
                Tradeoff = "The 'Recommended' section in the Start Menu will be hidden.",
                RequiresRestart = true,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 0);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection", 1);
                    ProcessHelper.RestartExplorer();
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations", 1);
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Explorer", "HideRecommendedSection");
                    ProcessHelper.RestartExplorer();
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_disable_edge_prelaunch",
                Title = "Disable Microsoft Edge Pre-launch & Background Tabs",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Windows11,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Prevents Microsoft Edge from silently pre-loading background processes at Windows boot.",
                Tradeoff = "Edge will only launch when you explicitly open it.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\Main", "AllowPrelaunch") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\Main", "AllowPrelaunch", 0);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\TabPreloader", "AllowTabPreloading", 0);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "BackgroundModeEnabled", 0);
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\Main", "AllowPrelaunch");
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\TabPreloader", "AllowTabPreloading");
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Edge", "BackgroundModeEnabled");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_vbs_hypervisor",
                Title = "Disable VBS / Hypervisor Launch for Gaming FPS",
                Category = OptimizationCategory.GamingLatency,
                TargetOs = OsTarget.Windows11,
                TargetDevice = DeviceTarget.DesktopOnly,
                Risk = RiskLevel.Caution,
                Description = "Disables the Windows hypervisor layer (VBS/Core Isolation) to reduce frame-time variance and boost gaming FPS.",
                Tradeoff = "50/50: Disables Core Isolation / Memory Integrity virtualization defense against ring-0 rootkits.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    var (code, outStr, _) = ProcessHelper.RunCommand("bcdedit.exe", "/enum {current}");
                    return outStr.Contains("hypervisorlaunchtype") && outStr.Contains("Off");
                },
                ApplyAction = () => ProcessHelper.RunCommand("bcdedit.exe", "/set hypervisorlaunchtype off").ExitCode == 0,
                RevertAction = () => ProcessHelper.RunCommand("bcdedit.exe", "/set hypervisorlaunchtype auto").ExitCode == 0
            });
            #endregion

            #region Windows 10 Dedicated Tweaks
            list.Add(new OptimizationItem
            {
                Id = "w10_disable_news_interests",
                Title = "Disable News & Interests on Taskbar",
                Category = OptimizationCategory.InterfaceErgonomics,
                TargetOs = OsTarget.Windows10,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables the weather widget and MSN news feed popout on the Windows 10 taskbar.",
                Tradeoff = "Weather and MSN headlines will not be displayed on the taskbar.",
                RequiresRestart = true,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode") == 2,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 2);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds", "EnableFeeds", 0);
                    ProcessHelper.RestartExplorer();
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode", 0);
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Feeds", "EnableFeeds");
                    ProcessHelper.RestartExplorer();
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w10_disable_timeline",
                Title = "Disable Activity History & Timeline Sync",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Windows10,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables continuous logging of user activity and prevents syncing document history to Microsoft cloud.",
                Tradeoff = "Task View will show only active windows, not history from past days.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0);
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed");
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities");
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w10_remove_3d_objects",
                Title = "Remove 3D Objects from This PC",
                Category = OptimizationCategory.InterfaceErgonomics,
                TargetOs = OsTarget.Windows10,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Removes the unused '3D Objects' folder from File Explorer's This PC directory tree.",
                Tradeoff = "Hides the 3D Objects shortcut from This PC.",
                RequiresRestart = true,
                CheckAction = () => !RegistryHelper.KeyExists(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}"),
                ApplyAction = () =>
                {
                    RegistryHelper.DeleteSubKeyTree(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                    RegistryHelper.DeleteSubKeyTree(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                    ProcessHelper.RestartExplorer();
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetString(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}", "", "");
                    RegistryHelper.SetString(RegistryHive.LocalMachine, @"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}", "", "");
                    ProcessHelper.RestartExplorer();
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w10_disable_cortana",
                Title = "Disable Cortana Voice Assistant",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Windows10,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables Cortana autostart, background microphone hooks, and voice queries.",
                Tradeoff = "Disables Cortana voice search.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana", 0);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0);
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana");
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 1);
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w10_disable_lockscreen_ads",
                Title = "Disable Lock Screen Spotlight Ads",
                Category = OptimizationCategory.InterfaceErgonomics,
                TargetOs = OsTarget.Windows10,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables lock screen tips, tricks, and sponsored wallpapers placed by Microsoft.",
                Tradeoff = "Lock screen displays static picture without promotional overlays.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 0);
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 1);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 1);
                    return true;
                }
            });
            #endregion

            #region Dedicated Laptop Battery & Power Tweaks
            list.Add(new OptimizationItem
            {
                Id = "laptop_wifi_power_save",
                Title = "Prevent WiFi Adapter Battery Dropouts",
                Category = OptimizationCategory.LaptopBattery,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.LaptopOnly,
                Risk = RiskLevel.Safe,
                Description = "Prevents Windows from aggressively sleeping the WiFi wireless adapter on battery, eliminating ping spikes and sudden disconnects.",
                Tradeoff = "Maintains high wireless throughput even while unplugged.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    // Check power plan wireless adapter settings
                    var (code, outStr, _) = ProcessHelper.RunCommand("powercfg.exe", "/query scheme_current sub_wifi");
                    return outStr.Contains("Current AC Power Setting Index: 0x00000000") && outStr.Contains("Current DC Power Setting Index: 0x00000000");
                },
                ApplyAction = () =>
                {
                    ProcessHelper.RunCommand("powercfg.exe", "/setdcvalueindex scheme_current 19cbb8fa-5279-450e-9fac-8a3d5fedd0c1 12bbebe6-58d6-4636-95bb-3217ef867c1a 0");
                    ProcessHelper.RunCommand("powercfg.exe", "/setactive scheme_current");
                    return true;
                },
                RevertAction = () =>
                {
                    ProcessHelper.RunCommand("powercfg.exe", "/setdcvalueindex scheme_current 19cbb8fa-5279-450e-9fac-8a3d5fedd0c1 12bbebe6-58d6-4636-95bb-3217ef867c1a 2");
                    ProcessHelper.RunCommand("powercfg.exe", "/setactive scheme_current");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "laptop_modern_standby_drain",
                Title = "Fix Modern Standby Sleep Battery Drain",
                Category = OptimizationCategory.LaptopBattery,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.LaptopOnly,
                Risk = RiskLevel.Safe,
                Description = "Prevents Windows Modern Standby (Connected Standby) from keeping network activity alive while the laptop lid is closed.",
                Tradeoff = "Laptop does not download updates or sync mail while asleep in your backpack, preventing overheating and battery drain.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "PlatformAoAcOverride") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "PlatformAoAcOverride", 0);
                    // Disallow wake timers on battery
                    ProcessHelper.RunCommand("powercfg.exe", "/setdcvalueindex scheme_current sub_sleep bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d 0");
                    ProcessHelper.RunCommand("powercfg.exe", "/setactive scheme_current");
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "PlatformAoAcOverride");
                    ProcessHelper.RunCommand("powercfg.exe", "/setdcvalueindex scheme_current sub_sleep bd3b718a-0680-4d9d-8ab2-e1d2b4ac806d 1");
                    ProcessHelper.RunCommand("powercfg.exe", "/setactive scheme_current");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "laptop_battery_telemetry_reduction",
                Title = "Battery-Saver Telemetry Suspension",
                Category = OptimizationCategory.LaptopBattery,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.LaptopOnly,
                Risk = RiskLevel.Safe,
                Description = "Disables heavy background telemetry indexing services that poll the CPU and drain laptop battery runtime by 15-20%.",
                Tradeoff = "Saves substantial battery life. Highly recommended on laptops.",
                RequiresRestart = false,
                CheckAction = () => ServiceHelper.IsServiceDisabled("DiagTrack") && ServiceHelper.IsServiceDisabled("dmwappushservice"),
                ApplyAction = () =>
                {
                    ServiceHelper.DisableService("DiagTrack");
                    ServiceHelper.DisableService("dmwappushservice");
                    return true;
                },
                RevertAction = () =>
                {
                    ServiceHelper.EnableServiceAuto("DiagTrack");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "laptop_balanced_power_profile",
                Title = "Enforce Energy-Efficient CPU States on Battery",
                Category = OptimizationCategory.LaptopBattery,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.LaptopOnly,
                Risk = RiskLevel.Safe,
                Description = "Ensures CPU frequency scales down aggressively when on battery power to prevent fan noise and heat.",
                Tradeoff = "Slightly lower peak performance while unplugged in exchange for longer battery duration.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    var (code, outStr, _) = ProcessHelper.RunCommand("powercfg.exe", "/getactivescheme");
                    return outStr.Contains("381b4222-f694-41f0-9685-ff5bb260f59e"); // Balanced scheme
                },
                ApplyAction = () =>
                {
                    ProcessHelper.RunCommand("powercfg.exe", "/setactive 381b4222-f694-41f0-9685-ff5bb260f59e");
                    return true;
                },
                RevertAction = () => true
            });
            #endregion

            #region Universal Privacy & Telemetry (CTT WinUtil Baseline)
            list.Add(new OptimizationItem
            {
                Id = "uni_disable_telemetry",
                Title = "Disable Windows Telemetry & Diagnostic Data",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables Connected User Experiences and Telemetry (DiagTrack) and sets telemetry level to 0 (Security).",
                Tradeoff = "Reduces background data transmission to Microsoft servers. Zero workflow impact.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry") == 0 && ServiceHelper.IsServiceDisabled("DiagTrack"),
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "MaxTelemetryAllowed", 0);
                    ServiceHelper.DisableService("DiagTrack");
                    ServiceHelper.DisableService("dmwappushservice");
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry");
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "MaxTelemetryAllowed");
                    ServiceHelper.EnableServiceAuto("DiagTrack");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_error_reporting",
                Title = "Disable Windows Error Reporting (WER)",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Prevents Windows from generating and sending crash dump logs and telemetry to Microsoft.",
                Tradeoff = "Windows will not prompt to send error reports after an application crashes.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled") == 1,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled", 1);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled", 1);
                    ServiceHelper.SetServiceManual("WerSvc");
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled");
                    RegistryHelper.DeleteValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_location",
                Title = "Disable Geolocation & Sensor Tracking",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables the Windows Geolocation Service (lfsvc) and prevents desktop apps from polling your physical location.",
                Tradeoff = "Weather and map apps will not auto-detect physical GPS location.",
                RequiresRestart = false,
                CheckAction = () => ServiceHelper.IsServiceDisabled("lfsvc") || RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation") == 1,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation", 1);
                    ServiceHelper.DisableService("lfsvc");
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation");
                    ServiceHelper.SetServiceManual("lfsvc");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_advertising_id",
                Title = "Disable Advertising ID for Tailored Ads",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables the unique Windows Advertising ID used by apps to track user browsing patterns.",
                Tradeoff = "Apps cannot serve targeted ads based on your advertising ID profile.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy", 1);
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 1);
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\AdvertisingInfo", "DisabledByGroupPolicy");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_delivery_optimization",
                Title = "Disable P2P Delivery Optimization (DoSvc)",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Stops Windows from using your internet upload bandwidth to seed Windows updates to other PCs.",
                Tradeoff = "Updates are downloaded directly from Microsoft servers only. Saves upload bandwidth.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 0);
                    ServiceHelper.SetServiceManual("DoSvc");
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_consumer_bloat",
                Title = "Disable Automatic Installation of Suggested Apps",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Prevents Windows from automatically downloading sponsored partner apps (Candy Crush, TikTok, etc.).",
                Tradeoff = "Stops automated installation of recommended store apps.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 0);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures", 1);
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled", 1);
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsConsumerFeatures");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_bing_search",
                Title = "Disable Bing Web Search in Start Menu",
                Category = OptimizationCategory.PrivacyTelemetry,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Forces the Start Menu search to only query local files and apps, eliminating web search delays and Bing ads.",
                Tradeoff = "Start Menu search results will not include web page links.",
                RequiresRestart = true,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions") == 1,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions", 1);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "CortanaConsent", 0);
                    ProcessHelper.RestartExplorer();
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions");
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 1);
                    ProcessHelper.RestartExplorer();
                    return true;
                }
            });
            #endregion

            #region Universal System & Performance
            list.Add(new OptimizationItem
            {
                Id = "uni_disable_hibernation",
                Title = "Disable Hibernation (Reclaim 8GB-32GB SSD Space)",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Deletes the large hidden hiberfil.sys file and disables Fast Startup, reclaiming GBs of SSD space immediately.",
                Tradeoff = "Disables Hibernate mode and Fast Startup (prevents disk corruption on dual-boot systems).",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled") == 0,
                ApplyAction = () => ProcessHelper.RunCommand("powercfg.exe", "-h off").ExitCode == 0,
                RevertAction = () => ProcessHelper.RunCommand("powercfg.exe", "-h on").ExitCode == 0
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_set_services_manual",
                Title = "Set Non-Essential Windows Services to Manual",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Sets non-critical background services to Demand/Manual mode, preventing them from idling and wasting RAM.",
                Tradeoff = "Services still start on-demand when an app explicitly requests them.",
                RequiresRestart = false,
                CheckAction = () => ServiceHelper.IsServiceManualOrDisabled("MapsBroker"),
                ApplyAction = () =>
                {
                    string[] services = { "MapsBroker", "RetailDemo", "SharedRealitySvc", "TrkWks", "WbioSrvc" };
                    foreach (var s in services) ServiceHelper.SetServiceManual(s);
                    return true;
                },
                RevertAction = () =>
                {
                    ServiceHelper.EnableServiceAuto("MapsBroker");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_optimize_visual_effects",
                Title = "Optimize Visual Effects (Keep Font Smoothing)",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables sluggish window minimize/maximize animation delays while preserving smooth ClearType fonts and thumbnails.",
                Tradeoff = "Window animation effects are simplified for responsive instant window opening.",
                RequiresRestart = true,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "MinAnimate") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "MinAnimate", "0");
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 2);
                    ProcessHelper.RestartExplorer();
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "MinAnimate", "1");
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\VisualEffects", "VisualFXSetting", 1);
                    ProcessHelper.RestartExplorer();
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_enable_end_task",
                Title = "Enable 'End Task' on Taskbar Right-Click",
                Category = OptimizationCategory.InterfaceErgonomics,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Enables direct 'End Task' option when right-clicking running apps on the taskbar to terminate frozen apps instantly.",
                Tradeoff = "Adds an End Task action to taskbar item jump lists.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarEndTask") == 1,
                ApplyAction = () => RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarEndTask", 1),
                RevertAction = () => RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarEndTask", 0)
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_mouse_acceleration",
                Title = "Disable Mouse Acceleration (1:1 Raw Input)",
                Category = OptimizationCategory.GamingLatency,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Disables 'Enhance Pointer Precision' for pure 1:1 hardware mouse tracking without artificial acceleration curves.",
                Tradeoff = "Cursor movement is linear and proportional to physical hand movement.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed") == "0",
                ApplyAction = () =>
                {
                    RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", "0");
                    RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", "0");
                    RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", "0");
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed", "1");
                    RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold1", "6");
                    RegistryHelper.SetString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseThreshold2", "10");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_storage_sense",
                Title = "Configure Automated Storage Sense",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Safe,
                Description = "Enables Windows Storage Sense to automatically purge obsolete temporary files and caches.",
                Tradeoff = "Temporary caches are purged automatically on low disk space.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01") == 1,
                ApplyAction = () => RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01", 1),
                RevertAction = () => RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01", 0)
            });
            #endregion

            #region Universal Gaming & Latency (50/50 and Risky)
            list.Add(new OptimizationItem
            {
                Id = "uni_disable_gamedvr",
                Title = "Disable Xbox GameDVR & Background Recording",
                Category = OptimizationCategory.GamingLatency,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Caution,
                Description = "Disables background video capture and game bar telemetry, eliminating FPS dips and frame stutter.",
                Tradeoff = "50/50: Disables Win+G instant clip recording. Use OBS or GPU shadowplay instead.",
                RequiresRestart = false,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled") == 0,
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 0);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0);
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled", 1);
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_xbox_services",
                Title = "Disable Xbox Live Background Services",
                Category = OptimizationCategory.GamingLatency,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Caution,
                Description = "Disables Xbox Live Auth Manager, Game Save, and Net API background services.",
                Tradeoff = "50/50: If you play games from Xbox App or PC Game Pass, keep this active. Steam/Epic users can disable.",
                RequiresRestart = false,
                CheckAction = () => ServiceHelper.IsServiceDisabled("XblAuthManager"),
                ApplyAction = () =>
                {
                    ServiceHelper.DisableService("XblAuthManager");
                    ServiceHelper.DisableService("XblGameSave");
                    ServiceHelper.DisableService("XboxNetApiSvc");
                    return true;
                },
                RevertAction = () =>
                {
                    ServiceHelper.SetServiceManual("XblAuthManager");
                    ServiceHelper.SetServiceManual("XblGameSave");
                    ServiceHelper.SetServiceManual("XboxNetApiSvc");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_network_throttling",
                Title = "Disable Network Throttling Index (MMCSS)",
                Category = OptimizationCategory.GamingLatency,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Caution,
                Description = "Disables Windows multimedia network packet throttling and maximizes responsiveness for competitive gaming.",
                Tradeoff = "50/50: High-throughput network tuning. Safe for gaming PCs.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    int? val = RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex");
                    return val == -1 || val == unchecked((int)0xffffffff);
                },
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xffffffff));
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0);
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", 10);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 20);
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_tcp_tuning",
                Title = "Optimize TCP Window Auto-Tuning & RSS",
                Category = OptimizationCategory.GamingLatency,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Caution,
                Description = "Enables TCP Receive Side Scaling (RSS) and sets TCP Auto-Tuning to Normal for lower packet jitter.",
                Tradeoff = "50/50: Network stack optimization.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    var (code, outStr, _) = ProcessHelper.RunCommand("netsh.exe", "int tcp show global");
                    return outStr.Contains("Receive-Side Scaling State          : enabled") || outStr.Contains("autotuninglevel : normal");
                },
                ApplyAction = () =>
                {
                    ProcessHelper.RunCommand("netsh.exe", "int tcp set global autotuninglevel=normal");
                    ProcessHelper.RunCommand("netsh.exe", "int tcp set global rss=enabled");
                    return true;
                },
                RevertAction = () => ProcessHelper.RunCommand("netsh.exe", "int tcp set global autotuninglevel=normal").ExitCode == 0
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_sysmain",
                Title = "Disable SysMain / Superfetch (Recommended for NVMe)",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Caution,
                Description = "Disables SysMain caching service. Eliminates disk thrashing and high RAM consumption on SSD drives.",
                Tradeoff = "50/50: Highly recommended on modern SSDs. Users with traditional mechanical HDDs should keep it enabled.",
                RequiresRestart = false,
                CheckAction = () => ServiceHelper.IsServiceDisabled("SysMain"),
                ApplyAction = () => ServiceHelper.DisableService("SysMain"),
                RevertAction = () => ServiceHelper.EnableServiceAuto("SysMain")
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_search_indexing",
                Title = "Disable Windows Search Indexer (WSearch)",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Caution,
                Description = "Disables background file indexing service (WSearch), freeing background CPU and SSD write endurance.",
                Tradeoff = "50/50: File Explorer search will scan files directly instead of using an index. Recommended for high-speed SSDs.",
                RequiresRestart = false,
                CheckAction = () => ServiceHelper.IsServiceDisabled("WSearch"),
                ApplyAction = () => ServiceHelper.DisableService("WSearch"),
                RevertAction = () => ServiceHelper.EnableServiceAuto("WSearch")
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_print_spooler",
                Title = "Disable Print Spooler (Non-Printer Systems)",
                Category = OptimizationCategory.SystemPerformance,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Caution,
                Description = "Disables Print Spooler service, closing a notorious Windows vulnerability vector and saving memory.",
                Tradeoff = "50/50: You will not be able to print to physical printers until re-enabled.",
                RequiresRestart = false,
                CheckAction = () => ServiceHelper.IsServiceDisabled("Spooler"),
                ApplyAction = () => ServiceHelper.DisableService("Spooler"),
                RevertAction = () => ServiceHelper.EnableServiceAuto("Spooler")
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_ultimate_performance_plan",
                Title = "Unlock & Activate Ultimate Performance Power Plan",
                Category = OptimizationCategory.GamingLatency,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.DesktopOnly,
                Risk = RiskLevel.Risky,
                Description = "Activates the hidden Windows Ultimate Performance ACPI power scheme, eliminating CPU core parking and throttling.",
                Tradeoff = "RISKY: Desktop PCs only. CPU runs at constant high clocks. Will rapidly drain battery if used on a laptop.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    var (code, outStr, _) = ProcessHelper.RunCommand("powercfg.exe", "/getactivescheme");
                    return outStr.Contains("e9a42b02-d5df-448d-aa00-03f14749eb61") || outStr.IndexOf("Ultimate Performance", StringComparison.OrdinalIgnoreCase) >= 0;
                },
                ApplyAction = () =>
                {
                    ProcessHelper.RunCommand("powercfg.exe", "-duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61");
                    ProcessHelper.RunCommand("powercfg.exe", "/setactive e9a42b02-d5df-448d-aa00-03f14749eb61");
                    return true;
                },
                RevertAction = () =>
                {
                    ProcessHelper.RunCommand("powercfg.exe", "/setactive 381b4222-f694-41f0-9685-ff5bb260f59e");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_paging_executive",
                Title = "Disable Paging Executive (Lock Kernel in RAM)",
                Category = OptimizationCategory.GamingLatency,
                TargetOs = OsTarget.Universal,
                TargetDevice = DeviceTarget.Universal,
                Risk = RiskLevel.Risky,
                Description = "Forces the Windows OS kernel and executive drivers to reside permanently in physical RAM instead of paging to disk.",
                Tradeoff = "RISKY: Requires 16GB+ RAM. Can cause low memory instability on systems with 8GB or less.",
                RequiresRestart = true,
                CheckAction = () => RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive") == 1,
                ApplyAction = () => RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive", 1),
                RevertAction = () => RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive", 0)
            });
            #endregion

            return list;
        }
    }
}
