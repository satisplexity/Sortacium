using System;
using System.Collections.Generic;
using System.ComponentModel;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

using Sortacium.Presentation.Components;
using Sortacium.Presentation.Models;

namespace Sortacium.Presentation.Controls;

public sealed class BarsRenderer : Control
{
    private const double GrowthSpeed = 1000.0f;

    private static EnchancedBackEaseOut GrowthEasing = new EnchancedBackEaseOut()
    {
        Amplitude = 0.2
    };

    public static readonly StyledProperty<IReadOnlyList<Bar>?> BarsProperty =
        AvaloniaProperty.Register<BarsRenderer, IReadOnlyList<Bar>?>(nameof(Bars));

    public IReadOnlyList<Bar>? Bars
    {
        get => GetValue(BarsProperty);
        set => SetValue(BarsProperty, value);
    }

    public static readonly StyledProperty<int> SnapVersionProperty =
        AvaloniaProperty.Register<BarsRenderer, int>(nameof(SnapVersion));

    public int SnapVersion
    {
        get => GetValue(SnapVersionProperty);
        set => SetValue(SnapVersionProperty, value);
    }

    private bool _snapRequested;

    private const double AnimationDuration = 250;
    private const double WidthFactor = 0.8f;

    private sealed class VisualBar(Bar bar)
    {
        public double X = bar.Index;
        public double FromX = bar.Index;
        public int TargetX = bar.Index;

        public double Value = bar.Value;
        public double FromValue = bar.Value;
        public double TargetValue = bar.Value;

        public Color Color = GetColor(bar.State);
        public Color FromColor = GetColor(bar.State);
        public Color TargetColor = GetColor(bar.State);

        public TimeSpan StartTime;
        public bool Animating;

        public double Growth = bar.State == BarState.Initialize ? 0 : 1;
        public bool Growing = bar.State == BarState.Initialize;
        public TimeSpan? GrowthStartTime;

        public BarState LastState = bar.State;

        public double GrowthDurationMs;
    }

    private readonly Dictionary<Bar, VisualBar> _visuals = new();
    private bool _frameQueued;

    private static Color GetColor(BarState state) => state switch
    {
        BarState.Comparing => Colors.Orange,
        BarState.Swapping => Colors.Red,
        _ => Colors.MediumPurple
    };

    protected override void OnPropertyChanged(
        AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SnapVersionProperty)
        {
            _snapRequested = true;
            RequestFrame();
            return;
        }

        if (change.Property != BarsProperty)
        {
            return;
        }

        foreach (var bar in _visuals.Keys)
        {
            bar.PropertyChanged -= OnBarChanged;
        }   

        _visuals.Clear();

        if (Bars is not null)
        {
            foreach (Bar bar in Bars)
            {
                _visuals.Add(bar, new VisualBar(bar));
                bar.PropertyChanged += OnBarChanged;
            }
        }

        InvalidateVisual();
        RequestFrame();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        RequestFrame();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _frameQueued = false;
    }

    private void OnBarChanged(object? sender, PropertyChangedEventArgs e)
    {
        RequestFrame();
    }

    private void RequestFrame()
    {
        if (_frameQueued)
        {
            return;
        }
            
        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null)
        {
            return;
        }
            
        _frameQueued = true;
        topLevel.RequestAnimationFrame(OnFrame);
    }

    private void OnFrame(TimeSpan time)
    {
        _frameQueued = false;

        if (_snapRequested)
        {
            _snapRequested = false;

            foreach (var (bar, visual) in _visuals)
            {
                Color color = GetColor(bar.State);

                visual.X = bar.Index;
                visual.FromX = bar.Index;
                visual.TargetX = bar.Index;

                visual.Value = bar.Value;
                visual.FromValue = bar.Value;
                visual.TargetValue = bar.Value;

                visual.Color = color;
                visual.FromColor = color;
                visual.TargetColor = color;

                visual.Animating = false;

                visual.Growth = 1;
                visual.Growing = false;
                visual.GrowthStartTime = null;

                visual.LastState = bar.State;
            }

            InvalidateVisual();

            return;
        }

        bool anyAnimating = false;

        foreach (var (bar, visual) in _visuals)
        {
            // Restart appearance if the bar enters Initialize again.
            if (bar.State == BarState.Initialize &&
                visual.LastState != BarState.Initialize)
            {
                visual.Growth = 0;
                visual.Growing = true;
                visual.GrowthStartTime = null;
            }

            visual.LastState = bar.State;

            // Update movement, value and color targets.
            Color targetColor = GetColor(bar.State);

            if (visual.TargetX != bar.Index ||
                visual.TargetValue != bar.Value ||
                visual.TargetColor != targetColor)
            {
                visual.FromX = visual.X;
                visual.FromValue = visual.Value;
                visual.FromColor = visual.Color;

                visual.TargetX = bar.Index;
                visual.TargetValue = bar.Value;
                visual.TargetColor = targetColor;

                visual.StartTime = time;
                visual.Animating = true;
            }

            if (visual.Animating)
            {
                double delta = Math.Clamp(
                    (time - visual.StartTime).TotalMilliseconds
                    / AnimationDuration,
                    0,
                    1);

                double eased = Smoothstep(delta);

                visual.X = Lerp(
                    visual.FromX,
                    visual.TargetX,
                    eased);

                visual.Value = Lerp(
                    visual.FromValue,
                    visual.TargetValue,
                    eased);

                visual.Color = LerpColor(
                    visual.FromColor,
                    visual.TargetColor,
                    eased);

                visual.Animating = delta < 1;
                anyAnimating |= visual.Animating;
            }

            // Animate initial growth independently of Value.
            if (visual.Growing)
            {
                if (Bars is not { Count: > 0 } bars ||
                    Bounds.Width <= 0 ||
                    Bounds.Height <= 0)
                {
                    anyAnimating = true;
                    continue;
                }

                if (visual.GrowthStartTime is null)
                {
                    visual.GrowthStartTime = time;

                    double targetHeight = CalculateBarHeight(
                        visual.TargetValue,
                        bars.Count);

                    visual.GrowthDurationMs =
                        Math.Max(1, targetHeight / GrowthSpeed * 1000);
                }

                double elapsedMs =
                    (time - visual.GrowthStartTime.Value).TotalMilliseconds;

                double t = Math.Clamp(
                    elapsedMs / visual.GrowthDurationMs,
                    0,
                    1);

                visual.Growth = GrowthEasing.Ease(t);

                visual.Growing = t < 1;
                anyAnimating |= visual.Growing;

                if (!visual.Growing &&
                    bar.State == BarState.Initialize)
                {
                    bar.State = BarState.Idle;
                    visual.LastState = BarState.Idle;
                }
            }
        }

        InvalidateVisual();

        if (anyAnimating)
        {
            RequestFrame();
        }   
    }

    private double CalculateBarHeight(double value, int count)
    {
        if (count <= 0)
        {
            return 0;
        }

        double barWidth = Bounds.Width / count * WidthFactor;

        double minHeight = Math.Min(
            barWidth,
            Bounds.Height / 2);

        double maxHeight = Math.Max(
            minHeight,
            Bounds.Height - barWidth);

        return minHeight +
               (maxHeight - minHeight) * Math.Clamp(value, 0, 1);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Bars is not { Count: > 0 } bars)
            return;

        double containerWidth = Bounds.Width;
        double containerHeight = Bounds.Height;

        if (containerWidth <= 0 || containerHeight <= 0)
            return;

        double step = containerWidth / bars.Count;
        double barWidth = step * WidthFactor;

        double minHeight = Math.Min(
            barWidth,
            containerHeight / 2);

        double maxHeight = Math.Max(
            minHeight,
            containerHeight - barWidth);

        double heightRange = maxHeight - minHeight;

        foreach (var bar in bars)
        {
            if (!_visuals.TryGetValue(bar, out var visual))
                continue;

            double targetHeight = CalculateBarHeight(
    visual.Value,
    bars.Count);

            double height = targetHeight * visual.Growth;

            if (height <= 0)
                continue;

            double x =
                visual.X * step +
                (step - barWidth) / 2;

            double y = Bounds.Height - height;

            double radius = barWidth / 4;

            DrawBar(context, visual, x, y, barWidth, height, radius);
        }
    }

    private void DrawBar(DrawingContext context, VisualBar visual, double x, double y, double barWidth, double height, double radius)
    {
        if (radius >= 10)
        {
            HsvColor hsv = visual.Color.ToHsv();

            double top = (float)Math.Clamp(hsv.V + 0.2, 0, 1);
            double bottom = (float)Math.Clamp(hsv.V - 0.2, 0, 1);

            LinearGradientBrush borderBrush = new()
            {
                StartPoint = new RelativePoint(
                    0.5, 0, RelativeUnit.Relative),

                EndPoint = new RelativePoint(
                    0.5, 1, RelativeUnit.Relative)
            };

            borderBrush.GradientStops.Add(
                new GradientStop(
                    new HsvColor(1, hsv.H, hsv.S, top).ToRgb(),
                    0));

            borderBrush.GradientStops.Add(
                new GradientStop(visual.Color, 0.2));

            borderBrush.GradientStops.Add(
                new GradientStop(visual.Color, 0.8));

            borderBrush.GradientStops.Add(
                new GradientStop(
                    new HsvColor(1, hsv.H, hsv.S, bottom).ToRgb(),
                    1));

            context.DrawRectangle(
                new SolidColorBrush(visual.Color),
                new Pen(borderBrush, 3),
                new Rect(x, y, barWidth, height),
                radius,
                radius);
        }
        else
        {
            if (radius > 2)
            {
                context.DrawRectangle(
                                    new SolidColorBrush(visual.Color),
                                    null,
                                    new Rect(x, y, barWidth, height),
                                    radius,
                                    radius);
            }
            else
            {
                context.DrawRectangle(
                                    new SolidColorBrush(visual.Color),
                                    null,
                                    new Rect(x, y, barWidth, height));
            }
        }
    }

    private static double Smoothstep(double t)
        => t * t * (3 - 2 * t);

    private static double Lerp(double a, double b, double t)
        => a + (b - a) * t;

    private static Color LerpColor(Color a, Color b, double t)
        => Color.FromArgb(
            (byte) Math.Round(Lerp(a.A, b.A, t)),
            (byte) Math.Round(Lerp(a.R, b.R, t)),
            (byte) Math.Round(Lerp(a.G, b.G, t)),
            (byte) Math.Round(Lerp(a.B, b.B, t)));
}
