using System.Text;
using System.Text.Json;
using TuesberryAPIServer.Services;
using ZLogger;

namespace TuesberryAPIServer.Middleware
{
    public class CheckUserVersion
    {
        readonly RequestDelegate _next;
        readonly ILogger<CheckUserVersion> _logger;
        readonly IMasterDb _masterDb;

        public CheckUserVersion(RequestDelegate next, ILogger<CheckUserVersion> logger, IMasterDb masterDb)
        {
            _next = next;
            _logger = logger;
            _masterDb = masterDb;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if(string.Compare(path, "/CreateAccount", StringComparison.OrdinalIgnoreCase) == 0)
            {
                // call the next delegate/middleware in the pipeline
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            string appVersion = string.Empty;
            string masterDataVersion = string.Empty;

            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                var bodyStr = await reader.ReadToEndAsync();
                
                // check body string
                if(IsValueExist(context, bodyStr) == false)
                {
                    return;
                }

                // string -> Json DOM
                var document = JsonDocument.Parse(bodyStr);

                // check valid Json Format
                if(IsInvalidJsonFormat(context, document, out appVersion, out masterDataVersion))
                {
                    return;
                }

                // check version
                if(IsInvalidVersion(context, appVersion, masterDataVersion))
                {
                    return;
                }
            }

            // log
            _logger.ZLogDebug($"[CheckUserVersion] Check User Version Complete");

            // position reset
            context.Request.Body.Position = 0;

            // call the next delegate/middleware in the pipeline
            await _next(context);
        }

        void WriteErrorOnContext(HttpContext context, ErrorCode error)
        {
            // return error
            // using serialize, c# class -> json
            var errorJsonResponse = JsonSerializer.Serialize(new MiddlewareResponse
            {
                Result = error
            });
            // encode
            var bytes = Encoding.UTF8.GetBytes(errorJsonResponse);
            // write on the body
            context.Response.Body.Write(bytes, 0, bytes.Length);

            // log
            _logger.ZLogError($"[CheckUserVersion] ErrorCode: {error}");
        }
        
        bool IsInvalidVersion(HttpContext context, string appVersion, string masterDataVersion)
        {
            if((appVersion != _masterDb.AppVersion)&&(masterDataVersion != _masterDb.MasterDataVersion))
            {
                WriteErrorOnContext(context, ErrorCode.Invalid_AppVersion_And_MasterDataVersion);
                return true;
            }
            if(appVersion != _masterDb.AppVersion)
            {
                WriteErrorOnContext(context, ErrorCode.Invalid_AppVersion);
                return true;
            }
            if(masterDataVersion != _masterDb.MasterDataVersion)
            {
                WriteErrorOnContext(context, ErrorCode.Invalid_MasterDataVersion);
                return true;
            }

            return false;
        }

        bool IsValueExist(HttpContext context, string bodyStr)
        {
            if(string.IsNullOrEmpty(bodyStr) == false) 
            {
                return true;
            }

            WriteErrorOnContext(context, ErrorCode.Invalid_Request_Http_Body);
            return false;
        }

        bool IsInvalidJsonFormat(HttpContext context, JsonDocument document, out string appVersion, out string masterDataVersion)
        {
            try
            {
                appVersion = document.RootElement.GetProperty("AppVersion").GetString() ?? string.Empty;
                masterDataVersion = document.RootElement.GetProperty("MasterDataVersion").GetString() ?? string.Empty;
                return false;
            }
            catch
            {
                appVersion = "";
                masterDataVersion = "";

                WriteErrorOnContext(context, ErrorCode.Invalid_Version_Fail_Wrong_Keyword);

                return true;
            }
        }
    }
}
