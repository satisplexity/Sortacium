using Sortacium.Core.Execution;

namespace Sortacium.Core.Algorithms;

public sealed class QuickSortAlgorithm : ISortingAlgorithm
{
    public SortingTrace Bake(
        IReadOnlyList<double> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        double[] initial = values.ToArray();
        double[] working = (double[])initial.Clone();

        var actions = new List<SortingAction>();

        void Swap(int left, int right, int i, int j)
        {
            if (left == right)
                return;

            actions.Add(new SortingAction(
                SortingActionKind.Swap,
                left,
                right,
                i,
                j,
                working[left],
                working[right]));

            (working[left], working[right]) =
                (working[right], working[left]);
        }

        void Sort(int low, int high)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (low >= high)
                return;

            double pivot = working[high];

            int i = low;

            for (int j = low; j < high; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                actions.Add(new SortingAction(
                    SortingActionKind.Compare,
                    j,
                    high,
                    i,
                    j,
                    working[j],
                    pivot));

                if (working[j] >= pivot)
                    continue;

                Swap(i, j, i, j);
                i++;
            }

            Swap(i, high, i, high);

            Sort(low, i - 1);
            Sort(i + 1, high);
        }

        Sort(0, working.Length - 1);

        return new SortingTrace(initial, actions.ToArray());
    }
}
