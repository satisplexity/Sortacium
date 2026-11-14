using CommunityToolkit.Mvvm.ComponentModel;

using Sortacium.Core.Execution;

namespace Sortacium.Presentation.ViewModels;

public partial class CodeLineViewModel : ObservableObject
{
    public int Number { get; }

    public string Text { get; }

    public SortingCodePoint? CodePoint { get; }

    [ObservableProperty]
    private bool _isActive;

    public CodeLineViewModel(int number, string text, SortingCodePoint? codePoint = null)
    {
        Number = number;
        Text = text;
        CodePoint = codePoint;
    }
}
