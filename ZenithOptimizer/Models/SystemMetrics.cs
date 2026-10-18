using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZenithOptimizer.Models
{
    public class SystemMetrics : INotifyPropertyChanged
    {
        private float _cpuUsage;
        private float _ramUsagePercent;
        private double _ramUsedGb;
        private double _ramTotalGb;
        private float _diskUsagePercent;
        private int _processCount;
        private string _uptimeString = "00:00:00";
        private int _batteryPercent = 100;
        private bool _isLaptop = false;
        private bool _isUserLaptopMode = false;
        private bool _isCharging = true;
        private string _osName = "Windows";
        private string _osBuild = "";
        private int _osBuildNumber = 0;
        private int _cpuCores = Environment.ProcessorCount;
        private bool _isSsd = true;
        private string _diskType = "SSD Storage";
        private int _optimizationScore = 0;
        private int _totalAppliedTweaks = 0;
        private int _totalAvailableTweaks = 0;
        private bool _isRestorePointCreated = false;
        private string _smartAuditSummary = "System ready for hardware audit.";

        public float CpuUsage
        {
            get => _cpuUsage;
            set { _cpuUsage = value; OnPropertyChanged(); }
        }

        public float RamUsagePercent
        {
            get => _ramUsagePercent;
            set { _ramUsagePercent = value; OnPropertyChanged(); }
        }

        public double RamUsedGb
        {
            get => _ramUsedGb;
            set { _ramUsedGb = value; OnPropertyChanged(); }
        }

        public double RamTotalGb
        {
            get => _ramTotalGb;
            set { _ramTotalGb = value; OnPropertyChanged(); }
        }

        public float DiskUsagePercent
        {
            get => _diskUsagePercent;
            set { _diskUsagePercent = value; OnPropertyChanged(); }
        }

        public int ProcessCount
        {
            get => _processCount;
            set { _processCount = value; OnPropertyChanged(); }
        }

        public string UptimeString
        {
            get => _uptimeString;
            set { _uptimeString = value; OnPropertyChanged(); }
        }

        public int BatteryPercent
        {
            get => _batteryPercent;
            set { _batteryPercent = value; OnPropertyChanged(); }
        }

        public bool IsLaptop
        {
            get => _isLaptop;
            set
            {
                _isLaptop = value;
                OnPropertyChanged();
            }
        }

        public bool IsUserLaptopMode
        {
            get => _isUserLaptopMode;
            set
            {
                _isUserLaptopMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DeviceModeText));
            }
        }

        public string DeviceModeText => IsUserLaptopMode ? "Laptop (Battery Profile)" : "Desktop (High Performance Profile)";

        public bool IsCharging
        {
            get => _isCharging;
            set { _isCharging = value; OnPropertyChanged(); }
        }

        public string OsName
        {
            get => _osName;
            set { _osName = value; OnPropertyChanged(); }
        }

        public string OsBuild
        {
            get => _osBuild;
            set { _osBuild = value; OnPropertyChanged(); }
        }

        public int OsBuildNumber
        {
            get => _osBuildNumber;
            set { _osBuildNumber = value; OnPropertyChanged(); }
        }

        public int CpuCores
        {
            get => _cpuCores;
            set { _cpuCores = value; OnPropertyChanged(); }
        }

        public bool IsSsd
        {
            get => _isSsd;
            set { _isSsd = value; OnPropertyChanged(); }
        }

        public string DiskType
        {
            get => _diskType;
            set { _diskType = value; OnPropertyChanged(); }
        }

        public int OptimizationScore
        {
            get => _optimizationScore;
            set { _optimizationScore = value; OnPropertyChanged(); }
        }

        public int TotalAppliedTweaks
        {
            get => _totalAppliedTweaks;
            set { _totalAppliedTweaks = value; OnPropertyChanged(); }
        }

        public int TotalAvailableTweaks
        {
            get => _totalAvailableTweaks;
            set { _totalAvailableTweaks = value; OnPropertyChanged(); }
        }

        public bool IsRestorePointCreated
        {
            get => _isRestorePointCreated;
            set { _isRestorePointCreated = value; OnPropertyChanged(); }
        }

        public string SmartAuditSummary
        {
            get => _smartAuditSummary;
            set { _smartAuditSummary = value; OnPropertyChanged(); }
        }

        public bool IsWindows11 => OsBuildNumber >= 22000;
        public bool IsWindows10 => OsBuildNumber < 22000;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
