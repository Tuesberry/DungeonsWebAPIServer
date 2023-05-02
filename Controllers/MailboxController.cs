using Microsoft.AspNetCore.Mvc;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.ModelDb;
using TuesberryAPIServer.Services;
using ZLogger;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MailboxController : ControllerBase
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly ILogger<MailboxController> _logger;
        readonly IGameDb _gameDb;
        readonly IMemoryDb _memoryDb;

        public MailboxController(IHttpContextAccessor httpContextAccessor, ILogger<MailboxController> logger, IGameDb gameDb, IMemoryDb memoryDb)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _gameDb = gameDb;
            _memoryDb = memoryDb;
        }

        [HttpPost("OpenMailbox")]
        public async Task<PkOpenMailboxResponse> OpenMailbox([FromBody]PkOpenMailboxRequest request)
        {
            var response = new PkOpenMailboxResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // load mailbox list
            var (errorCode, mailList) = await _gameDb.LoadMailboxData(userInfo.AccountId, 1);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[OpenMail] Load Mail List Fail, userId : {request.Id}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogInformation($"[OpenMail] Load Mail List, page: 1, mail num: {mailList.Count}");
            response.MailboxDatum = mailList;

            // set title & comment
            response.MailboxTitle = MasterData.MailboxTitle;
            response.MailboxComment = MasterData.MailboxComment;

            return response;
        }

        [HttpPost("LoadMail")]
        public async Task<PkLoadMailboxResponse> LoadMailList([FromBody]PkLoadMailboxRequest request)
        {
            var response = new PkLoadMailboxResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if(userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // load mailbox list
            var (errorCode, mailList) = await _gameDb.LoadMailboxData(userInfo.AccountId, request.PageNum);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[LoadMail] Load Mail List Fail, userId : {request.Id}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogInformation($"[LoadMail] Load Mail List, page:{request.PageNum}, mail num: {mailList.Count}");
            response.MailboxDatum = mailList;


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

            // item 존재여부 확인 & mail에서 item 정보 받아오기
            var (errorCode, mailItem) = await _gameDb.LoadMailItemData(userInfo.AccountId, request.MailId);
            if(errorCode != ErrorCode.None) 
            {
                _logger.ZLogError($"[GetMailItem] Load Mail Item Fail , userId: {request.Id}");
                response.Result = errorCode;
                return response;
            }

            // Item 받고, 메일 삭제
            errorCode = await _gameDb.LoadAndDeleteItemFromMail(userInfo.AccountId, request.MailId);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GetMailItem] Get Item & Delete Mail Fail , userId: {request.Id}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogInformation($"[GetMailItem] Get Item Complete, userId: {request.Id}");
            return response;
        }
    }
}
