namespace DataMatrix;

/// <summary>Размер символа Data Matrix.</summary>
internal record MatrixSize(int Rows, int Columns, RegionCounts HorizontalRegions, RegionCounts VerticalRegions, int EccCount, int BlockCount)
{
    /// <summary>Преобразует кортеж в размер символа.</summary>
    public static implicit operator MatrixSize((int rows, int columns, RegionCounts horizontalRegions, RegionCounts verticalRegion, int eccCount, int blockCount) size) =>
        new(size.rows, size.columns, size.horizontalRegions, size.verticalRegion, size.eccCount, size.blockCount);
}
