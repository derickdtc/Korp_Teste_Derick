namespace Microserv2Fatu.Models;

public class ItemNotaFiscal
{
    public Guid Id { get; set; }
    public Guid NotaFiscalId { get; set; }
    public NotaFiscal NotaFiscal { get; set; } = null!;
    public Guid ProdutoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
}
