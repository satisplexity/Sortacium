using System.ComponentModel;
using System.Collections.Generic;
using Sortacium.Presentation.Services;

namespace Sortacium.Presentation.Models;

public sealed class AlgorithmOption : INotifyPropertyChanged
{
    public string Id { get; }
    public AlgorithmDefinition Definition { get; }
    public event PropertyChangedEventHandler? PropertyChanged;
    public AlgorithmOption(string id, AlgorithmDefinition definition)
    {
        Id = id;
        Definition = definition;
        LocalizationService.Instance.LanguageChanged += (_, _) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
    private AlgorithmTranslation? Translation => Definition.Translations.GetValueOrDefault(LocalizationService.Instance.Language);
    public string Name => Translation?.Name ?? Definition.Name;
    public string Description => Translation?.Description ?? Definition.Description;
    public string Author => Translation?.Author ?? Definition.Author;
    public string Year => Translation?.Year ?? Definition.Year;
    public string BestTime => Definition.BestTime;
    public string AverageTime => Definition.AverageTime;
    public string WorstTime => Definition.WorstTime;
    public string SpaceComplexity => Translation?.SpaceComplexity ?? Definition.SpaceComplexity;
    public string Stability => LocalizationService.Instance[Definition.IsStable ? "Stable" : "Unstable"];
}
