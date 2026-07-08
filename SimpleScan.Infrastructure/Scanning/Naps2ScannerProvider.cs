using NAPS2.Images;
using NAPS2.Images.ImageSharp;
using NAPS2.Scan;
using SimpleScan.Application.FileStorage;
using SimpleScan.Application.Scanners;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Scanners;
using SimpleScan.Domain.Scanning;

namespace SimpleScan.Infrastructure.Scanning;

public sealed class Naps2ScannerProvider : IScannerProvider
{
    private const string IdPrefix = "naps2:";
    private const string Manufacturer = "NAPS2";

    public static bool CanHandle(string scannerId) =>
        scannerId.StartsWith(IdPrefix, StringComparison.Ordinal);

    public async Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken)
    {
        var scanners = new Dictionary<string, ScannerDevice>(StringComparer.Ordinal);

        foreach (var driver in GetSupportedDrivers())
        {
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var device in await TryGetDevicesAsync(driver, cancellationToken))
            {
                scanners.TryAdd(ToSimpleScanId(device), ToScannerDevice(device));
            }
        }

        return scanners.Values
            .OrderBy(scanner => scanner.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<ScannerCapabilities> GetCapabilitiesAsync(
        string scannerId,
        CancellationToken cancellationToken)
    {
        var device = await GetDeviceAsync(scannerId, cancellationToken);

        try
        {
            using var context = CreateScanningContext();
            var controller = new ScanController(context);
            var caps = await controller.GetCaps(device, cancellationToken);

            return ToScannerCapabilities(caps);
        }
        catch
        {
            return ScannerCapabilities.Default;
        }
    }

    public async Task<ScannerStatus> GetStatusAsync(string scannerId, CancellationToken cancellationToken)
    {
        try
        {
            _ = await GetDeviceAsync(scannerId, cancellationToken);

            return new ScannerStatus(
                scannerId,
                ScannerConnectionState.Online,
                DateTime.UtcNow);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return new ScannerStatus(
                scannerId,
                ScannerConnectionState.Offline,
                DateTime.UtcNow,
                exception.Message);
        }
    }

    public async Task<BinaryFile> ScanAsync(
        string scannerId,
        ScanSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        var device = await GetDeviceAsync(scannerId, cancellationToken);

        using var context = CreateScanningContext();
        var controller = new ScanController(context);
        var options = new ScanOptions
        {
            Device = device,
            Driver = device.Driver,
            Dpi = settings.Dpi,
            BitDepth = ToBitDepth(settings.ColorMode),
            PageSize = ToNaps2PageSize(settings.PaperSize),
            PaperSource = ToPaperSource(settings),
            MaxQuality = true
        };

        await foreach (var image in controller.Scan(options, cancellationToken))
        {
            using (image)
            {
                var content = new MemoryStream();
                image.Save(content, ImageFileFormat.Png);
                content.Position = 0;

                return new BinaryFile(
                    content,
                    $"scan-{DateTime.UtcNow:yyyyMMdd-HHmmss}.png",
                    "image/png",
                    content.Length);
            }
        }

        throw new DomainException($"Scanner '{scannerId}' did not return a page.");
    }

    private async Task<ScanDevice> GetDeviceAsync(
        string scannerId,
        CancellationToken cancellationToken)
    {
        var deviceKey = DecodeScannerId(scannerId);
        var devices = await TryGetDevicesAsync(deviceKey.Driver, cancellationToken);

        return devices.FirstOrDefault(device => string.Equals(device.ID, deviceKey.DeviceId, StringComparison.Ordinal))
            ?? TryCreateManualDevice(deviceKey)
            ?? throw new DomainException($"Scanner '{scannerId}' is not available.");
    }

    private static ScanDevice? TryCreateManualDevice(Naps2DeviceKey deviceKey)
    {
        if (deviceKey.Driver != Driver.Escl ||
            !Uri.TryCreate(deviceKey.DeviceId, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            return null;
        }

        return new ScanDevice(
            Driver.Escl,
            deviceKey.DeviceId,
            uri.Host,
            string.Empty,
            deviceKey.DeviceId);
    }

    private static async Task<IReadOnlyList<ScanDevice>> TryGetDevicesAsync(
        Driver driver,
        CancellationToken cancellationToken)
    {
        try
        {
            using var context = CreateScanningContext();
            var controller = new ScanController(context);
            var devices = new List<ScanDevice>();

            await foreach (var device in controller.GetDevices(driver, cancellationToken))
            {
                devices.Add(device);
            }

            return devices;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return [];
        }
    }

    private static IReadOnlyList<Driver> GetSupportedDrivers()
    {
        if (OperatingSystem.IsWindows())
        {
            return [Driver.Wia, Driver.Twain, Driver.Escl];
        }

        if (OperatingSystem.IsLinux())
        {
            return [Driver.Sane, Driver.Escl];
        }

        return [Driver.Escl];
    }

    private static ScanningContext CreateScanningContext() =>
        new(new ImageSharpImageContext());

    private static ScannerDevice ToScannerDevice(ScanDevice device) =>
        new(
            ToSimpleScanId(device),
            device.Name,
            ToScannerProtocol(device.Driver),
            Manufacturer,
            model: device.Name,
            networkAddress: device.IconUri);

    private static ScannerCapabilities ToScannerCapabilities(ScanCaps caps)
    {
        var sourceCaps = GetSourceCaps(caps).ToArray();
        var combinedCaps = sourceCaps.Length > 0
            ? PerSourceCaps.UnionAll(sourceCaps)
            : null;

        var supportedDpi = GetSupportedDpi(combinedCaps).ToArray();
        var supportedColorModes = GetSupportedColorModes(combinedCaps).ToArray();
        var supportedPaperSizes = GetSupportedPaperSizes(combinedCaps).ToArray();
        var supportedSources = GetSupportedSources(caps).ToArray();

        return new ScannerCapabilities(
            supportedDpi.Length > 0 ? supportedDpi : ScannerCapabilities.Default.SupportedDpi,
            supportedColorModes.Length > 0 ? supportedColorModes : ScannerCapabilities.Default.SupportedColorModes,
            supportedPaperSizes.Length > 0 ? supportedPaperSizes : ScannerCapabilities.Default.SupportedPaperSizes,
            supportedSources.Length > 0 ? supportedSources : ScannerCapabilities.Default.SupportedSources,
            caps.PaperSourceCaps?.SupportsDuplex ?? caps.DuplexCaps is not null);
    }

    private static IEnumerable<PerSourceCaps> GetSourceCaps(ScanCaps caps)
    {
        if (caps.FlatbedCaps is not null)
        {
            yield return caps.FlatbedCaps;
        }

        if (caps.FeederCaps is not null)
        {
            yield return caps.FeederCaps;
        }

        if (caps.DuplexCaps is not null)
        {
            yield return caps.DuplexCaps;
        }
    }

    private static IEnumerable<int> GetSupportedDpi(PerSourceCaps? caps)
    {
        var values = caps?.DpiCaps?.Values ?? caps?.DpiCaps?.CommonValues;
        if (values is null)
        {
            yield break;
        }

        foreach (var dpi in values.Where(value => value > 0).Distinct().Order())
        {
            yield return dpi;
        }
    }

    private static IEnumerable<ScanColorMode> GetSupportedColorModes(PerSourceCaps? caps)
    {
        var bitDepthCaps = caps?.BitDepthCaps;
        if (bitDepthCaps is null)
        {
            yield break;
        }

        if (bitDepthCaps.SupportsColor)
        {
            yield return ScanColorMode.Color;
        }

        if (bitDepthCaps.SupportsGrayscale)
        {
            yield return ScanColorMode.Grayscale;
        }

        if (bitDepthCaps.SupportsBlackAndWhite)
        {
            yield return ScanColorMode.BlackAndWhite;
        }
    }

    private static IEnumerable<string> GetSupportedPaperSizes(PerSourceCaps? caps)
    {
        var pageSizeCaps = caps?.PageSizeCaps;
        if (pageSizeCaps is null)
        {
            yield break;
        }

        foreach (var candidate in new[]
                 {
                     NAPS2.Images.PageSize.A4,
                     NAPS2.Images.PageSize.A5,
                     NAPS2.Images.PageSize.Letter,
                     NAPS2.Images.PageSize.Legal,
                     NAPS2.Images.PageSize.A3
                 })
        {
            if (pageSizeCaps.Fits(candidate))
            {
                yield return candidate.ToString();
            }
        }
    }

    private static IEnumerable<string> GetSupportedSources(ScanCaps caps)
    {
        if (caps.PaperSourceCaps?.SupportsFlatbed == true || caps.FlatbedCaps is not null)
        {
            yield return "Flatbed";
        }

        if (caps.PaperSourceCaps?.SupportsFeeder == true || caps.FeederCaps is not null)
        {
            yield return "Feeder";
        }

        if (caps.PaperSourceCaps?.SupportsDuplex == true || caps.DuplexCaps is not null)
        {
            yield return "Duplex";
        }
    }

    private static BitDepth ToBitDepth(ScanColorMode colorMode) =>
        colorMode switch
        {
            ScanColorMode.Grayscale => BitDepth.Grayscale,
            ScanColorMode.BlackAndWhite => BitDepth.BlackAndWhite,
            _ => BitDepth.Color
        };

    private static PaperSource ToPaperSource(ScanSettings settings)
    {
        if (settings.Duplex)
        {
            return PaperSource.Duplex;
        }

        return settings.Source.Equals("Feeder", StringComparison.OrdinalIgnoreCase)
            ? PaperSource.Feeder
            : PaperSource.Flatbed;
    }

    private static NAPS2.Images.PageSize? ToNaps2PageSize(string pageSize) =>
        NAPS2.Images.PageSize.Parse(pageSize) ?? pageSize.ToUpperInvariant() switch
        {
            "A3" => NAPS2.Images.PageSize.A3,
            "A4" => NAPS2.Images.PageSize.A4,
            "A5" => NAPS2.Images.PageSize.A5,
            "LETTER" => NAPS2.Images.PageSize.Letter,
            "LEGAL" => NAPS2.Images.PageSize.Legal,
            _ => NAPS2.Images.PageSize.A4
        };

    private static ScannerProtocol ToScannerProtocol(Driver driver) =>
        driver switch
        {
            Driver.Escl => ScannerProtocol.Escl,
            Driver.Sane => ScannerProtocol.Sane,
            Driver.Twain => ScannerProtocol.Twain,
            Driver.Wia => ScannerProtocol.Wia,
            _ => ScannerProtocol.Unknown
        };

    private static string ToSimpleScanId(ScanDevice device) =>
        $"{IdPrefix}{device.Driver}:{Base64UrlEncode(device.ID)}";

    private static Naps2DeviceKey DecodeScannerId(string scannerId)
    {
        if (!CanHandle(scannerId))
        {
            throw new DomainException($"Scanner '{scannerId}' is not a NAPS2 scanner.");
        }

        var value = scannerId[IdPrefix.Length..];
        var separatorIndex = value.IndexOf(':', StringComparison.Ordinal);
        if (separatorIndex <= 0 || separatorIndex == value.Length - 1)
        {
            throw new DomainException($"Scanner '{scannerId}' has an invalid NAPS2 id.");
        }

        var driverName = value[..separatorIndex];
        var encodedDeviceId = value[(separatorIndex + 1)..];

        if (!Enum.TryParse<Driver>(driverName, ignoreCase: false, out var driver))
        {
            throw new DomainException($"Scanner '{scannerId}' has an unsupported NAPS2 driver.");
        }

        return new Naps2DeviceKey(driver, Base64UrlDecode(encodedDeviceId));
    }

    private static string Base64UrlEncode(string value) =>
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static string Base64UrlDecode(string value)
    {
        var padded = value
            .Replace('-', '+')
            .Replace('_', '/');

        padded = padded.PadRight(padded.Length + ((4 - padded.Length % 4) % 4), '=');

        return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(padded));
    }

    private sealed record Naps2DeviceKey(Driver Driver, string DeviceId);
}
