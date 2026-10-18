using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ZenithOptimizer.Models
{
    public class OptimizationItem : INotifyPropertyChanged
    {
        private bool _isApplied;
        private bool _isSelected;
        private bool _isProcessing;
        private string _statusText = "Checking...";

        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public OptimizationCategory Category { get; set; }
        public OsTarget TargetOs { get; set; } = OsTarget.Universal;
        public DeviceTarget TargetDevice { get; set; } = DeviceTarget.Universal;
        public RiskLevel Risk { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Tradeoff { get; set; } = string.Empty;
        public bool RequiresRestart { get; set; } = false;

        public Func<bool>? CheckAction { get; set; }
        public Func<bool>? ApplyAction { get; set; }
        public Func<bool>? RevertAction { get; set; }

        public bool IsApplied
        {
            get => _isApplied;
            set
            {
                if (_isApplied != value)
                {
                    _isApplied = value;
                    OnPropertyChanged();
                    StatusText = _isApplied ? "Applied" : "Default";
                }
            }
        }

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

        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (_isProcessing != value)
                {
                    _isProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RiskBadgeText => Risk switch
        {
            RiskLevel.Safe => "SAFE",
            RiskLevel.Caution => "50/50 CAUTION",
            RiskLevel.Risky => "RISKY",
            _ => "UNKNOWN"
        };

        public string CategoryBadgeText => Category switch
        {
            OptimizationCategory.SystemPerformance => "SYSTEM",
            OptimizationCategory.PrivacyTelemetry => "PRIVACY",
            OptimizationCategory.GamingLatency => "GAMING",
            OptimizationCategory.LaptopBattery => "LAPTOP",
            OptimizationCategory.InterfaceErgonomics => "INTERFACE",
            _ => "TWEAK"
        };

        public string OsBadgeText => TargetOs switch
        {
            OsTarget.Windows11 => "Win 11 Only",
            OsTarget.Windows10 => "Win 10 Only",
            _ => "Universal"
        };

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void RefreshStatus()
        {
            if (CheckAction != null)
            {
                try
                {
                    IsApplied = CheckAction();
                }
                catch
                {
                    IsApplied = false;
                    StatusText = "Unknown";
                }
            }
        }
    }
}
