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

            #region Windows 11 Specific Optimizations
            list.Add(new OptimizationItem
            {
                Id = "w11_classic_context_menu",
                Title = "Classic Windows 10 Context Menu",
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Restores the instant full classic right-click menu, eliminating the Windows 11 'Show more options' delay.",
                Tradeoff = "Replaces the modern rounded right-click menu with the classic Windows 10 menu.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.KeyExists(RegistryHive.CurrentUser, @"Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32");
                },
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
                Title = "Disable Windows Copilot",
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Completely disables the Windows Copilot AI integration and removes the sidebar icon.",
                Tradeoff = "Disables Microsoft Copilot AI assistant across the operating system.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot") == 1;
                },
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
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Disables Windows Recall AI background screen analysis, continuous snapshots, and telemetry.",
                Tradeoff = "Blocks AI activity snapshots. Ideal for user privacy and saving disk/CPU overhead.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows AI", "DisableAIDataAnalysis") == 1;
                },
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
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Removes the MSN News and Weather widget from the taskbar, saving background memory and network bandwidth.",
                Tradeoff = "Disables the taskbar widgets panel.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Dsh", "AllowNewsAndInterests") == 0;
                },
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
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Positions Start button and taskbar icons on the left side, matching traditional Windows ergonomics.",
                Tradeoff = "Changes taskbar alignment from Center to Left.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl") == 0;
                },
                ApplyAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 0);
                },
                RevertAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 1);
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_disable_teams_chat",
                Title = "Remove Teams Chat Icon from Taskbar",
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Removes the integrated consumer Microsoft Teams Chat icon from the Windows 11 taskbar.",
                Tradeoff = "Hides the built-in personal Teams Chat icon.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn") == 0;
                },
                ApplyAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 0);
                },
                RevertAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarMn", 1);
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_disable_start_recommendations",
                Title = "Disable Start Menu Recommended Feed",
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Cleans the Start Menu by hiding recent files, recommended apps, and sponsored tips.",
                Tradeoff = "The 'Recommended' section in the Start Menu will be empty.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_IrisRecommendations") == 0;
                },
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
                Id = "w11_disable_snap_assist_flyouts",
                Title = "Disable Snap Assist Hover Delay & Flyouts",
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Makes window snapping instantaneous without laggy flyout animation popups on maximize button hover.",
                Tradeoff = "Snapping windows relies on keyboard shortcuts (Win+Arrows) or direct dragging to screen edges.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout") == 0;
                },
                ApplyAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", 0);
                },
                RevertAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "EnableSnapAssistFlyout", 1);
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_disable_edge_prelaunch",
                Title = "Disable Microsoft Edge Pre-launch & Background Tabs",
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Prevents Microsoft Edge from silently pre-loading background processes at Windows boot.",
                Tradeoff = "Edge will only launch when you explicitly open it.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\MicrosoftEdge\Main", "AllowPrelaunch") == 0;
                },
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
                Id = "w11_explorer_show_extensions",
                Title = "Always Show File Extensions & Hidden Files",
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Safe,
                Description = "Ensures known file extensions (.exe, .bat, .dll) and hidden files are visible in File Explorer.",
                Tradeoff = "All file extensions and hidden items will be visible.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt") == 0;
                },
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1);
                    ProcessHelper.RestartExplorer();
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 1);
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 2);
                    ProcessHelper.RestartExplorer();
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w11_vbs_hypervisor",
                Title = "Disable VBS / Hypervisor Launch for Gaming FPS",
                Category = OptimizationCategory.Windows11,
                Risk = RiskLevel.Caution,
                Description = "Disables the Windows hypervisor layer (VBS/Core Isolation) to reduce frame-time variance and boost gaming FPS.",
                Tradeoff = "50/50: Disables Core Isolation / Memory Integrity virtualization defense against ring-0 malware.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    var (code, outStr, _) = ProcessHelper.RunCommand("bcdedit.exe", "/enum {current}");
                    return outStr.Contains("hypervisorlaunchtype") && outStr.Contains("Off");
                },
                ApplyAction = () =>
                {
                    var res = ProcessHelper.RunCommand("bcdedit.exe", "/set hypervisorlaunchtype off");
                    return res.ExitCode == 0;
                },
                RevertAction = () =>
                {
                    var res = ProcessHelper.RunCommand("bcdedit.exe", "/set hypervisorlaunchtype auto");
                    return res.ExitCode == 0;
                }
            });
            #endregion

            #region Windows 10 Specific Optimizations
            list.Add(new OptimizationItem
            {
                Id = "w10_disable_news_interests",
                Title = "Disable News & Interests on Taskbar",
                Category = OptimizationCategory.Windows10,
                Risk = RiskLevel.Safe,
                Description = "Disables the weather widget and MSN news feed popout on the Windows 10 taskbar.",
                Tradeoff = "Weather and MSN headlines will not be displayed on the taskbar.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Feeds", "ShellFeedsTaskbarViewMode") == 2;
                },
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
                Category = OptimizationCategory.Windows10,
                Risk = RiskLevel.Safe,
                Description = "Disables continuous logging of user activity and prevents syncing document history to Microsoft cloud.",
                Tradeoff = "Task View will show only active windows, not history from past days.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed") == 0;
                },
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
                Category = OptimizationCategory.Windows10,
                Risk = RiskLevel.Safe,
                Description = "Removes the unused '3D Objects' folder from File Explorer's This PC directory tree.",
                Tradeoff = "Hides the 3D Objects shortcut from This PC.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return !RegistryHelper.KeyExists(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\MyComputer\NameSpace\{0DB7E03F-FC29-4DC6-9020-FF41B59E513A}");
                },
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
                Id = "w10_disable_meet_now",
                Title = "Remove 'Meet Now' Taskbar Icon",
                Category = OptimizationCategory.Windows10,
                Risk = RiskLevel.Safe,
                Description = "Removes the Skype Meet Now icon from the notification area on the taskbar.",
                Tradeoff = "Removes the Meet Now quick button.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow") == 1;
                },
                ApplyAction = () =>
                {
                    RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow", 1);
                    RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow", 1);
                    ProcessHelper.RestartExplorer();
                    return true;
                },
                RevertAction = () =>
                {
                    RegistryHelper.DeleteValue(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow");
                    RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "HideSCAMeetNow");
                    ProcessHelper.RestartExplorer();
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "w10_disable_cortana",
                Title = "Disable Cortana Voice Assistant",
                Category = OptimizationCategory.Windows10,
                Risk = RiskLevel.Safe,
                Description = "Disables Cortana autostart, background microphone hooks, and voice queries.",
                Tradeoff = "Disables Cortana voice search.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Search", "AllowCortana") == 0;
                },
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
                Title = "Disable Lock Screen Spotlight Ads & Suggestions",
                Category = OptimizationCategory.Windows10,
                Risk = RiskLevel.Safe,
                Description = "Disables lock screen tips, tricks, and sponsored wallpapers placed by Microsoft.",
                Tradeoff = "The lock screen will display your chosen static picture or slideshow without promotional overlays.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled") == 0;
                },
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

            list.Add(new OptimizationItem
            {
                Id = "w10_disable_driver_updates_wu",
                Title = "Prevent Windows Update From Overwriting Drivers",
                Category = OptimizationCategory.Windows10,
                Risk = RiskLevel.Caution,
                Description = "Stops Windows Update from automatically installing generic GPU or audio drivers over your custom drivers.",
                Tradeoff = "50/50: You will need to manually download driver updates from AMD, NVIDIA, Intel, or your laptop vendor.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate") == 1;
                },
                ApplyAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate", 1);
                },
                RevertAction = () =>
                {
                    return RegistryHelper.DeleteValue(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate", "ExcludeWUDriversInQualityUpdate");
                }
            });
            #endregion

            #region Universal Privacy & Telemetry (CTT WinUtil Baseline)
            list.Add(new OptimizationItem
            {
                Id = "uni_disable_telemetry",
                Title = "Disable Windows Telemetry & Diagnostic Data",
                Category = OptimizationCategory.UniversalPrivacy,
                Risk = RiskLevel.Safe,
                Description = "Disables Connected User Experiences and Telemetry (DiagTrack) and sets telemetry level to 0 (Security).",
                Tradeoff = "Reduces background data transmission to Microsoft servers. Highly recommended.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    bool reg = RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry") == 0;
                    bool svc = ServiceHelper.IsServiceDisabled("DiagTrack");
                    return reg && svc;
                },
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
                Category = OptimizationCategory.UniversalPrivacy,
                Risk = RiskLevel.Safe,
                Description = "Prevents Windows from generating and sending crash dump logs and telemetry to Microsoft.",
                Tradeoff = "Windows will not prompt to send error reports after an application crashes.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\Windows Error Reporting", "Disabled") == 1;
                },
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
                Category = OptimizationCategory.UniversalPrivacy,
                Risk = RiskLevel.Safe,
                Description = "Disables the Windows Geolocation Service (lfsvc) and prevents desktop apps from tracking your physical location.",
                Tradeoff = "Weather and map apps will not auto-detect your physical GPS location.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return ServiceHelper.IsServiceDisabled("lfsvc") || RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\LocationAndSensors", "DisableLocation") == 1;
                },
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
                Category = OptimizationCategory.UniversalPrivacy,
                Risk = RiskLevel.Safe,
                Description = "Disables the unique Windows Advertising ID used by apps to track user browsing patterns.",
                Tradeoff = "Apps cannot serve targeted ads based on your advertising ID profile.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled") == 0;
                },
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
                Category = OptimizationCategory.UniversalPrivacy,
                Risk = RiskLevel.Safe,
                Description = "Stops Windows from using your internet upload bandwidth to seed Windows updates to other PCs.",
                Tradeoff = "Updates are downloaded directly from Microsoft servers only. Saves upload bandwidth.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode") == 0;
                },
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
                Title = "Disable Automatic Consumer Bloatware Installs",
                Category = OptimizationCategory.UniversalPrivacy,
                Risk = RiskLevel.Safe,
                Description = "Prevents Windows from automatically downloading sponsored partner apps (Candy Crush, TikTok, etc.).",
                Tradeoff = "Stops automated installation of recommended store apps.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SilentInstalledAppsEnabled") == 0;
                },
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
                Category = OptimizationCategory.UniversalPrivacy,
                Risk = RiskLevel.Safe,
                Description = "Forces the Start Menu search to only query local files and apps, eliminating web search delays and Bing ads.",
                Tradeoff = "Start Menu search results will not include web page links.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Policies\Microsoft\Windows\Explorer", "DisableSearchBoxSuggestions") == 1;
                },
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
                Category = OptimizationCategory.UniversalSystem,
                Risk = RiskLevel.Safe,
                Description = "Deletes the large hidden hiberfil.sys file and disables Fast Startup, reclaiming GBs of SSD space immediately.",
                Tradeoff = "Disables Hibernate mode and Fast Startup (prevents disk corruption on dual-boot systems).",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Power", "HibernateEnabled") == 0;
                },
                ApplyAction = () =>
                {
                    var (code, _, _) = ProcessHelper.RunCommand("powercfg.exe", "-h off");
                    return code == 0;
                },
                RevertAction = () =>
                {
                    var (code, _, _) = ProcessHelper.RunCommand("powercfg.exe", "-h on");
                    return code == 0;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_set_services_manual",
                Title = "Set Non-Essential Windows Services to Manual",
                Category = OptimizationCategory.UniversalSystem,
                Risk = RiskLevel.Safe,
                Description = "Sets non-critical background services to Demand/Manual mode, preventing them from idling and wasting RAM.",
                Tradeoff = "Services still start on-demand when an app explicitly requests them.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return ServiceHelper.IsServiceManualOrDisabled("MapsBroker");
                },
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
                Category = OptimizationCategory.UniversalSystem,
                Risk = RiskLevel.Safe,
                Description = "Disables sluggish window minimize/maximize animation delays while preserving smooth ClearType fonts and thumbnails.",
                Tradeoff = "Window animation effects are simplified for responsive instant window opening.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Control Panel\Desktop\WindowMetrics", "MinAnimate") == 0;
                },
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
                Category = OptimizationCategory.UniversalSystem,
                Risk = RiskLevel.Safe,
                Description = "Enables direct 'End Task' option when right-clicking running apps on the taskbar to terminate frozen apps instantly.",
                Tradeoff = "Adds an End Task action to taskbar item jump lists.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarEndTask") == 1;
                },
                ApplyAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarEndTask", 1);
                },
                RevertAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarEndTask", 0);
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_mouse_acceleration",
                Title = "Disable Mouse Acceleration (1:1 Raw Input)",
                Category = OptimizationCategory.UniversalSystem,
                Risk = RiskLevel.Safe,
                Description = "Disables 'Enhance Pointer Precision' for pure 1:1 hardware mouse tracking without artificial acceleration curves.",
                Tradeoff = "Cursor movement is linear and proportional to physical hand movement.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetString(RegistryHive.CurrentUser, @"Control Panel\Mouse", "MouseSpeed") == "0";
                },
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
                Category = OptimizationCategory.UniversalSystem,
                Risk = RiskLevel.Safe,
                Description = "Enables Windows Storage Sense to automatically purge obsolete temporary files and caches.",
                Tradeoff = "Temporary caches are purged automatically on low disk space.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01") == 1;
                },
                ApplyAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01", 1);
                },
                RevertAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\StorageSense\Parameters\StoragePolicy", "01", 0);
                }
            });
            #endregion

            #region Universal Gaming & Latency (50/50 and Risky)
            list.Add(new OptimizationItem
            {
                Id = "uni_disable_gamedvr",
                Title = "Disable Xbox GameDVR & Background Recording",
                Category = OptimizationCategory.UniversalGaming,
                Risk = RiskLevel.Caution,
                Description = "Disables background video capture and game bar telemetry, eliminating FPS dips and frame stutter.",
                Tradeoff = "50/50: Disables Win+G instant clip recording. Use OBS or GPU shadowplay instead.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.CurrentUser, @"System\GameConfigStore", "GameDVR_Enabled") == 0;
                },
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
                Category = OptimizationCategory.UniversalGaming,
                Risk = RiskLevel.Caution,
                Description = "Disables Xbox Live Auth Manager, Game Save, and Net API background services.",
                Tradeoff = "50/50: If you play games from Xbox App or PC Game Pass, keep this active. Steam/Epic users can disable.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return ServiceHelper.IsServiceDisabled("XblAuthManager");
                },
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
                Category = OptimizationCategory.UniversalGaming,
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
                Category = OptimizationCategory.UniversalGaming,
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
                RevertAction = () =>
                {
                    ProcessHelper.RunCommand("netsh.exe", "int tcp set global autotuninglevel=normal");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_sysmain",
                Title = "Disable SysMain / Superfetch (Recommended for NVMe)",
                Category = OptimizationCategory.UniversalGaming,
                Risk = RiskLevel.Caution,
                Description = "Disables SysMain caching service. Eliminates disk thrashing and high RAM consumption on SSD drives.",
                Tradeoff = "50/50: Recommended on modern SSDs. Users with traditional mechanical HDDs should keep it enabled.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return ServiceHelper.IsServiceDisabled("SysMain");
                },
                ApplyAction = () =>
                {
                    return ServiceHelper.DisableService("SysMain");
                },
                RevertAction = () =>
                {
                    return ServiceHelper.EnableServiceAuto("SysMain");
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_search_indexing",
                Title = "Disable Windows Search Indexer (WSearch)",
                Category = OptimizationCategory.UniversalGaming,
                Risk = RiskLevel.Caution,
                Description = "Disables background file indexing service (WSearch), freeing background CPU and SSD write endurance.",
                Tradeoff = "50/50: File Explorer search will scan files directly instead of using an index. Recommended for high-speed SSDs.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return ServiceHelper.IsServiceDisabled("WSearch");
                },
                ApplyAction = () =>
                {
                    return ServiceHelper.DisableService("WSearch");
                },
                RevertAction = () =>
                {
                    return ServiceHelper.EnableServiceAuto("WSearch");
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_print_spooler",
                Title = "Disable Print Spooler (Non-Printer Systems)",
                Category = OptimizationCategory.UniversalGaming,
                Risk = RiskLevel.Caution,
                Description = "Disables Print Spooler service, closing a notorious Windows vulnerability vector and saving memory.",
                Tradeoff = "50/50: You will not be able to print to physical printers or export to PDF via Print Spooler until re-enabled.",
                RequiresRestart = false,
                CheckAction = () =>
                {
                    return ServiceHelper.IsServiceDisabled("Spooler");
                },
                ApplyAction = () =>
                {
                    return ServiceHelper.DisableService("Spooler");
                },
                RevertAction = () =>
                {
                    return ServiceHelper.EnableServiceAuto("Spooler");
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_ultimate_performance_plan",
                Title = "Unlock & Activate Ultimate Performance Power Plan",
                Category = OptimizationCategory.UniversalGaming,
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
                    var (code, outStr, _) = ProcessHelper.RunCommand("powercfg.exe", "/list");
                    // Set active
                    ProcessHelper.RunCommand("powercfg.exe", "/setactive e9a42b02-d5df-448d-aa00-03f14749eb61");
                    return true;
                },
                RevertAction = () =>
                {
                    // Revert to Balanced (381b4222-f694-41f0-9685-ff5bb260f59e)
                    ProcessHelper.RunCommand("powercfg.exe", "/setactive 381b4222-f694-41f0-9685-ff5bb260f59e");
                    return true;
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_disable_paging_executive",
                Title = "Disable Paging Executive (Lock Kernel in RAM)",
                Category = OptimizationCategory.UniversalGaming,
                Risk = RiskLevel.Risky,
                Description = "Forces the Windows OS kernel and executive drivers to reside permanently in physical RAM instead of paging to disk.",
                Tradeoff = "RISKY: Requires 16GB+ RAM. Can cause low memory instability on systems with 8GB or less.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive") == 1;
                },
                ApplyAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive", 1);
                },
                RevertAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "DisablePagingExecutive", 0);
                }
            });

            list.Add(new OptimizationItem
            {
                Id = "uni_large_system_cache",
                Title = "Enable Large System File Cache",
                Category = OptimizationCategory.UniversalGaming,
                Risk = RiskLevel.Risky,
                Description = "Increases system working set cache size for high-speed file operations and massive memory throughput.",
                Tradeoff = "RISKY: Allocates more physical memory to system file cache rather than user processes.",
                RequiresRestart = true,
                CheckAction = () =>
                {
                    return RegistryHelper.GetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache") == 1;
                },
                ApplyAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", 1);
                },
                RevertAction = () =>
                {
                    return RegistryHelper.SetDword(RegistryHive.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Memory Management", "LargeSystemCache", 0);
                }
            });
            #endregion

            return list;
        }
    }
}
