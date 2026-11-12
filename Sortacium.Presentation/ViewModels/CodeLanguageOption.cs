using System.ComponentModel;
using Sortacium.Presentation.Services;

namespace Sortacium.Presentation.ViewModels;

public sealed class CodeLanguageOption : INotifyPropertyChanged
{
    public string Id { get; }
    private readonly string _name;
    public string DisplayName => Id switch
    {
        "pseudocode" => LocalizationService.Instance["Pseudocode"],
        "assembly" => LocalizationService.Instance["Assembly"],
        _ => _name
    };
    public event PropertyChangedEventHandler? PropertyChanged;
    public CodeLanguageOption(string id, string displayName)
    {
        Id = id;
        _name = displayName;
        LocalizationService.Instance.LanguageChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayName)));
    }
    public override string ToString() => DisplayName;
}
