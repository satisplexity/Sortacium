
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

using Avalonia.Platform;

using Sortacium.Presentation.Models;

namespace Sortacium.Presentation.Services;

public sealed class AlgorithmCodeService
{
    private readonly AlgorithmCatalog _catalog;

    public AlgorithmCodeService()
    {
        using Stream stream = AssetLoader.Open(
            new Uri("avares://Sortacium.Presentation/Resources/Algorithms.json"));

        _catalog = JsonSerializer.Deserialize<AlgorithmCatalog>(
            stream,
            new JsonSerializerOptions(JsonSerializerDefaults.Web))
            ?? throw new InvalidOperationException(
                "Failed to load algorithms.");
    }

    public IReadOnlyList<AlgorithmOption> GetAlgorithms()
    {
        return _catalog.Algorithms
            .Select(pair => new AlgorithmOption(
                pair.Key,
                pair.Value))
            .ToArray();
    }

    public IReadOnlyCollection<string> GetLanguages(string algorithmId)
    {
        return GetAlgorithm(algorithmId)
            .Implementations.Keys.ToArray();
    }

    public AlgorithmDefinition GetAlgorithm(string algorithmId)
    {
        if (!_catalog.Algorithms.TryGetValue(
                algorithmId,
                out AlgorithmDefinition? algorithm))
        {
            throw new KeyNotFoundException(
                $"Algorithm '{algorithmId}' not found.");
        }

        return algorithm;
    }

    public CodeImplementation GetCode(
        string algorithmId,
        string language)
    {
        AlgorithmDefinition algorithm = GetAlgorithm(algorithmId);

        if (!algorithm.Implementations.TryGetValue(
                language,
                out CodeImplementation? implementation))
        {
            throw new KeyNotFoundException(
                $"Language '{language}' not found.");
        }

        return implementation;
    }

    public int[] GetHighlightedLines(
        string algorithmId,
        string language,
        string codePointId)
    {
        CodeImplementation code = GetCode(algorithmId, language);

        return code.Mapping.TryGetValue(
            codePointId,
            out int[]? lines)
                ? lines
                : [];
    }
}
