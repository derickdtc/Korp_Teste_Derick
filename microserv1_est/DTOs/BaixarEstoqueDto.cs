using System.ComponentModel.DataAnnotations;

namespace Microserv1Est.DTOs;

public class BaixarEstoqueDto
{
    [Required(ErrorMessage = "A lista de itens é obrigatória.")]
    [MinLength(1, ErrorMessage = "Informe pelo menos um item para baixa.")]
    public List<ItemBaixaEstoqueDto> Itens { get; set; } = [];
}

public class ItemBaixaEstoqueDto
{
    public Guid ProdutoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}
