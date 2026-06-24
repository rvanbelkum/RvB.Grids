using RvB.Graphs;
using System.Text;

namespace RvB.Grids;

public static class BooleanGridExtensions {
#if NET10_0_OR_GREATER
    extension<TGrid>(TGrid grid) where TGrid : IGrid<bool> {
    }
#else
#endif
}

public static class GridExtensions {
    public static void Display(this Grid<char> grid) {
        foreach (var row in grid.Rows) {
            foreach (var pos in row) {
                Console.Write(pos.Value);
            }
            Console.WriteLine();
        }
    }

    public static string Serialize(this Grid<char> grid) {
        var buf = new StringBuilder();
        for (var row = 0; row < grid.Height; ++row) {
            for (var col = 0; col < grid.Width; ++col) {
                buf.Append(grid[col, row]);
            }
            if (row < grid.Height - 1)
                buf.AppendLine();
        }
        return buf.ToString();
    }

    public static int GetMinimalDistance<TGrid>(this TGrid grid, (int col, int row) start, (int col, int row) end) where TGrid : IGrid<bool> {
        return Dijkstra.CalcMinimalDistance(start, end, GetNeighbors, 0);

        IEnumerable<(int, int)> GetNeighbors((int, int) node) {
            foreach (var n in grid.GetCardinalNeighbors(node)) {
                if (grid[n])
                    yield return n;
            }
        }
    }

    public delegate bool NeighborDistance(GridPos from, GridPos to, out int distance);

    public static int GetMinimalDistanceCardinal<T>(this IGrid<T> grid, GridPos start, Func<GridPos, bool> finished, Func<GridPos, GridPos, bool> canMoveTo)
        => GetMinimalDistance(start, finished, canMoveTo, p => grid.GetCardinalNeighbors(p));

    public static int GetMinimalDistanceCardinal<T>(this IGrid<T> grid, GridPos start, GridPos end, Func<GridPos, GridPos, bool> canMoveTo)
        => GetMinimalDistance(start, end, canMoveTo, p => grid.GetCardinalNeighbors(p));

    public static int GetMinimalDistanceCardinal<T>(this IGrid<T> grid, GridPos start, GridPos end, NeighborDistance tryGetNeighborDistance)
        => GetMinimalDistance(start, end, tryGetNeighborDistance, p => grid.GetCardinalNeighbors(p));

    public static int GetMinimalDistanceAll<T>(this IGrid<T> grid, GridPos start, Func<GridPos, bool> finished, Func<GridPos, GridPos, bool> canMoveTo)
        => GetMinimalDistance(start, finished, canMoveTo, p => grid.GetAllNeighbors(p));

    public static int GetMinimalDistanceAll<T>(this IGrid<T> grid, GridPos start, GridPos end, Func<GridPos, GridPos, bool> canMoveTo)
        => GetMinimalDistance(start, end, canMoveTo, p => grid.GetAllNeighbors(p));

    public static int GetMinimalDistanceAll<T>(this IGrid<T> grid, GridPos start, GridPos end, NeighborDistance tryGetNeighborDistance)
        => GetMinimalDistance(start, end, tryGetNeighborDistance, p => grid.GetAllNeighbors(p));

    private static int GetMinimalDistance(GridPos start, GridPos end, NeighborDistance tryGetNeighborDistance, Func<GridPos, List<GridPos>> neighbors) {
        return Dijkstra.CalcMinimalDistance(start, end, GetNeighbors, 0);

        IEnumerable<(GridPos, int)> GetNeighbors(GridPos node) {
            foreach (var neighbor in neighbors(node)) {
                if (tryGetNeighborDistance(node, neighbor, out var dist)) {
                    yield return (neighbor, dist);
                }
            }
        }
    }

    private static int GetMinimalDistance(GridPos start, GridPos end, Func<GridPos, GridPos, bool> canMoveTo, Func<GridPos, List<GridPos>> neighbors) {
        return Dijkstra.CalcMinimalDistance(start, end, GetNeighbors, 0);

        IEnumerable<(GridPos, int)> GetNeighbors(GridPos node) {
            foreach (var neighbor in neighbors(node)) {
                if (canMoveTo(node, neighbor)) {
                    yield return (neighbor, 1);
                }
            }
        }
    }

    private static int GetMinimalDistance(GridPos start, Func<GridPos, bool> finished, Func<GridPos, GridPos, bool> canMoveTo, Func<GridPos, List<GridPos>> neighbors) {
        return Dijkstra.CalcMinimalDistance(start, finished, GetNeighbors, 0);

        IEnumerable<(GridPos, int)> GetNeighbors(GridPos node) {
            foreach (var neighbor in neighbors(node)) {
                if (canMoveTo(node, neighbor)) {
                    yield return (neighbor, 1);
                }
            }
        }
    }
}

public static class GridSerializer {
    public static void Serialize<TGrid>(TGrid grid, Stream outStream) where TGrid : IGrid<bool> {
        using (var binaryWriter = new BinaryWriter(outStream)) {
            binaryWriter.Write(grid.Width);
            binaryWriter.Write(grid.Height);
            binaryWriter.Write(grid.TopLeftIndex.Col);
            binaryWriter.Write(grid.TopLeftIndex.Row);

            var data = new byte[(grid.Width + 7) >> 3];

            for (var row = 0; row < grid.Height; ++row) {
                var byteIndex = 0;
                var bitOffset = 0;
                ref var currByte = ref data[byteIndex];
                for (var col = 0; col < grid.Width; ++col) {
                    if (bitOffset == 8) {
                        bitOffset = 0;
                        currByte = ref data[++byteIndex];
                    }
                    if (grid[col, row]) {
                        currByte |= (byte)(1 << bitOffset);
                    }
                    bitOffset += 1;
                }
                binaryWriter.Write(data);
                Array.Clear(data);
            }
        }
    }

    public static TGrid Deserialize<TGrid>(Stream inStream, Func<int, int, (int, int), TGrid> createGrid) where TGrid : IGrid<bool> {
        using (var binaryReader = new BinaryReader(inStream)) {
            var width = binaryReader.ReadInt32();
            var height = binaryReader.ReadInt32();
            var topLeftIndexCol = binaryReader.ReadInt32();
            var topLeftIndexRow = binaryReader.ReadInt32();

            var grid = createGrid(width, height, (topLeftIndexCol, topLeftIndexRow));
            var data = new byte[(grid.Width + 7) >> 3];
            for (var row = 0; row < grid.Height; ++row) {
                binaryReader.Read(data);
                var byteIndex = 0;
                var bitOffset = 0;
                ref var currByte = ref data[byteIndex];
                for (var col = 0; col < grid.Width; ++col) {
                    if (bitOffset == 8) {
                        bitOffset = 0;
                        currByte = ref data[++byteIndex];
                    }
                    if ((currByte & (byte)(1 << bitOffset)) != 0) {
                        grid[col, row] = true;
                    }
                    bitOffset += 1;
                }
            }
            return grid;
        }
    }
}
