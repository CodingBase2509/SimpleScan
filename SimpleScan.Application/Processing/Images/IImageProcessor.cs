using SimpleScan.Application.FileStorage;

namespace SimpleScan.Application.Processing.Images;

public interface IImageProcessor
{
    Task<BinaryFile> CreatePreviewAsync(
        Stream originalImage,
        ImageProcessingOptions options,
        CancellationToken cancellationToken);

    Task<BinaryFile> CreateThumbnailAsync(
        Stream originalImage,
        ImageProcessingOptions options,
        CancellationToken cancellationToken);

    Task<BinaryFile> RenderForPdfAsync(
        Stream originalImage,
        ImageProcessingOptions options,
        CancellationToken cancellationToken);
}
