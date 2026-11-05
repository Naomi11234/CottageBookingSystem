using CottageBookingSystem.Service;

namespace CottageBookingSystem.API.Middleware;

public sealed class BookingConflictExceptionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (BookingConflictException exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            context.Response.Clear();
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await context.Response.WriteAsJsonAsync(
                new { message = exception.Message },
                cancellationToken: context.RequestAborted);
        }
    }
}
