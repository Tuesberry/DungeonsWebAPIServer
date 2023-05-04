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
        readonly IGameDb _gameDb;

        public CreateAccountController(ILogger<CreateAccountController> logger, IAccountDb accountDb, IGameDb gameDb)
        {
            _logger = logger;
            _accountDb = accountDb;
            _gameDb = gameDb;
        }

        [HttpPost]
        public async Task<PkCreateAccountResponse> Post([FromBody]PkCreateAccountRequest request)
        {
            var response = new PkCreateAccountResponse { Result = ErrorCode.None };

            // create account
            var errorCode = await _accountDb.CreateAccount(request.Id, request.Pw);
            if(errorCode != ErrorCode.None) 
            {
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[CreateAccount] UserId = {request.Id}");

            // create game data
            (errorCode, var accountId) = await _gameDb.CreateGameData(request.Id);
            if(errorCode != ErrorCode.None) 
            {
                // rollback, Delete AccountData
                if(await _accountDb.DeleteAccount(request.Id) != ErrorCode.None)
                {
                    _logger.ZLogError($"[CreateAccount.CreateGameData] Rollback Error, ErrorCode = {errorCode}, UserId = {request.Id}");
                }
               
                // return
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[CreateAccount.CreateGameData] UserId = {request.Id}, AccountId = {accountId}");

            // create default item data
            errorCode = await _gameDb.CreateDefaultItemData(accountId);
            if(errorCode != ErrorCode.None)
            {
                // rollback, Delete AccountData & GameData
                if (await _accountDb.DeleteAccount(request.Id) != ErrorCode.None)
                {
                    _logger.ZLogError($"[CreateAccount.CreateDefaultItemData] Rollback Error, UserId = {request.Id}");
                }
                if (await _gameDb.DeleteGameData(request.Id) != ErrorCode.None)
                {
                    _logger.ZLogError($"[CreateAccount.CreateGameData] Rollback Error, UserId = {request.Id}");
                }
                // return 
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[CreateAccount.CreateDefaultItemData] UserId = {request.Id}, AccountId = {accountId}");

            return response;
        }
    }
}
