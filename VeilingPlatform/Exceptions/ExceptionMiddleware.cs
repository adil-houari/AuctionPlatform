using System.Text.Json;

namespace VeilingPlatform.Exceptions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = exception is CustomException ? StatusCodes.Status400BadRequest : StatusCodes.Status500InternalServerError;

            var result = JsonSerializer.Serialize(new
            {
                StatusCode = context.Response.StatusCode,
                Message = exception.Message
            });

            return context.Response.WriteAsync(result);
        }
    }

}
