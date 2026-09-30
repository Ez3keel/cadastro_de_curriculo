using System.Net;
using Curriculos.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Curriculos.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private const int SqlErrorNumeroViolacaoUnique = 2601;
    private const int SqlErrorNumeroViolacaoUniqueConstraint = 2627;

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var (status, mensagem) = Mapear(ex);

            if (status == HttpStatusCode.InternalServerError)
            {
                _logger.LogError(ex, "Erro não tratado ao processar {Metodo} {Caminho}", context.Request.Method, context.Request.Path);
            }
            else
            {
                _logger.LogWarning(ex, "Erro de negócio ao processar {Metodo} {Caminho}: {Mensagem}", context.Request.Method, context.Request.Path, mensagem);
            }

            var problemDetails = new ProblemDetails
            {
                Status = (int)status,
                Title = mensagem,
                Type = $"https://httpstatuses.com/{(int)status}",
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = (int)status;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }

    private static (HttpStatusCode Status, string Mensagem) Mapear(Exception ex) => ex switch
    {
        EmailDuplicadoException => (HttpStatusCode.Conflict, ex.Message),
        DbUpdateException dbEx when EhViolacaoDeIndiceUnico(dbEx) => (HttpStatusCode.Conflict, "Já existe um candidato com este e-mail."),
        ArquivoInvalidoException => (HttpStatusCode.BadRequest, ex.Message),
        FalhaLeituraPdfException => (HttpStatusCode.UnprocessableEntity, ex.Message),
        _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado."),
    };

    private static bool EhViolacaoDeIndiceUnico(DbUpdateException ex) =>
        ex.InnerException is SqlException sqlEx &&
        (sqlEx.Number == SqlErrorNumeroViolacaoUnique || sqlEx.Number == SqlErrorNumeroViolacaoUniqueConstraint);
}
