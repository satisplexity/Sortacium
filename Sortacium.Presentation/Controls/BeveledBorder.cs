using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using System;

namespace Sortacium.Presentation.Controls;

public sealed class BeveledBorder : Border
{
    public static readonly StyledProperty<Color> StrokeHighlightColorProperty =
        AvaloniaProperty.Register<BeveledBorder, Color>(nameof(StrokeHighlightColor),Colors.Transparent);

    public Color StrokeHighlightColor
    {
        get => GetValue(StrokeHighlightColorProperty);
        set => SetValue(StrokeHighlightColorProperty, value);
    }

    public static readonly StyledProperty<Color> StrokeShadowColorProperty =
        AvaloniaProperty.Register<BeveledBorder, Color>(nameof(StrokeShadowColor), Colors.Transparent);

    public Color StrokeShadowColor
    {
        get => GetValue(StrokeShadowColorProperty);
        set => SetValue(StrokeShadowColorProperty, value);
    }

    private readonly LinearGradientBrush _gradient;

    public BeveledBorder()
    {
        _gradient = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
            EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Colors.Transparent, 0),
                new GradientStop(Colors.Transparent, 0),
                new GradientStop(Colors.Transparent, 1),
                new GradientStop(Colors.Transparent, 1)
            }
        };

        BorderBrush = _gradient;

        UpdateColors();
        UpdateOffsets();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (_gradient is null)
        {
            return;
        }

        if (change.Property == StrokeHighlightColorProperty ||
            change.Property == StrokeShadowColorProperty ||
            change.Property == BackgroundProperty)
        {
            UpdateColors();
        }

        if (change.Property == BoundsProperty ||
            change.Property == CornerRadiusProperty)
        {
            UpdateOffsets();
        }
    }

    private void UpdateColors()
    {
        SolidColorBrush? background = Background as SolidColorBrush;

        if (background is null)
        {
            return;
        }

        _gradient.GradientStops[0].Color = StrokeHighlightColor;
        _gradient.GradientStops[1].Color = background.Color;

        _gradient.GradientStops[2].Color = background.Color;
        _gradient.GradientStops[3].Color = StrokeShadowColor;
    }

    private void UpdateOffsets()
    {
        double height = Bounds.Height;

        if (!double.IsFinite(height) || height <= 0)
        {
            return;
        }

        double topRadius = Math.Max(CornerRadius.TopLeft, CornerRadius.TopRight);
        double bottomRadius = Math.Max(CornerRadius.BottomLeft, CornerRadius.BottomRight);

        double topOffset = Math.Clamp(topRadius / height, 0, 0.5);
        double bottomOffset = 1 - Math.Clamp(bottomRadius / height, 0, 0.5);

        _gradient.GradientStops[1].Offset = topOffset;
        _gradient.GradientStops[2].Offset = bottomOffset;
    }
}
