using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Processing.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace SimpleScan.Infrastructure.Processing.Images;

public sealed class ImageSharpImageProcessor : IImageProcessor
{
    private const int PreviewMaxWidth = 1400;
    private const int PreviewMaxHeight = 2000;
    private const int ThumbnailMaxWidth = 280;
    private const int ThumbnailMaxHeight = 400;
    private const int DefaultJpegQuality = 82;

    public Task<BinaryFile> CreatePreviewAsync(
        Stream originalImage,
        ImageProcessingOptions options,
        CancellationToken cancellationToken) =>
        ProcessRasterOrCopyAsync(
            originalImage,
            options,
            "preview.jpg",
            PreviewMaxWidth,
            PreviewMaxHeight,
            cancellationToken);

    public Task<BinaryFile> CreateThumbnailAsync(
        Stream originalImage,
        ImageProcessingOptions options,
        CancellationToken cancellationToken) =>
        ProcessRasterOrCopyAsync(
            originalImage,
            options,
            "thumbnail.jpg",
            ThumbnailMaxWidth,
            ThumbnailMaxHeight,
            cancellationToken);

    public Task<BinaryFile> RenderForPdfAsync(
        Stream originalImage,
        ImageProcessingOptions options,
        CancellationToken cancellationToken) =>
        ProcessRasterOrCopyAsync(
            originalImage,
            options,
            "pdf-page.jpg",
            PreviewMaxWidth,
            PreviewMaxHeight,
            cancellationToken);

    private static async Task<BinaryFile> ProcessRasterOrCopyAsync(
        Stream originalImage,
        ImageProcessingOptions options,
        string outputFileName,
        int maxWidth,
        int maxHeight,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(originalImage);
        ArgumentNullException.ThrowIfNull(options);
        cancellationToken.ThrowIfCancellationRequested();

        if (originalImage.CanSeek)
        {
            originalImage.Position = 0;
        }

        try
        {
            using var image = await Image.LoadAsync(originalImage, cancellationToken);

            image.Mutate(context =>
            {
                ApplyEdits(context, image, options);
                context.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(maxWidth, maxHeight)
                });
            });

            var output = new MemoryStream();
            await image.SaveAsJpegAsync(output, new JpegEncoder
            {
                Quality = options.JpegQuality ?? DefaultJpegQuality
            }, cancellationToken);

            output.Position = 0;
            return new BinaryFile(output, outputFileName, "image/jpeg", output.Length);
        }
        catch (UnknownImageFormatException)
        {
            return await CopyUnsupportedAsync(originalImage, outputFileName, cancellationToken);
        }
    }

    private static void ApplyEdits(IImageProcessingContext context, Image image, ImageProcessingOptions options)
    {
        var edits = options.EditSettings;

        if (edits.Crop is not null)
        {
            var crop = edits.Crop;
            var width = Math.Min(crop.Width, image.Width - crop.X);
            var height = Math.Min(crop.Height, image.Height - crop.Y);

            if (width > 0 && height > 0)
            {
                context.Crop(new Rectangle(crop.X, crop.Y, width, height));
            }
        }

        if (edits.RotationDegrees != 0)
        {
            context.Rotate(edits.RotationDegrees);
        }

        if (edits.Resize is not null)
        {
            context.Resize(edits.Resize.Width, edits.Resize.Height);
        }

        if (edits.ConvertToGrayscale)
        {
            context.Grayscale();
        }

        if (edits.Brightness != 0)
        {
            context.Brightness(ToAdjustmentFactor(edits.Brightness));
        }

        if (edits.Contrast != 0)
        {
            context.Contrast(ToAdjustmentFactor(edits.Contrast));
        }
    }

    private static float ToAdjustmentFactor(int percentage) =>
        1F + percentage / 100F;

    private static async Task<BinaryFile> CopyUnsupportedAsync(
        Stream source,
        string outputFileName,
        CancellationToken cancellationToken)
    {
        if (source.CanSeek)
        {
            source.Position = 0;
        }

        var output = new MemoryStream();
        await source.CopyToAsync(output, cancellationToken);
        output.Position = 0;

        var fileName = Path.ChangeExtension(outputFileName, ".svg");
        return new BinaryFile(output, fileName, "image/svg+xml", output.Length);
    }
}
