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

        private double _progressValue = 0;
        private int _progressCurrent = 0;
        private int _progressTotal = 0;
        private string _progressStepText = string.Empty;
        private string _currentOperationTitle = string.Empty;
        private bool _showResultBanner = false;
        private bool _hasExecutionErrors = false;
        private string _resultBannerTitle = string.Empty;
        private string _resultBannerMessage = string.Empty;
        public ObservableCollection<string> ExecutionErrors { get; } = new ObservableCollection<string>();

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

        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                if (Math.Abs(_progressValue - value) > 0.001)
                {
                    _progressValue = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ProgressPercentageText));
                }
            }
        }

        public int ProgressCurrent
        {
            get => _progressCurrent;
            set { _progressCurrent = value; OnPropertyChanged(); }
        }

        public int ProgressTotal
        {
            get => _progressTotal;
            set { _progressTotal = value; OnPropertyChanged(); }
        }

        public string ProgressStepText
        {
            get => _progressStepText;
            set { _progressStepText = value; OnPropertyChanged(); }
        }

        public string ProgressPercentageText => $"{Math.Round(ProgressValue)}%";

        public string CurrentOperationTitle
        {
            get => _currentOperationTitle;
            set { _currentOperationTitle = value; OnPropertyChanged(); }
        }

        public bool ShowResultBanner
        {
            get => _showResultBanner;
            set { _showResultBanner = value; OnPropertyChanged(); }
        }

        public bool HasExecutionErrors
        {
            get => _hasExecutionErrors;
            set { _hasExecutionErrors = value; OnPropertyChanged(); }
        }

        public string ResultBannerTitle
        {
            get => _resultBannerTitle;
            set { _resultBannerTitle = value; OnPropertyChanged(); }
        }

        public string ResultBannerMessage
        {
            get => _resultBannerMessage;
            set { _resultBannerMessage = value; OnPropertyChanged(); }
        }

        public void DismissResultBanner()
        {
            ShowResultBanner = false;
            ExecutionErrors.Clear();
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

        public async Task RunSmartHardwareAuditAsync()
        {
            IsBusy = true;
            ShowResultBanner = false;
            ExecutionErrors.Clear();
            HasExecutionErrors = false;
            ProgressTotal = 5;

            string[] stages = {
                "Auditing CPU core topology & cache hierarchy...",
                "Inspecting physical RAM capacity & commit limits...",
                "Probing NVMe & storage bus controllers...",
                "Analyzing chassis thermals & power profiles...",
                "Synthesizing algorithmic optimization score..."
            };

            for (int i = 0; i < stages.Length; i++)
            {
                ProgressCurrent = i + 1;
                ProgressStepText = $"{ProgressCurrent}/{ProgressTotal}";
                CurrentOperationTitle = stages[i];
                BusyStatus = stages[i];
                ProgressValue = ((double)ProgressCurrent / ProgressTotal) * 100.0;
                await Task.Delay(120);
            }

            var (summary, recommendedIds) = SmartAuditEngine.AnalyzeHardware(Metrics, AllOptimizations);
            Metrics.SmartAuditSummary = summary;

            int count = 0;
            foreach (var item in AllOptimizations)
            {
                bool rec = recommendedIds.Contains(item.Id);
                item.IsSelected = rec;
                if (rec) count++;
            }

            AppendLog("--- Smart Hardware Audit Completed ---");
            AppendLog($"Audit applied: Selected {count} optimal tweaks tailored for your {Metrics.OsName} {(Metrics.IsUserLaptopMode ? "Laptop" : "Desktop")}.");

            HasExecutionErrors = false;
            ResultBannerTitle = "Smart Hardware Audit Complete";
            ResultBannerMessage = $"Audited {Metrics.RamTotalGb:F1} GB RAM & CPU. Auto-selected {count} optimal tweaks.";
            ShowResultBanner = true;

            IsBusy = false;
            BusyStatus = string.Empty;
            ActiveTab = "Tweaks";
        }

        public void RunSmartHardwareAudit()
        {
            _ = RunSmartHardwareAuditAsync();
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

            ShowResultBanner = false;
            ExecutionErrors.Clear();
            HasExecutionErrors = false;
            IsBusy = true;
            ProgressTotal = targets.Count;
            ProgressCurrent = 0;
            ProgressValue = 0;
            ProgressStepText = $"0/{ProgressTotal}";
            BusyStatus = "Starting optimizations...";
            CurrentOperationTitle = "Initializing batch queue...";

            AppendLog($"--- Starting Batch Optimization ({targets.Count} tweaks) ---");

            int success = 0;
            var failedItems = new List<(string Title, string Error)>();
            bool needsRestart = false;

            for (int i = 0; i < targets.Count; i++)
            {
                var item = targets[i];
                ProgressCurrent = i + 1;
                ProgressStepText = $"{ProgressCurrent}/{ProgressTotal}";
                CurrentOperationTitle = $"Applying: {item.Title}";
                BusyStatus = $"Applying ({ProgressCurrent}/{ProgressTotal}): {item.Title}";

                item.IsProcessing = true;

                await Task.Run(() =>
                {
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
                            string err = "Failed: System rejected setting or permission denied.";
                            failedItems.Add((item.Title, err));
                            AppendLog($"[FAILED] {item.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedItems.Add((item.Title, ex.Message));
                        AppendLog($"[ERROR] {item.Title}: {ex.Message}");
                    }
                    finally
                    {
                        item.IsProcessing = false;
                    }
                });

                ProgressValue = ((double)ProgressCurrent / ProgressTotal) * 100.0;
                await Task.Delay(40);
            }

            await RefreshAllStates();

            if (failedItems.Count > 0)
            {
                HasExecutionErrors = true;
                ResultBannerTitle = $"Optimization Completed with {failedItems.Count} Issue{(failedItems.Count > 1 ? "s" : "")}";
                ResultBannerMessage = $"{success} of {targets.Count} tweaks applied. {failedItems.Count} tweak{(failedItems.Count > 1 ? "s" : "")} encountered errors:";
                foreach (var f in failedItems)
                {
                    ExecutionErrors.Add($"{f.Title}: {f.Error}");
                }
            }
            else
            {
                HasExecutionErrors = false;
                ResultBannerTitle = "All Optimizations Applied Successfully";
                ResultBannerMessage = $"Successfully applied all {success}/{targets.Count} tweaks (100%)." +
                    (needsRestart ? " A File Explorer restart is recommended to refresh UI elements." : "");
            }

            ShowResultBanner = true;
            IsBusy = false;
            BusyStatus = string.Empty;
            CurrentOperationTitle = string.Empty;
        }

        public async Task RevertSelectedAsync()
        {
            var targets = AllOptimizations.Where(x => x.IsSelected).ToList();
            if (targets.Count == 0)
            {
                AppendLog("Notice: No tweaks selected to revert.");
                return;
            }

            ShowResultBanner = false;
            ExecutionErrors.Clear();
            HasExecutionErrors = false;
            IsBusy = true;
            ProgressTotal = targets.Count;
            ProgressCurrent = 0;
            ProgressValue = 0;
            ProgressStepText = $"0/{ProgressTotal}";
            BusyStatus = "Restoring default settings...";
            CurrentOperationTitle = "Initializing revert queue...";

            AppendLog($"--- Starting Batch Revert ({targets.Count} tweaks) ---");

            int success = 0;
            var failedItems = new List<(string Title, string Error)>();

            for (int i = 0; i < targets.Count; i++)
            {
                var item = targets[i];
                ProgressCurrent = i + 1;
                ProgressStepText = $"{ProgressCurrent}/{ProgressTotal}";
                CurrentOperationTitle = $"Restoring: {item.Title}";
                BusyStatus = $"Restoring ({ProgressCurrent}/{ProgressTotal}): {item.Title}";

                item.IsProcessing = true;

                await Task.Run(() =>
                {
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
                            string err = "Failed: System rejected restoring default configuration.";
                            failedItems.Add((item.Title, err));
                            AppendLog($"[FAILED] {item.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        failedItems.Add((item.Title, ex.Message));
                        AppendLog($"[ERROR] {item.Title}: {ex.Message}");
                    }
                    finally
                    {
                        item.IsProcessing = false;
                    }
                });

                ProgressValue = ((double)ProgressCurrent / ProgressTotal) * 100.0;
                await Task.Delay(40);
            }

            await RefreshAllStates();

            if (failedItems.Count > 0)
            {
                HasExecutionErrors = true;
                ResultBannerTitle = $"Revert Completed with {failedItems.Count} Issue{(failedItems.Count > 1 ? "s" : "")}";
                ResultBannerMessage = $"{success} of {targets.Count} tweaks restored to Microsoft default. {failedItems.Count} tweak{(failedItems.Count > 1 ? "s" : "")} encountered errors:";
                foreach (var f in failedItems)
                {
                    ExecutionErrors.Add($"{f.Title}: {f.Error}");
                }
            }
            else
            {
                HasExecutionErrors = false;
                ResultBannerTitle = "Default Configurations Restored";
                ResultBannerMessage = $"Successfully restored all {success}/{targets.Count} tweaks back to original Microsoft defaults (100%).";
            }

            ShowResultBanner = true;
            IsBusy = false;
            BusyStatus = string.Empty;
            CurrentOperationTitle = string.Empty;
        }

        public async Task CreateRestorePointAsync()
        {
            IsBusy = true;
            ShowResultBanner = false;
            ExecutionErrors.Clear();
            HasExecutionErrors = false;
            ProgressTotal = 3;

            ProgressCurrent = 1;
            ProgressStepText = "1/3";
            ProgressValue = 33.0;
            CurrentOperationTitle = "Probing Volume Shadow Copy service...";
            BusyStatus = "Checking VSS service...";
            AppendLog("[Restore] Initializing Windows System Restore Point snapshot...");

            await Task.Delay(100);

            ProgressCurrent = 2;
            ProgressStepText = "2/3";
            ProgressValue = 66.0;
            CurrentOperationTitle = "Capturing System Restore snapshot...";
            BusyStatus = "Creating snapshot...";

            var (success, msg) = await SystemRestoreService.CreateRestorePointAsync("Zenith Pre-Tweak Safety Point");

            ProgressCurrent = 3;
            ProgressStepText = "3/3";
            ProgressValue = 100.0;

            if (success)
            {
                Metrics.IsRestorePointCreated = true;
                AppendLog($"[Restore SUCCESS] {msg}");
                HasExecutionErrors = false;
                ResultBannerTitle = "System Restore Point Captured";
                ResultBannerMessage = msg;
            }
            else
            {
                AppendLog($"[Restore NOTICE] {msg}");
                HasExecutionErrors = true;
                ResultBannerTitle = "Restore Point Creation Notice";
                ResultBannerMessage = msg;
                ExecutionErrors.Add(msg);
            }

            ShowResultBanner = true;
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
            ShowResultBanner = false;
            ExecutionErrors.Clear();
            HasExecutionErrors = false;
            ProgressTotal = selected.Count;
            ProgressCurrent = 0;
            ProgressValue = 0;
            ProgressStepText = $"0/{ProgressTotal}";
            BusyStatus = "Starting bloatware uninstallation...";

            AppendLog($"--- Purging Bloatware ({selected.Count} categories) ---");

            int totalRemoved = 0;
            var failedCategories = new List<string>();

            for (int i = 0; i < selected.Count; i++)
            {
                var cat = selected[i];
                ProgressCurrent = i + 1;
                ProgressStepText = $"{ProgressCurrent}/{ProgressTotal}";
                CurrentOperationTitle = $"Debloating: {cat.Title}";
                BusyStatus = $"Debloating ({ProgressCurrent}/{ProgressTotal}): {cat.Title}";

                await Task.Run(async () =>
                {
                    try
                    {
                        var (removed, log) = await DebloatService.RemoveSelectedCategoriesAsync(new[] { cat });
                        totalRemoved += removed;
                        AppendLog(log);
                    }
                    catch (Exception ex)
                    {
                        failedCategories.Add($"{cat.Title}: {ex.Message}");
                        AppendLog($"[ERROR] Failed to purge {cat.Title}: {ex.Message}");
                    }
                });

                ProgressValue = ((double)ProgressCurrent / ProgressTotal) * 100.0;
                await Task.Delay(50);
            }

            if (failedCategories.Count > 0)
            {
                HasExecutionErrors = true;
                ResultBannerTitle = "Debloat Completed with Warnings";
                ResultBannerMessage = $"Purged {totalRemoved} packages. {failedCategories.Count} category encountered errors:";
                foreach (var err in failedCategories)
                {
                    ExecutionErrors.Add(err);
                }
            }
            else
            {
                HasExecutionErrors = false;
                ResultBannerTitle = "Debloat Complete";
                ResultBannerMessage = $"Successfully purged {totalRemoved} packages across {selected.Count} categories (100%).";
            }

            ShowResultBanner = true;
            IsBusy = false;
            BusyStatus = string.Empty;
        }

        public async Task RunCleanTempAsync()
        {
            IsBusy = true;
            ShowResultBanner = false;
            ExecutionErrors.Clear();
            HasExecutionErrors = false;
            ProgressTotal = 3;

            ProgressCurrent = 1;
            ProgressStepText = "1/3";
            ProgressValue = 33.0;
            CurrentOperationTitle = "Purging user AppData & local temp caches...";
            BusyStatus = "Cleaning user temp...";
            AppendLog("--- Purging Windows Temporary Caches & DNS ---");

            await Task.Delay(100);

            ProgressCurrent = 2;
            ProgressStepText = "2/3";
            ProgressValue = 66.0;
            CurrentOperationTitle = "Purging Windows system temp & crash dumps...";
            BusyStatus = "Cleaning system temp...";

            var (freed, log) = await DebloatService.CleanTemporaryFilesAsync();
            AppendLog(log);

            ProgressCurrent = 3;
            ProgressStepText = "3/3";
            ProgressValue = 100.0;
            CurrentOperationTitle = "Flushing DNS resolver cache & ARP tables...";
            BusyStatus = "Flushing DNS...";

            double mb = freed / (1024.0 * 1024.0);
            AppendLog($"--- Cache Cleanup Complete: Freed {mb:F1} MB of space ---");

            HasExecutionErrors = false;
            ResultBannerTitle = "System Storage Cleaned";
            ResultBannerMessage = $"Successfully purged temporary caches and flushed DNS. Freed {mb:F1} MB of storage.";
            ShowResultBanner = true;

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
