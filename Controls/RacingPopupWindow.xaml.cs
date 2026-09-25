using System;
using System.Windows;
using System.Windows.Media;

namespace RacingTimeClock.Controls;

public partial class RacingPopupWindow : Window
{
    public MessageBoxResult Result { get; private set; } =
        MessageBoxResult.None;

    private readonly MessageBoxButton buttons;

    public RacingPopupWindow(
        string title,
        string message,
        MessageBoxButton buttons,
        MessageBoxImage image,
        string primaryText,
        bool destructive)
    {
        InitializeComponent();

        this.buttons = buttons;

        TitleText.Text = title;
        MessageText.Text = message;
        PrimaryButton.Content = primaryText;

        ConfigureIcon(image);
        ConfigureButtons(destructive);
    }

    private void ConfigureIcon(MessageBoxImage image)
    {
        string resourceName;
        string symbol;

        switch (image)
        {
            case MessageBoxImage.Error:
                resourceName = "DangerBrush";
                symbol = "!";
                break;

            case MessageBoxImage.Warning:
                resourceName = "DangerBrush";
                symbol = "!";
                break;

            case MessageBoxImage.Information:
                resourceName = "AccentBrush";
                symbol = "i";
                break;

            default:
                resourceName = "AccentBrush";
                symbol = "i";
                break;
        }

        IconBorder.Background =
            (Brush)FindResource(resourceName);

        IconText.Text = symbol;
    }

    private void ConfigureButtons(bool destructive)
    {
        bool hasSecondary =
            buttons == MessageBoxButton.YesNo ||
            buttons == MessageBoxButton.YesNoCancel;

        SecondaryButton.Visibility =
            hasSecondary
                ? Visibility.Visible
                : Visibility.Collapsed;

        if (destructive)
        {
            PrimaryButton.Style =
                (Style)FindResource(
                    "RacingPopupDangerButtonStyle");
        }
        else
        {
            PrimaryButton.Style =
                (Style)FindResource(
                    "RacingPopupPrimaryButtonStyle");
        }
    }

    private void PrimaryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        if (buttons == MessageBoxButton.YesNo ||
            buttons == MessageBoxButton.YesNoCancel)
        {
            Result = MessageBoxResult.Yes;
        }
        else
        {
            Result = MessageBoxResult.OK;
        }

        DialogResult = true;
        Close();
    }

    private void SecondaryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Result = MessageBoxResult.No;

        DialogResult = false;
        Close();
    }

    private void CloseButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Result =
            buttons == MessageBoxButton.YesNo ||
            buttons == MessageBoxButton.YesNoCancel
                ? MessageBoxResult.No
                : MessageBoxResult.Cancel;

        DialogResult = false;
        Close();
    }
}
