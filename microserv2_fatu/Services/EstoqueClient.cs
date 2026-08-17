using System.Net;
using System.Net.Http.Json;
using Microserv2Fatu.Exceptions;
using Microserv2Fatu.Models;

namespace Microserv2Fatu.Services;

public class EstoqueClient(HttpClient httpClient) : IEstoqueClient
{
    public async Task BaixarEstoqueAsync(IReadOnlyCollection<ItemNotaFiscal> itens, CancellationToken cancellationToken)
    {
        var request = new
        {
            itens = itens.Select(item => new { produtoId = item.ProdutoId, quantidade = item.Quantidade })
        };

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("api/produtos/baixar-estoque", request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            throw new ServicoEstoqueIndisponivelException();
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new ServicoEstoqueIndisponivelException();
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound or HttpStatusCode.Conflict)
            {
                throw new BaixaEstoqueRecusadaException();
            }

            throw new ServicoEstoqueIndisponivelException();
        }
    }
}
