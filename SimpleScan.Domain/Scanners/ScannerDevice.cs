using SimpleScan.Domain.Common;

namespace SimpleScan.Domain.Scanners;

public sealed class ScannerDevice
{
    public ScannerDevice(
        string id,
        string name,
        ScannerProtocol protocol,
        string? manufacturer = null,
        string? model = null,
        string? networkAddress = null,
        IEnumerable<DeviceFunction>? functions = null)
    {
        Id = Guard.NotWhiteSpace(id, nameof(id));
        Name = Guard.NotWhiteSpace(name, nameof(name));
        Protocol = protocol;
        Manufacturer = string.IsNullOrWhiteSpace(manufacturer) ? null : manufacturer.Trim();
        Model = string.IsNullOrWhiteSpace(model) ? null : model.Trim();
        NetworkAddress = string.IsNullOrWhiteSpace(networkAddress) ? null : networkAddress.Trim();
        Functions = NormalizeFunctions(functions);
    }

    public string Id { get; }

    public string Name { get; }

    public ScannerProtocol Protocol { get; }

    public string? Manufacturer { get; }

    public string? Model { get; }

    public string? NetworkAddress { get; }

    public IReadOnlyList<DeviceFunction> Functions { get; }

    public bool CanScan => Functions.Contains(DeviceFunction.Scan);

    public bool CanPrint => Functions.Contains(DeviceFunction.Print);

    private static IReadOnlyList<DeviceFunction> NormalizeFunctions(IEnumerable<DeviceFunction>? functions)
    {
        var normalized = (functions ?? [DeviceFunction.Scan])
            .Distinct()
            .Order()
            .ToArray();

        if (normalized.Length == 0)
        {
            throw new ArgumentException("Device must expose at least one function.", nameof(functions));
        }

        return normalized;
    }
}
