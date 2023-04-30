using Microsoft.AspNetCore.Mvc;
using TuesberryAPIServer.ModelDb;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.Services;
using ZLogger;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LogoutController : ControllerBase
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly IMemoryDb _memoryDb;
        readonly ILogger<LogoutController> _logger;

        public LogoutController(IHttpContextAccessor httpContextAccessor, IMemoryDb memoryDb, ILogger<LogoutController> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _memoryDb = memoryDb;
            _logger = logger;
        }

        [HttpPost]
        public async Task<PKLogoutResponse> Logout([FromBody] PKLogoutRequest request)
        {
            var response = new PKLogoutResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // logout
            var errorCode = await _memoryDb.DelUserAsync(request.Id);
            if(errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogInformation($"[Logout] Complete, id: {request.Id}");

            return response;
        }

    }
}
