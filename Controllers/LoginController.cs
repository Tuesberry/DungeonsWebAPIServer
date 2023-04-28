using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlKata.Execution;
using ZLogger;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.Services;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        readonly ILogger _logger;
        readonly IAccountDb _accountDb;
        readonly IMemoryDb _memoryDb;

        public LoginController(ILogger<LoginController> logger, IAccountDb accountDb, IMemoryDb memoryDb)
        {
            _logger = logger;   
            _accountDb = accountDb;
            _memoryDb = memoryDb;
        }

        [HttpPost]
        public async Task<PKLoginResponse> Post([FromBody]PKLoginRequest request)
        {
            var response = new PKLoginResponse();

            // verify account
            var(errorCode, accountId) = await _accountDb.VerifyAccount(request.Id, request.Pw);
            if(errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }

            // create authToken
            var authToken = Security.CreateAuthToken();
            errorCode = await _memoryDb.RegistUserAsync(request.Id, authToken, accountId);
            if(errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }

            // log
            _logger.ZLogInformation($"[LoginController] id: {request.Id}, pw: {request.Pw}");

            response.Authtoken = authToken;
            return response;
        }
    }
}
