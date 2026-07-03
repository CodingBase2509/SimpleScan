namespace SimpleScan.Application.Scanning;

internal static class ScanSettingsValidator
{
    public static void EnsureSupported(ScanSettings settings, ScannerCapabilities capabilities)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(capabilities);

        if (!capabilities.SupportedDpi.Contains(settings.Dpi))
        {
            throw new DomainException($"DPI value '{settings.Dpi}' is not supported by the scanner.");
        }

        if (!capabilities.SupportedColorModes.Contains(settings.ColorMode))
        {
            throw new DomainException($"Color mode '{settings.ColorMode}' is not supported by the scanner.");
        }

        if (!ContainsIgnoreCase(capabilities.SupportedPaperSizes, settings.PaperSize))
        {
            throw new DomainException($"Paper size '{settings.PaperSize}' is not supported by the scanner.");
        }

        if (!ContainsIgnoreCase(capabilities.SupportedSources, settings.Source))
        {
            throw new DomainException($"Scan source '{settings.Source}' is not supported by the scanner.");
        }

        if (settings.Duplex && !capabilities.SupportsDuplex)
        {
            throw new DomainException("Duplex scanning is not supported by the scanner.");
        }
    }

    private static bool ContainsIgnoreCase(IEnumerable<string> values, string value) =>
        values.Contains(value, StringComparer.OrdinalIgnoreCase);
}
