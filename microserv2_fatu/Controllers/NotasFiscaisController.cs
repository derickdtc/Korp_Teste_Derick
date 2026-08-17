using Microserv2Fatu.DTOs;
using Microserv2Fatu.Services;
using Microsoft.AspNetCore.Mvc;

namespace Microserv2Fatu.Controllers;

[ApiController]
[Route("api/notas-fiscais")]
public class NotasFiscaisController(INotaFiscalService notaFiscalService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(NotaFiscalDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NotaFiscalDto>> Criar(CriarNotaFiscalDto dto, CancellationToken cancellationToken)
    {
        var nota = await notaFiscalService.CriarAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(BuscarPorId), new { id = nota.Id }, nota);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<NotaFiscalDto>>> Listar(CancellationToken cancellationToken) =>
        Ok(await notaFiscalService.ListarAsync(cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<NotaFiscalDto>> BuscarPorId(Guid id, CancellationToken cancellationToken) =>
        Ok(await notaFiscalService.BuscarPorIdAsync(id, cancellationToken));

    [HttpPost("{id:guid}/imprimir")]
    [ProducesResponseType(typeof(NotaFiscalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<NotaFiscalDto>> Imprimir(Guid id, CancellationToken cancellationToken) =>
        Ok(await notaFiscalService.ImprimirAsync(id, cancellationToken));
}
