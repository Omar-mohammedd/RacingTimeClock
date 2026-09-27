using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace RacingTimeClock.Controls;

public sealed class RacingPopupAdorner : Adorner
{
    private readonly VisualCollection visuals;

    public RacingPopupAdorner(
        UIElement adornedElement,
        UIElement popup)
        : base(adornedElement)
    {
        Popup = popup;
        visuals = new VisualCollection(this)
        {
            popup
        };

        IsHitTestVisible = true;
    }

    public UIElement Popup { get; }

    protected override int VisualChildrenCount =>
        visuals.Count;

    protected override Visual GetVisualChild(
        int index)
    {
        return visuals[index];
    }

    protected override Size MeasureOverride(
        Size constraint)
    {
        Popup.Measure(constraint);

        return constraint;
    }

    protected override Size ArrangeOverride(
        Size finalSize)
    {
        Popup.Arrange(
            new Rect(
                new Point(0, 0),
                finalSize));

        return finalSize;
    }
}
