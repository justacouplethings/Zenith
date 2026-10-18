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
        private AccentColor _currentAccent = AccentColor.AppleBlue;

        public SystemMetrics Metrics { get; } = new SystemMetrics();
        public ObservableCollection<OptimizationItem> AllOptimizations { get; } = new ObservableCollection<OptimizationItem>();
        public ICollectionView FilteredOptimizations { get; }
        public ObservableCollection<BloatPackageItem> BloatwareList { get; } = new ObservableCollection<BloatPackageItem>();

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

            // Load Bloatware list
            var bloat = DebloatService.GetDefaultBloatwareList();
            foreach (var b in bloat)
            {
                BloatwareList.Add(b);
            }

            // Setup CollectionView filtering
            FilteredOptimizations = CollectionViewSource.GetDefaultView(AllOptimizations);
            FilteredOptimizations.Filter = FilterOptimization;

            // Initial state check in background
            Task.Run(RefreshAllStates);
        }

        private bool FilterOptimization(object obj)
        {
            if (obj is not OptimizationItem item) return false;

            // 1. Tab filter
            bool matchTab = ActiveTab switch
            {
                "Dashboard" => false, // Dashboard displays summary cards
                "Win11" => item.Category == OptimizationCategory.Windows11,
                "Win10" => item.Category == OptimizationCategory.Windows10,
                "Universal" => item.Category == OptimizationCategory.UniversalPrivacy ||
                               item.Category == OptimizationCategory.UniversalSystem ||
                               item.Category == OptimizationCategory.UniversalGaming,
                "All" => true,
                _ => true
            };
            if (!matchTab) return false;

            // 2. Risk filter
            bool matchRisk = SelectedRiskFilter switch
            {
                "Safe" => item.Risk == RiskLevel.Safe,
                "Caution" => item.Risk == RiskLevel.Caution,
                "Risky" => item.Risk == RiskLevel.Risky,
                _ => true
            };
            if (!matchRisk) return false;

            // 3. Search query
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                string query = SearchQuery.Trim().ToLowerInvariant();
                return item.Title.ToLowerInvariant().Contains(query) ||
                       item.Description.ToLowerInvariant().Contains(query) ||
                       item.Tradeoff.ToLowerInvariant().Contains(query);
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
                foreach (var item in AllOptimizations)
                {
                    item.RefreshStatus();
                    if (item.IsApplied) applied++;
                }

                Metrics.TotalAvailableTweaks = AllOptimizations.Count;
                Metrics.TotalAppliedTweaks = applied;
                Metrics.OptimizationScore = AllOptimizations.Count > 0 ? (int)((applied * 100.0) / AllOptimizations.Count) : 0;
            });
        }

        #region Presets
        public void ApplyPreset(string presetName)
        {
            AppendLog($"Loading preset: {presetName}");
            foreach (var item in AllOptimizations)
            {
                item.IsSelected = false;
            }

            switch (presetName)
            {
                case "SafeDefault":
                    // Select all Safe tweaks across all categories
                    foreach (var item in AllOptimizations.Where(x => x.Risk == RiskLevel.Safe))
                    {
                        item.IsSelected = true;
                    }
                    AppendLog("Preset loaded: Recommended Safe tweaks selected.");
                    break;

                case "ProGaming":
                    // All safe tweaks + Network throttling + GameDVR + SysMain
                    foreach (var item in AllOptimizations.Where(x =>
                        x.Risk == RiskLevel.Safe ||
                        x.Id == "uni_disable_gamedvr" ||
                        x.Id == "uni_network_throttling" ||
                        x.Id == "uni_tcp_tuning" ||
                        x.Id == "uni_disable_sysmain" ||
                        x.Id == "w11_vbs_hypervisor"))
                    {
                        item.IsSelected = true;
                    }
                    AppendLog("Preset loaded: Pro Gaming & Low Latency tweaks selected.");
                    break;

                case "MaximumStrip":
                    // All Safe + All Caution tweaks + Ultimate Performance plan
                    foreach (var item in AllOptimizations.Where(x =>
                        x.Risk == RiskLevel.Safe ||
                        x.Risk == RiskLevel.Caution ||
                        x.Id == "uni_ultimate_performance_plan" ||
                        x.Id == "uni_disable_paging_executive"))
                    {
                        item.IsSelected = true;
                    }
                    AppendLog("Preset loaded: Maximum Strip tweaks selected.");
                    break;

                case "DeselectAll":
                    foreach (var item in AllOptimizations) item.IsSelected = false;
                    AppendLog("All optimizations deselected.");
                    break;
            }
        }
        #endregion

        #region Action Execution
        public async Task ApplySelectedAsync()
        {
            var targets = AllOptimizations.Where(x => x.IsSelected).ToList();
            if (targets.Count == 0)
            {
                AppendLog("Notice: No optimizations selected. Use presets or check boxes to select tweaks.");
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
                            AppendLog($"[SUCCESS] Applied: {item.Title}");
                            if (item.RequiresRestart) needsRestart = true;
                        }
                        else
                        {
                            AppendLog($"[FAILED] Error applying: {item.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"[ERROR] Exception on {item.Title}: {ex.Message}");
                    }
                    finally
                    {
                        item.IsProcessing = false;
                    }
                }

                AppendLog($"--- Finished: {success}/{targets.Count} applied successfully ---");
                if (needsRestart)
                {
                    AppendLog("[Notice] One or more tweaks recommend restarting File Explorer or your PC for full effect.");
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
                AppendLog("Notice: No optimizations selected to revert.");
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
                            AppendLog($"[REVERTED] Restored: {item.Title}");
                        }
                        else
                        {
                            AppendLog($"[FAILED] Error reverting: {item.Title}");
                        }
                    }
                    catch (Exception ex)
                    {
                        AppendLog($"[ERROR] Exception on {item.Title}: {ex.Message}");
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

            var (success, msg) = await SystemRestoreService.CreateRestorePointAsync("Zenith Optimizer Safe Snapshot");
            if (success)
            {
                AppendLog($"[Restore SUCCESS] {msg}");
            }
            else
            {
                AppendLog($"[Restore WARNING] {msg}");
            }

            IsBusy = false;
            BusyStatus = string.Empty;
        }

        public async Task RunDebloatPurgeAsync()
        {
            var selected = BloatwareList.Where(x => x.IsSelected).ToList();
            if (selected.Count == 0)
            {
                AppendLog("Notice: No bloatware packages selected for removal.");
                return;
            }

            IsBusy = true;
            BusyStatus = "Removing selected UWP bloatware packages...";
            AppendLog($"--- Starting UWP Bloatware Removal ({selected.Count} packages) ---");

            var (removed, log) = await DebloatService.RemoveBloatwarePackagesAsync(selected);
            AppendLog(log);
            AppendLog($"--- Finished: {removed} package variations removed ---");

            IsBusy = false;
            BusyStatus = string.Empty;
        }

        public async Task RunCleanTempAsync()
        {
            IsBusy = true;
            BusyStatus = "Cleaning temporary caches and files...";
            AppendLog("--- Purging Windows Temporary Caches & DNS ---");

            var (freed, log) = await DebloatService.CleanTemporaryFilesAsync();
            AppendLog(log);
            double mb = freed / (1024.0 * 1024.0);
            AppendLog($"--- Cache Cleanup Complete: Freed {mb:F1} MB of disk space ---");

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
