using System.Net;
using Microserv2Fatu.DTOs;
using Microserv2Fatu.Exceptions;

namespace Microserv2Fatu.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            var (statusCode, message) = exception switch
            {
                NotaFiscalNaoEncontradaException => (HttpStatusCode.NotFound, exception.Message),
                NotaFiscalFechadaException => (HttpStatusCode.Conflict, exception.Message),
                NumeroNotaDuplicadoException => (HttpStatusCode.Conflict, exception.Message),
                BaixaEstoqueRecusadaException => (HttpStatusCode.Conflict, exception.Message),
                ServicoEstoqueIndisponivelException => (HttpStatusCode.ServiceUnavailable, exception.Message),
                ArgumentException => (HttpStatusCode.BadRequest, exception.Message),
                _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado.")
            };

            if (statusCode == HttpStatusCode.InternalServerError)
            {
                logger.LogError(exception, "Erro não tratado ao processar {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new ErrorResponseDto((int)statusCode, message));
        }
    }
}
