using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace Sortacium.Presentation.Components;

public sealed class StrokeGradientAdapter : AvaloniaObject
{
    public static readonly AttachedProperty<IBrush?> SourceProperty =
        AvaloniaProperty.RegisterAttached<StrokeGradientAdapter, Border, IBrush?>("Source");

    static StrokeGradientAdapter()
    {
        SourceProperty.Changed.AddClassHandler<Border>(OnSourceChanged);
    }

    public static IBrush? GetSource(Border border) => border.GetValue(SourceProperty);

    public static void SetSource(Border border, IBrush? value) => border.SetValue(SourceProperty, value);

    private static void OnSourceChanged(Border border, AvaloniaPropertyChangedEventArgs args)
    {
        border.PropertyChanged -= OnBorderPropertyChanged;
        var source = GetSource(border);
        if (source is not ILinearGradientBrush { GradientStops.Count: 4 } gradient)
        {
            border.BorderBrush = source;
            return;
        }

        var brush = new LinearGradientBrush
        {
            StartPoint = gradient.StartPoint,
            EndPoint = gradient.EndPoint,
            SpreadMethod = gradient.SpreadMethod,
            Opacity = gradient.Opacity,
            Transform = gradient.Transform,
            TransformOrigin = gradient.TransformOrigin,
        };

        foreach (var stop in gradient.GradientStops)
        {
            brush.GradientStops.Add(new GradientStop(stop.Color, stop.Offset));
        }

        UpdateOffsets(border, brush);

        border.BorderBrush = brush;
        border.PropertyChanged += OnBorderPropertyChanged;
    }

    private static void OnBorderPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if ((args.Property == Visual.BoundsProperty || args.Property == Border.CornerRadiusProperty)
            && sender is Border border && border.GetBaseValue(Border.BorderBrushProperty).GetValueOrDefault() is LinearGradientBrush brush)
        {
            UpdateOffsets(border, brush);
        }
    }

    private static void UpdateOffsets(Border border, LinearGradientBrush brush)
    {
        double height = border.Bounds.Height;

        if (!double.IsFinite(height) || height <= 0)
            return;

        CornerRadius radius = border.CornerRadius;

        double top = Math.Max(radius.TopLeft, radius.TopRight);
        double bottom = Math.Max(radius.BottomLeft, radius.BottomRight);

        brush.GradientStops[1].Offset = Math.Clamp(top / height, 0, 0.5);
        brush.GradientStops[2].Offset = 1 - Math.Clamp(bottom / height, 0, 0.5);
    }
}
