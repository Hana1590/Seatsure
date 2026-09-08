using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Seatsure.Application.Exceptions;

namespace Seatsure.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    { _next = next; _logger = logger; }

    public async Task Invoke(HttpContext context)
    {
        try { await _next(context); }
        catch (AppException ex)
        {
            context.Response.StatusCode = ex.SatusCode;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails { Status = ex.SatusCode, Title = ex.GetType().Name, Detail = ex.Message, Instance = context.Request.Path };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/problem+json";
            var problem = new ProblemDetails { Status = 500, Title = "Internal Server Error", Detail = "An unexpected error occurred.", Instance = context.Request.Path };
            await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
        }
    }
}
