using Microsoft.AspNetCore.Mvc;
using TuesberryAPIServer.ModelDb;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.Services;
using ZLogger;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnchanceController
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly ILogger<EnchanceController> _logger;
        readonly IGameDb _gameDb;
        readonly IMasterDb _masterDb;

        public EnchanceController(IHttpContextAccessor httpContextAccessor, ILogger<EnchanceController> logger, IGameDb gameDb, IMasterDb masterDb)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _gameDb = gameDb;
            _masterDb = masterDb;
        }

        [HttpPost]
        public async Task<PkEnchanceResponse> Enchance([FromBody]PkEnchanceRequest request)
        {
            var response = new PkEnchanceResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // item 가져오기
            var (errorCode, itemData) = await _gameDb.LoadItemData(userInfo.AccountId, request.UserItemId);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[Enchance] LoadItem Fail, UserId = {request.Id}, AccountId = {userInfo.AccountId}, UserItemId = {request.UserItemId}");
                response.Result = errorCode;
                return response;
            }

            // 강화 가능 여부 확인
            // 최대 강화 횟수 <= 현재 강화 횟수 -> 강화 불가능
            if (_masterDb.Items[itemData.ItemCode].EnchanceMaxCount <= itemData.EnchanceCount)
            {
                _logger.ZLogError($"[Enchance] ErrorCode = {ErrorCode.Enchance_Fail_Enchance_Count_Exceed}, AccountId = {userInfo.AccountId}, UserItemId = {request.UserItemId}");
                response.Result = ErrorCode.Enchance_Fail_Enchance_Count_Exceed;
                return response;
            }

            // 강화
            bool success = GetEnchanceResult();

            // 아이템 업데이트
            if(success)
            {
                // 강화 성공 => 무기는 공격력, 방어구는 방어력 상승 / 10% 상승
                if (_masterDb.Items[itemData.ItemCode].Attribute == _masterDb.ItemAttributes["Weapon"])
                {
                    // 무기
                    itemData.Attack = itemData.Attack * 1.1m;
                }
                else
                {
                    // 방어구
                    itemData.Defence = itemData.Defence * 1.1m;
                }
                itemData.EnchanceCount ++;

                // 업데이트
                errorCode = await _gameDb.UpdateItemData(userInfo.AccountId, itemData);
                if(errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[Enchance] Update Enchance Success Item Data Fail, AccountId = {userInfo.AccountId}, UserItemId = {request.UserItemId}");
                    response.Result = errorCode;
                    return response;
                }
            }
            else
            {
                // 강화 실패 => 아이템 삭제
                errorCode = await _gameDb.DeleteItemData(userInfo.AccountId, request.UserItemId);
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[Enchance] Delete Item Fail, AccountId = {userInfo.AccountId}, UserItemId = {request.UserItemId}");
                    response.Result = errorCode;
                    return response;
                }
            }

            _logger.ZLogDebug($"[Enchance] Complete, AccountId = {userInfo.AccountId}, Result = {success}, UserItemId = {request.UserItemId}");

            response.IsSuccess = success;
            return response;
        }

        bool GetEnchanceResult()
        {
            // 랜덤 값 생성, 0 이상, 100 미만 사이의 난수
            Random random = new Random();
            var enchanceProbability = random.Next(100);

            // 70% 확률로 강화를 성공함
            if(enchanceProbability < 70)
            {
                // 강화 성공
                return true;
            }

            return false;
        }
    }
}
