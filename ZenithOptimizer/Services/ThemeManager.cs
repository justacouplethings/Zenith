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
        RoyalViolet, // Signature Zenith Purple matching logo
        AppleBlue,
        EmeraldGreen,
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
                AccentColor.RoyalViolet => (Color)ColorConverter.ConvertFromString("#9333EA"), // Zenith Purple
                AccentColor.AppleBlue => (Color)ColorConverter.ConvertFromString("#0A84FF"),
                AccentColor.EmeraldGreen => (Color)ColorConverter.ConvertFromString("#10B981"),
                AccentColor.SunsetAmber => (Color)ColorConverter.ConvertFromString("#F59E0B"),
                AccentColor.CoralCrimson => (Color)ColorConverter.ConvertFromString("#EF4444"),
                _ => (Color)ColorConverter.ConvertFromString("#9333EA")
            };

            dict["AccentBrush"] = new SolidColorBrush(accentColor);
            dict["AccentBrushLight"] = new SolidColorBrush(Color.FromArgb(40, accentColor.R, accentColor.G, accentColor.B));
            dict["AccentBrushHover"] = new SolidColorBrush(Color.FromArgb(215, accentColor.R, accentColor.G, accentColor.B));

            switch (theme)
            {
                case AppTheme.CupertinoDark:
                    SetBrush(dict, "BackgroundBrush", "#141417");
                    SetBrush(dict, "CardBrush", "#1C1C21");
                    SetBrush(dict, "CardHoverBrush", "#25252C");
                    SetBrush(dict, "CardBorderBrush", "#2C2C35");
                    SetBrush(dict, "TextPrimaryBrush", "#F8FAFC");
                    SetBrush(dict, "TextSecondaryBrush", "#94A3B8");
                    SetBrush(dict, "SegmentedTabBgBrush", "#18181E");
                    SetBrush(dict, "SegmentedTabActiveBrush", "#262632");
                    SetBrush(dict, "PillBgBrush", "#22222B");
                    break;

                case AppTheme.CupertinoLight:
                    SetBrush(dict, "BackgroundBrush", "#F8FAFC");
                    SetBrush(dict, "CardBrush", "#FFFFFF");
                    SetBrush(dict, "CardHoverBrush", "#F1F5F9");
                    SetBrush(dict, "CardBorderBrush", "#E2E8F0");
                    SetBrush(dict, "TextPrimaryBrush", "#0F172A");
                    SetBrush(dict, "TextSecondaryBrush", "#64748B");
                    SetBrush(dict, "SegmentedTabBgBrush", "#E2E8F0");
                    SetBrush(dict, "SegmentedTabActiveBrush", "#FFFFFF");
                    SetBrush(dict, "PillBgBrush", "#F1F5F9");
                    break;

                case AppTheme.MidnightSlate:
                    SetBrush(dict, "BackgroundBrush", "#090D14");
                    SetBrush(dict, "CardBrush", "#111827");
                    SetBrush(dict, "CardHoverBrush", "#1A2234");
                    SetBrush(dict, "CardBorderBrush", "#1F2937");
                    SetBrush(dict, "TextPrimaryBrush", "#F9FAFB");
                    SetBrush(dict, "TextSecondaryBrush", "#9CA3AF");
                    SetBrush(dict, "SegmentedTabBgBrush", "#0D131F");
                    SetBrush(dict, "SegmentedTabActiveBrush", "#1F2937");
                    SetBrush(dict, "PillBgBrush", "#192231");
                    break;

                case AppTheme.GraphiteMinimal:
                    SetBrush(dict, "BackgroundBrush", "#101010");
                    SetBrush(dict, "CardBrush", "#181818");
                    SetBrush(dict, "CardHoverBrush", "#202020");
                    SetBrush(dict, "CardBorderBrush", "#282828");
                    SetBrush(dict, "TextPrimaryBrush", "#EDEDED");
                    SetBrush(dict, "TextSecondaryBrush", "#888888");
                    SetBrush(dict, "SegmentedTabBgBrush", "#141414");
                    SetBrush(dict, "SegmentedTabActiveBrush", "#242424");
                    SetBrush(dict, "PillBgBrush", "#1D1D1D");
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
            dict[key] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hexColor));
        }
    }
}
