using System.Net;
using System.Text.Json;
using ProductManagement.Core.DTOs;
using ProductManagement.Core.Exceptions;

namespace ProductManagement.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled Exception occurred during HTTP {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            NotFoundException notFoundEx => (
                (int)HttpStatusCode.NotFound,
                notFoundEx.Message,
                new List<string> { notFoundEx.Message }
            ),
            BadRequestException badReqEx => (
                (int)HttpStatusCode.BadRequest,
                badReqEx.Message,
                badReqEx.Errors ?? new List<string> { badReqEx.Message }
            ),
            UnauthorizedException unauthEx => (
                (int)HttpStatusCode.Unauthorized,
                unauthEx.Message,
                new List<string> { unauthEx.Message }
            ),
            ForbiddenException forbiddenEx => (
                (int)HttpStatusCode.Forbidden,
                forbiddenEx.Message,
                new List<string> { forbiddenEx.Message }
            ),
            KeyNotFoundException keyNotFoundEx => (
                (int)HttpStatusCode.NotFound,
                keyNotFoundEx.Message,
                new List<string> { keyNotFoundEx.Message }
            ),
            ArgumentException argEx => (
                (int)HttpStatusCode.BadRequest,
                argEx.Message,
                new List<string> { argEx.Message }
            ),
            _ => (
                (int)HttpStatusCode.InternalServerError,
                "An unexpected internal server error occurred. Please try again later.",
                _env.IsDevelopment() ? new List<string> { exception.Message, exception.StackTrace ?? string.Empty } : new List<string> { "An internal server error occurred." }
            )
        };

        context.Response.StatusCode = statusCode;

        var response = ApiResponse<object>.FailureResponse(message, errors);
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }
}
