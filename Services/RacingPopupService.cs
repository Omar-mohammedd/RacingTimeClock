using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
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
        return Show(message, title, buttons, image);
    }

    public static MessageBoxResult Show(
        string message,
        string title,
        MessageBoxButton buttons,
        MessageBoxImage image)
    {
        return ShowPopup(
            message,
            title,
            buttons,
            image,
            GetPrimaryText(buttons),
            image == MessageBoxImage.Error);
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
        return ShowPopup(
            message,
            title,
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            confirmText,
            destructive);
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
            destructive) ==
            MessageBoxResult.Yes;
    }

    private static MessageBoxResult ShowPopup(
        string message,
        string title,
        MessageBoxButton buttons,
        MessageBoxImage image,
        string primaryText,
        bool destructive)
    {
        Window? owner =
            Application.Current?
                .Windows
                .OfType<Window>()
                .FirstOrDefault(
                    window =>
                        window.IsVisible &&
                        window.IsActive);

        if (owner == null)
        {
            return MessageBox.Show(
                message,
                title,
                buttons,
                image);
        }

        UIElement? content =
            owner.Content as UIElement;

        if (content == null)
        {
            return MessageBox.Show(
                message,
                title,
                buttons,
                image);
        }

        AdornerLayer? layer =
            FindAdornerLayer(content);

        if (layer == null)
        {
            return MessageBox.Show(
                message,
                title,
                buttons,
                image);
        }

        RacingPopupWindow popup =
            new()
            {
                PopupTitle = title,
                Message = message,
                Buttons = buttons,
                Image = image,
                PrimaryButtonText = primaryText,
                SecondaryButtonText = "CANCEL",
                Destructive = destructive
            };

        RacingPopupAdorner adorner =
            new(content, popup);

        MessageBoxResult result =
            MessageBoxResult.None;

        DispatcherFrame frame =
            new();

        void Complete(
            object? sender,
            MessageBoxResult popupResult)
        {
            result = popupResult;
            frame.Continue = false;
        }

        void OwnerClosed(
            object? sender,
            EventArgs e)
        {
            result = MessageBoxResult.Cancel;
            frame.Continue = false;
        }

        popup.Completed += Complete;
        owner.Closed += OwnerClosed;

        try
        {
            layer.Add(adorner);

            popup.Focus();

            Dispatcher.PushFrame(frame);

            return result == MessageBoxResult.None
                ? MessageBoxResult.Cancel
                : result;
        }
        finally
        {
            popup.Completed -= Complete;
            owner.Closed -= OwnerClosed;

            if (layer.GetAdorners(content)
                ?.Contains(adorner) == true)
            {
                layer.Remove(adorner);
            }
        }
    }

    private static AdornerLayer? FindAdornerLayer(
        DependencyObject element)
    {
        DependencyObject? current = element;

        while (current != null)
        {
            if (current is UIElement uiElement)
            {
                AdornerLayer? layer =
                    AdornerLayer.GetAdornerLayer(
                        uiElement);

                if (layer != null)
                {
                    return layer;
                }
            }

            current =
                VisualTreeHelper.GetParent(current);
        }

        return null;
    }

    private static string GetPrimaryText(
        MessageBoxButton buttons)
    {
        return buttons == MessageBoxButton.YesNo ||
               buttons == MessageBoxButton.YesNoCancel
            ? "CONFIRM"
            : "OK";
    }
}
