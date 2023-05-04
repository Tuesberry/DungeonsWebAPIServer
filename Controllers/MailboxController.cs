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
        readonly IMasterDb _masterDb;

        public MailboxController(IHttpContextAccessor httpContextAccessor, ILogger<MailboxController> logger, IGameDb gameDb, IMemoryDb memoryDb, IMasterDb masterDb)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _gameDb = gameDb;
            _memoryDb = memoryDb;
            _masterDb = masterDb;
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
            var (errorCode, mailList) = await _gameDb.LoadMailList(userInfo.AccountId, 1);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[OpenMail] Load Mail List Fail, userId = {request.Id}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[OpenMail] Load Mail List, page = 1, mailCount = {mailList.Count}");
            response.MailboxDatum = mailList;

            // set title & comment
            response.MailboxTitle = _masterDb.MailboxTitle;
            response.MailboxComment = _masterDb.MailboxComment;

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
            var (errorCode, mailList) = await _gameDb.LoadMailList(userInfo.AccountId, request.PageNum);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[LoadMail] Load Mail List Fail, userId = {request.Id}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[LoadMail] Load Mail List, page = {request.PageNum}, mail num = {mailList.Count}");
            response.MailboxDatum = mailList;


            return response;
        }

        [HttpPost("GetMailDetail")]
        public async Task<PKGetMailDetailResponse> GetMailDetail([FromBody]PKGetMailDetailRequest request)
        {
            var response = new PKGetMailDetailResponse();

            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            var(errorCode, detail) = await _gameDb.LoadMailDetail(userInfo.AccountId, request.MailId);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GetMailDetail] Get Mail Detail Fail , UserId = {request.Id}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[GetMailDetail] Complete, UserId = {request.Id}");
            response.Detail = detail;

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

            // receive item
            var errorCode = await _gameDb.ReceiveMailItem(userInfo.AccountId, request.MailId);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GetMailItem] ReceiveMailItem Fail, UserId = {request.Id}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[GetMailItem] Complete, UserId = {request.Id}");
            return response;
        }

        [HttpPost("DeleteMail")]
        public async Task<PKDeleteMailResponse> DeleteMail([FromBody] PKDeleteMailRequest request)
        {
            var response = new PKDeleteMailResponse();

            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // delete mail
            var errorCode = await _gameDb.DeleteMail(userInfo.AccountId, request.MailId);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[DeleteMail] DeleteMail Fail, UserId = {request.Id}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[DeleteMail] Complete, UserId = {request.Id}");
            return response;
        }

    }
}
