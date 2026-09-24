using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace RacingTimeClock.Services;

public enum ThemeMode
{
    Light,
    Dark,
    System
}

public enum AccentColor
{
    Red,
    Blue,
    Green,
    Purple,
    Orange
}

public static class ThemeService
{
    private static readonly string SettingsDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "RacingTimeClock");

    private static readonly string ThemeFile =
        Path.Combine(SettingsDirectory, "theme.txt");

    private static readonly string AccentFile =
        Path.Combine(SettingsDirectory, "accent.txt");

    public static ThemeMode CurrentMode { get; private set; } =
        ThemeMode.System;

    public static AccentColor CurrentAccent { get; private set; } =
        AccentColor.Red;

    public static void Initialize()
    {
        ThemeMode mode = LoadThemeMode();
        AccentColor accent = LoadAccentColor();

        Apply(mode, accent);
    }

    public static void Apply(
        ThemeMode mode,
        AccentColor? accent = null)
    {
        CurrentMode = mode;

        if (accent.HasValue)
            CurrentAccent = accent.Value;

        bool dark =
            mode == ThemeMode.Dark ||
            (mode == ThemeMode.System && IsSystemDark());

        ResourceDictionary resources =
            Application.Current.Resources;

        resources["PageBackgroundBrush"] =
            CreateBrush(dark ? "#111315" : "#F4F5F7");

        resources["SurfaceBrush"] =
            CreateBrush(dark ? "#1B1F22" : "#FFFFFF");

        resources["SurfaceSecondaryBrush"] =
            CreateBrush(dark ? "#24292D" : "#EEF0F3");

        resources["TextPrimaryBrush"] =
            CreateBrush(dark ? "#F5F5F5" : "#20232A");

        resources["TextSecondaryBrush"] =
            CreateBrush(dark ? "#AEB4BA" : "#6B7280");

        resources["TextMutedBrush"] =
            CreateBrush(dark ? "#858C93" : "#777777");

        resources["BorderBrush"] =
            CreateBrush(dark ? "#343A40" : "#D9DDE2");

        resources["AccentBrush"] =
            CreateBrush(GetAccentHex(CurrentAccent));

        resources["AccentHoverBrush"] =
            CreateBrush(GetAccentHoverHex(CurrentAccent));

        resources["AccentSoftBrush"] =
            CreateBrush(
                GetAccentSoftHex(
                    CurrentAccent,
                    dark));

        resources["GoldBrush"] =
            CreateBrush("#E5B93F");

        resources["ButtonTextBrush"] =
            CreateBrush("#FFFFFF");

        resources["ButtonSecondaryBrush"] =
            CreateBrush(
                dark ? "#2A3035" : "#E7EAEE");

        resources["ButtonSecondaryTextBrush"] =
            CreateBrush(
                dark ? "#F1F3F4" : "#20232A");

        resources["ButtonGlowColor"] =
            CreateColor("#000000");

        resources["ButtonShadowColor"] =
            CreateColor("#000000");

        resources["DangerBrush"] =
            CreateBrush(
                dark ? "#E05252" : "#C62828");

        resources["SuccessBrush"] =
            CreateBrush(
                dark ? "#29B978" : "#00844A");

        resources["SidebarBackgroundBrush"] =
            CreateBrush(
                dark ? "#0C0F11" : "#101820");

        resources["SidebarHoverBrush"] =
            CreateBrush(
                dark ? "#20272C" : "#1F2A33");

        resources["SidebarTextBrush"] =
            CreateBrush("#AEB6C0");

        resources["SidebarTextHoverBrush"] =
            CreateBrush("#FFFFFF");

        SaveThemeMode(mode);
        SaveAccentColor(CurrentAccent);
    }

    public static void SetAccent(
        AccentColor accent)
    {
        Apply(
            CurrentMode,
            accent);
    }

    public static string GetAccentHex(
        AccentColor accent)
    {
        return accent switch
        {
            AccentColor.Blue => "#2878D0",
            AccentColor.Green => "#16845A",
            AccentColor.Purple => "#7856C7",
            AccentColor.Orange => "#D97720",
            _ => "#CE1126"
        };
    }

    private static string GetAccentHoverHex(
        AccentColor accent)
    {
        return accent switch
        {
            AccentColor.Blue => "#3489E5",
            AccentColor.Green => "#1D9A6A",
            AccentColor.Purple => "#8968D8",
            AccentColor.Orange => "#E6872F",
            _ => "#E31B32"
        };
    }

    private static string GetAccentSoftHex(
        AccentColor accent,
        bool dark)
    {
        if (dark)
        {
            return accent switch
            {
                AccentColor.Blue => "#182B40",
                AccentColor.Green => "#153027",
                AccentColor.Purple => "#28203D",
                AccentColor.Orange => "#392717",
                _ => "#3A171D"
            };
        }

        return accent switch
        {
            AccentColor.Blue => "#EAF3FF",
            AccentColor.Green => "#E9F6F0",
            AccentColor.Purple => "#F1ECFF",
            AccentColor.Orange => "#FFF2E5",
            _ => "#FFF0EC"
        };
    }

    private static bool IsSystemDark()
    {
        try
        {
            using RegistryKey? key =
                Registry.CurrentUser.OpenSubKey(
                    @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");

            object? value =
                key?.GetValue("AppsUseLightTheme");

            return value is int intValue &&
                   intValue == 0;
        }
        catch
        {
            return false;
        }
    }

    private static ThemeMode LoadThemeMode()
    {
        try
        {
            if (!File.Exists(ThemeFile))
                return ThemeMode.System;

            string value =
                File.ReadAllText(ThemeFile).Trim();

            if (Enum.TryParse(
                    value,
                    true,
                    out ThemeMode mode))
            {
                return mode;
            }
        }
        catch
        {
        }

        return ThemeMode.System;
    }

    private static AccentColor LoadAccentColor()
    {
        try
        {
            if (!File.Exists(AccentFile))
                return AccentColor.Red;

            string value =
                File.ReadAllText(AccentFile).Trim();

            if (Enum.TryParse(
                    value,
                    true,
                    out AccentColor accent))
            {
                return accent;
            }
        }
        catch
        {
        }

        return AccentColor.Red;
    }

    private static void SaveThemeMode(
        ThemeMode mode)
    {
        try
        {
            Directory.CreateDirectory(
                SettingsDirectory);

            File.WriteAllText(
                ThemeFile,
                mode.ToString());
        }
        catch
        {
        }
    }

    private static void SaveAccentColor(
        AccentColor accent)
    {
        try
        {
            Directory.CreateDirectory(
                SettingsDirectory);

            File.WriteAllText(
                AccentFile,
                accent.ToString());
        }
        catch
        {
        }
    }

    private static SolidColorBrush CreateBrush(
        string hex)
    {
        return new SolidColorBrush(
            (Color)ColorConverter.ConvertFromString(hex)!);
    }

    private static Color CreateColor(
        string hex)
    {
        return (Color)ColorConverter.ConvertFromString(hex)!;
    }
}
