using SimpleScan.Domain.Scanners;

namespace SimpleScan.State;

public sealed record ManualScannerEntry(
    string Id,
    string Name,
    string Address)
{
    public static ManualScannerEntry Create(string? name, string address)
    {
        var normalizedAddress = NormalizeAddress(address);
        var scannerName = string.IsNullOrWhiteSpace(name)
            ? CreateDefaultName(normalizedAddress)
            : name.Trim();

        return new ManualScannerEntry(
            CreateId(normalizedAddress),
            scannerName,
            normalizedAddress);
    }

    public ScannerDevice ToScannerDevice() =>
        new(
            Id,
            Name,
            ScannerProtocol.Escl,
            manufacturer: "Manual",
            model: "eSCL scanner",
            networkAddress: Address);

    private static string NormalizeAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            throw new ArgumentException("Scanner address cannot be empty.", nameof(address));
        }

        var value = address.Trim();
        if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
            !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
        {
            value = $"http://{value}";
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps) ||
            string.IsNullOrWhiteSpace(uri.Host))
        {
            throw new ArgumentException("Scanner address must be a valid HTTP or HTTPS address.", nameof(address));
        }

        if (string.IsNullOrWhiteSpace(uri.AbsolutePath) || uri.AbsolutePath == "/")
        {
            var builder = new UriBuilder(uri)
            {
                Path = "eSCL/"
            };

            uri = builder.Uri;
        }

        return uri.ToString();
    }

    private static string CreateDefaultName(string normalizedAddress)
    {
        var uri = new Uri(normalizedAddress);
        return $"Scanner {uri.Host}";
    }

    private static string CreateId(string normalizedAddress) =>
        $"naps2:Escl:{Base64UrlEncode(normalizedAddress)}";

    private static string Base64UrlEncode(string value) =>
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(value))
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
}
