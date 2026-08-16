namespace Microserv1Est.Exceptions;

public class CodigoProdutoDuplicadoException(string codigo)
    : Exception($"Já existe um produto cadastrado com o código {codigo}.");
