namespace Sortacium.Presentation.ViewModels;

public sealed record CodeLanguageOption(string Id, string DisplayName)
{
    public override string ToString() => DisplayName;
}
