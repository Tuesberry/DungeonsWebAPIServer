using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;
using ZLogger;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.Services;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateAccountController : ControllerBase
    {
        readonly ILogger _logger;
        readonly IAccountDb _accountDb;

        public CreateAccountController(ILogger<CreateAccountController> logger, IAccountDb accountDb)
        {
            _logger = logger;
            _accountDb = accountDb;
        }

        [HttpPost]
        public async Task<PkCreateAccountResponse> Post([FromBody]PkCreateAccountRequest request)
        {
            var response = new PkCreateAccountResponse { Result = ErrorCode.None };

            var errorCode = await _accountDb.CreateAccount(request.Id, request.Pw);
            if(errorCode != ErrorCode.None) 
            {
                response.Result = errorCode;
                return response;
            }

            // TODO : log

            return response;
        }
    }
}
