namespace Microserv2Fatu.Exceptions;

public class BaixaEstoqueRecusadaException : Exception
{
    public BaixaEstoqueRecusadaException() : base("A baixa de estoque foi recusada. A nota continua aberta.") { }
}
