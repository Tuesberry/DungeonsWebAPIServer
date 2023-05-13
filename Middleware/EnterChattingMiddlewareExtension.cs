namespace TuesberryAPIServer.Middleware
{
    public static class EnterChattingMiddlewareExtension
    {
        public static IApplicationBuilder UseEnterChatting(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<EnterChatting>();
        }

    }
}
