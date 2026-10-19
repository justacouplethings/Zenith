using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
            InitializeBranding();

            Loaded += (s, e) =>
            {
                if (DataContext is MainViewModel vm)
                {
                    vm.PropertyChanged += ViewModel_PropertyChanged;
                }
            };
        }

        private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.ProgressValue))
            {
                Dispatcher.Invoke(() =>
                {
                    if (AnimatedProgressSlider != null)
                    {
                        var anim = new DoubleAnimation
                        {
                            To = ViewModel.ProgressValue,
                            Duration = TimeSpan.FromMilliseconds(280),
                            EasingFunction = new QuarticEase { EasingMode = EasingMode.EaseOut }
                        };
                        AnimatedProgressSlider.BeginAnimation(System.Windows.Controls.Primitives.RangeBase.ValueProperty, anim);
                    }
                });
            }
        }

        private void DismissBanner_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DismissResultBanner();
        }

        private void InitializeBranding()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string? processDir = Path.GetDirectoryName(Environment.ProcessPath);

                // 1. Setup crisp App Mark Icon (for window title bar and taskbar)
                ImageSource? appIcon = null;
                string[] iconCandidates = { "app_icon.png", "app.ico" };
                foreach (var candidate in iconCandidates)
                {
                    string p1 = Path.Combine(baseDir, candidate);
                    string? p2 = processDir != null ? Path.Combine(processDir, candidate) : null;
                    if (File.Exists(p1)) { appIcon = LoadIconFrameSafely(p1); if (appIcon != null) break; }
                    if (p2 != null && File.Exists(p2)) { appIcon = LoadIconFrameSafely(p2); if (appIcon != null) break; }
                }

                if (appIcon == null)
                {
                    appIcon = LoadResourceBitmapSafely("app_icon.png") ?? LoadResourceBitmapSafely("app.ico");
                }

                if (appIcon == null)
                {
                    appIcon = CreateFallbackAppIcon();
                }

                if (appIcon != null)
                {
                    this.Icon = appIcon;
                }

                // 2. Setup Wordmark Logo ("Zenith." in purple)
                BitmapSource? wordmarkRaw = null;
                string[] wordmarkCandidates = { "logo_trimmed.png", "logo.png", "logo.ico", "logo.jpg" };
                foreach (var candidate in wordmarkCandidates)
                {
                    string p1 = Path.Combine(baseDir, candidate);
                    string? p2 = processDir != null ? Path.Combine(processDir, candidate) : null;
                    if (File.Exists(p1)) { wordmarkRaw = LoadBitmapSafely(p1); if (wordmarkRaw != null) break; }
                    if (p2 != null && File.Exists(p2)) { wordmarkRaw = LoadBitmapSafely(p2); if (wordmarkRaw != null) break; }
                }

                if (wordmarkRaw == null)
                {
                    wordmarkRaw = LoadResourceBitmapSafely("logo_trimmed.png") ?? LoadResourceBitmapSafely("logo.png");
                }

                if (wordmarkRaw != null && CustomLogoImage != null)
                {
                    // Auto-crop transparent boundaries to avoid tiny/squished wordmark
                    BitmapSource croppedWordmark = AutoCropTransparentBorders(wordmarkRaw);
                    CustomLogoImage.Source = croppedWordmark;
                    CustomLogoImage.Visibility = Visibility.Visible;
                    if (BrandText != null) BrandText.Visibility = Visibility.Collapsed;
                }
                else if (BrandText != null)
                {
                    BrandText.Visibility = Visibility.Visible;
                    if (CustomLogoImage != null) CustomLogoImage.Visibility = Visibility.Collapsed;
                }
            }
            catch { }
        }

        private static BitmapSource AutoCropTransparentBorders(BitmapSource source)
        {
            try
            {
                var formatted = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
                int width = formatted.PixelWidth;
                int height = formatted.PixelHeight;
                int stride = width * 4;
                byte[] pixels = new byte[height * stride];
                formatted.CopyPixels(pixels, stride, 0);

                int minX = width, minY = height, maxX = 0, maxY = 0;
                bool found = false;

                for (int y = 0; y < height; y++)
                {
                    int row = y * stride;
                    for (int x = 0; x < width; x++)
                    {
                        byte alpha = pixels[row + (x * 4) + 3];
                        if (alpha > 15)
                        {
                            if (x < minX) minX = x;
                            if (x > maxX) maxX = x;
                            if (y < minY) minY = y;
                            if (y > maxY) maxY = y;
                            found = true;
                        }
                    }
                }

                if (!found || minX >= maxX || minY >= maxY)
                    return source;

                // 2px margin within source bounds
                minX = Math.Max(0, minX - 2);
                minY = Math.Max(0, minY - 2);
                maxX = Math.Min(width - 1, maxX + 2);
                maxY = Math.Min(height - 1, maxY + 2);

                int cropWidth = maxX - minX + 1;
                int cropHeight = maxY - minY + 1;

                var rect = new Int32Rect(minX, minY, cropWidth, cropHeight);
                return new CroppedBitmap(formatted, rect);
            }
            catch
            {
                return source;
            }
        }

        private static ImageSource? LoadIconFrameSafely(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                if (filePath.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = new Uri(filePath, UriKind.Absolute);
                    var decoder = new IconBitmapDecoder(uri, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    return decoder.Frames.OrderByDescending(f => f.PixelWidth).FirstOrDefault();
                }
                return LoadBitmapSafely(filePath);
            }
            catch
            {
                return null;
            }
        }

        private static BitmapImage? LoadBitmapSafely(string filePath)
        {
            try
            {
                if (!File.Exists(filePath)) return null;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = new Uri(filePath, UriKind.Absolute);
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        private static BitmapImage? LoadResourceBitmapSafely(string resourceName)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/{resourceName.TrimStart('/')}", UriKind.Absolute);
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.UriSource = uri;
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        private static ImageSource CreateFallbackAppIcon()
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                var rect = new Rect(0, 0, 64, 64);
                var brush = new LinearGradientBrush(
                    Color.FromRgb(0x8B, 0x5C, 0xF6),
                    Color.FromRgb(0x6D, 0x28, 0xD9),
                    new Point(0, 0),
                    new Point(1, 1));
                dc.DrawRoundedRectangle(brush, null, rect, 14, 14);

                var streamGeom = new StreamGeometry();
                using (var ctx = streamGeom.Open())
                {
                    ctx.BeginFigure(new Point(18, 18), true, true);
                    ctx.LineTo(new Point(46, 18), true, false);
                    ctx.LineTo(new Point(24, 46), true, false);
                    ctx.LineTo(new Point(46, 46), true, false);
                    ctx.LineTo(new Point(46, 41), true, false);
                    ctx.LineTo(new Point(30, 41), true, false);
                    ctx.LineTo(new Point(46, 21), true, false);
                    ctx.LineTo(new Point(46, 18), true, false);
                    ctx.LineTo(new Point(18, 18), true, false);
                }
                streamGeom.Freeze();
                dc.DrawGeometry(Brushes.White, null, streamGeom);
            }

            var rtb = new RenderTargetBitmap(64, 64, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            rtb.Freeze();
            return rtb;
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
        private async void RunSmartAudit_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.RunSmartHardwareAuditAsync();
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

        private async void RepairNetwork_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.RunRepairNetworkAsync();
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
                    if (ok)
                    {
                        ViewModel.HasExecutionErrors = false;
                        ViewModel.ResultBannerTitle = "Optimization Applied";
                        ViewModel.ResultBannerMessage = $"Successfully applied '{item.Title}'.";
                        ViewModel.ShowResultBanner = true;
                        ViewModel.AppendLog($"[SUCCESS] {item.Title}");
                        if (item.RequiresRestart)
                        {
                            ViewModel.AppendLog($"[Notice] {item.Title} recommends restarting File Explorer or system.");
                        }
                    }
                    else
                    {
                        ViewModel.HasExecutionErrors = true;
                        ViewModel.ResultBannerTitle = "Optimization Failed";
                        ViewModel.ResultBannerMessage = $"Could not apply '{item.Title}'. System rejected setting or permission denied.";
                        ViewModel.ExecutionErrors.Clear();
                        ViewModel.ExecutionErrors.Add($"Error applying {item.Title}: Access Denied or policy restriction.");
                        ViewModel.ShowResultBanner = true;
                        ViewModel.AppendLog($"[FAILED] Could not apply {item.Title}");
                    }
                }
                catch (Exception ex)
                {
                    ViewModel.HasExecutionErrors = true;
                    ViewModel.ResultBannerTitle = "Optimization Error";
                    ViewModel.ResultBannerMessage = $"Exception applying '{item.Title}'.";
                    ViewModel.ExecutionErrors.Clear();
                    ViewModel.ExecutionErrors.Add($"{item.Title}: {ex.Message}");
                    ViewModel.ShowResultBanner = true;
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
                    if (ok)
                    {
                        ViewModel.HasExecutionErrors = false;
                        ViewModel.ResultBannerTitle = "Setting Restored";
                        ViewModel.ResultBannerMessage = $"Successfully restored '{item.Title}' back to Microsoft default.";
                        ViewModel.ShowResultBanner = true;
                        ViewModel.AppendLog($"[RESTORED] {item.Title}");
                    }
                    else
                    {
                        ViewModel.HasExecutionErrors = true;
                        ViewModel.ResultBannerTitle = "Restore Failed";
                        ViewModel.ResultBannerMessage = $"Could not restore '{item.Title}' to default.";
                        ViewModel.ExecutionErrors.Clear();
                        ViewModel.ExecutionErrors.Add($"Error restoring {item.Title}: Access Denied.");
                        ViewModel.ShowResultBanner = true;
                        ViewModel.AppendLog($"[FAILED] Could not revert {item.Title}");
                    }
                }
                catch (Exception ex)
                {
                    ViewModel.HasExecutionErrors = true;
                    ViewModel.ResultBannerTitle = "Restore Error";
                    ViewModel.ResultBannerMessage = $"Exception restoring '{item.Title}'.";
                    ViewModel.ExecutionErrors.Clear();
                    ViewModel.ExecutionErrors.Add($"{item.Title}: {ex.Message}");
                    ViewModel.ShowResultBanner = true;
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