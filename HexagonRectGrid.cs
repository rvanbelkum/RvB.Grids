using System.Collections;
using System.Runtime.CompilerServices;

namespace RvB.Grids;

public readonly record struct Cell<T> {
    private readonly HexagonRectGrid<T> _hexagonRectGrid;

    public Cell(HexagonRectGrid<T> grid, HexagonRectGrid.Id id, T value) {
        _hexagonRectGrid = grid;
        Id = id;
        Value = value;
    }

    public HexagonRectGrid.Id Id { get; }

    public int Index => _hexagonRectGrid.Index(Id.Col, Id.Row);

    public T Value { get; }

    public IEnumerable<Cell<T>> GetNeighbors() {
        return _hexagonRectGrid.GetNeighbors(Id);
    }

    public IEnumerable<Cell<T>> WalkNeighbors(HexagonRectGrid.Direction direction) {
        return _hexagonRectGrid.WalkNeighbors(Id, direction);
    }
}

public abstract class HexagonRectGrid {
    private static readonly Dictionary<Direction, (int dCol, int dRow)> s_flatNeighbors = new() {
        [Direction.North] = (0, -2),
        [Direction.South] = (0, 2),
        [Direction.NorthWest] = (-1, -1),
        [Direction.NorthEast] = (1, -1),
        [Direction.SouthWest] = (-1, 1),
        [Direction.SouthEast] = (1, 1)
    };
    private static readonly Dictionary<Direction, (int dCol, int dRow)> s_pointyNeighbors = new() {
        [Direction.West] = (-2, 0),
        [Direction.East] = (2, 0),
        [Direction.NorthWest] = (-1, -1),
        [Direction.NorthEast] = (1, -1),
        [Direction.SouthWest] = (-1, 1),
        [Direction.SouthEast] = (1, 1)
    };

    protected Dictionary<Direction, (int dCol, int dRow)> Neighbors = s_flatNeighbors;

    public readonly record struct Id {
        public Id(int col, int row) {
            Col = col;
            Row = row;
        }

        public int Col { get; }
        public int Row { get; }
    }

    public enum HexOrientation {
        PointyTopped,
        FlatTopped,
    }

    public enum HexStagger {
        Even,
        Odd,
    }

    public enum Direction {
        North,
        South,
        East,
        West,
        NorthWest,
        NorthEast,
        SouthWest,
        SouthEast,
    }

    public HexOrientation Orientation {
        get;
        init {
            Neighbors = value == HexOrientation.FlatTopped ? s_flatNeighbors : s_pointyNeighbors;
            field = value;
        }
    } = HexOrientation.FlatTopped;

    public HexStagger Stagger { get; init; } = HexStagger.Odd;
}

public sealed class HexagonRectGrid<T> : HexagonRectGrid, IEnumerable<Cell<T>> {
    private readonly T[] _grid;

    public int Width { get; }

    public int Height { get; }

    public HexagonRectGrid(int width, int height) {
        Width = width;
        Height = height;
        var hexagonCount = (width * height + 1) / 2;
        _grid = new T[hexagonCount];
    }

    public HexagonRectGrid(int width, int height, T defaultValue) : this(width, height) {
        Array.Fill(_grid, defaultValue);
    }

    public HexagonRectGrid<T> Clone() {
        var clone = new HexagonRectGrid<T>(Width, Height);
        _grid.CopyTo(clone._grid);
        return clone;
    }

    public ref T this[Id id] {
        get => ref _grid[Index(id.Col, id.Row)];
    }

    public IEnumerable<IEnumerable<Cell<T>>> TraverseByColumn() {
        for (var col = 0; col < Width; col += 1) {
            yield return Column(col);
        }
    }

    public IEnumerable<IEnumerable<Cell<T>>> TraverseByRow() {
        for (var row = 0; row < Height; row += 1) {
            yield return Row(row);
        }
    }

    public IEnumerable<Cell<T>> Column(int col) {
        var firstRow = Stagger == HexStagger.Odd ? col % 2 : 1 - col % 2;
        for (var row = firstRow; row < Height; row += 2) {
            yield return new(this, new(col, row), _grid[Index(col, row)]);
        }
    }

    public IEnumerable<Cell<T>> Row(int row) {
        var firstCol = Stagger == HexStagger.Odd ? row % 2 : 1 - row % 2;
        for (var col = firstCol; col < Width; col += 2) {
            yield return new(this, new(col, row), _grid[Index(col, row)]);
        }
    }

    public IEnumerable<Cell<T>> GetNeighbors(Id pos) {
        foreach (var (_, (dCol, dRow)) in Neighbors) {
            var col = pos.Col + dCol;
            var row = pos.Row + dRow;
            if ((uint)col < (uint)Width && (uint)row < (uint)Height) {
                yield return new(this, new(col, row), _grid[Index(col, row)]);
            }
        }
    }

    public bool TryGetNeighbor(Id id, Direction direction, out Cell<T> neighbor) {
        if (Neighbors.TryGetValue(direction, out var dir)) {
            var col = id.Col + dir.dCol;
            var row = id.Row + dir.dRow;
            if ((uint)col < (uint)Width && (uint)row < (uint)Height) {
                neighbor = new(this, new(col, row), _grid[Index(col, row)]);
                return true;
            }
        }
        neighbor = default;
        return false;
    }

    public IEnumerable<Cell<T>> WalkNeighbors(Id id, Direction direction) {
        while (TryGetNeighbor(id, direction, out var neighbor)) {
            yield return neighbor;
            id = neighbor.Id;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int Index(int col, int row) {
        return Width * (row >> 1) + ((Width + (int)Stagger) * (row & 1) >> 1) + (col >> 1);
    }

    public IEnumerator<Cell<T>> GetEnumerator() {
        var row = 0;
        foreach (var hexRow in TraverseByRow()) {
            var col = Stagger == HexStagger.Odd ? row % 2 : 1 - row % 2;
            foreach (var cell in hexRow) {
                yield return new(this, new(col, row), cell.Value);
                col += 2;
            }
            row += 1;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/*
Small
Type = Pointy
Stagger = Odd   Stagger = Even
 /.\ / \          / \ / \
|...|   |        |   |   |
 \./ \ / \      / \ / \ /
  |   |   |    |   |   |
   \ / \ /      \ / \ /

Large
Type = Pointy
Stagger = Odd   Stagger = Even
  /.\   / \
 /...\ /   \
|.....|     |
|.....|     |
 \.../ \   / \
  \./   \ /   \
   |     |     |
   |     |     |
    \   / \   /
     \ /   \ /

Small
Type = Flat
Stagger = Odd   Stagger = Even  Tiny
 __    __          __    __      _   _ 
/..\__/  \      __/  \__/  \    /.\_/ \
\__/  \__/     /  \__/  \__/    \_/ \_/
/  \__/  \     \__/  \__/       / \_/ \
\__/  \__/        \__/          \_/ \_/

1 = H     (r+1)/2 H
2 = 1.5H
3 = 2H
4 = 2.5H
5 = 3H

Large
Type = Flat
Stagger = Odd       Stagger = Even
  ___       ___            ___       ___
 /...\     /   \          /   \     /   \
/.....\___/     \     ___/     \___/     \
\...../   \     /    /   \     /   \     /
 \___/     \___/    /     \___/     \___/
     \     /        \     /   \     /
      \___/          \___/     \___/
 */
public static class HexagonRectGridExtensions {
    extension<T>(HexagonRectGrid<T> hexagonRectGrid) {
        public void Print(T defaultValue) {
            hexagonRectGrid.Print(defaultValue, []);
        }

        public void Print(T defaultValue, HashSet<HexagonRectGrid.Id> highlights) {
            for (var row = 0; row < hexagonRectGrid.Height; row += 1) {
                if (row % 2 == 1)
                    Console.Write("  ");
                for (var col = row % 2; col < hexagonRectGrid.Width; col += 2) {
                    var value = hexagonRectGrid[new(col, row)];
                    if (!defaultValue!.Equals(value)) {
                        if (highlights.Contains(new(col, row))) {
                            Console.ForegroundColor = ConsoleColor.DarkYellow;
                        }
                        Console.Write($" {value}  ");
                    } else {
                        if (highlights.Contains(new(col, row))) {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.Write(" X  ");
                        } else {
                            Console.Write(" .  ");
                        }
                    }
                    Console.ResetColor();
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }
    }
}
