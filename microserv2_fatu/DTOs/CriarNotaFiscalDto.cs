using System.ComponentModel.DataAnnotations;

namespace Microserv2Fatu.DTOs;

public class CriarNotaFiscalDto
{
    [Required(ErrorMessage = "A lista de itens é obrigatória.")]
    [MinLength(1, ErrorMessage = "Informe pelo menos um item na nota fiscal.")]
    public List<CriarItemNotaFiscalDto> Itens { get; set; } = [];
}

public class CriarItemNotaFiscalDto
{
    public Guid ProdutoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}
