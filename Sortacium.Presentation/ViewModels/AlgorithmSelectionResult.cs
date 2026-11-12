using Sortacium.Presentation.Models;

namespace Sortacium.Presentation.ViewModels;

public sealed record AlgorithmSelectionResult(
    AlgorithmOption Algorithm,
    CodeLanguageOption Language);
