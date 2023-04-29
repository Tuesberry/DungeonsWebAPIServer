using System.Text.Json;
using System.Text;
using ZLogger;
using TuesberryAPIServer.Services;
using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Middleware
{
    public class CheckUserAuth
    {
        readonly RequestDelegate _next;
        readonly ILogger<CheckUserAuth> _logger;
        readonly IMemoryDb _memoryDb;

        public CheckUserAuth(RequestDelegate next, ILogger<CheckUserAuth> logger, IMemoryDb memoryDb)
        {
            _next = next;
            _logger = logger;
            _memoryDb = memoryDb;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (string.Compare(path, "/CreateAccount", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(path, "/Login", StringComparison.OrdinalIgnoreCase) == 0) 
            {
                // call the next delegate/middleware in the pipeline
                await _next(context);
                return;
            }

            context.Request.EnableBuffering();

            string authToken = string.Empty;
            string userId = string.Empty;
            string userLockKey = string.Empty;

            using(var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
            {
                var bodyStr = await reader.ReadToEndAsync();

                // check body string
                if (await IsValueExist(context, bodyStr) == false)
                {
                    return;
                }

                // string -> Json Dom
                var document = JsonDocument.Parse(bodyStr);

                // check valid json format & get authToken and userId
                (var IsInvalid, userId, authToken) = await IsInvalidJsonFormat(context, document);
                if (IsInvalid)
                {
                    return;
                }

                // check login
                var(isLogin, userInfo) = await _memoryDb.GetUserAsync(userId);
                if(isLogin == false)
                {
                    return;
                }

                // check AuthToken value
                if(await IsInvalidUserAuthToken(context, userInfo, authToken))
                {
                    return;
                }

                // lock user
                userLockKey = MemoryDbKeyMaker.MakeUserLockKey(userId);
                if(await SetLock(context, userLockKey))
                {
                    return;
                }

                context.Items[nameof(AuthUser)] = userInfo;
            }

            // log
            _logger.ZLogInformation($"[CheckUserAuth] Check User Auth Complete");

            // position reset
            context.Request.Body.Position = 0;

            // call the next delegate/middleware in the pipeline
            await _next(context);

            // trnasaction release
            await _memoryDb.DelUserReqLockAsync(userLockKey);
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
            _logger.ZLogError($"[CheckUserAuth] ErrorCode: {error}");
        }

        async Task<bool> IsValueExist(HttpContext context, string bodyStr)
        {
            if (string.IsNullOrEmpty(bodyStr) == false)
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
                string userId = document.RootElement.GetProperty("Id").GetString() ?? string.Empty;
                string authToken = document.RootElement.GetProperty("AuthToken").GetString() ?? string.Empty;
                return new Tuple<bool, string, string>(false, userId, authToken);
            }
            catch
            {
                await WriteErrorOnContext(context, ErrorCode.AuthToken_Fail_Wrong_keyword);
                return new Tuple<bool, string, string>(false, string.Empty, string.Empty);
            }
        }

        async Task<bool> IsInvalidUserAuthToken(HttpContext context, AuthUser userInfo ,string authToken)
        {
            if(string.CompareOrdinal(userInfo.AuthToken, authToken) == 0)
            {
                return false;
            }

            await WriteErrorOnContext(context, ErrorCode.AuthToken_Fail_Wrong_AuthToken);
            return true;
        }

        async Task<bool> SetLock(HttpContext context, string AuthToken)
        {
            if(await _memoryDb.SetUserReqLockAsync(AuthToken))
            {
                return false;
            }

            await WriteErrorOnContext(context, ErrorCode.AuthToken_Fail_SetNx);
            return true;
        }
    }
}
