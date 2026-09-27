using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using RacingTimeClock.Services;
using RacingTimeClock.Views;

namespace RacingTimeClock;

public partial class MainWindow : Window
{
    private bool isFullScreen;

    private double windowedLeft;
    private double windowedTop;
    private double windowedWidth;
    private double windowedHeight;

    private WindowStyle windowedWindowStyle;
    private ResizeMode windowedResizeMode;

    public bool IsFullScreen => isFullScreen;

    public MainWindow()
    {
        InitializeComponent();

        SourceInitialized += MainWindow_SourceInitialized;

        AppSeasonService.Instance.PropertyChanged +=
            AppSeasonService_PropertyChanged;

        UpdateSeasonLabel();

        ShowNewRace();
    }

    private void AppSeasonService_PropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (e.PropertyName ==
            nameof(AppSeasonService.CurrentSeason))
        {
            UpdateSeasonLabel();
        }
    }

    private void UpdateSeasonLabel()
    {
        SeasonLabel.Text =
            AppSeasonService.Instance.CurrentSeason != null
                ? $"Season {AppSeasonService.Instance.CurrentSeason.Name}"
                : "Season";
    }

    private void SetActiveNavigation(
        string page)
    {
        NewRaceNavButton.Tag =
            page == "NewRace"
                ? "Active"
                : null;

        RacersNavButton.Tag =
            page == "Racers"
                ? "Active"
                : null;

        RacesLogNavButton.Tag =
            page == "RacesLog"
                ? "Active"
                : null;

        SettingsNavButton.Tag =
            page == "Settings"
                ? "Active"
                : null;
    }

    private void ShowNewRace()
    {
        SetActiveNavigation("NewRace");

        NewRaceView newRaceView =
            new NewRaceView();

        newRaceView.RaceStarted +=
            OnRaceStarted;

        PageContent.Content =
            newRaceView;
    }

    private void OnRaceStarted(
        string distance,
        string raceType,
        string competitionType,
        int racerCount,
        int seasonId,
        List<NewRaceView.RacerSelectionItem> racers)
    {
        PageContent.Content =
            new RaceView(
                distance,
                raceType,
                competitionType,
                racerCount,
                seasonId,
                racers);
    }

    private void NewRace_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowNewRace();
    }

    private void Racers_Click(
        object sender,
        RoutedEventArgs e)
    {
        ShowRacers();
    }

    private void ShowRacers()
    {
        SetActiveNavigation("Racers");

        RacersView racersView =
            new RacersView();

        racersView.RacerSelected +=
            RacersView_RacerSelected;

        PageContent.Content =
            racersView;
    }

    private void RacersView_RacerSelected(
        object? sender,
        int racerDbId)
    {
        SetActiveNavigation("Racers");

        RacerDetailsView detailsView =
            new RacerDetailsView(racerDbId);

        detailsView.BackRequested +=
            RacerDetailsView_BackRequested;

        PageContent.Content =
            detailsView;
    }

    private void RacerDetailsView_BackRequested(
        object? sender,
        EventArgs e)
    {
        ShowRacers();
    }

    private void RacesLog_Click(
        object sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation("RacesLog");

        PageContent.Content =
            new RacesLogView();
    }
    private void SettingsView_FullScreenChanged(
        object? sender,
        bool enabled)
    {
        SetFullScreen(enabled);
    }
    private void Settings_Click(
        object sender,
        RoutedEventArgs e)
    {
        SetActiveNavigation("Settings");

        SettingsView settingsView =
            new SettingsView();

        settingsView.FullScreenChanged +=
            SettingsView_FullScreenChanged;

        PageContent.Content =
            settingsView;
    }

    private void MainWindow_SourceInitialized(
        object? sender,
        EventArgs e)
    {
        SetFullScreen(true);
    }

    public void SetFullScreen(bool enabled)
    {
        if (enabled == isFullScreen)
            return;

        IntPtr handle =
            new System.Windows.Interop.WindowInteropHelper(this).Handle;

        if (enabled)
        {
            windowedLeft = Left;
            windowedTop = Top;
            windowedWidth = Width;
            windowedHeight = Height;
            windowedWindowStyle = WindowStyle;
            windowedResizeMode = ResizeMode;

            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            WindowState = WindowState.Normal;

            IntPtr monitor =
                MonitorFromWindow(
                    handle,
                    MONITOR_DEFAULTTONEAREST);

            MONITORINFO monitorInfo = new MONITORINFO
            {
                cbSize =
                    System.Runtime.InteropServices.Marshal.SizeOf<MONITORINFO>()
            };

            if (!GetMonitorInfo(monitor, ref monitorInfo))
            {
                WindowStyle = windowedWindowStyle;
                ResizeMode = windowedResizeMode;
                return;
            }

            RECT bounds = monitorInfo.rcMonitor;

            SetWindowPos(
                handle,
                IntPtr.Zero,
                bounds.Left,
                bounds.Top,
                bounds.Right - bounds.Left,
                bounds.Bottom - bounds.Top,
                SWP_NOZORDER | SWP_FRAMECHANGED);

            isFullScreen = true;
        }
        else
        {
            WindowStyle = windowedWindowStyle;
            ResizeMode = windowedResizeMode;
            WindowState = WindowState.Normal;

            Left = windowedLeft;
            Top = windowedTop;
            Width = windowedWidth;
            Height = windowedHeight;

            SetWindowPos(
                handle,
                IntPtr.Zero,
                (int)Math.Round(windowedLeft),
                (int)Math.Round(windowedTop),
                (int)Math.Round(windowedWidth),
                (int)Math.Round(windowedHeight),
                SWP_NOZORDER | SWP_FRAMECHANGED);

            isFullScreen = false;
        }
    }

    private const uint MONITOR_DEFAULTTONEAREST = 2;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;

    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [System.Runtime.InteropServices.StructLayout(
        System.Runtime.InteropServices.LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;
    }

    [System.Runtime.InteropServices.DllImport(
        "user32.dll",
        SetLastError = true)]
    private static extern IntPtr MonitorFromWindow(
        IntPtr hWnd,
        uint dwFlags);

    [System.Runtime.InteropServices.DllImport(
        "user32.dll",
        SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(
        System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(
        IntPtr hMonitor,
        ref MONITORINFO lpmi);

    [System.Runtime.InteropServices.DllImport(
        "user32.dll",
        SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(
        System.Runtime.InteropServices.UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int X,
        int Y,
        int cx,
        int cy,
        uint uFlags);
}





