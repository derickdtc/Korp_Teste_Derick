namespace Microserv2Fatu.DTOs;

public class NotaFiscalDto
{
    public Guid Id { get; init; }
    public int Numero { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyCollection<ItemNotaFiscalDto> Itens { get; init; } = [];
}

public class ItemNotaFiscalDto
{
    public Guid ProdutoId { get; init; }
    public string Descricao { get; init; } = string.Empty;
    public int Quantidade { get; init; }
}
