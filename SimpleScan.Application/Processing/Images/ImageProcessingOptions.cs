using SimpleScan.Domain.PageEditing;

namespace SimpleScan.Application.Processing.Images;

public sealed class ImageProcessingOptions
{
    public ImageProcessingOptions(
        PageEditSettings? editSettings = null,
        int? targetDpi = null,
        int? jpegQuality = null)
    {
        EditSettings = editSettings ?? PageEditSettings.None;
        TargetDpi = targetDpi;
        JpegQuality = jpegQuality;

        if (targetDpi is <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(targetDpi), targetDpi, "Target DPI must be greater than zero.");
        }

        if (jpegQuality is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(jpegQuality), jpegQuality, "JPEG quality must be between 1 and 100.");
        }
    }

    public PageEditSettings EditSettings { get; }

    public int? TargetDpi { get; }

    public int? JpegQuality { get; }
}
