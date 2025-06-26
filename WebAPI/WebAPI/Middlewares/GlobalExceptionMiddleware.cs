using Application.Exceptions;
using Infrastructure.Exceptions;

namespace WebAPI.Middlewares
{
    public class GlobalExceptionMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Application.Exceptions.ApplicationException ex)
            {
                // todo push notification & writing log
                Console.WriteLine("========== GlobalExceptionMiddleware - Catched exception ==========");
                Console.WriteLine("========== ApplicationException ==========");
                Console.WriteLine("Error code: " + ex.ErrorCode);
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("========== GlobalExceptionMiddleware - End of exception ==========");
                context.Response.StatusCode = ex.StatusCode;

                //var hashMap = new Dictionary<string, string>();
                //hashMap[ex.ErrorCode] = ex.Message;

                await context.Response.WriteAsJsonAsync(new
                {
                    ex.ErrorCode,
                    ex.Message
                });
            }
            catch (InfrastructureException ex)
            {
                // todo push notification & writing log
                Console.WriteLine("========== GlobalExceptionMiddleware - Catched exception ==========");
                Console.WriteLine("========== InfrastructureException ==========");
                Console.WriteLine("Error code: " + ex.ErrorCode);
                Console.WriteLine("Message: " + ex.Message);
                Console.WriteLine("========== GlobalExceptionMiddleware - End of exception ==========");
                context.Response.StatusCode = ex.StatusCode;

                await context.Response.WriteAsJsonAsync(new
                {
                    ex.ErrorCode,
                    ex.Message
                });
            }
            catch (Exception ex)
            {
                // todo push notification & writing log
                Console.WriteLine("========== GlobalExceptionMiddleware - System exception ==========");
                Console.WriteLine(ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("Inner exception: " + ex.InnerException.Message);
                Console.WriteLine("========== GlobalExceptionMiddleware - End of exception ==========");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsJsonAsync(new
                {
                    ErrorCode = "Other exception",
                    ex.Message,
                    InnerMessage = ex.InnerException?.Message
                });
            }
        }
    }
}
