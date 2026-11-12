namespace Sortacium.Core.Algorithms;

public static class SortingAlgorithmFactory
{
    public static ISortingAlgorithm Create(string id)
    {
        return id switch
        {
            "bubble-sort" => new BubbleSortAlgorithm(),

            "selection-sort" => new SelectionSortAlgorithm(),

            "merge-sort" => new MergeSortAlgorithm(),

            "quick-sort" => new QuickSortAlgorithm(),

            _ => throw new ArgumentOutOfRangeException(
                nameof(id),
                id,
                "Unknown sorting algorithm.")
        };
    }
}
