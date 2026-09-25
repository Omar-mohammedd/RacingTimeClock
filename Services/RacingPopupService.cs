using System;
using System.Linq;
using System.Windows;
using RacingTimeClock.Controls;

namespace RacingTimeClock.Services;

public static class RacingPopupService
{
    public static MessageBoxResult Show(
        string message,
        string title,
        MessageBoxButton buttons,
        MessageBoxImage image,
        MessageBoxResult defaultResult)
    {
        try
        {
            return Show(
                message,
                title,
                buttons,
                image);
        }
        catch (Exception ex)
        {
            return ShowFallback(
                message,
                title,
                buttons,
                image,
                ex);
        }
    }

    public static MessageBoxResult Show(
        string message,
        string title,
        MessageBoxButton buttons,
        MessageBoxImage image)
    {
        try
        {
            Window? owner =
                Application.Current?
                    .Windows
                    .OfType<Window>()
                    .FirstOrDefault(
                        window =>
                            window.IsVisible &&
                            window.IsActive);

            RacingPopupWindow popup =
                new(
                    title,
                    message,
                    buttons,
                    image,
                    buttons == MessageBoxButton.YesNo ||
                    buttons == MessageBoxButton.YesNoCancel
                        ? "CONFIRM"
                        : "OK",
                    image == MessageBoxImage.Error);

            PrepareWindow(popup, owner);

            popup.ShowDialog();

            return popup.Result;
        }
        catch (Exception ex)
        {
            return ShowFallback(
                message,
                title,
                buttons,
                image,
                ex);
        }
    }

    public static MessageBoxResult Show(
        string message,
        string title,
        MessageBoxButton buttons)
    {
        return Show(
            message,
            title,
            buttons,
            MessageBoxImage.None);
    }

    public static MessageBoxResult Show(
        string message,
        string title)
    {
        return Show(
            message,
            title,
            MessageBoxButton.OK,
            MessageBoxImage.None);
    }

    public static MessageBoxResult Show(
        string message)
    {
        return Show(
            message,
            "Racing Time Clock",
            MessageBoxButton.OK,
            MessageBoxImage.None);
    }

    public static MessageBoxResult ShowConfirmation(
        string title,
        string message,
        string confirmText = "CONFIRM",
        bool destructive = false)
    {
        try
        {
            Window? owner =
                Application.Current?
                    .Windows
                    .OfType<Window>()
                    .FirstOrDefault(
                        window =>
                            window.IsVisible &&
                            window.IsActive);

            RacingPopupWindow popup =
                new(
                    title,
                    message,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    confirmText,
                    destructive);

            PrepareWindow(popup, owner);

            popup.ShowDialog();

            return popup.Result;
        }
        catch (Exception ex)
        {
            return ShowFallback(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                ex);
        }
    }

    public static bool Confirm(
        string title,
        string message,
        string confirmText = "CONFIRM",
        bool destructive = false)
    {
        return ShowConfirmation(
            title,
            message,
            confirmText,
            destructive) == MessageBoxResult.Yes;
    }

    private static MessageBoxResult ShowFallback(
        string message,
        string title,
        MessageBoxButton buttons,
        MessageBoxImage image,
        Exception exception)
    {
        string fallbackMessage =
            $"{message}\n\n" +
            "The custom popup could not be displayed.\n\n" +
            $"Technical details:\n{exception.Message}";

        return MessageBox.Show(
            fallbackMessage,
            title,
            buttons,
            image);
    }

    private static void PrepareWindow(
        RacingPopupWindow popup,
        Window? owner)
    {
        if (owner != null)
        {
            popup.Owner = owner;

            popup.Left = owner.Left;
            popup.Top = owner.Top;
            popup.Width = owner.ActualWidth;
            popup.Height = owner.ActualHeight;

            return;
        }

        popup.Left =
            SystemParameters.WorkArea.Left;

        popup.Top =
            SystemParameters.WorkArea.Top;

        popup.Width =
            SystemParameters.WorkArea.Width;

        popup.Height =
            SystemParameters.WorkArea.Height;
    }
}
