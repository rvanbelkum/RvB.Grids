using RvB.Linq;
using System.Numerics;

namespace RvB.Grids;

public static class GridFactory {
    private static readonly string[] s_rowSeparators = ["\r\n", "\n"];

    public static Grid<char> CreateCharGrid(StringRange text, ReadOnlySpan<string> rowSeparators = default)
        => Create(text, c => c, rowSeparators);

    public static Grid<T> CreateNumericGrid<T>(StringRange text, ReadOnlySpan<string> rowSeparators = default) where T : struct, INumber<T> {
        if (rowSeparators.IsEmpty)
            rowSeparators = s_rowSeparators;
        var rows = text.SplitAny(rowSeparators);
        var (height, width) = GetCountAndMinMaxLength(rows);
        var grid = new Grid<T>(width, height);

        var rowNr = 0;
        foreach (var row in rows) {
            var colNr = 0;
            foreach (var cell in row) {
                grid[colNr, rowNr] = T.Parse(new ReadOnlySpan<char>(in cell), null);
                colNr += 1;
            }
            rowNr += 1;
        }
        return grid;
    }

    public static Grid<TEnum> CreateEnumGrid<TEnum>(StringRange text, Dictionary<char, TEnum> mapping, ReadOnlySpan<string> rowSeparators = default) where TEnum : Enum
        => CreateEnumGrid(text, c => mapping[c], rowSeparators);

    public static Grid<TEnum> CreateEnumGrid<TEnum>(StringRange text, Func<char, TEnum> mapping, ReadOnlySpan<string> rowSeparators = default) where TEnum : Enum {
        if (rowSeparators.IsEmpty)
            rowSeparators = s_rowSeparators;
        Iterable<Split, StringRange> rows = text.SplitAny(rowSeparators);
        var (height, width) = GetCountAndMinMaxLength(rows);
        var grid = new Grid<TEnum>(width, height);

        var rowNr = 0;
        foreach (var row in rows) {
            var colNr = 0;
            foreach (var cell in row) {
                grid[colNr, rowNr] = mapping(cell);
                colNr += 1;
            }
            rowNr += 1;
        }
        return grid;
    }

    public static Grid<int> Create(StringRange text, Dictionary<char, int> mapping, ReadOnlySpan<string> rowSeparators = default)
        => Create(text, c => mapping[c], rowSeparators);

    public static Grid<T> Create<T>(StringRange text, Func<char, T> mapping, ReadOnlySpan<string> rowSeparators = default) {
        if (rowSeparators.IsEmpty)
            rowSeparators = s_rowSeparators;
        var rows = text.SplitAny(rowSeparators);
        var (height, width) = GetCountAndMinMaxLength(rows);
        var grid = new Grid<T>(width, height);

        var rowNr = 0;
        foreach (var row in rows) {
            var colNr = 0;
            foreach (var cell in row) {
                grid[colNr, rowNr] = mapping(cell);
                colNr += 1;
            }
            rowNr += 1;
        }
        return grid;
    }

    public static SparseGrid<T> CreateSparseGrid<T>(StringRange text, Func<char, T> mapping, ReadOnlySpan<string> rowSeparators = default) where T : notnull {
        if (rowSeparators.IsEmpty)
            rowSeparators = s_rowSeparators;
        var rows = text.SplitAny(rowSeparators);
        var grid = new SparseGrid<T>(1) { AutoGrow = SparseGridBehavior.Autogrow };

        var rowNr = 0;
        foreach (var row in rows) {
            var colNr = 0;
            foreach (var cell in row) {
                var value = mapping(cell);
                if (!EqualityComparer<T>.Default.Equals(value, default)) {
                    grid[colNr, rowNr] = value;
                }
                colNr += 1;
            }
            rowNr += 1;
        }
        return grid;
    }

    private static (int Count, int Length) GetCountAndMinMaxLength(Iterable<Split, StringRange> rows) {
        var length = 0;
        var count = 0;
        foreach (var row in rows) {
            length = Math.Max(length, row.Length);
            count += 1;
        }
        return (count, length);
    }
}
