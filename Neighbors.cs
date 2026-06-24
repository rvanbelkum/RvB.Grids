using System.Runtime.CompilerServices;

namespace RvB.Grids;

public static class Neighbors {
    public static readonly GridPos North = (0, -1);
    public static readonly GridPos South = (0, 1);
    public static readonly GridPos West = (-1, 0);
    public static readonly GridPos East = (1, 0);
    public static readonly GridPos NorthEast = (1, -1);
    public static readonly GridPos NorthWest = (-1, -1);
    public static readonly GridPos SouthEast = (1, 1);
    public static readonly GridPos SouthWest = (-1, 1);
    public static readonly GridPos[] CardinalNeighbors = [East, South, West, North];
    public static readonly GridPos[] OrdinalNeighbors = [SouthEast, SouthWest, NorthWest, NorthEast];
    public static readonly GridPos[] AllNeighbors = [.. CardinalNeighbors, .. OrdinalNeighbors];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GridPos[] GetNeighbors(GridPos pos, GridPos[] directions) {
        GridPos[] neighbors = new GridPos[directions.Length];
        for (var n = 0; n < directions.Length; n += 1) {
            neighbors[n] = pos.Shift(directions[n]);
        }
        return neighbors;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<GridPos> GetCardinalNeighbors(GridPos pos) {
        return GetNeighbors(pos, CardinalNeighbors);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<GridPos> GetAllNeighbors(GridPos pos) {
        return GetNeighbors(pos, AllNeighbors);
    }
}

public static class Directions {
    public static readonly GridPosInternal North = (0, uint.MaxValue);
    public static readonly GridPosInternal South = (0, 1);
    public static readonly GridPosInternal West = (uint.MaxValue, 0);
    public static readonly GridPosInternal East = (1, 0);
    public static readonly GridPosInternal NorthEast = (1, uint.MaxValue);
    public static readonly GridPosInternal NorthWest = (uint.MaxValue, uint.MaxValue);
    public static readonly GridPosInternal SouthEast = (1, 1);
    public static readonly GridPosInternal SouthWest = (uint.MaxValue, 1);
    public static readonly GridPosInternal[] CardinalDirections = [East, South, West, North];
    public static readonly GridPosInternal[] OrdinalDirections = [SouthEast, SouthWest, NorthWest, NorthEast];
    public static readonly GridPosInternal[] AllDirections = [.. CardinalDirections, .. OrdinalDirections];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static GridPosInternal[] GetNeighbors(GridPosInternal pos, GridPosInternal[] directions) {
        var neighbors = new GridPosInternal[directions.Length];
        for (var n = 0; n < directions.Length; n += 1) {
            var (dc, dr) = directions[n];
            neighbors[n] = (pos.Col + dc, pos.Row + dr);
        }
        return neighbors;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<GridPosInternal> GetCardinalNeighbors(GridPosInternal pos) {
        return GetNeighbors(pos, CardinalDirections);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<GridPosInternal> GetAllNeighbors(GridPosInternal pos) {
        return GetNeighbors(pos, AllDirections);
    }
}
