using Microserv1Est.DTOs;

namespace Microserv1Est.Services;

public interface IProdutoService
{
    Task<ProdutoDto> CriarAsync(CriarProdutoDto dto, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<ProdutoDto>> ListarAsync(CancellationToken cancellationToken);
    Task<ProdutoDto> BuscarPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task BaixarEstoqueAsync(BaixarEstoqueDto dto, CancellationToken cancellationToken);
}
