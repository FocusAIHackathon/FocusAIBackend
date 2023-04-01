using Cloops.Exceptions;

namespace Cloops.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // middleware logic

        // Call the next delegate/middleware in the pipeline.
        try {
            await _next(context);
        }
        catch (CLHttpException cle) {
            // short circuit
            context.Response.StatusCode = cle.Hs;
            await context.Response.WriteAsync(cle.serialize());
        }
        catch (Exception e) {
            var cle = new CLHttpException(e.Message);
            context.Response.StatusCode = cle.Hs;
            await context.Response.WriteAsync(cle.serialize());
        }
        
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseCLExceptionHandling(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
