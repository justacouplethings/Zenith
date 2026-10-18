using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ZenithOptimizer.Services
{
    public class FriendlyBloatCategory : INotifyPropertyChanged
    {
        private bool _isSelected = true;

        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string IconSymbol { get; set; } = "📦";
        public string Description { get; set; } = string.Empty;
        public string ImpactDescription { get; set; } = string.Empty;
        public string RiskBadge { get; set; } = "SAFE";
        public bool IsRecommended { get; set; } = true;
        public string PackagePattern { get; set; } = string.Empty;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class DebloatService
    {
        public static List<FriendlyBloatCategory> GetFriendlyBloatwareCategories()
        {
            return new List<FriendlyBloatCategory>
            {
                new FriendlyBloatCategory
                {
                    Id = "promo_games",
                    Title = "Pre-installed Games & Sponsored Ads",
                    IconSymbol = "🎮",
                    RiskBadge = "SAFE",
                    IsRecommended = true,
                    Description = "Includes Candy Crush, Solitaire Collection, and preloaded trial games.",
                    ImpactDescription = "100% Safe. Cleans sponsored tile junk from your Start Menu. Can be redownloaded from Store anytime.",
                    PackagePattern = "*MicrosoftSolitaireCollection*|*CandyCrush*|*HiddenCity*|*MarchofEmpires*"
                },
                new FriendlyBloatCategory
                {
                    Id = "news_weather",
                    Title = "Microsoft News & MSN Weather",
                    IconSymbol = "📰",
                    RiskBadge = "SAFE",
                    IsRecommended = true,
                    Description = "Pre-installed MSN News and Weather apps that poll the network continuously.",
                    ImpactDescription = "Safe to remove. Eliminates background network requests and notification popups. Check weather in browser.",
                    PackagePattern = "*BingNews*|*BingWeather*|*Microsoft.BingSports*|*Microsoft.BingFinance*"
                },
                new FriendlyBloatCategory
                {
                    Id = "feedback_help",
                    Title = "Feedback Hub, Get Help & Tips",
                    IconSymbol = "💬",
                    RiskBadge = "SAFE",
                    IsRecommended = true,
                    Description = "Diagnostic feedback prompts and introductory Windows usage tips.",
                    ImpactDescription = "Safe to remove. Stops annoying 'How likely are you to recommend Windows?' survey popups.",
                    PackagePattern = "*WindowsFeedbackHub*|*GetHelp*|*Getstarted*"
                },
                new FriendlyBloatCategory
                {
                    Id = "media_tools",
                    Title = "Trial Media Tools & 3D Viewers",
                    IconSymbol = "🎬",
                    RiskBadge = "SAFE",
                    IsRecommended = true,
                    Description = "Includes Clipchamp commercial video editor trial, Paint 3D, and 3D Builder.",
                    ImpactDescription = "Safe to remove. Reclaims ~800 MB storage. VLC or your preferred editor is lighter and faster.",
                    PackagePattern = "*Clipchamp*|*3DBuilder*|*Microsoft3DViewer*|*Paint3D*"
                },
                new FriendlyBloatCategory
                {
                    Id = "promo_partners",
                    Title = "Partner Promo Stubs (Spotify, Disney+)",
                    IconSymbol = "✨",
                    RiskBadge = "SAFE",
                    IsRecommended = true,
                    Description = "Sponsored shortcuts for third-party streaming apps silently downloaded at installation.",
                    ImpactDescription = "Safe to remove. Does not affect standalone desktop installers from Spotify or web apps.",
                    PackagePattern = "*SpotifyMusic*|*Disney*|*TikTok*"
                },
                new FriendlyBloatCategory
                {
                    Id = "phone_link",
                    Title = "Phone Link / Your Phone",
                    IconSymbol = "📱",
                    RiskBadge = "SAFE",
                    IsRecommended = false,
                    Description = "Smartphone synchronization background agent that connects your Android/iOS phone.",
                    ImpactDescription = "Safe if you do not sync phone notifications to PC. Keeps background memory free.",
                    PackagePattern = "*YourPhone*"
                },
                new FriendlyBloatCategory
                {
                    Id = "skype_teams",
                    Title = "Skype & Personal Teams Consumer",
                    IconSymbol = "📞",
                    RiskBadge = "SAFE",
                    IsRecommended = true,
                    Description = "Consumer Skype and consumer Teams preloaded into Windows taskbar.",
                    ImpactDescription = "Safe to remove. Does NOT affect Discord or Microsoft Teams for Work/School accounts.",
                    PackagePattern = "*SkypeApp*|*MicrosoftTeams*"
                },
                new FriendlyBloatCategory
                {
                    Id = "xbox_suite",
                    Title = "Xbox Gaming App & Game Bar Overlays",
                    IconSymbol = "🎯",
                    RiskBadge = "50/50 CAUTION",
                    IsRecommended = false, // Not recommended by default so Game Pass users don't accidentally break it
                    Description = "Xbox Companion, Xbox Gaming Overlay, and background speech-to-text overlays.",
                    ImpactDescription = "50/50 Caution: Keep if you play games from Xbox App or PC Game Pass! Steam and Epic users can safely purge.",
                    PackagePattern = "*Xbox*|*GamingApp*|*XboxGamingOverlay*|*XboxSpeechToTextOverlay*"
                }
            };
        }

        public static async Task<(int RemovedCount, string Log)> RemoveSelectedCategoriesAsync(IEnumerable<FriendlyBloatCategory> selectedCategories)
        {
            return await Task.Run(() =>
            {
                int count = 0;
                var log = new System.Text.StringBuilder();

                foreach (var cat in selectedCategories)
                {
                    try
                    {
                        var patterns = cat.PackagePattern.Split('|');
                        foreach (var pat in patterns)
                        {
                            string script = $"Get-AppxPackage -Name '{pat.Trim()}' -AllUsers | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue; " +
                                            $"Get-AppxProvisionedPackage -Online | Where-Object DisplayName -like '{pat.Trim()}' | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue";

                            var (code, outStr, errStr) = ProcessHelper.RunPowerShell(script, 30000);
                            log.AppendLine($"[Purged] {cat.Title} ({pat})");
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine($"[Notice] {cat.Title}: {ex.Message}");
                    }
                }

                return (count, log.ToString());
            });
        }

        public static async Task<(long BytesFreed, string Log)> CleanTemporaryFilesAsync()
        {
            return await Task.Run(() =>
            {
                long bytesFreed = 0;
                var log = new System.Text.StringBuilder();

                string userTemp = Path.GetTempPath();
                bytesFreed += CleanDirectory(userTemp, log, "User Temp");

                string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
                bytesFreed += CleanDirectory(winTemp, log, "Windows Temp");

                string prefetch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
                bytesFreed += CleanDirectory(prefetch, log, "Windows Prefetch");

                string swDist = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
                bytesFreed += CleanDirectory(swDist, log, "Windows Update Cache");

                var (dnsCode, _, _) = ProcessHelper.RunCommand("ipconfig.exe", "/flushdns");
                if (dnsCode == 0) log.AppendLine("[Network] Successfully flushed DNS resolver cache.");

                return (bytesFreed, log.ToString());
            });
        }

        private static long CleanDirectory(string dirPath, System.Text.StringBuilder log, string label)
        {
            long freed = 0;
            if (!Directory.Exists(dirPath)) return 0;

            try
            {
                var dir = new DirectoryInfo(dirPath);
                foreach (var file in dir.GetFiles())
                {
                    try
                    {
                        long size = file.Length;
                        file.Delete();
                        freed += size;
                    }
                    catch { }
                }

                foreach (var subDir in dir.GetDirectories())
                {
                    try { subDir.Delete(true); } catch { }
                }

                log.AppendLine($"[{label}] Purged temporary files in: {dirPath}");
            }
            catch { }

            return freed;
        }
    }
}
