using Sortacium.Core.Execution;

namespace Sortacium.Core.Algorithms;

public sealed class MergeSortAlgorithm : ISortingAlgorithm
{
    public SortingTrace Bake(
        IReadOnlyList<double> values,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);

        double[] initial = values.ToArray();
        double[] working = (double[])initial.Clone();

        int[] ids = Enumerable.Range(0, working.Length).ToArray();

        var actions = new List<SortingAction>();

        void Sort(int left, int right)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (left >= right)
                return;

            int middle = left + (right - left) / 2;

            Sort(left, middle);
            Sort(middle + 1, right);

            Merge(left, middle, right);
        }

        void Merge(int left, int middle, int right)
        {
            int[] leftIds = ids[left..(middle + 1)];
            int[] rightIds = ids[(middle + 1)..(right + 1)];

            int[] result = new int[right - left + 1];

            int i = 0;
            int j = 0;
            int k = 0;

            while (i < leftIds.Length && j < rightIds.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int leftId = leftIds[i];
                int rightId = rightIds[j];

                int leftIndex = left + i;
                int rightIndex = middle + 1 + j;

                actions.Add(new SortingAction(
                    SortingActionKind.Compare,
                    leftIndex,
                    rightIndex,
                    i,
                    j,
                    working[leftIndex],
                    working[rightIndex]));

                if (initial[leftId] <= initial[rightId])
                {
                    result[k++] = leftId;
                    i++;
                }
                else
                {
                    result[k++] = rightId;
                    j++;
                }
            }

            while (i < leftIds.Length)
                result[k++] = leftIds[i++];

            while (j < rightIds.Length)
                result[k++] = rightIds[j++];

            var positions = new Dictionary<int, int>();

            for (int position = left; position <= right; position++)
                positions[ids[position]] = position;

            for (int offset = 0; offset < result.Length; offset++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int target = left + offset;
                int desiredId = result[offset];

                if (ids[target] == desiredId)
                    continue;

                int source = positions[desiredId];

                actions.Add(new SortingAction(
                    SortingActionKind.Swap,
                    target,
                    source,
                    target,
                    source,
                    working[target],
                    working[source]));

                int displacedId = ids[target];

                (ids[target], ids[source]) =
                    (ids[source], ids[target]);

                (working[target], working[source]) =
                    (working[source], working[target]);

                positions[desiredId] = target;
                positions[displacedId] = source;
            }
        }

        Sort(0, working.Length - 1);

        return new SortingTrace(initial, actions.ToArray());
    }
}
