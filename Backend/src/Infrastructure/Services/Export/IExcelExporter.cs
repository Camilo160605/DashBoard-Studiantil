using Domain.Entities;

namespace Infrastructure.Services.Export;

public interface IExcelExporter
{
    Task<byte[]> GenerateBoardExcelAsync(Board board, CancellationToken cancellationToken = default);
}
