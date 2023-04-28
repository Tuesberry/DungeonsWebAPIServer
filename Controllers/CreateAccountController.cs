﻿using Microsoft.AspNetCore.Mvc;
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
            var(errorCode, accountId) = await _accountDb.CreateAccount(request.Id, request.Pw);
            if(errorCode != ErrorCode.None) 
            {
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogInformation($"[CreateAccount.CreateAccount] id: {request.Id}, accountId: {accountId}");

            // create game data
            errorCode = await _gameDb.CreateGameData(accountId);
            if(errorCode != ErrorCode.None) 
            {
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogInformation($"[CreateAccount.CreateGameData] id: {request.Id}, accountId: {accountId}");

            return response;
        }
    }
}
