using Microserv2Fatu.Models;

namespace Microserv2Fatu.Services;

public interface IEstoqueClient
{
    Task BaixarEstoqueAsync(IReadOnlyCollection<ItemNotaFiscal> itens, CancellationToken cancellationToken);
}
