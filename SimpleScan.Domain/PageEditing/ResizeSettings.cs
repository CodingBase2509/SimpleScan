using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.PageEditing;

public sealed record ResizeSettings
{
    public ResizeSettings(int width, int height)
    {
        Width = Guard.Positive(width, nameof(width));
        Height = Guard.Positive(height, nameof(height));
    }

    public int Width { get; }

    public int Height { get; }
}
