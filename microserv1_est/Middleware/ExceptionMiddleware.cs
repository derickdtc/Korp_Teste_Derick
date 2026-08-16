using System.Net;
using Microserv1Est.DTOs;
using Microserv1Est.Exceptions;

namespace Microserv1Est.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try { await next(context); }
        catch (Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                ProdutoNaoEncontradoException => (HttpStatusCode.NotFound, exception.Message),
                CodigoProdutoDuplicadoException => (HttpStatusCode.Conflict, exception.Message),
                SaldoInsuficienteException => (HttpStatusCode.Conflict, exception.Message),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado.")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
                logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponseDto((int)statusCode, message));
        }
    }
}
