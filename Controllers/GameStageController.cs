using Microsoft.AspNetCore.Mvc;
using TuesberryAPIServer.ModelDb;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.Services;
using ZLogger;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GameStageController : ControllerBase
    {
        readonly ILogger<GameStageController> _logger;
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly IGameDb _gameDb;
        readonly IMasterDb _masterDb;
        readonly IMemoryDb _memoryDb;

        public GameStageController(ILogger<GameStageController> logger, IGameDb gameDb, IMasterDb masterDb, IHttpContextAccessor httpContextAccessor, IMemoryDb memoryDb)
        {
            _logger = logger;
            _gameDb = gameDb;
            _masterDb = masterDb;
            _httpContextAccessor = httpContextAccessor;
            _memoryDb = memoryDb;
        }

        [HttpPost("LoadStage")]
        public async Task<PkLoadStageResponse> LoadStage([FromBody] PkLoadStageRequest request)
        {
            var response = new PkLoadStageResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // 최종 클리어 스테이지 번호를 가져온다.
            var (errorCode, stageNum) = await _gameDb.LoadFinalCompletedStageNum(userInfo.AccountId);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.LoadStage] Load Final Completed Stage Number Error, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[GameStageController.LoadStage] Complete, AccountId = {userInfo.AccountId}");
            
            response.FinalCompletedStageNum = stageNum;
            return response;
        }

        [HttpPost("SelectStage")]
        public async Task<PkSelectStageResponse> SelectStage([FromBody] PkSelectStageRequest request)
        {
            var response = new PkSelectStageResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // 선택 가능 검증
            var (errorCode, finalStageNum) = await _gameDb.LoadFinalCompletedStageNum(userInfo.AccountId);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.SelectStage] Load Final Completed Stage Error, AccountId = {userInfo.AccountId}, SelectedStageNum = {request.StageNum}");
                response.Result = errorCode;
                return response;
            }
            if (finalStageNum + 1 < request.StageNum)
            {
                _logger.ZLogError($"[GameStageController.SelectStage] Invalid Selected Stage Number Error, AccountId = {userInfo.AccountId}, SelectedStageNum = {request.StageNum}");
                response.Result = ErrorCode.SelectStage_Fail_Invalid_SelectedStageNum;
                return response;
            }

            // 플레이 시작 저장
            errorCode = await _memoryDb.SetPlayingStage(userInfo.AccountId, request.StageNum);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.SelectStage] Set Playing Stage Error, AccountId = {userInfo.AccountId}, SelectedStageNum = {request.StageNum}");
                response.Result = errorCode;
                return response;
            }

            // 던전에 생성될 아이템 리스트 + NPC 리스트 전송
            response.StageItems = _masterDb.StageItems[request.StageNum];
            response.StageNpcs = _masterDb.StageNpc[request.StageNum];

            _logger.ZLogDebug($"[GameStageController.SelectStage] Complete, AccountId = {userInfo.AccountId}, SelectedStageNum = {request.StageNum}");

            return response;
        }

        [HttpPost("SaveFoundItem")]
        public async Task<PkSaveFoundItemResponse> SaveFoundItem([FromBody] PkSaveFoundItemRequest request)
        {
            var response = new PkSaveFoundItemResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // 플레이 시작한 스테이지 번호인지 검증
            var (errorCode, stageNum) = await _memoryDb.GetPlayingStage(userInfo.AccountId);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.SaveFoundItem] Get Playing Stage Error, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }
            if(stageNum != request.StageNum)
            {
                _logger.ZLogError($"[GameStageController.SaveFoundItem] ErrorCode = {ErrorCode.SaveFoundItem_Fail_Invalid_StageNum}, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}, ItemCode = {request.ItemCode}");
                response.Result = ErrorCode.SaveFoundItem_Fail_Invalid_StageNum;
                return response;
            }

            // 찾은 아이템 코드 검증
            if(!_masterDb.StageItems[stageNum].Contains(request.ItemCode))
            {
                _logger.ZLogError($"[GameStageController.SaveFoundItem] ErrorCode = {ErrorCode.SaveFoundItem_Fail_Invalid_ItemCode}, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}, ItemCode = {request.ItemCode}");
                response.Result = ErrorCode.SaveFoundItem_Fail_Invalid_ItemCode;
                return response;
            }

            // 데이터 저장
            errorCode = await _memoryDb.SetStageFoundItem(userInfo.AccountId, request.ItemCode);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.SaveFoundItem] Set Stage Found Item Fail, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}, ItemCode = {request.ItemCode}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[GameStageController.SaveFoundItem] Complete, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}, ItemCode = {request.ItemCode}");
            return response;
        }

        [HttpPost("SaveKilledNpc")]
        public async Task<PkSaveKilledNpcResponse> SaveKilledNpc([FromBody] PkSaveKilledNpcRequest request)
        {
            var response = new PkSaveKilledNpcResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // 플레이 시작한 스테이지 번호인지 검증
            var (errorCode, stageNum) = await _memoryDb.GetPlayingStage(userInfo.AccountId);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.SaveKilledNpc] Get Playing Stage Error, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }
            if (stageNum != request.StageNum)
            {
                _logger.ZLogError($"[GameStageController.SaveKilledNpc] ErrorCode = {ErrorCode.SaveKilledNpc_Fail_Invalid_StageNum}, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}, NpcCode = {request.NpcCode}");
                response.Result = ErrorCode.SaveKilledNpc_Fail_Invalid_StageNum;
                return response;
            }

            // Npc 코드 검증
            if (!IsValidNpcCode(request.StageNum, request.NpcCode))
            {
                _logger.ZLogError($"[GameStageController.SaveKilledNpc] ErrorCode = {ErrorCode.SaveKilledNpc_Fail_Invalid_ItemCode}, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}, NpcCode = {request.NpcCode}");
                response.Result = ErrorCode.SaveKilledNpc_Fail_Invalid_ItemCode;
                return response;
            }

            // 스테이지에 나타날 수 있는 Npc 수를 초과했는지 검증
            if(await IsExceedNpcNum(userInfo.AccountId, request.StageNum, request.NpcCode))
            {
                _logger.ZLogError($"[GameStageController.SaveKilledNpc] ErrorCode = {ErrorCode.SaveKilledNpc_Fail_Exceed_Number_Can_Appear}, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}, NpcCode = {request.NpcCode}");
                response.Result = ErrorCode.SaveKilledNpc_Fail_Exceed_Number_Can_Appear;
                return response;
            }

            // 데이터 저장
            errorCode = await _memoryDb.SetStageKilledNpc(userInfo.AccountId, request.NpcCode);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.SaveKilledNpc] Set Stage Killed Npc Fail, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}, NpcCode = {request.NpcCode}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[GameStageController.SaveKilledNpc] Complete, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}, NpcCode = {request.NpcCode}");
            return response;
        }

        [HttpPost("EndStage")]
        public async Task<PkEndStageResponse> EndStage([FromBody] PkEndStageRequest request)
        {
            var response = new PkEndStageResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // 플레이 시작한 스테이지 번호인지 검증
            var (errorCode, stageNum) = await _memoryDb.GetPlayingStage(userInfo.AccountId);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.EndStage] Get Playing Stage Error, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }
            if (stageNum != request.StageNum)
            {
                _logger.ZLogError($"[GameStageController.EndStage] ErrorCode = {ErrorCode.SaveKilledNpc_Fail_Invalid_StageNum}, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}");
                response.Result = ErrorCode.SaveKilledNpc_Fail_Invalid_StageNum;
                return response;
            }

            // 저장한 아이템, Npc 정보 가져오기
            (errorCode, var stageData) = await _memoryDb.LoadPlayingStageInfo(userInfo.AccountId, request.StageNum);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.EndStage] Load Playing Stage Info Fail, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}");
                response.Result = errorCode;
                return response;
            }

            // 클리어 여부 확인
            if(!IsStageClear(request.StageNum, stageData))
            {
                // 클리어 실패
                _logger.ZLogDebug($"[GameStageController.EndStage] Stage Clear Fail, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}");

                // 스테이지 저장 데이터 삭제
                errorCode = await _memoryDb.DelPlayingStage(userInfo.AccountId);
                if(errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameStageController.EndStage] Delete Playing Stage Info Fail, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}");
                    response.Result = errorCode;
                    return response;
                }

                response.IsClear = false;
                return response;
            }

            // 클리어 스테이지 정보 업데이트


            // 기존 레벨 & 경험치 로드
            (errorCode, var level, var exp) = await _gameDb.LoadLevelAndExp(userInfo.AccountId);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.EndStage] Load Level And Exp Fail, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}");
                response.Result = errorCode;
                return response;
            }

            // 경험치 주고, 레벨 업데이트
            (errorCode, var newLevel, var newExp) = await UpdateExpAndLevel(userInfo.AccountId, request.StageNum, level, exp);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.EndStage] Give Exp And Update Level Fail, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}");
                response.Result = errorCode;
                return response;
            }

            // 스테이지 안에서 찾은 아이템 추가
            (errorCode, var items) = await InsertFoundItems(userInfo.AccountId, request.StageNum, stageData);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.EndStage] Give Reward Items Fail, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}");

                // 경험치 & 레벨 롤백
                var rollbackResult = await _gameDb.UpdateLevelAndExp(userInfo.AccountId, level, exp);
                if (rollbackResult != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameStageController.GiveExpAndUpdateLevel] Rollback Level & Exp Fail, AccountId = {userInfo.AccountId}, StageNum = {stageNum}");
                }

                response.Result = errorCode;
                return response;
            }

            // 스테이지 저장 데이터 삭제
            errorCode = await _memoryDb.DelPlayingStage(userInfo.AccountId);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.EndStage] Delete Playing Stage Info Fail, AccountId = {userInfo.AccountId}, StageNum = {request.StageNum}");

                // 경험치 & 레벨 롤백
                var rollbackResult = await _gameDb.UpdateLevelAndExp(userInfo.AccountId, level, exp);
                if (rollbackResult != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameStageController.GiveExpAndUpdateLevel] Rollback Level & Exp Fail, AccountId = {userInfo.AccountId}, StageNum = {stageNum}");
                }
                // 찾은 아이템 롤백 
                rollbackResult = await _gameDb.DeleteOrUpdateItem(userInfo.AccountId, items);
                if(rollbackResult != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameStageController.GiveExpAndUpdateLevel] Rollback Items Fail, AccountId = {userInfo.AccountId}, StageNum = {stageNum}");
                }

                response.Result = errorCode;
                return response;
            }

            // 변경된 레벨 & 아이템, 추가된 아이템 정보 클라이언트로 전송
            response.Level = newLevel;
            response.Exp = newExp;
            response.Items = items;

            return response;
        }

        bool IsValidNpcCode(Int32 stageNum, Int32 npcCode)
        {
            foreach(var npcData in _masterDb.StageNpc[stageNum])
            {
                if(npcData.NpcCode == npcCode)
                {
                    return true;
                }
            }
            return false;
        }

        async Task<bool> IsExceedNpcNum(Int64 accountId, Int32 stageNum, Int32 npcCode)
        {
            var (errorCode, npcNum) = await _memoryDb.LoadStageKilledNpcNum(accountId, npcCode);
            if(errorCode == ErrorCode.LoadStageKilledNpcNum_Fail_Exception)
            {
                _logger.ZLogError($"[GameStageController.IsExceedNpcNum] Load Stage Killed Enemy Num Fail, AccountId = {accountId}, StageNum = {stageNum}, NpcCode = {npcCode}");
                return true;
            }

            foreach (var npc in _masterDb.StageNpc[stageNum])
            {
                if (npc.NpcCode == npcCode)
                {
                    if(npc.Count <= npcNum)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        bool IsStageClear(Int32 stageNum, Dictionary<string, Int32> stageData)
        {
            // 모든 npc들을 처치했는지 체크한다.
            Int32 count = 0;
            
            foreach (var npc in _masterDb.StageNpc[stageNum])
            {
                var key = MemoryDbKeyMaker.MakeStageNpcKey(npc.NpcCode);

                if(stageData.TryGetValue(key, out count))
                {
                    if(count != npc.Count)
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        async Task<Tuple<ErrorCode, Int32, Int32>> UpdateExpAndLevel(Int64 accountId, Int32 stageNum, Int32 level, Int32 exp)
        {
            // 경험치 계산
            Int32 updateExp = CalculateStageExp(stageNum);

            // 새로운 레벨, 경험치 값 계산
            var(newLevel, newExp) = UpdateLevelAndExp(level, exp, updateExp);

            // 새로운 값으로 업데이트
            var errorCode = await _gameDb.UpdateLevelAndExp(accountId, newLevel, newExp);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.GiveExpAndUpdateLevel] Update Level And Exp Fail, AccountId = {accountId}, StageNum = {stageNum}");
                return new Tuple<ErrorCode, Int32, Int32>(errorCode, 0, 0);
            }

            _logger.ZLogDebug($"[GameStageController.GiveExpAndUpdateLevel] Complete, AccountId = {accountId}, StageNum = {stageNum}");
            return new Tuple<ErrorCode, Int32, Int32>(ErrorCode.None, newLevel, newExp);
        }

        Int32 CalculateStageExp(Int32 stageNum)
        {
            Int32 exp = 0;
            foreach(var npc in _masterDb.StageNpc[stageNum])
            {
                exp += npc.Count * npc.Exp;
            }
            return exp;
        }

        (Int32, Int32) UpdateLevelAndExp(Int32 level, Int32 exp, Int32 updateExp)
        {
            exp += updateExp;

            while(_masterDb.LevelUpExp[level] >= exp)
            {
                exp -= _masterDb.LevelUpExp[level];
                level += 1;
            }

            return (level, exp);
        }

        async Task<Tuple<ErrorCode, List<ItemData>>> InsertFoundItems(Int64 accountId, Int32 stageNum, Dictionary<string, Int32> stageData)
        {
            List<ItemData> items = new List<ItemData>();
            Int32 amount = 0;

            // 추가할 아이템 리스트
            foreach(var itemCode in _masterDb.StageItems[stageNum])
            {
                var key = MemoryDbKeyMaker.MakeStageItemKey(itemCode);

                if(stageData.TryGetValue(key, out amount))
                {
                    if (_masterDb.Items[itemCode].IsOverlapped)
                    {
                        // 겹쳐질 수 있는 경우 => 한번에 추가
                        items.Add(new ItemData
                        {
                            ItemCode = itemCode,
                            Amount = amount,
                            EnchanceCount = 0,
                            Attack = _masterDb.Items[itemCode].Attack,
                            Defence = _masterDb.Items[itemCode].Defence,
                            Magic = _masterDb.Items[itemCode].Magic
                        });
                    }
                    else
                    {
                        // 겹쳐지지 않음 => 하나씩 insert 해야 함
                        for(int i =0; i  < amount; i++)
                        {
                            items.Add(new ItemData
                            {
                                ItemCode = itemCode,
                                Amount = 1,
                                EnchanceCount = 0,
                                Attack = _masterDb.Items[itemCode].Attack,
                                Defence = _masterDb.Items[itemCode].Defence,
                                Magic = _masterDb.Items[itemCode].Magic
                            });
                        }
                    }
                }
            }

            var errorCode = await _gameDb.InsertOrUpdateItem(accountId, items);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[GameStageController.GiveRewardItems] InsertOrUpdateItem Fail, AccountId = {accountId}, StageNum = {stageNum}");
                return new Tuple<ErrorCode, List<ItemData>>(errorCode, null);
            }

            _logger.ZLogDebug($"[GameStageController.GiveRewardItems] Complete, AccountId = {accountId}, StageNum = {stageNum}");
            return new Tuple<ErrorCode, List<ItemData>>(ErrorCode.None, items);
        }

    }
}
