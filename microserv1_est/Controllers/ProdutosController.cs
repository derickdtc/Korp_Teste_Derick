using Microserv1Est.DTOs;
using Microserv1Est.Services;
using Microsoft.AspNetCore.Mvc;

namespace Microserv1Est.Controllers;

[ApiController]
[Route("api/produtos")]
public class ProdutosController(IProdutoService produtoService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProdutoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProdutoDto>> Criar(CriarProdutoDto dto, CancellationToken cancellationToken)
    {
        var produto = await produtoService.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(BuscarPorId), new { id = produto.Id }, produto);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ProdutoDto>>> Listar(CancellationToken cancellationToken) => Ok(await produtoService.ListarAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProdutoDto>> BuscarPorId(Guid id, CancellationToken cancellationToken) => Ok(await produtoService.BuscarPorIdAsync(id, cancellationToken));

    [HttpPost("baixar-estoque")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> BaixarEstoque(BaixarEstoqueDto dto, CancellationToken cancellationToken)
    {
        await produtoService.BaixarEstoqueAsync(dto, cancellationToken);
        return NoContent();
    }
}
