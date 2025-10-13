using Core;

namespace AlphaLising.Extensions;

public static class ExceptionHandlerCatcher
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        return app.Use(async (context, next) =>
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("CustomExceptionHandler");

                switch (ex)
                {
                    case BusinessException businessEx:
                        logger.LogWarning("Business error: {Message}", businessEx.Message);
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = businessEx.Message,
                            code = businessEx.ErrorCode
                        });
                        break;

                    case NotFoundException notFoundEx:
                        context.Response.StatusCode = 404;
                        await context.Response.WriteAsJsonAsync(new { error = notFoundEx.Message });
                        break;

                    default:
                        logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
                        context.Response.StatusCode = 500;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Internal server error",
                            detail = "An unexpected error occurred."
                        });
                        break;
                }
            }
        });
    }
}