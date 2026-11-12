namespace Sortacium.Presentation.Models;

public sealed record AlgorithmOption(
    string Id,
    AlgorithmDefinition Definition)
{
    public string Name => Definition.Name;

    public string Description => Definition.Description;

    public string Author => Definition.Author;

    public string Year => Definition.Year;

    public string BestTime => Definition.BestTime;

    public string AverageTime => Definition.AverageTime;

    public string WorstTime => Definition.WorstTime;

    public string SpaceComplexity => Definition.SpaceComplexity;

    public string Stability =>
        Definition.IsStable ? "Stable" : "Unstable";
}
