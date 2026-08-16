namespace Microserv1Est.DTOs;

public class ProdutoDto
{
    public Guid Id { get; init; }
    public string Codigo { get; init; } = string.Empty;
    public string Descricao { get; init; } = string.Empty;
    public int Saldo { get; init; }
}
