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
        else if (exception is Polly.CircuitBreaker.BrokenCircuitException || exception.GetType().Name.Contains("BrokenCircuitException"))
        {
            statusCode = HttpStatusCode.ServiceUnavailable;
            message = "External downstream service is temporarily unavailable due to broken circuit. Please try again shortly.";
        }
        else if (exception is Polly.Timeout.TimeoutRejectedException || exception.GetType().Name.Contains("TimeoutRejectedException"))
        {
            statusCode = HttpStatusCode.GatewayTimeout;
            message = "The operation timed out while communicating with downstream dependency.";
        }

        var response = new ApiResponse(statusCode, message, null);
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}
