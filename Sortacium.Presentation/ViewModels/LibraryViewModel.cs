using System.Collections.Generic;
using System.Linq;

using AvaloniaEdit.Document;

using CommunityToolkit.Mvvm.ComponentModel;

using Sortacium.Presentation.Models;
using Sortacium.Presentation.Services;

namespace Sortacium.Presentation.ViewModels;

public partial class LibraryViewModel : ViewModelBase
{
    private readonly AlgorithmCodeService _codeService;
    private readonly IReadOnlyList<CodeLanguageOption> _allLanguages;

    public IReadOnlyList<AlgorithmOption> AvailableAlgorithms { get; }

    [ObservableProperty]
    private AlgorithmOption? _previewAlgorithm;

    [ObservableProperty]
    private IReadOnlyList<CodeLanguageOption> _previewAvailableLanguages = [];

    [ObservableProperty]
    private CodeLanguageOption? _previewLanguage;

    [ObservableProperty]
    private TextDocument _previewCodeDocument = new();

    public LibraryViewModel(
        AlgorithmCodeService codeService,
        IReadOnlyList<AlgorithmOption> availableAlgorithms,
        IReadOnlyList<CodeLanguageOption> allLanguages)
    {
        _codeService = codeService;
        _allLanguages = allLanguages;

        AvailableAlgorithms = availableAlgorithms;
    }

    public void BeginSelection(AlgorithmOption activeAlgorithm, CodeLanguageOption? activeLanguage)
    {
        PreviewAlgorithm = activeAlgorithm;

        RefreshPreviewLanguages(activeLanguage?.Id);
    }

    partial void OnPreviewAlgorithmChanged(AlgorithmOption? value)
    {
        RefreshPreviewLanguages(PreviewLanguage?.Id);
    }

    partial void OnPreviewLanguageChanged(CodeLanguageOption? value)
    {
        LoadPreviewCode();
    }

    private void RefreshPreviewLanguages(string? preferredLanguageId)
    {
        if (PreviewAlgorithm is not { } algorithm)
        {
            PreviewAvailableLanguages = [];
            PreviewLanguage = null;
            PreviewCodeDocument = new TextDocument();

            return;
        }

        IReadOnlyCollection<string> supportedLanguages = _codeService.GetLanguages(algorithm.Id);

        PreviewAvailableLanguages =
            _allLanguages
            .Where(language => supportedLanguages.Contains(language.Id))
            .ToArray();

        PreviewLanguage =
            PreviewAvailableLanguages
            .FirstOrDefault(language => language.Id == preferredLanguageId)
            ??
            PreviewAvailableLanguages.FirstOrDefault();

        LoadPreviewCode();
    }

    private void LoadPreviewCode()
    {
        if (GetSelection() is not { } selection)
        {
            PreviewCodeDocument = new TextDocument();

            return;
        }

        CodeImplementation implementation =
            _codeService.GetCode(selection.Algorithm.Id, selection.Language.Id);

        PreviewCodeDocument = new TextDocument(implementation.Source);
    }

    public AlgorithmSelectionResult? GetSelection()
    {
        if (PreviewAlgorithm is not { } algorithm
            || PreviewLanguage is not { } language)
        {
            return null;
        }

        if (!_codeService.GetLanguages(algorithm.Id).Contains(language.Id))
        {
            return null;
        }

        return new AlgorithmSelectionResult(algorithm, language);
    }
}
