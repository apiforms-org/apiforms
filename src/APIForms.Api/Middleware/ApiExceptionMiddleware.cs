using System.Net;
using System.Text.Json;
using MongoDB.Driver;

namespace APIForms.Api.Middleware;

public sealed class ApiExceptionMiddleware(RequestDelegate next, ILogger<ApiExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);

            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";
                var payload = JsonSerializer.Serialize(new
                {
                    status = 401,
                    message = "No autorizado",
                    detail = "Falta token JWT válido o expiró.",
                    traceId = context.TraceIdentifier
                });
                await context.Response.WriteAsync(payload);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled API exception. TraceId: {TraceId}", context.TraceIdentifier);
            await WriteErrorResponse(context, ex);
        }
    }

    private static async Task WriteErrorResponse(HttpContext context, Exception ex)
    {
        var (status, message, detail) = Map(ex);

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var payload = JsonSerializer.Serialize(new
        {
            status,
            message,
            detail,
            traceId = context.TraceIdentifier
        });

        await context.Response.WriteAsync(payload);
    }

    private static (int Status, string Message, string Detail) Map(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "No autorizado", ex.Message),
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Recurso no encontrado", ex.Message),
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, "Solicitud inválida", ex.Message),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Parámetros inválidos", ex.Message),
            MongoAuthenticationException => ((int)HttpStatusCode.InternalServerError, "Error de autenticación MongoDB", ex.Message),
            MongoConnectionException => ((int)HttpStatusCode.InternalServerError, "Error de conexión MongoDB", ex.Message),
            TimeoutException => ((int)HttpStatusCode.GatewayTimeout, "Tiempo de espera agotado", ex.Message),
            _ => ((int)HttpStatusCode.InternalServerError, "Error interno", ex.Message)
        };
    }
}
