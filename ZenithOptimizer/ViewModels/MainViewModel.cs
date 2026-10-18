using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using ZenithOptimizer.Models;
using ZenithOptimizer.Services;

namespace ZenithOptimizer.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly HardwareMonitorService _hardwareMonitor;
        private string _activeTab = "Dashboard";
        private string _searchQuery = string.Empty;
        private string _selectedRiskFilter = "All";
        private bool _isBusy = false;
        private string _busyStatus = string.Empty;
        private string _statusLog = "System ready. Welcome to Zenith Optimizer.\n";
        private AppTheme _currentTheme = AppTheme.CupertinoDark;
        private AccentColor _currentAccent = AccentColor.RoyalViolet; // Zenith Purple default

        public SystemMetrics Metrics { get; } = new SystemMetrics();
        public ObservableCollection<OptimizationItem> AllOptimizations { get; } = new ObservableCollection<OptimizationItem>();
        public ICollectionView FilteredOptimizations { get; }
        public ObservableCollection<FriendlyBloatCategory> BloatwareCategories { get; } = new ObservableCollection<FriendlyBloatCategory>();

        public string ActiveTab
        {
            get => _activeTab;
            set
            {
                if (_activeTab != value)
                {
                    _activeTab = value;
                    OnPropertyChanged();
                    FilteredOptimizations.Refresh();
                }
            }
        }

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    FilteredOptimizations.Refresh();
                }
            }
        }

        public string SelectedRiskFilter
        {
            get => _selectedRiskFilter;
            set
            {
                if (_selectedRiskFilter != value)
                {
                    _selectedRiskFilter = value;
                    OnPropertyChanged();
                    FilteredOptimizations.Refresh();
                }
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string BusyStatus
        {
            get => _busyStatus;
            set { _busyStatus = value; OnPropertyChanged(); }
        }

        public string StatusLog
        {
            get => _statusLog;
            set { _statusLog = value; OnPropertyChanged(); }
        }

        public AppTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    OnPropertyChanged();
                    ThemeManager.ApplyTheme(_currentTheme, _currentAccent);
                }
            }
        }

        public AccentColor CurrentAccent
        {
            get => _currentAccent;
            set
            {
                if (_currentAccent != value)
                {
                    _currentAccent = value;
                    OnPropertyChanged();
                    ThemeManager.ApplyTheme(_currentTheme, _currentAccent);
                }
            }
        }

        public MainViewModel()
        {
            _hardwareMonitor = new HardwareMonitorService(Metrics);

            // Load Optimizations
            var list = OptimizationDatabase.GetAllOptimizations();
            foreach (var opt in list)
            {
                AllOptimizations.Add(opt);
            }

            // Load Friendly Bloatware categories
            var bloat = DebloatService.GetFriendlyBloatwareCategories();
            foreach (var b in bloat)
            {
                BloatwareCategories.Add(b);
            }

            // Setup CollectionView filtering
            FilteredOptimizations = CollectionViewSource.GetDefaultView(AllOptimizations);
            FilteredOptimizations.Filter = FilterOptimization;

            // Run initial status check and hardware audit recommendation
            Task.Run(async () =>
            {
                await RefreshAllStates();
                RunHardwareAuditSilently();
            });
        }

        private bool FilterOptimization(object obj)
        {
            if (obj is not OptimizationItem item) return false;

            // 1. Only display in Tweaks tab
            if (ActiveTab != "Tweaks" && ActiveTab != "All") return false;

            // 2. Automatic OS Compatibility Filtering:
            // If running on Windows 11, hide Windows 10 tweaks completely
            if (Metrics.IsWindows11 && item.TargetOs == OsTarget.Windows10) return false;
            // If running on Windows 10, hide Windows 11 tweaks completely
            if (Metrics.IsWindows10 && item.TargetOs == OsTarget.Windows11) return false;

            // 3. Device Profile Filtering:
            // If in Laptop profile, hide DesktopOnly tweaks (like Ultimate ACPI scheme or hypervisor off if dangerous)
            if (Metrics.IsUserLaptopMode && item.TargetDevice == DeviceTarget.DesktopOnly)
            {
                // In laptop mode, exclude pure desktop-only schemes unless explicitly searched
                if (string.IsNullOrWhiteSpace(SearchQuery)) return false;
            }

            // 4. Risk / Category Filter
            bool matchFilter = SelectedRiskFilter switch
            {
                "Safe" => item.Risk == RiskLevel.Safe,
                "Caution" => item.Risk == RiskLevel.Caution,
                "Risky" => item.Risk == RiskLevel.Risky,
                "Laptop" => item.Category == OptimizationCategory.LaptopBattery,
                "Gaming" => item.Category == OptimizationCategory.GamingLatency,
                _ => true
            };
            if (!matchFilter) return false;

            // 5. Search query
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string query = SearchQuery.Trim().ToLowerInvariant();
                return item.Title.ToLowerInvariant().Contains(query) ||
                       item.Description.ToLowerInvariant().Contains(query) ||
                       item.Tradeoff.ToLowerInvariant().Contains(query) ||
                       item.CategoryBadgeText.ToLowerInvariant().Contains(query);
            }

            return true;
        }

        public void AppendLog(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            StatusLog += $"[{time}] {message}\n";
        }

        public async Task RefreshAllStates()
        {
            await Task.Run(() =>
            {
                int applied = 0;
                int compatibleTotal = 0;

                foreach (var item in AllOptimizations)
                {
                    item.RefreshStatus();

                    // Count only compatible tweaks for the user's OS
                    bool isCompatible = (Metrics.IsWindows11 && item.TargetOs != OsTarget.Windows10) ||
                                       (Metrics.IsWindows10 && item.TargetOs != OsTarget.Windows11);

                    if (isCompatible)
                    {
                        compatibleTotal++;
                        if (item.IsApplied) applied++;
                    }
                }

                Metrics.TotalAvailableTweaks = compatibleTotal;
                Metrics.TotalAppliedTweaks = applied;
                Metrics.OptimizationScore = compatibleTotal > 0 ? (int)((applied * 100.0) / compatibleTotal) : 0;
            });
        }

        public void ToggleDeviceProfile(bool isLaptop)
        {
            Metrics.IsUserLaptopMode = isLaptop;
            AppendLog($"[Profile] Active Device Profile switched to: {(isLaptop ? "Laptop" : "Desktop")}");
            FilteredOptimizations.Refresh();
        }

        #region Smart Audit
        private void RunHardwareAuditSilently()
        {
            var (summary, _) = SmartAuditEngine.AnalyzeHardware(Metrics, AllOptimizations);
            Metrics.SmartAuditSummary = summary;
        }

        public void RunSmartHardwareAudit()
        {
            var (summary, recommendedIds) = SmartAuditEngine.AnalyzeHardware(Metrics, AllOptimizations);
            Metrics.SmartAuditSummary = summary;

            // Auto-select recommended tweaks
            foreach (var item in AllOptimizations)
            {
                item.IsSelected = recommendedIds.Contains(item.Id);
            }

            AppendLog("--- Smart Hardware Audit Completed ---");
            AppendLog($"Audit applied: Selected {recommendedIds.Count} optimal tweaks tailored for your {Metrics.OsName} {(Metrics.IsUserLaptopMode ? "Laptop" : "Desktop")}.");
            ActiveTab = "Tweaks";
        }
        #endregion

        #region Presets
        public void ApplyPreset(string presetName)
        {
            foreach (var item in AllOptimizations) item.IsSelected = false;

            switch (presetName)
            {
                case "SafeDefault":
                    foreach (var item in AllOptimizations.Where(x => x.Risk == RiskLevel.Safe))
                    {
                        // Check OS compatibility
                        if (Metrics.IsWindows11 && item.TargetOs == OsTarget.Windows10) continue;
                        if (Metrics.IsWindows10 && item.TargetOs == OsTarget.Windows11) continue;
                        item.IsSelected = true;
                    }
                    AppendLog("Preset loaded: Safe & Recommended tweaks selected.");
                    break;

                case "LaptopBattery":
                    foreach (var item in AllOptimizations.Where(x =>
                        x.Risk == RiskLevel.Safe &&
                        x.TargetDevice != DeviceTarget.DesktopOnly))
                    {
                        if (Metrics.IsWindows11 && item.TargetOs == OsTarget.Windows10) continue;
                        if (Metrics.IsWindows10 && item.TargetOs == OsTarget.Windows11) continue;
                        item.IsSelected = true;
                    }
                    AppendLog("Preset loaded: Laptop Battery & Efficiency profile selected.");
                    break;

                case "ProGaming":
                    foreach (var item in AllOptimizations.Where(x =>
                        x.Risk == RiskLevel.Safe ||
                        x.Id == "uni_disable_gamedvr" ||
                        x.Id == "uni_network_throttling" ||
                        x.Id == "uni_tcp_tuning" ||
                        x.Id == "uni_disable_sysmain" ||
                        x.Id == "w11_vbs_hypervisor"))
                    {
                        if (Metrics.IsWindows11 && item.TargetOs == OsTarget.Windows10) continue;
                        if (Metrics.IsWindows10 && item.TargetOs == OsTarget.Windows11) continue;
                        item.IsSelected = true;
                    }
                    AppendLog("Preset loaded: Pro Gaming & Low Latency tweaks selected.");
                    break;

                case "MaximumStrip":
                    foreach (var item in AllOptimizations.Where(x =>
                        x.Risk == RiskLevel.Safe ||
                        x.Risk == RiskLevel.Caution ||
                        x.Id == "uni_ultimate_performance_plan" ||
                        x.Id == "uni_disable_paging_executive"))
                    {
                        if (Metrics.IsWindows11 && item.TargetOs == OsTarget.Windows10) continue;
                        if (Metrics.IsWindows10 && item.TargetOs == OsTarget.Windows11) continue;
                        item.IsSelected = true;
                    }
                    AppendLog("Preset loaded: Maximum Strip tweaks selected.");
                    break;

                case "DeselectAll":
                    foreach (var item in AllOptimizations) item.IsSelected = false;
                    AppendLog("All tweaks deselected.");
                    break;
            }
        }
        #endregion

        #region Batch Action Execution
        public async Task ApplySelectedAsync()
        {
            var targets = AllOptimizations.Where(x => x.IsSelected).ToList();
            if (targets.Count == 0)
            {
                AppendLog("Notice: No tweaks selected. Use presets or checkboxes to select tweaks.");
                return;
            }

            IsBusy = true;
            BusyStatus = "Applying optimizations...";
            AppendLog($"--- Starting Batch Optimization ({targets.Count} tweaks) ---");

            await Task.Run(() =>
            {
                int success = 0;
                bool needsRestart = false;

                foreach (var item in targets)
                {
                    item.IsProcessing = true;
                    try
                    {
                        bool ok = item.ApplyAction?.Invoke() ?? false;
                        item.RefreshStatus();
                        if (ok)
                        {
                            success++;
                            AppendLog($"[SUCCESS] {item.Title}");
                            if (item.RequiresRestart) needsRestart = true;
                        }
                        else
                        {
                            AppendLog($"[FAILED] {item.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"[ERROR] {item.Title}: {ex.Message}");
                    }
                    finally
                    {
                        item.IsProcessing = false;
                    }
                }

                AppendLog($"--- Finished: {success}/{targets.Count} applied successfully ---");
                if (needsRestart)
                {
                    AppendLog("[Notice] One or more tweaks recommend restarting File Explorer to refresh interface elements.");
                }
            });

            await RefreshAllStates();
            IsBusy = false;
            BusyStatus = string.Empty;
        }

        public async Task RevertSelectedAsync()
        {
            var targets = AllOptimizations.Where(x => x.IsSelected).ToList();
            if (targets.Count == 0)
            {
                AppendLog("Notice: No tweaks selected to revert.");
                return;
            }

            IsBusy = true;
            BusyStatus = "Restoring default settings...";
            AppendLog($"--- Starting Batch Revert ({targets.Count} tweaks) ---");

            await Task.Run(() =>
            {
                int success = 0;
                foreach (var item in targets)
                {
                    item.IsProcessing = true;
                    try
                    {
                        bool ok = item.RevertAction?.Invoke() ?? false;
                        item.RefreshStatus();
                        if (ok)
                        {
                            success++;
                            AppendLog($"[RESTORED] {item.Title}");
                        }
                        else
                        {
                            AppendLog($"[FAILED] {item.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"[ERROR] {item.Title}: {ex.Message}");
                    }
                    finally
                    {
                        item.IsProcessing = false;
                    }
                }

                AppendLog($"--- Finished: {success}/{targets.Count} restored to default ---");
            });

            await RefreshAllStates();
            IsBusy = false;
            BusyStatus = string.Empty;
        }

        public async Task CreateRestorePointAsync()
        {
            IsBusy = true;
            BusyStatus = "Creating Windows System Restore Point...";
            AppendLog("[Restore] Initializing Windows System Restore Point snapshot...");

            var (success, msg) = await SystemRestoreService.CreateRestorePointAsync("Zenith Pre-Tweak Safety Point");
            if (success)
            {
                Metrics.IsRestorePointCreated = true;
                AppendLog($"[Restore SUCCESS] {msg}");
            }
            else
            {
                AppendLog($"[Restore NOTICE] {msg}");
            }

            IsBusy = false;
            BusyStatus = string.Empty;
        }

        public void SelectSafeDebloatOnly()
        {
            foreach (var cat in BloatwareCategories)
            {
                cat.IsSelected = cat.IsRecommended;
            }
            AppendLog("[Debloat] Selected Safe Consumer Bloatware (Games, News, Feedback, Media tools).");
        }

        public void SelectAllDebloat()
        {
            foreach (var cat in BloatwareCategories) cat.IsSelected = true;
            AppendLog("[Debloat] Selected All Bloatware categories.");
        }

        public void DeselectAllDebloat()
        {
            foreach (var cat in BloatwareCategories) cat.IsSelected = false;
            AppendLog("[Debloat] Deselected all bloatware categories.");
        }

        public async Task RunDebloatPurgeAsync()
        {
            var selected = BloatwareCategories.Where(x => x.IsSelected).ToList();
            if (selected.Count == 0)
            {
                AppendLog("Notice: No bloatware categories selected.");
                return;
            }

            IsBusy = true;
            BusyStatus = "Removing selected bloatware apps...";
            AppendLog($"--- Purging Bloatware ({selected.Count} categories) ---");

            var (removed, log) = await DebloatService.RemoveSelectedCategoriesAsync(selected);
            AppendLog(log);
            AppendLog($"--- Finished: {removed} app packages uninstalled ---");

            IsBusy = false;
            BusyStatus = string.Empty;
        }

        public async Task RunCleanTempAsync()
        {
            IsBusy = true;
            BusyStatus = "Purging temporary cache and flushing DNS...";
            AppendLog("--- Purging Windows Temporary Caches & DNS ---");

            var (freed, log) = await DebloatService.CleanTemporaryFilesAsync();
            AppendLog(log);
            double mb = freed / (1024.0 * 1024.0);
            AppendLog($"--- Cache Cleanup Complete: Freed {mb:F1} MB of space ---");

            IsBusy = false;
            BusyStatus = string.Empty;
        }

        public void RestartExplorer()
        {
            AppendLog("[Explorer] Restarting Windows File Explorer...");
            ProcessHelper.RestartExplorer();
            AppendLog("[Explorer] File Explorer restarted successfully.");
        }
        #endregion

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
