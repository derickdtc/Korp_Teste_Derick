using Microserv1Est.Data;
using Microserv1Est.DTOs;
using Microserv1Est.Exceptions;
using Microserv1Est.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Microserv1Est.Services;

public class ProdutoService(EstoqueDbContext context) : IProdutoService
{
    public async Task<ProdutoDto> CriarAsync(CriarProdutoDto dto, CancellationToken cancellationToken)
    {
        if (dto.Saldo < 0)
        {
            throw new ArgumentException("O saldo não pode ser negativo.");
        }

        var codigo = dto.Codigo.Trim();
        var descricao = dto.Descricao.Trim();

        if (string.IsNullOrWhiteSpace(codigo) || string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("Código e descrição não podem estar em branco.");
        }

        if (await context.Produtos.AnyAsync(produto => produto.Codigo == codigo, cancellationToken))
        {
            throw new CodigoProdutoDuplicadoException(codigo);
        }

        var produto = new Produto { Id = Guid.NewGuid(), Codigo = codigo, Descricao = descricao, Saldo = dto.Saldo };
        context.Produtos.Add(produto);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new CodigoProdutoDuplicadoException(codigo);
        }

        return ParaDto(produto);
    }

    public async Task<IReadOnlyCollection<ProdutoDto>> ListarAsync(CancellationToken cancellationToken) =>
        await context.Produtos.AsNoTracking().OrderBy(produto => produto.Codigo)
            .Select(produto => new ProdutoDto { Id = produto.Id, Codigo = produto.Codigo, Descricao = produto.Descricao, Saldo = produto.Saldo })
            .ToListAsync(cancellationToken);

    public async Task<ProdutoDto> BuscarPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var produto = await context.Produtos.AsNoTracking().FirstOrDefaultAsync(produto => produto.Id == id, cancellationToken)
            ?? throw new ProdutoNaoEncontradoException("Produto não encontrado.");
        return ParaDto(produto);
    }

    public async Task BaixarEstoqueAsync(BaixarEstoqueDto dto, CancellationToken cancellationToken)
    {
        var quantidadesPorProduto = dto.Itens.GroupBy(item => item.ProdutoId)
            .ToDictionary(grupo => grupo.Key, grupo => grupo.Sum(item => (long)item.Quantidade));

        if (quantidadesPorProduto.ContainsKey(Guid.Empty)) throw new ArgumentException("O identificador do produto é obrigatório.");
        if (quantidadesPorProduto.Any(item => item.Value > int.MaxValue)) throw new ArgumentException("A quantidade total de um produto é muito alta.");

        var ids = quantidadesPorProduto.Keys.ToList();
        await using var transacao = await context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await BloquearProdutosParaBaixaAsync(ids, transacao, cancellationToken);

            var produtos = await context.Produtos.Where(produto => ids.Contains(produto.Id)).ToListAsync(cancellationToken);
            var produtosPorId = produtos.ToDictionary(produto => produto.Id);

            foreach (var produtoId in ids)
                if (!produtosPorId.ContainsKey(produtoId)) throw new ProdutoNaoEncontradoException($"Produto {produtoId} não encontrado.");

            foreach (var produto in produtos)
                if (produto.Saldo < quantidadesPorProduto[produto.Id]) throw new SaldoInsuficienteException(produto.Codigo);

            foreach (var produto in produtos) produto.Saldo -= (int)quantidadesPorProduto[produto.Id];

            await context.SaveChangesAsync(cancellationToken);
            await transacao.CommitAsync(cancellationToken);
        }
        catch
        {
            await transacao.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static ProdutoDto ParaDto(Produto produto) => new()
    {
        Id = produto.Id, Codigo = produto.Codigo, Descricao = produto.Descricao, Saldo = produto.Saldo
    };

    private async Task BloquearProdutosParaBaixaAsync(
        IReadOnlyCollection<Guid> ids,
        IDbContextTransaction transacao,
        CancellationToken cancellationToken)
    {
        await using var comando = context.Database.GetDbConnection().CreateCommand();
        comando.Transaction = transacao.GetDbTransaction();
        comando.CommandText = """
            SELECT "Id"
            FROM "Produtos"
            WHERE "Id" = ANY (@ids)
            ORDER BY "Id"
            FOR UPDATE;
            """;

        var parametroIds = comando.CreateParameter();
        parametroIds.ParameterName = "ids";
        parametroIds.Value = ids.ToArray();
        comando.Parameters.Add(parametroIds);

        await comando.ExecuteNonQueryAsync(cancellationToken);
    }
}
