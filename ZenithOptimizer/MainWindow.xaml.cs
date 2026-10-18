using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ZenithOptimizer.Models;
using ZenithOptimizer.Services;
using ZenithOptimizer.ViewModels;

namespace ZenithOptimizer
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            TryLoadCustomLogo();
        }

        private void TryLoadCustomLogo()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string[] logoCandidates = { "logo.png", "logo.ico", "icon.png", "logo.jpg" };
                foreach (var candidate in logoCandidates)
                {
                    string path = Path.Combine(baseDir, candidate);
                    if (File.Exists(path))
                    {
                        var uri = new Uri(path, UriKind.Absolute);
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = uri;
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        this.Icon = bitmap;
                        if (CustomLogoImage != null && PlaceholderLogoPath != null)
                        {
                            CustomLogoImage.Source = bitmap;
                            CustomLogoImage.Visibility = Visibility.Visible;
                            PlaceholderLogoPath.Visibility = Visibility.Collapsed;
                        }
                        ViewModel?.AppendLog($"[Branding] Loaded custom brand logo from {candidate}");
                        break;
                    }
                }
            }
            catch { }
        }

        #region Navigation
        private void TabButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn && btn.Tag is string tag)
            {
                ViewModel.ActiveTab = tag;
            }
        }
        #endregion

        #region Risk Filtering
        private void RiskFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn && btn.Tag is string riskTag)
            {
                ViewModel.SelectedRiskFilter = riskTag;
            }
        }
        #endregion

        #region Presets
        private void PresetSafe_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyPreset("SafeDefault");
            ViewModel.ActiveTab = "Universal";
        }

        private void PresetGaming_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyPreset("ProGaming");
            ViewModel.ActiveTab = "Universal";
        }

        private void PresetUltra_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "The Maximum Strip preset enables the Ultimate Performance ACPI scheme and memory/service modifications.\n\nRecommended for desktop gaming rigs. Continue?",
                "Zenith Optimizer — High Performance Notice",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (res == MessageBoxResult.Yes)
            {
                ViewModel.ApplyPreset("MaximumStrip");
                ViewModel.ActiveTab = "Universal";
            }
        }

        private void RevertAll_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "Are you sure you want to select all optimizations to revert back to Microsoft default configurations?",
                "Revert Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                ViewModel.ApplyPreset("SafeDefault");
                _ = ViewModel.RevertSelectedAsync();
            }
        }
        #endregion

        #region Quick Utilities
        private async void CreateRestorePoint_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.CreateRestorePointAsync();
        }

        private void RestartExplorer_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RestartExplorer();
        }

        private async void CheckStatus_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.BusyStatus = "Refreshing tweak status...";
            ViewModel.IsBusy = true;
            await ViewModel.RefreshAllStates();
            ViewModel.IsBusy = false;
            ViewModel.BusyStatus = string.Empty;
        }

        private async void CleanTemp_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.RunCleanTempAsync();
        }

        private void FreeStorage_Click(object sender, RoutedEventArgs e)
        {
            var hibItem = System.Linq.Enumerable.FirstOrDefault(ViewModel.AllOptimizations, x => x.Id == "uni_disable_hibernation");
            if (hibItem != null)
            {
                hibItem.ApplyAction?.Invoke();
                hibItem.RefreshStatus();
                ViewModel.AppendLog("[Storage] Hibernation disabled. Reclaimed SSD storage.");
            }
        }
        #endregion

        #region Batch Execution
        private async void ApplySelected_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.ApplySelectedAsync();
        }

        private async void RevertSelected_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.RevertSelectedAsync();
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ViewModel.FilteredOptimizations)
            {
                if (item is OptimizationItem opt) opt.IsSelected = true;
            }
        }

        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ViewModel.FilteredOptimizations)
            {
                if (item is OptimizationItem opt) opt.IsSelected = false;
            }
        }

        private void ApplySingle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is OptimizationItem item)
            {
                item.IsProcessing = true;
                try
                {
                    bool ok = item.ApplyAction?.Invoke() ?? false;
                    item.RefreshStatus();
                    ViewModel.AppendLog(ok ? $"[SUCCESS] Applied {item.Title}" : $"[FAILED] Could not apply {item.Title}");
                    if (item.RequiresRestart)
                    {
                        ViewModel.AppendLog($"[Notice] {item.Title} recommends restarting File Explorer or system.");
                    }
                }
                catch (Exception ex)
                {
                    ViewModel.AppendLog($"[ERROR] {ex.Message}");
                }
                finally
                {
                    item.IsProcessing = false;
                }
                _ = ViewModel.RefreshAllStates();
            }
        }

        private void RevertSingle_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is OptimizationItem item)
            {
                item.IsProcessing = true;
                try
                {
                    bool ok = item.RevertAction?.Invoke() ?? false;
                    item.RefreshStatus();
                    ViewModel.AppendLog(ok ? $"[REVERTED] Restored {item.Title}" : $"[FAILED] Could not revert {item.Title}");
                }
                catch (Exception ex)
                {
                    ViewModel.AppendLog($"[ERROR] {ex.Message}");
                }
                finally
                {
                    item.IsProcessing = false;
                }
                _ = ViewModel.RefreshAllStates();
            }
        }
        #endregion

        #region Bloatware & Maintenance
        private async void PurgeBloatware_Click(object sender, RoutedEventArgs e)
        {
            var res = MessageBox.Show(
                "Are you sure you want to remove selected pre-installed UWP bloatware packages for all user accounts?",
                "Purge Bloatware Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (res == MessageBoxResult.Yes)
            {
                await ViewModel.RunDebloatPurgeAsync();
            }
        }
        #endregion

        #region Themes & Settings
        private void ThemeSelector_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn && btn.Tag is string themeStr && Enum.TryParse<AppTheme>(themeStr, out var theme))
            {
                ViewModel.CurrentTheme = theme;
            }
        }

        private void AccentSelector_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton btn && btn.Tag is string accentStr && Enum.TryParse<AccentColor>(accentStr, out var accent))
            {
                ViewModel.CurrentAccent = accent;
            }
        }

        private void ClearLogs_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StatusLog = string.Empty;
        }
        #endregion
    }
}