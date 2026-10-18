using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using ZenithOptimizer.Models;

namespace ZenithOptimizer.Converters
{
    public class TabVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null) return Visibility.Collapsed;
            string currentTab = value.ToString() ?? "";
            string param = parameter.ToString() ?? "";

            if (param.Equals("List", StringComparison.OrdinalIgnoreCase))
            {
                // List view is active for Win11, Win10, and Universal
                bool isList = currentTab.Equals("Win11", StringComparison.OrdinalIgnoreCase) ||
                              currentTab.Equals("Win10", StringComparison.OrdinalIgnoreCase) ||
                              currentTab.Equals("Universal", StringComparison.OrdinalIgnoreCase);
                return isList ? Visibility.Visible : Visibility.Collapsed;
            }

            var allowedTabs = param.Split(new[] { ',', '|' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(t => t.Trim());

            return allowedTabs.Any(t => t.Equals(currentTab, StringComparison.OrdinalIgnoreCase))
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RiskToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RiskLevel risk)
            {
                return risk switch
                {
                    RiskLevel.Safe => Application.Current.Resources["SafeBadgeBrush"],
                    RiskLevel.Caution => Application.Current.Resources["CautionBadgeBrush"],
                    RiskLevel.Risky => Application.Current.Resources["RiskyBadgeBrush"],
                    _ => Application.Current.Resources["TextSecondaryBrush"]
                };
            }
            return Application.Current.Resources["TextSecondaryBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RiskToBgConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RiskLevel risk)
            {
                return risk switch
                {
                    RiskLevel.Safe => Application.Current.Resources["SafeBadgeBgBrush"],
                    RiskLevel.Caution => Application.Current.Resources["CautionBadgeBgBrush"],
                    RiskLevel.Risky => Application.Current.Resources["RiskyBadgeBgBrush"],
                    _ => Application.Current.Resources["PillBgBrush"]
                };
            }
            return Application.Current.Resources["PillBgBrush"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b) return !b;
            return false;
        }
    }
}
