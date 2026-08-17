namespace Microserv2Fatu.Exceptions;

public class NumeroNotaDuplicadoException : Exception
{
    public NumeroNotaDuplicadoException() : base("Não foi possível gerar uma numeração sequencial para a nota. Tente novamente.") { }
}
