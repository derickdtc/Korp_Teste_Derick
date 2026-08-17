namespace Microserv2Fatu.Models;

public class NotaFiscal
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public StatusNotaFiscal Status { get; set; }
    public List<ItemNotaFiscal> Itens { get; set; } = [];
}
