using Domain.Entities;

namespace Infrastructure.Services.Export;

public interface IPdfExporter
{
    Task<byte[]> GenerateBoardPdfAsync(Board board, CancellationToken cancellationToken = default);
}
