using Microserv2Fatu.Models;

namespace Microserv2Fatu.Services;

public interface IEstoqueClient
{
    Task<IReadOnlyDictionary<Guid, string>> ObterDescricoesAsync(IReadOnlyCollection<Guid> produtoIds, CancellationToken cancellationToken);
    Task BaixarEstoqueAsync(IReadOnlyCollection<ItemNotaFiscal> itens, CancellationToken cancellationToken);
}
