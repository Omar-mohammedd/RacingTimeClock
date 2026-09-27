using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RacingTimeClock.Controls;

public partial class RacingPopupWindow : UserControl
{
    public MessageBoxResult Result { get; private set; } =
        MessageBoxResult.None;

    public string PopupTitle { get; set; } =
        "Racing Time Clock";

    public string Message { get; set; } =
        string.Empty;

    public MessageBoxButton Buttons { get; set; } =
        MessageBoxButton.OK;

    public MessageBoxImage Image { get; set; } =
        MessageBoxImage.None;

    public string PrimaryButtonText { get; set; } =
        "OK";

    public string SecondaryButtonText { get; set; } =
        "CANCEL";

    public bool Destructive { get; set; }

    public event EventHandler<MessageBoxResult>? Completed;

    public RacingPopupWindow()
    {
        InitializeComponent();

        Loaded += PopupWindow_Loaded;
    }

    private void PopupWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        TitleText.Text = PopupTitle;
        MessageText.Text = Message;
        PrimaryButton.Content = PrimaryButtonText;
        SecondaryButton.Content = SecondaryButtonText;

        ConfigureIcon();
        ConfigureButtons();

        Focus();
    }

    private void ConfigureIcon()
    {
        switch (Image)
        {
            case MessageBoxImage.Error:
            case MessageBoxImage.Warning:
                IconBorder.SetResourceReference(
                    Border.BackgroundProperty,
                    "DangerBrush");

                IconText.Text = "!";
                break;

            default:
                IconBorder.SetResourceReference(
                    Border.BackgroundProperty,
                    "AccentBrush");

                IconText.Text = "i";
                break;
        }
    }

    private void ConfigureButtons()
    {
        bool hasSecondary =
            Buttons == MessageBoxButton.OKCancel ||
            Buttons == MessageBoxButton.YesNo ||
            Buttons == MessageBoxButton.YesNoCancel;

        SecondaryButton.Visibility =
            hasSecondary
                ? Visibility.Visible
                : Visibility.Collapsed;

        PrimaryButton.SetResourceReference(
            Control.StyleProperty,
            Destructive
                ? "RacingPopupDangerButtonStyle"
                : "RacingPopupPrimaryButtonStyle");

        if (!hasSecondary)
        {
            PrimaryButton.Focus();
        }
        else
        {
            SecondaryButton.Focus();
        }
    }

    private void PrimaryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Result =
            Buttons == MessageBoxButton.YesNo ||
            Buttons == MessageBoxButton.YesNoCancel
                ? MessageBoxResult.Yes
                : MessageBoxResult.OK;

        Complete();
    }

    private void SecondaryButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        Result =
            Buttons == MessageBoxButton.OKCancel
                ? MessageBoxResult.Cancel
                : MessageBoxResult.No;

        Complete();
    }

    private void Complete()
    {
        Completed?.Invoke(
            this,
            Result);
    }
}
