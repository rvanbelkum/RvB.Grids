using System.Collections;

namespace RvB.Grids;

public sealed class JaggedGrid<T> : IEnumerable<GridPos> {
    readonly T[][] _grid;

    public JaggedGrid(T[][] grid) {
        ArgumentNullException.ThrowIfNull(grid, nameof(grid));
        if (grid.Length == 0) {
            throw new ArgumentException("Grid has no rows", nameof(grid));
        }
        _grid = grid;
        Height = grid.Length;
        MaxWidth = grid.Max(r => r.Length);
    }

    public JaggedGrid(IEnumerable<IEnumerable<T>> grid) {
        ArgumentNullException.ThrowIfNull(grid, nameof(grid));
        _grid = grid.Select(r => r.ToArray()).ToArray();
        if (_grid.Length == 0) {
            throw new ArgumentException("Grid has no rows", nameof(grid));
        }
        Height = _grid.Length;
        MaxWidth = _grid.Max(r => r.Length);
    }

    public int Height { get; }

    public int MaxWidth { get; }

    public int Width(Index rowIndex) {
        int row = rowIndex.IsFromEnd ? Height - rowIndex.Value : rowIndex.Value;
        if (!InBounds(row))
            throw new ArgumentOutOfRangeException(nameof(rowIndex));
        return _grid[row].Length;
    }

    public T[] this[int row] {
        get {
            if (!InBounds(row))
                throw new ArgumentOutOfRangeException(nameof(row));
            return _grid[row];
        }
    }

    public T this[Index rowIndex, Index colIndex] {
        get {
            int row = rowIndex.IsFromEnd ? Height - rowIndex.Value : rowIndex.Value;
            int col = colIndex.IsFromEnd ? Width(row) - colIndex.Value : colIndex.Value;
            if (!InBounds(row))
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            if (!InBounds(col, row))
                throw new ArgumentOutOfRangeException(nameof(colIndex));
            return _grid[rowIndex][colIndex];
        }
        set {
            int row = rowIndex.IsFromEnd ? Height - rowIndex.Value : rowIndex.Value;
            int col = colIndex.IsFromEnd ? Width(row) - colIndex.Value : colIndex.Value;
            if (!InBounds(row))
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            if (!InBounds(col, row))
                throw new ArgumentOutOfRangeException(nameof(colIndex));
            _grid[rowIndex][colIndex] = value;
        }
    }

    public T this[GridPos pos] {
        get {
            return this[pos.Row, pos.Col];
        }
        set {
            this[pos.Row, pos.Col] = value;
        }
    }

    public bool InBounds(GridPos pos) {
        return pos.Col >= 0 && pos.Row >= 0 && pos.Row < _grid.Length && pos.Col < _grid[pos.Row].Length;
    }

    public bool InBounds(int col, int row) {
        return col >= 0 && row >= 0 && row < _grid.Length && col < _grid[row].Length;
    }

    public bool InBounds(int row) {
        return row >= 0 && row < _grid.Length;
    }

    public IEnumerable<(int, int)> GetNeighbors(int col, int row, GridPos[] directions) {
        foreach (var neighbor in Neighbors.GetNeighbors((col, row), directions)) {
            if (InBounds(neighbor))
                yield return neighbor;
        }
    }

    public IEnumerable<GridPos> GetCardinalNeigbors(int col, int row) {
        return GetNeighbors(col, row, Neighbors.CardinalNeighbors);
    }

    public IEnumerable<GridPos> GetAllNeighbors(int col, int row) {
        return GetNeighbors(col, row, Neighbors.AllNeighbors);
    }

    public IEnumerable<GridPos> GetCardinalNeigbors(GridPos pos) {
        return GetNeighbors(pos.Col, pos.Row, Neighbors.CardinalNeighbors);
    }

    public IEnumerable<GridPos> GetAllNeighbors(GridPos pos) {
        return GetNeighbors(pos.Col, pos.Row, Neighbors.AllNeighbors);
    }

    public IEnumerator<GridPos> GetEnumerator() {
        for (int row = 0; row < _grid.Length; row++) {
            for (int col = 0; col < _grid[row].Length; ++col) {
                yield return (col, row);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
