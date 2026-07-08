using System.Text.Json;
using Microsoft.JSInterop;

namespace SimpleScan.State;

public sealed class ManualScannerStorage(IJSRuntime jsRuntime)
{
    private const string StorageKey = "simpleScan.manualScanners.v1";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<ManualScannerEntry>> LoadAsync(CancellationToken cancellationToken)
    {
        var json = await jsRuntime.InvokeAsync<string?>(
            "simpleScan.storage.get",
            cancellationToken,
            StorageKey);

        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<IReadOnlyList<ManualScannerEntry>>(json, JsonOptions) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    public async Task SaveAsync(
        IReadOnlyList<ManualScannerEntry> entries,
        CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(entries, JsonOptions);

        await jsRuntime.InvokeVoidAsync(
            "simpleScan.storage.set",
            cancellationToken,
            StorageKey,
            json);
    }
}
