namespace TuesberryAPIServer.Middleware
{
    public class CheckUserVersion
    {
        readonly RequestDelegate _next;

        public CheckUserVersion(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {


            // position reset
            context.Request.Body.Position = 0;

            // call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
