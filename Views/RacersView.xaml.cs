﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using RacingTimeClock.Models;
using RacingTimeClock.Services;

namespace RacingTimeClock.Views;

public partial class RacersView : UserControl
{
    private readonly ObservableCollection<RacerDisplayItem> racers = new();

    private RacerFilterCriteria filterCriteria = new();

    private string sortProperty = nameof(RacerDisplayItem.Name);
    private ListSortDirection sortDirection = ListSortDirection.Ascending;

    private ScrollViewer? racerScrollViewer;
    private bool updatingRacerScrollBar;

    public RacersView()
    {
        InitializeComponent();
        Loaded += RacersView_Loaded;
    }

    private async void RacersView_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        await LoadRacersAsync();
    }

    private async Task LoadRacersAsync()
    {
        try
        {
            DatabaseService databaseService = new();

            List<Racer> racerList =
                await databaseService.GetRacersAsync();

            racers.Clear();

            Season? season =
                AppSeasonService.Instance.CurrentSeason;

            foreach (Racer racer in racerList)
            {
                string category =
                    season != null
                        ? SeasonService.GetCategory(racer, season)
                        : "Unknown";

                racers.Add(
                    new RacerDisplayItem
                    {
                        Racer = racer,
                        CategoryDisplay = category
                    });
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not load racers.\n\n{ex.Message}",
                "Racers Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ApplyFilters()
    {
        if (!IsInitialized ||
            SearchTextBox == null ||
            RacersDataGrid == null)
        {
            return;
        }

        IEnumerable<RacerDisplayItem> filtered = racers;

        string search =
            SearchTextBox.Text?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(search))
        {
            filtered =
                filtered.Where(
                    r =>
                        r.Name.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        r.RacingNumber.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase)
                        ||
                        r.Racer.RacerId.Contains(
                            search,
                            StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(
                filterCriteria.Gender,
                "All",
                StringComparison.OrdinalIgnoreCase))
        {
            filtered =
                filtered.Where(
                    r => string.Equals(
                        r.GenderDisplay,
                        filterCriteria.Gender,
                        StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(
                filterCriteria.Category,
                "All",
                StringComparison.OrdinalIgnoreCase))
        {
            filtered =
                filtered.Where(
                    r => string.Equals(
                        r.CategoryDisplay,
                        filterCriteria.Category,
                        StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(
                filterCriteria.Status,
                "All",
                StringComparison.OrdinalIgnoreCase))
        {
            bool active =
                string.Equals(
                    filterCriteria.Status,
                    "Active",
                    StringComparison.OrdinalIgnoreCase);

            filtered =
                filtered.Where(
                    r => r.IsActive == active);
        }

        if (filterCriteria.MinBirthYear.HasValue)
        {
            int minimumBirthYear =
                filterCriteria.MinBirthYear.Value;

            filtered =
                filtered.Where(
                    r =>
                        r.BirthYear >= minimumBirthYear);
        }

        if (filterCriteria.MaxBirthYear.HasValue)
        {
            int maximumBirthYear =
                filterCriteria.MaxBirthYear.Value;

            filtered =
                filtered.Where(
                    r =>
                        r.BirthYear <= maximumBirthYear);
        }

        if (filterCriteria.MinRacingNumber.HasValue)
        {
            int minimum =
                filterCriteria.MinRacingNumber.Value;

            filtered =
                filtered.Where(
                    r =>
                        TryGetRacingNumber(
                            r,
                            out int number)
                        &&
                        number >= minimum);
        }

        if (filterCriteria.MaxRacingNumber.HasValue)
        {
            int maximum =
                filterCriteria.MaxRacingNumber.Value;

            filtered =
                filtered.Where(
                    r =>
                        TryGetRacingNumber(
                            r,
                            out int number)
                        &&
                        number <= maximum);
        }

        filtered =
            SortRacers(filtered);

        RacersDataGrid.ItemsSource =
            filtered.ToList();
        UpdateRacersTableHeight();

        Dispatcher.BeginInvoke(
            new Action(UpdateRacersTableHeight),
            System.Windows.Threading.DispatcherPriority.Render);

        UpdateRacersTableHeight();
UpdateFilterButtonText();
        UpdateSortIndicators();
    }

    private IEnumerable<RacerDisplayItem> SortRacers(
        IEnumerable<RacerDisplayItem> source)
    {
        return sortProperty switch
        {
            nameof(RacerDisplayItem.RacingNumber) =>
                sortDirection == ListSortDirection.Ascending
                    ? source.OrderBy(
                        r =>
                            TryGetRacingNumber(
                                r,
                                out int n)
                                ? n
                                : int.MaxValue)
                    : source.OrderByDescending(
                        r =>
                            TryGetRacingNumber(
                                r,
                                out int n)
                                ? n
                                : int.MinValue),

            nameof(RacerDisplayItem.GenderDisplay) =>
                sortDirection == ListSortDirection.Ascending
                    ? source.OrderBy(r => r.GenderDisplay)
                    : source.OrderByDescending(r => r.GenderDisplay),

            nameof(RacerDisplayItem.BirthYear) =>
                sortDirection == ListSortDirection.Ascending
                    ? source.OrderBy(r => r.BirthYear)
                    : source.OrderByDescending(r => r.BirthYear),

            nameof(RacerDisplayItem.CategoryDisplay) =>
                sortDirection == ListSortDirection.Ascending
                    ? source.OrderBy(r => r.CategoryDisplay)
                    : source.OrderByDescending(r => r.CategoryDisplay),

            nameof(RacerDisplayItem.StatusDisplay) =>
                sortDirection == ListSortDirection.Ascending
                    ? source.OrderBy(r => r.IsActive)
                    : source.OrderByDescending(r => r.IsActive),

            _ =>
                sortDirection == ListSortDirection.Ascending
                    ? source.OrderBy(r => r.Name)
                    : source.OrderByDescending(r => r.Name)
        };
    }

    private void RacersDataGrid_Sorting(
        object sender,
        DataGridSortingEventArgs e)
    {
        e.Handled = true;

        string property =
            e.Column.SortMemberPath;

        if (string.IsNullOrWhiteSpace(property))
            return;

        if (string.Equals(
                sortProperty,
                property,
                StringComparison.Ordinal))
        {
            sortDirection =
                sortDirection ==
                    ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
        }
        else
        {
            sortProperty = property;
            sortDirection =
                ListSortDirection.Ascending;
        }

        ApplyFilters();
    }

    private void UpdateSortIndicators()
    {
        if (RacersDataGrid == null)
            return;

        foreach (DataGridColumn column
                 in RacersDataGrid.Columns)
        {
            column.SortDirection =
                string.Equals(
                    column.SortMemberPath,
                    sortProperty,
                    StringComparison.Ordinal)
                    ? sortDirection
                    : null;
        }
    }

    private void RacersDataGrid_Loaded(
        object sender,
        RoutedEventArgs e)
    {

        racerScrollViewer =
            FindVisualChild<ScrollViewer>(
                RacersDataGrid);

        if (racerScrollViewer != null)
        {
            racerScrollViewer.ScrollChanged -=
                RacerScrollViewer_ScrollChanged;

            racerScrollViewer.ScrollChanged +=
                RacerScrollViewer_ScrollChanged;
        }

        Dispatcher.BeginInvoke(
            new Action(UpdateRacerScrollBar),
            System.Windows.Threading.DispatcherPriority.Loaded);
    }

    private void RacerScrollViewer_ScrollChanged(
    object? sender,
    ScrollChangedEventArgs e)
{
    UpdateRacerScrollBar();
}
private void RacerScrollBar_ValueChanged(
        object sender,
        RoutedPropertyChangedEventArgs<double> e)
    {
        if (updatingRacerScrollBar ||
            racerScrollViewer == null)
        {
            return;
        }

        racerScrollViewer.ScrollToVerticalOffset(
            e.NewValue);
    }

    private void RacersDataGrid_SizeChanged(
    object sender,
    SizeChangedEventArgs e)
{
    UpdateRacersDataGridClip();

    Dispatcher.BeginInvoke(
        new Action(() =>
        {
            UpdateRacersTableHeight();
            UpdateRacerScrollBar();
            UpdateRacersDataGridClip();
        }),
        System.Windows.Threading.DispatcherPriority.Render);
}
private void UpdateRacersTableHeight()
{
    if (RacersDataGrid == null ||
        RacersTableBorder == null)
    {
        return;
    }

    Grid? parentGrid =
        RacersTableBorder.Parent as Grid;

    if (parentGrid == null ||
        parentGrid.ActualHeight <= 0)
    {
        return;
    }

    double availableHeight =
        parentGrid.ActualHeight;

    RacersDataGrid.Height =
        availableHeight;

    RacersTableBorder.Height =
        availableHeight;

    RacerScrollBar.Height =
        Math.Max(
            1,
            availableHeight - 50);

    RacersTableBorder.ClipToBounds = true;
}
private void UpdateRacerScrollBar()
    {
        if (RacerScrollBar == null ||
            racerScrollViewer == null)
        {
            return;
        }

        double maximum =
            Math.Max(
                0,
                racerScrollViewer.ExtentHeight -
                racerScrollViewer.ViewportHeight);

        updatingRacerScrollBar = true;

        RacerScrollBar.Maximum = maximum;
        RacerScrollBar.ViewportSize = Math.Max(1, RacerScrollBar.Maximum / 3.0);

        RacerScrollBar.LargeChange =
            Math.Max(
                1,
                racerScrollViewer.ViewportHeight);

        RacerScrollBar.SmallChange = 1;

        RacerScrollBar.Value =
            Math.Min(
                maximum,
                racerScrollViewer.VerticalOffset);

        RacerScrollBar.Visibility =
            maximum > 0
                ? Visibility.Visible
                : Visibility.Collapsed;

        updatingRacerScrollBar = false;
    }

    private void UpdateRacersDataGridClip()
{
    if (RacersDataGrid.ActualWidth <= 0 ||
        RacersDataGrid.ActualHeight <= 0)
    {
        return;
    }

    RacersDataGrid.Clip =
        new System.Windows.Media.RectangleGeometry(
            new System.Windows.Rect(
                0,
                0,
                RacersDataGrid.ActualWidth,
                RacersDataGrid.ActualHeight),
            10,
            10);
}

private void RacersDataGrid_PreviewMouseWheel(
    object sender,
    MouseWheelEventArgs e)
{
    if (racerScrollViewer == null)
    {
        return;
    }

    const double pixelsPerWheelDelta = 0.25;

    double targetOffset =
        racerScrollViewer.VerticalOffset -
        (e.Delta * pixelsPerWheelDelta);

    targetOffset =
        Math.Max(
            0,
            Math.Min(
                racerScrollViewer.ScrollableHeight,
                targetOffset));

    racerScrollViewer.ScrollToVerticalOffset(
        targetOffset);

    e.Handled = true;
}
private static T? FindVisualChild<T>(
        DependencyObject? parent)
        where T : DependencyObject
    {
        if (parent == null)
            return null;

        int childCount =
            System.Windows.Media.VisualTreeHelper
                .GetChildrenCount(parent);

        for (int i = 0; i < childCount; i++)
        {
            DependencyObject child =
                System.Windows.Media.VisualTreeHelper
                    .GetChild(parent, i);

            if (child is T result)
                return result;

            T? descendant =
                FindVisualChild<T>(child);

            if (descendant != null)
                return descendant;
        }

        return null;
    }
    private void FilterButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        RacerFilterCriteria initial =
            filterCriteria.Clone();

        RacerFilterDialog dialog =
            new RacerFilterDialog(initial)
            {
                Owner = Window.GetWindow(this)
            };

        if (dialog.ShowDialog() == true)
        {
            filterCriteria =
                dialog.Criteria;

            ApplyFilters();
        }
    }

    private void UpdateFilterButtonText()
    {
        if (FilterButton == null)
            return;

        int count =
            filterCriteria.ActiveCount;

        FilterButton.Content =
            count == 0
                ? "FILTER"
                : $"FILTER ({count})";
    }


    private static bool TryGetRacingNumber(
        RacerDisplayItem item,
        out int number)
    {
        return int.TryParse(
            item.RacingNumber,
            out number);
    }

    private void SearchBorder_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        SearchTextBox.Focus();
        e.Handled = true;
    }

    private void SearchTextBox_TextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (SearchPlaceholder != null)
        {
            SearchPlaceholder.Visibility =
                string.IsNullOrWhiteSpace(
                    SearchTextBox.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
        }

        ApplyFilters();
    }

    private void AddRacerButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        AddRacerDialog dialog =
            new AddRacerDialog
            {
                Owner = Window.GetWindow(this)
            };

        bool? result =
            dialog.ShowDialog();

        if (result == true)
            _ = LoadRacersAsync();
    }

    private async void RacersDataGrid_MouseLeftButtonUp(
        object sender,
        MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left)
            return;

        DependencyObject? source =
            e.OriginalSource as DependencyObject;

        DataGridRow? row =
            FindParent<DataGridRow>(source);

        if (row?.Item is not RacerDisplayItem item)
            return;

        e.Handled = true;

        try
        {
            RacerDetailsDialog dialog =
                new RacerDetailsDialog(item.Racer.Id)
                {
                    Owner = Window.GetWindow(this)
                };

            dialog.ShowDialog();

            await LoadRacersAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Could not open racer details.\n\n{ex}",
                "Racer Details Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private static T? FindParent<T>(
        DependencyObject? child)
        where T : DependencyObject
    {
        while (child != null)
        {
            if (child is T parent)
                return parent;

            child =
                System.Windows.Media.VisualTreeHelper
                    .GetParent(child);
        }

        return null;
    }
}

public class RacerDisplayItem
{
    public Racer Racer { get; set; } = null!;

    public int Id => Racer.Id;

    public string Name => Racer.Name;

    public string RacingNumber =>
        Racer.RacingNumber;

    public string GenderDisplay =>
        Racer.IsMale ? "Male" : "Female";

    public int BirthYear =>
        Racer.YearOfBirth;

    public string CategoryDisplay { get; set; } =
        string.Empty;

    public bool IsActive =>
        Racer.IsActive;

    public string StatusDisplay =>
        Racer.IsActive
            ? "Active"
            : "Not Active";
}



















