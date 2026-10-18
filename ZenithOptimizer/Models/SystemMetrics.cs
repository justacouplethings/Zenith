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
        private bool _isCharging = true;
        private string _osName = "Windows";
        private string _osBuild = "";
        private int _optimizationScore = 0;
        private int _totalAppliedTweaks = 0;
        private int _totalAvailableTweaks = 0;

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
            set { _isLaptop = value; OnPropertyChanged(); }
        }

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

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
