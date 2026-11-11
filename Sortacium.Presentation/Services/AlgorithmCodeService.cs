using System;
using System.Collections.Generic;
using System.Text.Json;

using Avalonia.Platform;

using Sortacium.Presentation.Models;

namespace Sortacium.Presentation.Services;

public sealed class AlgorithmCodeService
{
    private readonly AlgorithmCatalog _catalog;

    public AlgorithmCodeService()
    {
        _catalog =
            JsonSerializer.Deserialize<AlgorithmCatalog>(
                AssetLoader.Open(new Uri("avares://Sortacium/Resources/Algorithms.json")),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException("Failed to load algorithms.");
    }

    public CodeImplementation GetCode(string algorithmId, string language)
    {
        if (!_catalog.Algorithms.TryGetValue(
                algorithmId, out AlgorithmDefinition? algorithm))
        {
            throw new KeyNotFoundException($"Algorithm '{algorithmId}' not found.");
        }

        if (!algorithm.Implementations.TryGetValue(
                language, out CodeImplementation? implementation))
        {
            throw new KeyNotFoundException($"Language '{language}' not found.");
        }

        return implementation;
    }

    public int[] GetHighlightedLines(
        string algorithmId,
        string language,
        string codePointId)
    {
        var code = GetCode(algorithmId, language);

        return code.Mapping.TryGetValue(codePointId, out var lines)
            ? lines
            : [];
    }
}
