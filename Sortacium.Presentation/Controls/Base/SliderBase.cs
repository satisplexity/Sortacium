using System;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

using Sortacium.Presentation.Services;

namespace Sortacium.Presentation.Controls.Base;

public class SliderBase : Slider
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<HorizontalSlider, string>(nameof(Title));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> SuffixProperty =
        AvaloniaProperty.Register<HorizontalSlider, string>(nameof(Suffix));

    public string Suffix
    {
        get => GetValue(SuffixProperty);
        set => SetValue(SuffixProperty, value);
    }

    public static readonly StyledProperty<int> TickSegmentsProperty =
        AvaloniaProperty.Register<SliderBase, int>(
            nameof(TickSegments), 10);

    public int TickSegments
    {
        get => GetValue(TickSegmentsProperty);
        set => SetValue(TickSegmentsProperty, value);
    }

    private bool _isDragging;
    private int _lastSegment;

    public SliderBase()
    {
        ValueChanged += OnValueChanged;
    }

    protected override void OnThumbDragStarted(VectorEventArgs e)
    {
        base.OnThumbDragStarted(e);

        _lastSegment = GetSegment(Value);
        _isDragging = true;
    }

    protected override void OnThumbDragCompleted(VectorEventArgs e)
    {
        _isDragging = false;

        base.OnThumbDragCompleted(e);
    }

    private void OnValueChanged(object? sender,
        RangeBaseValueChangedEventArgs e)
    {
        if (!_isDragging || TickSegments <= 0)
            return;

        int segment = GetSegment(Value);

        if (segment == _lastSegment)
            return;

        _lastSegment = segment;

        SoundService.Instance.Play(UiSound.Tick);
    }

    private int GetSegment(double value)
    {
        double range = Maximum - Minimum;

        if (range <= 0 || TickSegments <= 0)
            return 0;

        double progress = (value - Minimum) / range;

        return Math.Clamp(
            (int) Math.Floor(progress * TickSegments + 1e-9),
            0,
            TickSegments);
    }

}
