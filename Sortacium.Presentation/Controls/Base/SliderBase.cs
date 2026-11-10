using Avalonia.Controls;
using Avalonia;

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
}
