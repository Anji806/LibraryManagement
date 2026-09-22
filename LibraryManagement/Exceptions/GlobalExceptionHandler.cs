using System.Net;
using System.Text.Json;

namespace LibraryManagement.Exceptions;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;

    public GlobalExceptionHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.NotFound,
                ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.Conflict,
                ex.Message);
        }
        catch (ArgumentException ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.BadRequest,
                ex.Message);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(
                context,
                HttpStatusCode.InternalServerError,
               ex.Message);
        }
    }

    private static async Task HandleExceptionAsync(
        HttpContext context,
        HttpStatusCode statusCode,
        string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            statusCode = (int)statusCode,
            message = message
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response));
    }
}