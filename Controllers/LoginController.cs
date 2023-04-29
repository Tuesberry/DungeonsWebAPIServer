using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SqlKata.Execution;
using ZLogger;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.Services;
using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        readonly ILogger _logger;
        readonly IAccountDb _accountDb;
        readonly IMemoryDb _memoryDb;
        readonly IGameDb _gameDb;

        public LoginController(ILogger<LoginController> logger, IAccountDb accountDb, IMemoryDb memoryDb, IGameDb gameDb)
        {
            _logger = logger;   
            _accountDb = accountDb;
            _memoryDb = memoryDb;
            _gameDb = gameDb;
        }

        [HttpPost]
        public async Task<PKLoginResponse> Post([FromBody]PKLoginRequest request)
        {
            var response = new PKLoginResponse();

            // verify account
            var errorCode = await _accountDb.VerifyAccount(request.Id, request.Pw);
            if(errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }

            // get GameData
            (errorCode, var gameData, var accountId) = await _gameDb.LoadGameData(request.Id);
            if (errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }

            response.GameData = gameData;

            // create authToken
            var authToken = Security.CreateAuthToken();
            errorCode = await _memoryDb.RegistUserAsync(request.Id, authToken, accountId);
            if(errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }

            response.Authtoken = authToken;

            // get ItemData
            (errorCode, var itemDatum) = await _gameDb.LoadItemData(accountId);
            if(errorCode != ErrorCode.None)
            {
                response.Result = errorCode;
                return response;
            }

            response.ItemDatum = itemDatum;

            // get notice
            (errorCode, string notice) = await _memoryDb.GetNotice();
            if(errorCode != ErrorCode.None) 
            {
                response.Result = errorCode;
                return response;
            }

            response.Notice = notice;

            _logger.ZLogInformation($"[LoginController.Login] id: {request.Id}, pw: {request.Pw}");
            return response;
        }
    }
}
