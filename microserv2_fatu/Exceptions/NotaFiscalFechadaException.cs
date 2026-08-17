namespace Microserv2Fatu.Exceptions;

public class NotaFiscalFechadaException : Exception
{
    public NotaFiscalFechadaException() : base("A nota fiscal já está fechada e não pode ser impressa novamente.") { }
}
