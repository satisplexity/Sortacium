namespace Sortacium.Core.Execution;

public readonly record struct SortingAction(
    SortingActionKind Kind,
    int Left,
    int Right,
    int I,
    int J,
    double LeftValue,
    double RightValue)
{
    public SortingCodePoint CodePoint
        => Kind == SortingActionKind.Compare
            ? SortingCodePoint.CompareAdjacent
            : SortingCodePoint.SwapAdjacent;
}
