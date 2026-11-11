using System.Collections.Generic;

namespace Sortacium.Presentation.Models;

public sealed class CodeImplementation
{
    public List<string> Lines { get; init; } = [];

    public Dictionary<string, int[]> Mapping { get; init; } = [];

    public string Source => string.Join("\n", Lines);
}
