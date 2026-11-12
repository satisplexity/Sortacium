using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;
using System;

namespace Sortacium.Presentation.Components;

public sealed class StrokeGradientAdapter : AvaloniaObject
{
    public static readonly AttachedProperty<bool> EnableProperty =
        AvaloniaProperty.RegisterAttached<StrokeGradientAdapter, Border, bool>("Enable");

    static StrokeGradientAdapter()
    {
        EnableProperty.Changed.AddClassHandler<Border>(OnEnableChanged);
    }

    public static bool GetEnable(Border border)
    {
        return border.GetValue(EnableProperty);
    }

    public static void SetEnable(Border border, bool value)
    {
        border.SetValue(EnableProperty, value);
    }

    private static void OnEnableChanged(
        Border border,
        AvaloniaPropertyChangedEventArgs args)
    {
        border.PropertyChanged -= OnBorderPropertyChanged;

        if (!args.GetNewValue<bool>())
        {
            return;
        }

        border.PropertyChanged += OnBorderPropertyChanged;

        UpdateGradient(border);
    }

    private static void OnBorderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (sender is not Border border)
        {
            return;
        }

        if (args.Property == Visual.BoundsProperty ||
            args.Property == Border.CornerRadiusProperty ||
            args.Property == Border.BorderBrushProperty)
        {
            UpdateGradient(border);
        }
    }

    private static void UpdateGradient(Border border)
    {
        if (border.BorderBrush is not LinearGradientBrush gradient)
        {
            return;
        }

        if (gradient.GradientStops.Count != 4)
        {
            return;
        }   

        double height = border.Bounds.Height;

        if (!double.IsFinite(height) || height <= 0)
        {
            return;
        }

        CornerRadius radius = border.CornerRadius;

        double topRadius =Math.Max(radius.TopLeft, radius.TopRight);

        double bottomRadius = Math.Max(radius.BottomLeft, radius.BottomRight);

        double topOffset = Math.Clamp(topRadius / height, 0, 0.5);

        double bottomOffset = 1 - Math.Clamp(bottomRadius / height, 0, 0.5);

        gradient.GradientStops[1].Offset = topOffset;
        gradient.GradientStops[2].Offset = bottomOffset;
    }
}
