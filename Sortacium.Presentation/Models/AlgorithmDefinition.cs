using System.Collections.Generic;

namespace Sortacium.Presentation.Models;

public sealed class AlgorithmDefinition
{
    public string Name { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string Author { get; init; } = string.Empty;

    public string Year { get; init; } = string.Empty;

    public string BestTime { get; init; } = string.Empty;

    public string AverageTime { get; init; } = string.Empty;

    public string WorstTime { get; init; } = string.Empty;

    public string SpaceComplexity { get; init; } = string.Empty;

    public bool IsStable { get; init; }

    public Dictionary<string, CodeImplementation> Implementations
    { get; init; } = [];
}
