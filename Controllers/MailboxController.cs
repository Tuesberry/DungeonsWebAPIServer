using Microsoft.AspNetCore.Mvc;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.ModelDb;
using TuesberryAPIServer.Services;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailboxController : ControllerBase
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly ILogger<MailboxController> _logger;
        readonly IGameDb _gameDb;

        public MailboxController(IHttpContextAccessor httpContextAccessor, ILogger<MailboxController> logger, IGameDb gameDb)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _gameDb = gameDb;
        }

        [HttpPost("LoadMail")]
        public async Task<PkLoadMailResponse> LoadMailList([FromBody]PkLoadMailRequest request)
        {
            var response = new PkLoadMailResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if(userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // page number 중복 확인 

            // load mailbox list

            // page 번호 기록

            return response;
        }

        [HttpPost("GetMailItem")]
        public async Task<PkGetMailItemResponse> GetMailItem([FromBody]PkGetMailItemRequest request)
        {
            var response = new PkGetMailItemResponse();

            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // item 받기

            return response;
        }
    }
}
