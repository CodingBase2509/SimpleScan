using SimpleScan.Application.Downloads;
using SimpleScan.Application.FileStorage;

namespace SimpleScan.Endpoints;

public static class DownloadEndpoints
{
    public static IEndpointRouteBuilder MapDownloadEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/downloads/{token}", DownloadAsync)
            .WithName("DownloadPdfExport");

        return endpoints;
    }

    private static async Task<IResult> DownloadAsync(
        string token,
        DownloadTicketService downloadTickets,
        IScanFileStorage files,
        CancellationToken cancellationToken)
    {
        var ticket = await downloadTickets.ConsumeAsync(token, cancellationToken);
        if (ticket is null)
        {
            return Results.NotFound();
        }

        if (string.IsNullOrWhiteSpace(ticket.FilePath) ||
            !await files.ExistsAsync(ticket.FilePath, cancellationToken))
        {
            return Results.NotFound();
        }

        Stream stream;

        try
        {
            stream = await files.OpenReadAsync(ticket.FilePath, cancellationToken);
        }
        catch (FileNotFoundException)
        {
            return Results.NotFound();
        }

        return Results.File(
            stream,
            "application/pdf",
            ticket.FileName,
            enableRangeProcessing: false);
    }
}
