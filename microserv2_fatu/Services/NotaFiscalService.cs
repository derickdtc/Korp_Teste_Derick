using Microserv2Fatu.Data;
using Microserv2Fatu.DTOs;
using Microserv2Fatu.Exceptions;
using Microserv2Fatu.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Microserv2Fatu.Services;

public class NotaFiscalService(FaturamentoDbContext context, IEstoqueClient estoqueClient) : INotaFiscalService
{
    public async Task<NotaFiscalDto> CriarAsync(CriarNotaFiscalDto dto, CancellationToken cancellationToken)
    {
        var quantidadesPorProduto = ConsolidarItens(dto.Itens);
        var descricoesPorProduto = await estoqueClient.ObterDescricoesAsync(quantidadesPorProduto.Keys.ToList(), cancellationToken);
        var ultimoNumero = await context.NotasFiscais.Select(nota => (int?)nota.Numero).MaxAsync(cancellationToken) ?? 0;
        var nota = new NotaFiscal
        {
            Id = Guid.NewGuid(),
            Numero = ultimoNumero + 1,
            Status = StatusNotaFiscal.Aberta,
            Itens = quantidadesPorProduto.Select(item => new ItemNotaFiscal
            {
                Id = Guid.NewGuid(),
                ProdutoId = item.Key,
                Descricao = descricoesPorProduto[item.Key],
                Quantidade = (int)item.Value
            }).ToList()
        };

        context.NotasFiscais.Add(nota);
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new NumeroNotaDuplicadoException();
        }

        return ParaDto(nota);
    }

    public async Task<IReadOnlyCollection<NotaFiscalDto>> ListarAsync(CancellationToken cancellationToken) =>
        await context.NotasFiscais.AsNoTracking().Include(nota => nota.Itens).OrderByDescending(nota => nota.Numero)
            .Select(nota => new NotaFiscalDto
            {
                Id = nota.Id,
                Numero = nota.Numero,
                Status = nota.Status.ToString(),
                Itens = nota.Itens.OrderBy(item => item.ProdutoId).Select(item => new ItemNotaFiscalDto
                {
                    ProdutoId = item.ProdutoId,
                    Descricao = item.Descricao,
                    Quantidade = item.Quantidade
                }).ToList()
            }).ToListAsync(cancellationToken);

    public async Task<NotaFiscalDto> BuscarPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var nota = await context.NotasFiscais.AsNoTracking().Include(nota => nota.Itens)
            .FirstOrDefaultAsync(nota => nota.Id == id, cancellationToken)
            ?? throw new NotaFiscalNaoEncontradaException("Nota fiscal não encontrada.");
        return ParaDto(nota);
    }

    public async Task<NotaFiscalDto> ImprimirAsync(Guid id, CancellationToken cancellationToken)
    {
        var nota = await context.NotasFiscais.Include(nota => nota.Itens)
            .FirstOrDefaultAsync(nota => nota.Id == id, cancellationToken)
            ?? throw new NotaFiscalNaoEncontradaException("Nota fiscal não encontrada.");

        if (nota.Status != StatusNotaFiscal.Aberta)
        {
            throw new NotaFiscalFechadaException();
        }

        await estoqueClient.BaixarEstoqueAsync(nota.Itens, cancellationToken);
        nota.Status = StatusNotaFiscal.Fechada;
        await context.SaveChangesAsync(cancellationToken);

        return ParaDto(nota);
    }

    private static Dictionary<Guid, long> ConsolidarItens(IEnumerable<CriarItemNotaFiscalDto> itens)
    {
        var quantidadesPorProduto = itens.GroupBy(item => item.ProdutoId)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => (long)item.Quantidade));

        if (quantidadesPorProduto.ContainsKey(Guid.Empty))
        {
            throw new ArgumentException("O identificador do produto é obrigatório.");
        }

        if (quantidadesPorProduto.Any(item => item.Value > int.MaxValue))
        {
            throw new ArgumentException("A quantidade total de um produto é muito alta.");
        }

        return quantidadesPorProduto;
    }

    private static NotaFiscalDto ParaDto(NotaFiscal nota) => new()
    {
        Id = nota.Id,
        Numero = nota.Numero,
        Status = nota.Status.ToString(),
        Itens = nota.Itens.OrderBy(item => item.ProdutoId).Select(item => new ItemNotaFiscalDto
        {
            ProdutoId = item.ProdutoId,
            Descricao = item.Descricao,
            Quantidade = item.Quantidade
        }).ToList()
    };
}
