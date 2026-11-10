namespace Sortacium.Core.Execution;

public readonly record struct SortingAction(
    SortingActionKind Kind,
    ushort Left,
    ushort Right,
    ushort I,
    ushort J,
    double LeftValue,
    double RightValue)
{
    public SortingCodePoint CodePoint
        => Kind == SortingActionKind.Compare
            ? SortingCodePoint.CompareAdjacent
            : SortingCodePoint.SwapAdjacent;
}
