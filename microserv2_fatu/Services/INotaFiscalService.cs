using Microserv2Fatu.DTOs;

namespace Microserv2Fatu.Services;

public interface INotaFiscalService
{
    Task<NotaFiscalDto> CriarAsync(CriarNotaFiscalDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<NotaFiscalDto>> ListarAsync(CancellationToken cancellationToken);
    Task<NotaFiscalDto> BuscarPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<NotaFiscalDto> ImprimirAsync(Guid id, CancellationToken cancellationToken);
}
