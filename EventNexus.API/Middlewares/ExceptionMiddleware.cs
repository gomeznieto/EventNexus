using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace EventNexus.API.Middleware;

public class ExceptionMiddleware{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext ctx){
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            await HandlingExceptionAsync(ctx, ex); 
        }
    }

    public async Task HandlingExceptionAsync(HttpContext ctx, Exception ex)
    {
        ctx.Response.ContentType = "application/json";

        // Pattern Matching avanzado de C#
        var statusCode = ex switch
        {
            // 404: Recurso no encontrado (Lo lanzas tú manualmente en el Service)
            KeyNotFoundException => (int)HttpStatusCode.NotFound,

            // 401: Sin credenciales (Lo lanza el framework o tú)
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,

            // 400: Error del cliente (Datos inválidos enviados desde el front)
            ArgumentException or ArgumentNullException => (int)HttpStatusCode.BadRequest,

            // 409: Conflicto de Concurrencia (EF Core)
            DbUpdateConcurrencyException => (int)HttpStatusCode.Conflict,

            // 409: Conflicto de Base de Datos (EF Core)
            DbUpdateException => (int)HttpStatusCode.Conflict,

            // 500: Todo lo demás (Bugs, NullReference, Server caído)
            _ => (int)HttpStatusCode.InternalServerError
        };

        ctx.Response.StatusCode = statusCode;

        var response = new
        {
            StatusCode = statusCode,
            Message = ex.Message,
            Details = _env.IsDevelopment() ? ex.StackTrace?.ToString() : null
        };

        var json = JsonSerializer.Serialize(response);
        await ctx.Response.WriteAsync(json);
    }
}
