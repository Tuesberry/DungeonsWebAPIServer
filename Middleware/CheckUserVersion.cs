using System;
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
            if(string.Compare(path, "/CreateAccount", StringComparison.OrdinalIgnoreCase) == 0 )
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
                if(await IsValueExist(context, bodyStr) == false)
                {
                    return;
                }

                // string -> Json DOM
                var document = JsonDocument.Parse(bodyStr);

                // check valid Json Format
                (var isInvalid, appVersion, masterDataVersion) = await IsInvalidJsonFormat(context, document);
                if (isInvalid)
                {
                    return;
                }

                // check version
                if(await IsInvalidVersion(context, appVersion, masterDataVersion))
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

        async Task WriteErrorOnContext(HttpContext context, ErrorCode error)
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
            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);

            // log
            _logger.ZLogError($"[CheckUserVersion] ErrorCode: {error}");
        }
        
        async Task<bool> IsInvalidVersion(HttpContext context, string appVersion, string masterDataVersion)
        {
            if((appVersion != _masterDb.AppVersion)&&(masterDataVersion != _masterDb.MasterDataVersion))
            {
                await WriteErrorOnContext(context, ErrorCode.Invalid_AppVersion_And_MasterDataVersion);
                return true;
            }
            if(appVersion != _masterDb.AppVersion)
            {
                 await WriteErrorOnContext(context, ErrorCode.Invalid_AppVersion);
                return true;
            }
            if(masterDataVersion != _masterDb.MasterDataVersion)
            {
                await WriteErrorOnContext(context, ErrorCode.Invalid_MasterDataVersion);
                return true;
            }

            return false;
        }

        async Task<bool> IsValueExist(HttpContext context, string bodyStr)
        {
            if(string.IsNullOrEmpty(bodyStr) == false) 
            {
                return true;
            }

            await WriteErrorOnContext(context, ErrorCode.Invalid_Request_Http_Body);
            return false;
        }

        async Task<Tuple<bool, string, string>> IsInvalidJsonFormat(HttpContext context, JsonDocument document)
        {
            try
            {
                string appVersion = document.RootElement.GetProperty("AppVersion").GetString() ?? string.Empty;
                string masterDataVersion = document.RootElement.GetProperty("MasterDataVersion").GetString() ?? string.Empty;
                
                return new Tuple<bool, string, string>(false, appVersion, masterDataVersion);
            }
            catch
            {
                await WriteErrorOnContext(context, ErrorCode.Invalid_Version_Fail_Wrong_Keyword);
                return new Tuple<bool, string, string>(true, string.Empty, string.Empty);
            }
        }
    }
}
