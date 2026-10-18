using System;
using System.IO;
using System.Linq;
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
                        if (CustomLogoImage != null && FallbackLogoBorder != null)
                        {
                            CustomLogoImage.Source = bitmap;
                            CustomLogoImage.Visibility = Visibility.Visible;
                            FallbackLogoBorder.Visibility = Visibility.Collapsed;
                            if (BrandText != null) BrandText.Visibility = Visibility.Collapsed;
                        }
                        ViewModel?.AppendLog($"[Branding] Custom logo active from {candidate}");
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

        #region Device Profile Toggle
        private void ChassisLaptop_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ToggleDeviceProfile(true);
        }

        private void ChassisDesktop_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ToggleDeviceProfile(false);
        }
        #endregion

        #region Smart Audit
        private void RunSmartAudit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.RunSmartHardwareAudit();
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
            ViewModel.ActiveTab = "Tweaks";
        }

        private void PresetLaptop_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyPreset("LaptopBattery");
            ViewModel.ActiveTab = "Tweaks";
        }

        private void PresetGaming_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ApplyPreset("ProGaming");
            ViewModel.ActiveTab = "Tweaks";
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

        private async void CleanTemp_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.RunCleanTempAsync();
        }

        private void FreeStorage_Click(object sender, RoutedEventArgs e)
        {
            var hibItem = ViewModel.AllOptimizations.FirstOrDefault(x => x.Id == "uni_disable_hibernation");
            if (hibItem != null)
            {
                hibItem.ApplyAction?.Invoke();
                hibItem.RefreshStatus();
                ViewModel.AppendLog("[Storage] Hibernation disabled. Reclaimed SSD storage capacity.");
            }
        }
        #endregion

        #region Batch Execution with Guardrails
        private async void ApplySelected_Click(object sender, RoutedEventArgs e)
        {
            // Check if user selected any Risky or 50/50 tweaks to show guardrail
            var selected = ViewModel.AllOptimizations.Where(x => x.IsSelected).ToList();
            var riskyCount = selected.Count(x => x.Risk == RiskLevel.Risky);
            var cautionCount = selected.Count(x => x.Risk == RiskLevel.Caution);

            if (riskyCount > 0 || cautionCount > 0)
            {
                var msg = $"You have selected {selected.Count} optimizations, including:\n" +
                          $"• {riskyCount} High-Impact (Risky) tweak(s)\n" +
                          $"• {cautionCount} 50/50 Caution tweak(s)\n\n" +
                          "Make sure you have reviewed their trade-offs or created a System Restore Point. Proceed with optimization?";

                var res = MessageBox.Show(msg, "Zenith Optimizer — Optimization Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Information);
                if (res != MessageBoxResult.Yes) return;
            }

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
                if (item.Risk == RiskLevel.Risky)
                {
                    var confirm = MessageBox.Show(
                        $"Notice: '{item.Title}' is classified as RISKY.\n\nTrade-off: {item.Tradeoff}\n\nApply this tweak?",
                        "Confirm High-Impact Tweak",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (confirm != MessageBoxResult.Yes) return;
                }

                item.IsProcessing = true;
                try
                {
                    bool ok = item.ApplyAction?.Invoke() ?? false;
                    item.RefreshStatus();
                    ViewModel.AppendLog(ok ? $"[SUCCESS] {item.Title}" : $"[FAILED] Could not apply {item.Title}");
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
                    ViewModel.AppendLog(ok ? $"[RESTORED] {item.Title}" : $"[FAILED] Could not revert {item.Title}");
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

        #region Friendly Bloatware Actions
        private void DebloatSelectRecommended_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectSafeDebloatOnly();
        }

        private void DebloatSelectAll_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectAllDebloat();
        }

        private void DebloatDeselectAll_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DeselectAllDebloat();
        }

        private async void PurgeBloatware_Click(object sender, RoutedEventArgs e)
        {
            var selected = ViewModel.BloatwareCategories.Where(x => x.IsSelected).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one app category to uninstall.", "Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var res = MessageBox.Show(
                $"Uninstall {selected.Count} selected bloatware categories for all user accounts?\n\n(Apps can still be reinstalled from the Microsoft Store anytime if needed).",
                "Uninstall Apps Confirmation",
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