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
        string? networkAddress = null)
    {
        Id = Guard.NotWhiteSpace(id, nameof(id));
        Name = Guard.NotWhiteSpace(name, nameof(name));
        Protocol = protocol;
        Manufacturer = string.IsNullOrWhiteSpace(manufacturer) ? null : manufacturer.Trim();
        Model = string.IsNullOrWhiteSpace(model) ? null : model.Trim();
        NetworkAddress = string.IsNullOrWhiteSpace(networkAddress) ? null : networkAddress.Trim();
    }

    public string Id { get; }

    public string Name { get; }

    public ScannerProtocol Protocol { get; }

    public string? Manufacturer { get; }

    public string? Model { get; }

    public string? NetworkAddress { get; }
}
