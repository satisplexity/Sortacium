using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Sortacium.Presentation.Models;

namespace Sortacium.Presentation.Controls;

public sealed class BarsRenderer : Control
{
    public static readonly StyledProperty<IReadOnlyList<Bar>?>
    BarsProperty =
        AvaloniaProperty.Register<
            BarsRenderer, IReadOnlyList<Bar>?>(nameof(Bars));

    public IReadOnlyList<Bar>? Bars
    {
        get => GetValue(BarsProperty);
        set => SetValue(BarsProperty, value);
    }

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
    }

    private readonly Dictionary<Bar, VisualBar> _visuals = new();
    private bool _frameQueued;

    private const double AnimationMs = 250;

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

        if (change.Property != BarsProperty)
            return;

        foreach (var bar in _visuals.Keys)
            bar.PropertyChanged -= OnBarChanged;

        _visuals.Clear();

        if (Bars is not null)
        {
            foreach (var bar in Bars)
            {
                _visuals.Add(bar, new VisualBar(bar));
                bar.PropertyChanged += OnBarChanged;
            }
        }

        InvalidateVisual();
    }

    private void OnBarChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        RequestFrame();
    }

    private void RequestFrame()
    {
        if (_frameQueued)
            return;

        var topLevel = TopLevel.GetTopLevel(this);

        if (topLevel is null)
            return;

        _frameQueued = true;
        topLevel.RequestAnimationFrame(OnFrame);
    }

    private void OnFrame(TimeSpan time)
    {
        _frameQueued = false;
        bool anyAnimating = false;

        foreach (var (bar, visual) in _visuals)
        {
            var targetColor = GetColor(bar.State);

            if (visual.TargetX != bar.Index ||
                visual.TargetValue != bar.Value ||
                visual.TargetColor != targetColor)
            {
                // Начинаем с текущего отображаемого положения.
                visual.FromX = visual.X;
                visual.FromValue = visual.Value;
                visual.FromColor = visual.Color;

                visual.TargetX = bar.Index;
                visual.TargetValue = bar.Value;
                visual.TargetColor = targetColor;

                visual.StartTime = time;
                visual.Animating = true;
            }

            if (!visual.Animating)
                continue;

            double t = Math.Clamp(
                (time - visual.StartTime).TotalMilliseconds
                    / AnimationMs,
                0, 1);

            // Smoothstep
            double eased = t * t * (3 - 2 * t);

            visual.X = Lerp(
                visual.FromX, visual.TargetX, eased);

            visual.Value = Lerp(
                visual.FromValue, visual.TargetValue, eased);

            visual.Color = LerpColor(
                visual.FromColor, visual.TargetColor, eased);

            visual.Animating = t < 1;
            anyAnimating |= visual.Animating;
        }

        InvalidateVisual();

        if (anyAnimating)
            RequestFrame();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        if (Bars is not { Count: > 0 } bars)
            return;

        double step = Bounds.Width / bars.Count;
        double barWidth = step * 0.8;

        foreach (var bar in bars)
        {
            var visual = _visuals[bar];

            double height = visual.Value * Bounds.Height;
            double x = visual.X * step +
                       (step - barWidth) / 2;
            double y = Bounds.Height - height;

            LinearGradientBrush borderBrush = new LinearGradientBrush()
            {
                StartPoint = new RelativePoint(0.5, 0, RelativeUnit.Relative),
                EndPoint = new RelativePoint(0.5, 1, RelativeUnit.Relative),
            };

            HsvColor hsv = visual.Color.ToHsv();
            double top = Math.Clamp(hsv.V + 0.2, 0, 1);
            double bottom = Math.Clamp(hsv.V - 0.2, 0, 1);

            borderBrush.GradientStops.Add(new GradientStop(new HsvColor(1, hsv.H, hsv.S, top).ToRgb(), 0));
            borderBrush.GradientStops.Add(new GradientStop(visual.Color, 0.2));
            borderBrush.GradientStops.Add(new GradientStop(visual.Color, 0.8));
            borderBrush.GradientStops.Add(new GradientStop(new HsvColor(1, hsv.H, hsv.S, bottom).ToRgb(), 1));

            context.DrawRectangle(new SolidColorBrush(visual.Color), new Pen(borderBrush, 2), new Rect(x, y, barWidth, height), 20, 20);
        }
    }

    private static double Lerp(double a, double b, double t)
        => a + (b - a) * t;

    private static Color LerpColor(Color a, Color b, double t)
        => Color.FromArgb(
            (byte)Math.Round(Lerp(a.A, b.A, t)),
            (byte)Math.Round(Lerp(a.R, b.R, t)),
            (byte)Math.Round(Lerp(a.G, b.G, t)),
            (byte)Math.Round(Lerp(a.B, b.B, t)));
}
