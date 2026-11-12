using Sortacium.Core.Execution;

namespace Sortacium.Core.Algorithms;

public interface ISortingAlgorithm
{
    SortingTrace Bake(
        IReadOnlyList<double> values,
        CancellationToken cancellationToken = default);
}
