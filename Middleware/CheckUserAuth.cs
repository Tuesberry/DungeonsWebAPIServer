namespace TuesberryAPIServer.Middleware
{
    public class CheckUserAuth
    {
        readonly RequestDelegate _next;

        public CheckUserAuth(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            // call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
