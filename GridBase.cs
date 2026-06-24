using System.Runtime.CompilerServices;

namespace RvB.Grids;

public enum NeighborBehavior {
    Bounded,
    WrapAround
}

public abstract class GridBase<T> : IGrid<T> {
    public virtual int Height { get; protected set; }

    public virtual int Width { get; protected set; }

    public virtual GridPos TopLeftIndex { get; protected set; }

    public abstract T? this[int col, int row] { get; set; }

    public virtual T? this[GridPos pos] {
        get => this[pos.Col, pos.Row];
        set => this[pos.Col, pos.Row] = value;
    }

    public abstract IEnumerable<(T? Value, int Col)> GetRow(int row);

    public abstract IEnumerable<(T? Value, int Row)> GetCol(int col);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool InBounds(int col, int row) {
        return (uint)(col - TopLeftIndex.Col) < (uint)Width && (uint)(row - TopLeftIndex.Row) < (uint)Height;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public virtual bool InBounds(GridPos pos)
        => InBounds(pos.Col, pos.Row);

    public virtual List<GridPos> GetNeighbors(int col, int row, GridPos[] directions, NeighborBehavior behavior) {
        var neighbors = new List<GridPos>(directions.Length);
        if (behavior == NeighborBehavior.Bounded) {
            foreach (var neighbor in Neighbors.GetNeighbors((col, row), directions)) {
                if (InBounds(neighbor)) {
                    neighbors.Add(neighbor);
                }
            }
        } else if (behavior == NeighborBehavior.WrapAround) {
            foreach (var neighbor in Neighbors.GetNeighbors((col, row), directions)) {
                var (ncol, nrow) = neighbor;
                if (ncol < TopLeftIndex.Col) {
                    ncol += Width;
                } else if (ncol >= TopLeftIndex.Col + Width) {
                    ncol -= Width;
                }
                if (nrow < TopLeftIndex.Row) {
                    nrow += Height;
                } else if (nrow >= TopLeftIndex.Row + Height) {
                    nrow -= Height;
                }
                neighbors.Add((ncol, nrow));
            }
        }
        return neighbors;
    }

    public virtual int GetNeighbors(int col, int row, GridPos[] directions, Func<T?, GridPos, bool> filter, GridPos[] neighbors) {
        int n = 0;
        foreach (var neighbor in Neighbors.GetNeighbors((col, row), directions)) {
            if (InBounds(neighbor)) {
                if (filter(this[neighbor], neighbor)) {
                    neighbors[n++] = neighbor;
                }
            }
        }
        return n;
    }

    public virtual List<GridPos> GetAllNeighbors(int col, int row, NeighborBehavior behavior = NeighborBehavior.Bounded)
        => GetNeighbors(col, row, Neighbors.AllNeighbors, behavior);

    public virtual List<GridPos> GetAllNeighbors(GridPos pos, NeighborBehavior behavior = NeighborBehavior.Bounded)
        => GetNeighbors(pos.Col, pos.Row, Neighbors.AllNeighbors, behavior);

    public virtual List<GridPos> GetCardinalNeighbors(int col, int row, NeighborBehavior behavior = NeighborBehavior.Bounded)
        => GetNeighbors(col, row, Neighbors.CardinalNeighbors, behavior);

    public virtual List<GridPos> GetCardinalNeighbors(GridPos pos, NeighborBehavior behavior = NeighborBehavior.Bounded)
        => GetNeighbors(pos.Col, pos.Row, Neighbors.CardinalNeighbors, behavior);

    public virtual int GetCardinalNeighborsPos(GridPos pos, Func<T?, GridPos, bool> filter, GridPos[] neighbors) {
        int n = 0;
        foreach (var direction in Neighbors.CardinalNeighbors) {
            var neighbor = pos.Shift(direction);
            if (InBounds(neighbor) && filter(this[neighbor], neighbor)) {
                neighbors[n++] = neighbor;
            }
        }
        return n;
    }
}
