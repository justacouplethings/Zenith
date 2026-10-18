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

        public string RiskTooltip => Risk switch
        {
            RiskLevel.Safe => "Recommended for all users. 100% reversible with zero adverse side effects.",
            RiskLevel.Caution => "Trade-off exists. Disables convenience features that some users might rely on.",
            RiskLevel.Risky => "High-impact tweak for gaming or extreme performance. Read trade-offs before applying.",
            _ => ""
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
