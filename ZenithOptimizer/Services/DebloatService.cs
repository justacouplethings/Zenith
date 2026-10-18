using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ZenithOptimizer.Services
{
    public class BloatPackageItem
    {
        public string Name { get; set; } = string.Empty;
        public string PackagePattern { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = true;
    }

    public static class DebloatService
    {
        public static List<BloatPackageItem> GetDefaultBloatwareList()
        {
            return new List<BloatPackageItem>
            {
                new BloatPackageItem { Name = "Xbox Gaming & Overlay", PackagePattern = "*Xbox*|*GamingApp*", Description = "Xbox app, gaming bar, and overlays (safe if not playing Game Pass)" },
                new BloatPackageItem { Name = "Microsoft News", PackagePattern = "*BingNews*", Description = "MSN news feed consumer application" },
                new BloatPackageItem { Name = "Microsoft Weather", PackagePattern = "*BingWeather*", Description = "MSN weather forecast UWP app" },
                new BloatPackageItem { Name = "Solitaire Collection", PackagePattern = "*MicrosoftSolitaireCollection*", Description = "Ad-supported card games" },
                new BloatPackageItem { Name = "Feedback Hub", PackagePattern = "*WindowsFeedbackHub*", Description = "Diagnostic feedback prompt tool" },
                new BloatPackageItem { Name = "Get Help & Tips", PackagePattern = "*GetHelp*|*Getstarted*", Description = "Windows tips and introductory tutorials" },
                new BloatPackageItem { Name = "Skype", PackagePattern = "*SkypeApp*", Description = "Pre-installed Skype messaging app" },
                new BloatPackageItem { Name = "Clipchamp Video Editor", PackagePattern = "*Clipchamp*", Description = "Commercial web-based video editor stub" },
                new BloatPackageItem { Name = "3D Viewer & Print 3D", PackagePattern = "*3DBuilder*|*Microsoft3DViewer*", Description = "Legacy 3D model viewers" },
                new BloatPackageItem { Name = "Phone Link", PackagePattern = "*YourPhone*", Description = "Smartphone synchronization background agent" },
                new BloatPackageItem { Name = "Spotify Stub", PackagePattern = "*SpotifyMusic*", Description = "Preloaded promotional Spotify client" },
                new BloatPackageItem { Name = "Disney+ Stub", PackagePattern = "*Disney*", Description = "Preloaded promotional Disney+ app" }
            };
        }

        public static async Task<(int RemovedCount, string Log)> RemoveBloatwarePackagesAsync(IEnumerable<BloatPackageItem> selectedItems)
        {
            return await Task.Run(() =>
            {
                int count = 0;
                var log = new System.Text.StringBuilder();

                foreach (var item in selectedItems)
                {
                    try
                    {
                        var patterns = item.PackagePattern.Split('|');
                        foreach (var pat in patterns)
                        {
                            string script = $"Get-AppxPackage -Name '{pat.Trim()}' -AllUsers | Remove-AppxPackage -AllUsers -ErrorAction SilentlyContinue; " +
                                            $"Get-AppxProvisionedPackage -Online | Where-Object DisplayName -like '{pat.Trim()}' | Remove-AppxProvisionedPackage -Online -ErrorAction SilentlyContinue";
                            
                            var (code, outStr, errStr) = ProcessHelper.RunPowerShell(script, 30000);
                            log.AppendLine($"[Purge] Removed package matching {pat}");
                            count++;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.AppendLine($"[Error] Could not remove {item.Name}: {ex.Message}");
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

                // 1. User Temp
                string userTemp = Path.GetTempPath();
                bytesFreed += CleanDirectory(userTemp, log, "User Temp");

                // 2. Windows Temp
                string winTemp = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Temp");
                bytesFreed += CleanDirectory(winTemp, log, "Windows Temp");

                // 3. Prefetch
                string prefetch = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "Prefetch");
                bytesFreed += CleanDirectory(prefetch, log, "Windows Prefetch");

                // 4. SoftwareDistribution Download Cache
                string swDist = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "SoftwareDistribution", "Download");
                bytesFreed += CleanDirectory(swDist, log, "Windows Update Cache");

                // 5. DNS Flush
                var (dnsCode, _, _) = ProcessHelper.RunCommand("ipconfig.exe", "/flushdns");
                if (dnsCode == 0) log.AppendLine("[DNS] Successfully flushed DNS resolver cache.");

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
                    catch { /* File in use */ }
                }

                foreach (var subDir in dir.GetDirectories())
                {
                    try
                    {
                        subDir.Delete(true);
                    }
                    catch { /* Directory in use */ }
                }

                log.AppendLine($"[{label}] Cleaned: {dirPath}");
            }
            catch (Exception ex)
            {
                log.AppendLine($"[{label}] Notice: {ex.Message}");
            }

            return freed;
        }
    }
}
