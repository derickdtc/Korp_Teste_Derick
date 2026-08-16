namespace Microserv1Est.Exceptions;

public class SaldoInsuficienteException(string codigo)
    : Exception($"Saldo insuficiente para o produto {codigo}.");
