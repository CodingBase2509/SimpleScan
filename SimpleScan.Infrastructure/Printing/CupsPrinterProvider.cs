using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using SimpleScan.Application.Printing;
using SimpleScan.Domain.Common;
using SimpleScan.Domain.Printing;
using SimpleScan.Domain.Scanners;

namespace SimpleScan.Infrastructure.Printing;

public sealed class CupsPrinterProvider(TimeProvider timeProvider) : IPrinterProvider
{
    private const string IdPrefix = "cups:";

    public static bool CanHandle(string printerId) =>
        printerId.StartsWith(IdPrefix, StringComparison.Ordinal);

    public async Task<IReadOnlyList<ScannerDevice>> DiscoverAsync(CancellationToken cancellationToken)
    {
        var result = await RunCommandAsync("lpstat", ["-e"], cancellationToken);
        if (result.ExitCode != 0)
        {
            return [];
        }

        return result.StandardOutput
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .Select(name => new ScannerDevice(
                ToSimpleScanId(name),
                name,
                ScannerProtocol.Unknown,
                manufacturer: "CUPS",
                model: "Printer",
                networkAddress: $"cups://{name}",
                functions: [DeviceFunction.Print]))
            .ToArray();
    }

    public Task<PrinterCapabilities> GetCapabilitiesAsync(string printerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        EnsureCanHandle(printerId);
        return Task.FromResult(PrinterCapabilities.Default);
    }

    public async Task<ScannerStatus> GetStatusAsync(string printerId, CancellationToken cancellationToken)
    {
        EnsureCanHandle(printerId);
        var printerName = DecodePrinterId(printerId);
        var result = await RunCommandAsync("lpstat", ["-p", printerName], cancellationToken);

        return new ScannerStatus(
            printerId,
            result.ExitCode == 0 ? ScannerConnectionState.Online : ScannerConnectionState.Offline,
            timeProvider.GetUtcNow().UtcDateTime,
            result.ExitCode == 0 ? "CUPS printer is available." : result.StandardError);
    }

    public async Task<PrintJob> PrintAsync(
        string printerId,
        PrintSettings settings,
        IReadOnlyList<PrintFileInput> files,
        CancellationToken cancellationToken)
    {
        EnsureCanHandle(printerId);
        ArgumentNullException.ThrowIfNull(settings);

        if (files.Count == 0)
        {
            throw new DomainException("Cannot submit an empty print job.");
        }

        var printerName = DecodePrinterId(printerId);
        var submittedJobs = new List<string>();

        foreach (var file in files.OrderBy(file => file.PageNumber))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var args = BuildLpArguments(printerName, settings, file.FilePath);
            var result = await RunCommandAsync("lp", args, cancellationToken);
            if (result.ExitCode != 0)
            {
                throw new DomainException($"CUPS failed to print '{file.FileName}': {result.StandardError}");
            }

            submittedJobs.Add(result.StandardOutput.Trim());
        }

        var message = submittedJobs.Count == 0
            ? "Print job submitted to CUPS."
            : string.Join(" ", submittedJobs);

        return new PrintJob(
            $"cups-print-{Guid.NewGuid():N}",
            printerId,
            PrintJobStatus.Queued,
            timeProvider.GetUtcNow().UtcDateTime,
            message);
    }

    private static IReadOnlyList<string> BuildLpArguments(
        string printerName,
        PrintSettings settings,
        string filePath)
    {
        var args = new List<string>
        {
            "-d",
            printerName,
            "-n",
            settings.Copies.ToString(System.Globalization.CultureInfo.InvariantCulture),
            "-o",
            $"media={settings.PaperSize}",
            "-o",
            $"sides={ToCupsSides(settings.DuplexMode)}",
            "-o",
            $"print-color-mode={ToCupsColorMode(settings.ColorMode)}",
            filePath
        };

        return args;
    }

    private static string ToCupsSides(PrintDuplexMode duplexMode) =>
        duplexMode switch
        {
            PrintDuplexMode.DuplexLongEdge => "two-sided-long-edge",
            PrintDuplexMode.DuplexShortEdge => "two-sided-short-edge",
            _ => "one-sided"
        };

    private static string ToCupsColorMode(PrintColorMode colorMode) =>
        colorMode == PrintColorMode.Grayscale ? "monochrome" : "color";

    private static void EnsureCanHandle(string printerId)
    {
        if (!CanHandle(printerId))
        {
            throw new DomainException($"Printer '{printerId}' is not a CUPS printer.");
        }
    }

    private static string ToSimpleScanId(string printerName) =>
        $"{IdPrefix}{Base64UrlEncode(printerName)}";

    private static string DecodePrinterId(string printerId)
    {
        EnsureCanHandle(printerId);
        return Base64UrlDecode(printerId[IdPrefix.Length..]);
    }

    private static string Base64UrlEncode(string value) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');

    private static string Base64UrlDecode(string value)
    {
        var normalized = value.Replace('-', '+').Replace('_', '/');
        var padding = (4 - normalized.Length % 4) % 4;
        normalized = normalized.PadRight(normalized.Length + padding, '=');
        return Encoding.UTF8.GetString(Convert.FromBase64String(normalized));
    }

    private static async Task<CommandResult> RunCommandAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        try
        {
            using var process = new Process();
            process.StartInfo.FileName = fileName;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;

            foreach (var argument in arguments)
            {
                process.StartInfo.ArgumentList.Add(argument);
            }

            process.Start();

            var standardOutput = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardError = await process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            return new CommandResult(process.ExitCode, standardOutput, standardError);
        }
        catch (Exception exception) when (exception is Win32Exception or FileNotFoundException)
        {
            return new CommandResult(127, string.Empty, exception.Message);
        }
    }

    private sealed record CommandResult(
        int ExitCode,
        string StandardOutput,
        string StandardError);
}
