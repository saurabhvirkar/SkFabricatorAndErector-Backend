using System.Net;
using SkFabricatorAndErector.Api.Common;
using SkFabricatorAndErector.Application.Exceptions;

namespace SkFabricatorAndErector.Api.Middleware;

public class ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode statusCode = HttpStatusCode.InternalServerError;
        string message = exception.InnerException != null 
            ? $"{exception.Message} | Inner: {exception.InnerException.Message}" 
            : exception.Message;

        if (exception is NotFoundException notFoundEx)
        {
            statusCode = HttpStatusCode.NotFound;
            message = notFoundEx.Message;
        }
        else if (exception is BusinessRuleException businessEx)
        {
            statusCode = HttpStatusCode.BadRequest;
            message = businessEx.Message;
        }

        var response = new ApiResponse(statusCode, message, null);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}
