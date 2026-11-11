using System.Collections.Generic;

namespace Sortacium.Presentation.Models;

public sealed class AlgorithmDefinition 
{
    public string Name { get; init; } = string.Empty;

    public Dictionary<string, CodeImplementation> Implementations { get; init; } = [];
}
