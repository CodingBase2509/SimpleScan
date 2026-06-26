using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.PageEditing;

public sealed record CropArea
{
    public CropArea(int x, int y, int width, int height)
    {
        if (x < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(x), x, "Value cannot be negative.");
        }

        if (y < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(y), y, "Value cannot be negative.");
        }

        X = x;
        Y = y;
        Width = Guard.Positive(width);
        Height = Guard.Positive(height);
    }

    public int X { get; }

    public int Y { get; }

    public int Width { get; }

    public int Height { get; }
}
