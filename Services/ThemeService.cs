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

public static class ThemeService
{
    private static readonly string SettingsDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "RacingTimeClock");

    private static readonly string ThemeFile =
        Path.Combine(SettingsDirectory, "theme.txt");

    public static ThemeMode CurrentMode { get; private set; } =
        ThemeMode.System;

    public static void Initialize()
    {
        ThemeMode mode = LoadThemeMode();
        Apply(mode);
    }

    public static void Apply(ThemeMode mode)
    {
        CurrentMode = mode;

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
            CreateBrush("#CE1126");

        resources["AccentHoverBrush"] =
            CreateBrush("#E31B32");

        resources["GoldBrush"] =
            CreateBrush("#E5B93F");

        resources["ButtonTextBrush"] =
            CreateBrush("#FFFFFF");

        resources["ButtonSecondaryBrush"] =
            CreateBrush(dark ? "#2A3035" : "#E7EAEE");

        resources["ButtonSecondaryTextBrush"] =
            CreateBrush(dark ? "#F1F3F4" : "#20232A");

        resources["ButtonGlowColor"] =
            CreateColor(dark ? "#CE1126" : "#CE1126");

        resources["DangerBrush"] =
            CreateBrush("#C62828");

        resources["SuccessBrush"] =
            CreateBrush("#00844A");

        SaveThemeMode(mode);
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

    private static void SaveThemeMode(ThemeMode mode)
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);

            File.WriteAllText(
                ThemeFile,
                mode.ToString());
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
