using System.Collections.Generic;

namespace Sortacium.Presentation.Models;

public sealed class AlgorithmCatalog
{
    public Dictionary<string, AlgorithmDefinition> Algorithms { get; init; } = [];
}
