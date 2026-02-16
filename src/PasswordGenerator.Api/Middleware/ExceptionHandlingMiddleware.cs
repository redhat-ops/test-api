using System.Net;
using System.Text.Json;
using PasswordGenerator.Api.DTOs;

namespace PasswordGenerator.Api.Middleware;

public class ExceptionHandlingMiddleware
{
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
        catch (OperationCanceledException)
        {
            // Client disconnected
            context.Response.StatusCode = 499; // Client Closed Request
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "Resource not found");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "A required resource could not be loaded"
                }
            };

            await context.Response.WriteAsJsonAsync(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Error = new ErrorDetail
                {
                    Code = "INTERNAL_ERROR",
                    Message = "An unexpected error occurred"
                }
            };

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
