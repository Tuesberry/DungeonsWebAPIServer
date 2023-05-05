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

            // load GameData
            (errorCode, var gameData, var accountId) = await _gameDb.LoadGameData(request.Id);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[LoginController] Load GameData Fail, UserId = {request.Id}");
                response.Result = errorCode;
                return response;
            }

            response.GameData = gameData;

            // create authToken
            var authToken = Security.CreateAuthToken();
            errorCode = await _memoryDb.RegistUserAsync(request.Id, authToken, accountId);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[LoginController] Create AuthToken Fail, UserId = {request.Id}");
                response.Result = errorCode;
                return response;
            }

            response.Authtoken = authToken;

            // load ItemData
            (errorCode, var itemDatum) = await _gameDb.LoadItemData(accountId);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[LoginController] Load ItemData Fail, UserId = {request.Id}");
                response.Result = errorCode;
                return response;
            }

            response.ItemDatum = itemDatum;

            // get notice
            (errorCode, string notice) = await _memoryDb.GetNotice();
            if(errorCode != ErrorCode.None) 
            {
                _logger.ZLogError($"[LoginController] Load Notice Fail, UserId = {request.Id}");
                response.Result = errorCode;
                return response;
            }

            response.Notice = notice;

            _logger.ZLogDebug($"[LoginController.Login] UserId = {request.Id}, Pw = {request.Pw}");
            return response;
        }
    }
}
