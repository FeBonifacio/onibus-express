using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OnibusExpress.Application.Common.Errors;
using OnibusExpress.Domain.Common;

namespace OnibusExpress.Api.Errors;

/// <summary>
/// Translates exceptions into RFC 7807 ProblemDetails. Business errors carry an
/// <see cref="IHasErrorCode"/> mapped to HTTP via <see cref="ErrorHttpMap"/>; the message
/// (in Portuguese) is user-facing feedback. 5xx messages are hidden.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        var (code, status) = exception switch
        {
            IHasErrorCode hasCode => (hasCode.ErrorCode, ErrorHttpMap.ToStatusCode(hasCode.ErrorCode)),
            ArgumentException => ("VALIDACAO", StatusCodes.Status400BadRequest),
            _ => ("ERRO_INTERNO", StatusCodes.Status500InternalServerError),
        };

        if (status >= StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception ({Code})", code);
        }

        var problem = new ProblemDetails
        {
            Status = status,
            Title = code,
            Detail = status >= StatusCodes.Status500InternalServerError
                ? "Erro interno no servidor."
                : exception.Message,
        };
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = status;
        await context.Response.WriteAsJsonAsync(problem, options: null,
            contentType: "application/problem+json", cancellationToken: ct);
        return true;
    }
}
