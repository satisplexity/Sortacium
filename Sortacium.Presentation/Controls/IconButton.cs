using Avalonia.Controls;
using Avalonia.Media;
using Avalonia;

namespace Sortacium.Presentation.Controls;

public sealed class IconButton : Button
{
    public static readonly StyledProperty<StreamGeometry> IconProperty =
        AvaloniaProperty.Register<IconButton, StreamGeometry>(nameof(Icon));

    public StreamGeometry Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}
