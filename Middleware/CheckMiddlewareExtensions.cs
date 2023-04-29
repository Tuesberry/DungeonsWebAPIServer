using System.Runtime.CompilerServices;

namespace TuesberryAPIServer.Middleware
{
    public static class CheckMiddlewareExtensions
    {
        public static IApplicationBuilder UseCheckUserVersion(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CheckUserVersion>();
        }

        public static IApplicationBuilder UseCheckUserAuth(this IApplicationBuilder builder) 
        {
            return builder.UseMiddleware<CheckUserAuth>();
        }
    }

    public class MiddlewareResponse
    {
        public ErrorCode Result { get; set; }
    }
}
