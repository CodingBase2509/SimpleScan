using System.Runtime.CompilerServices;

namespace SimpleScan.Domain.PageEditing;

public sealed class PageEditSettings
{
    public PageEditSettings(
        int rotationDegrees = 0,
        CropArea? crop = null,
        ResizeSettings? resize = null,
        bool convertToGrayscale = false,
        int brightness = 0,
        int contrast = 0)
    {
        RotationDegrees = NormalizeRotation(rotationDegrees);
        Crop = crop;
        Resize = resize;
        ConvertToGrayscale = convertToGrayscale;
        Brightness = ClampPercentage(brightness, nameof(brightness));
        Contrast = ClampPercentage(contrast, nameof(contrast));
    }

    public int RotationDegrees { get; }

    public CropArea? Crop { get; }

    public ResizeSettings? Resize { get; }

    public bool ConvertToGrayscale { get; }

    public int Brightness { get; }

    public int Contrast { get; }

    public static PageEditSettings None { get; } = new();

    private static int NormalizeRotation(int rotationDegrees)
    {
        var normalized = rotationDegrees % 360;

        if (normalized < 0)
        {
            normalized += 360;
        }

        if (normalized % 90 != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(rotationDegrees),
                rotationDegrees,
                "Rotation must be a multiple of 90 degrees.");
        }

        return normalized;
    }

    private static int ClampPercentage(int value, [CallerMemberName] string parameterName = "")
    {
        if (value is < -100 or > 100)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Value must be between -100 and 100.");
        }

        return value;
    }
}
