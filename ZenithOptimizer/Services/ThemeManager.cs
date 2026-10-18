using System;
using System.Windows;
using System.Windows.Media;

namespace ZenithOptimizer.Services
{
    public enum AppTheme
    {
        CupertinoDark,
        CupertinoLight,
        MidnightSlate,
        GraphiteMinimal
    }

    public enum AccentColor
    {
        AppleBlue,
        EmeraldGreen,
        RoyalViolet,
        SunsetAmber,
        CoralCrimson
    }

    public static class ThemeManager
    {
        public static void ApplyTheme(AppTheme theme, AccentColor accent)
        {
            var dict = Application.Current.Resources;

            // Accent brush
            Color accentColor = accent switch
            {
                AccentColor.AppleBlue => (Color)ColorConverter.ConvertFromString("#0A84FF"),
                AccentColor.EmeraldGreen => (Color)ColorConverter.ConvertFromString("#10B981"),
                AccentColor.RoyalViolet => (Color)ColorConverter.ConvertFromString("#8B5CF6"),
                AccentColor.SunsetAmber => (Color)ColorConverter.ConvertFromString("#F59E0B"),
                AccentColor.CoralCrimson => (Color)ColorConverter.ConvertFromString("#EF4444"),
                _ => (Color)ColorConverter.ConvertFromString("#0A84FF")
            };

            dict["AccentBrush"] = new SolidColorBrush(accentColor);
            dict["AccentBrushLight"] = new SolidColorBrush(Color.FromArgb(40, accentColor.R, accentColor.G, accentColor.B));
            dict["AccentBrushHover"] = new SolidColorBrush(Color.FromArgb(200, accentColor.R, accentColor.G, accentColor.B));

            switch (theme)
            {
                case AppTheme.CupertinoDark:
                    SetBrush(dict, "BackgroundBrush", "#18181B");
                    SetBrush(dict, "CardBrush", "#222227");
                    SetBrush(dict, "CardHoverBrush", "#292930");
                    SetBrush(dict, "CardBorderBrush", "#2E2E35");
                    SetBrush(dict, "TextPrimaryBrush", "#F4F4F5");
                    SetBrush(dict, "TextSecondaryBrush", "#A1A1AA");
                    SetBrush(dict, "SegmentedTabBgBrush", "#1F1F24");
                    SetBrush(dict, "SegmentedTabActiveBrush", "#2E2E36");
                    SetBrush(dict, "PillBgBrush", "#27272A");
                    break;

                case AppTheme.CupertinoLight:
                    SetBrush(dict, "BackgroundBrush", "#F4F4F7");
                    SetBrush(dict, "CardBrush", "#FFFFFF");
                    SetBrush(dict, "CardHoverBrush", "#FAFAFC");
                    SetBrush(dict, "CardBorderBrush", "#E4E4E8");
                    SetBrush(dict, "TextPrimaryBrush", "#18181B");
                    SetBrush(dict, "TextSecondaryBrush", "#71717A");
                    SetBrush(dict, "SegmentedTabBgBrush", "#EAEAEF");
                    SetBrush(dict, "SegmentedTabActiveBrush", "#FFFFFF");
                    SetBrush(dict, "PillBgBrush", "#E4E4E7");
                    break;

                case AppTheme.MidnightSlate:
                    SetBrush(dict, "BackgroundBrush", "#0B0F17");
                    SetBrush(dict, "CardBrush", "#131B26");
                    SetBrush(dict, "CardHoverBrush", "#1A2433");
                    SetBrush(dict, "CardBorderBrush", "#1E293B");
                    SetBrush(dict, "TextPrimaryBrush", "#F8FAFC");
                    SetBrush(dict, "TextSecondaryBrush", "#94A3B8");
                    SetBrush(dict, "SegmentedTabBgBrush", "#0F1622");
                    SetBrush(dict, "SegmentedTabActiveBrush", "#1E293B");
                    SetBrush(dict, "PillBgBrush", "#172231");
                    break;

                case AppTheme.GraphiteMinimal:
                    SetBrush(dict, "BackgroundBrush", "#111111");
                    SetBrush(dict, "CardBrush", "#1C1C1C");
                    SetBrush(dict, "CardHoverBrush", "#242424");
                    SetBrush(dict, "CardBorderBrush", "#2C2C2C");
                    SetBrush(dict, "TextPrimaryBrush", "#E5E5E5");
                    SetBrush(dict, "TextSecondaryBrush", "#888888");
                    SetBrush(dict, "SegmentedTabBgBrush", "#171717");
                    SetBrush(dict, "SegmentedTabActiveBrush", "#2A2A2A");
                    SetBrush(dict, "PillBgBrush", "#222222");
                    break;
            }

            // Risk badge brushes
            SetBrush(dict, "SafeBadgeBrush", "#10B981");
            SetBrush(dict, "SafeBadgeBgBrush", "#1F10B981");

            SetBrush(dict, "CautionBadgeBrush", "#F59E0B");
            SetBrush(dict, "CautionBadgeBgBrush", "#1FF59E0B");

            SetBrush(dict, "RiskyBadgeBrush", "#EF4444");
            SetBrush(dict, "RiskyBadgeBgBrush", "#1FEF4444");
        }

        private static void SetBrush(ResourceDictionary dict, string key, string hexColor)
        {
            if (hexColor.StartsWith("#1F") && hexColor.Length == 9)
            {
                byte a = Convert.ToByte(hexColor.Substring(1, 2), 16);
                byte r = Convert.ToByte(hexColor.Substring(3, 2), 16);
                byte g = Convert.ToByte(hexColor.Substring(5, 2), 16);
                byte b = Convert.ToByte(hexColor.Substring(7, 2), 16);
                dict[key] = new SolidColorBrush(Color.FromArgb(a, r, g, b));
            }
            else
            {
                dict[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
            }
        }
    }
}
