using Microsoft.AspNetCore.Mvc;
using TuesberryAPIServer.ModelDb;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.Services;
using ZLogger;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaymentController : ControllerBase
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly ILogger<PaymentController> _logger;
        readonly IGameDb _gameDb;
        readonly IMasterDb _masterDb;

        public PaymentController(IHttpContextAccessor contextAccessor, ILogger<PaymentController> logger, IGameDb gameDb, IMasterDb masterDb)
        {
            _httpContextAccessor = contextAccessor;
            _logger = logger;
            _gameDb = gameDb;
            _masterDb = masterDb;
        }

        [HttpPost]
        public async Task<PkPaymentResponse> Payment([FromBody]PkPaymentRequest request)
        {
            var response = new PkPaymentResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // 구매 내용 중복 확인
            var (errorCode, result) = await _gameDb.IsDuplicatePayment(userInfo.AccountId, request.OrderNumber, request.PurchaseDate);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[Payment] Duplicate Payment Check Error, UserId = {request.Id}, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }
            if(result)
            {
                _logger.ZLogError($"[Payment] Duplicate Payment Data, UserId = {request.Id}, AccountId = {userInfo.AccountId}, OrderNumber = {request.OrderNumber}");
                response.Result = ErrorCode.Payment_Fail_Duplicate_Data;
                return response;
            }

            // 구매 상품 전달 메일 만들기
            var (mailData, comment) = CreatePaymentMail(request.OrderNumber, request.PurchaseDate, request.ProductCode);

            // 우편으로 아이템 전달
            (errorCode, var mailId) = await _gameDb.InsertMail(userInfo.AccountId, mailData, comment);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[Payment] Insert Payment Mail Fail, UserId = {request.Id}, AccountId = {userInfo.AccountId}, OrderNumber = {request.OrderNumber}");
                response.Result = errorCode;
                return response;
            }    

            // 구매 데이터 넣기
            errorCode = await _gameDb.InsertPaymentData(userInfo.AccountId, request.OrderNumber, request.PurchaseDate, request.ProductCode);
            if(errorCode != ErrorCode.None)
            {
                // 롤백 필요
                var deleteResult = await _gameDb.DeleteMail(userInfo.AccountId, mailId);
                if(deleteResult != ErrorCode.None) 
                {
                    _logger.ZLogError($"[Payment] Rollback Payment Mail Fail, UserId = {request.Id}, AccountId = {userInfo.AccountId}, OrderNumber = {request.OrderNumber}");
                }
                // 로그
                _logger.ZLogError($"[Payment] Duplicate Payment Check Error, UserId = {request.Id}, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[Payment] Complete, UserId = {request.Id}, AccountId = {userInfo.AccountId}, OrderNumber = {request.OrderNumber}");
            return response;
        }

        Tuple<MailboxData, string> CreatePaymentMail(string orderNumber, DateTime purchaseDate, Int32 productCode)
        {
            MailboxData mailData = new MailboxData
            {
                Title = "Package Product Has Arrived",
                ExpiryDate = DateTime.Today.AddYears(100),
            };

            foreach(var item in _masterDb.BundleProducts[productCode])
            {
                mailData.MailboxItemData.Add(new MailboxItemData
                {
                    ItemCode = item.ItemCode,
                    Amount = item.ItemCount,
                    EnchanceCount = 0,
                    Attack = _masterDb.Items[item.ItemCode].Attack,
                    Defence = _masterDb.Items[item.ItemCode].Defence,
                    Magic = _masterDb.Items[item.ItemCode].Magic
                });
            }

            string comment = $"Purchase product: {productCode}, Would you like to receive your purchase?";

            return new Tuple<MailboxData, string>(mailData, comment);
        }
    }
}
