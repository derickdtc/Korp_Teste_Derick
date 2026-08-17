namespace Microserv2Fatu.Exceptions;

public class ServicoEstoqueIndisponivelException : Exception
{
    public ServicoEstoqueIndisponivelException() : base("O serviço de estoque está indisponível. A nota continua aberta; tente imprimir novamente mais tarde.") { }
}
