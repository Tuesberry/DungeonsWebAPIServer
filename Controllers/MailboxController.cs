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

        [HttpPost]
        public async Task<PkMailboxResponse> Post([FromBody]PkMailboxRequest request)
        {
            var response = new PkMailboxResponse();

            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if(userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            return response;
        }
       
    }
}
