namespace RvB.Grids;

public interface IGrid<T> {
    int Height { get; }

    int Width { get; }

    GridPos TopLeftIndex { get; }

    T? this[int colIndex, int rowIndex] { get; set; }

    public T? this[GridPos pos] { get; set; }

    List<GridPos> GetNeighbors(int col, int row, GridPos[] directions, NeighborBehavior behavior = NeighborBehavior.Bounded);

    List<GridPos> GetCardinalNeighbors(int col, int row, NeighborBehavior behavior = NeighborBehavior.Bounded);

    List<GridPos> GetCardinalNeighbors(GridPos pos, NeighborBehavior behavior = NeighborBehavior.Bounded);

    List<GridPos> GetAllNeighbors(int col, int row, NeighborBehavior behavior = NeighborBehavior.Bounded);

    List<GridPos> GetAllNeighbors(GridPos pos, NeighborBehavior behavior = NeighborBehavior.Bounded);
}

public interface IGridSerialize<T> {
    //byte[] Serialize();
    //static abstract Grid<T> Deserialize(byte[] bytes);

    string SerializeToString();

    string ComputeHash();

    static abstract IGrid<T> DeserializeFromString(string byteString);
}
