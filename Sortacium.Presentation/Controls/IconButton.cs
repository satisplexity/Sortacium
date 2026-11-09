using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia;

using Sortacium.Presentation.Services;

using System.Diagnostics;

namespace Sortacium.Presentation.Controls;

public sealed class IconButton : Button
{
    private Stopwatch _stopwatch = Stopwatch.StartNew();

    public static readonly StyledProperty<StreamGeometry> IconProperty =
        AvaloniaProperty.Register<IconButton, StreamGeometry>(nameof(Icon));

    public StreamGeometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _stopwatch.Restart();

        SoundService.Instance.Play(UiSound.Pressed);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (_stopwatch.ElapsedMilliseconds < 150)
        {
            return;
        }

        SoundService.Instance.Play(UiSound.Pressed);
    }
}
