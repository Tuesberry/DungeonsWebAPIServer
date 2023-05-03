using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using TuesberryAPIServer.ModelDb;
using ZLogger;

namespace TuesberryAPIServer.Services
{
    public class GameDb : IGameDb
    {
        readonly ILogger<GameDb> _logger;
        readonly IOptions<DbConfig> _dbConfig;

        readonly IMasterDb _masterDb;

        QueryFactory _queryFactory;
        IDbConnection _connection;
        MySqlCompiler _compiler;

        public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> options, IMasterDb masterDb)
        {
            _logger = logger;
            _dbConfig = options;
            _masterDb = masterDb;

            _connection = new MySqlConnection(_dbConfig.Value.GameDb);
            _connection.Open();

            _compiler = new MySqlCompiler();
            _queryFactory = new QueryFactory(_connection, _compiler);
        }

        public void Dispose()
        {
            _connection.Close();
        }

        public async Task<Tuple<ErrorCode, Int64>> CreateGameData(string userId)
        {
            try
            {
                Int64 accountId = await _queryFactory.Query("GameData").InsertGetIdAsync<Int64>(new
                {
                    UserId = userId,
                    Level = 1,
                    Exp = 0,
                    Hp = 20,
                    Ap = 10,
                    Mp = 10,
                    Stage = 0
                });

                if (accountId == 0)
                {
                    _logger.ZLogError($"[AccountDb.CreateAccount] ErrorCode : {ErrorCode.Create_GameData_Fail_Duplicate}, UserId: {userId}");
                    return new Tuple<ErrorCode, Int64>(ErrorCode.Create_GameData_Fail_Duplicate, 0);
                }

                return new Tuple<ErrorCode, Int64>(ErrorCode.None, accountId);
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode : {ErrorCode.Create_GameData_Fail_Exception}, UserId: {userId}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.Create_GameData_Fail_Exception, 0);
            }
        }

        public async Task<Tuple<ErrorCode, GameData, Int64>> LoadGameData(string userId)
        {
            try
            {
                var gameData = await _queryFactory.Query("GameData")
                    .Select("Level", "Exp", "Hp", "Ap", "Mp")
                    .Where("UserId", userId).FirstOrDefaultAsync<GameData>();

                var accountId = await _queryFactory.Query("GameData")
                    .Select("AccountId")
                    .Where("UserId", userId).FirstOrDefaultAsync<Int64>();

                if (accountId == 0)
                {
                    _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode : {ErrorCode.Get_GameDate_Fail_Not_Exist}, UserId: {userId}");
                    return new Tuple<ErrorCode, GameData, Int64>(ErrorCode.Get_GameDate_Fail_Not_Exist, null, 0);
                }

                _logger.ZLogInformation($"[GameDb.CreateGameData] Get Game Data Complete, UserId: {userId}");
                return new Tuple<ErrorCode, GameData, Int64>(ErrorCode.None, gameData, accountId);
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode : {ErrorCode.Get_GameData_Fail_Exception}, UserId: {userId}");
                return new Tuple<ErrorCode, GameData, Int64>(ErrorCode.Get_GameData_Fail_Exception, null, 0);
            }
        }

        public async Task<ErrorCode> CreateDefaultItemData(Int64 accountId)
        {
            try
            {
                ItemData itemData = new ItemData();
                itemData.ItemCode = 1;
                itemData.Amount = 10;

                var errorCode = await InsertItem(accountId, itemData);
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameDb.CreateDefaultItemData] ErrorCode : {errorCode}, AccountId: {accountId}");
                    return errorCode;
                }

                itemData.ItemCode = 2;
                itemData.Amount = 1;

                errorCode = await InsertItem(accountId, itemData);
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameDb.CreateDefaultItemData] ErrorCode : {errorCode}, AccountId: {accountId}");
                    return errorCode;
                }

                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateDefaultItemData] ErrorCode : {ErrorCode.Create_Item_Data_Fail_Exception}, AccountId: {accountId}");
                return ErrorCode.Create_Item_Data_Fail_Exception;
            }
        }

        public async Task<ErrorCode> InsertOrUpdateItem(Int64 accountId, ItemData itemData)
        {
            try
            {
                Int32 itemCode = itemData.ItemCode;

                // 겹침 가능 여부 확인
                if (_masterDb.Items[itemCode].IsOverlapped)
                {
                    // 겹쳐질 수 있음 => update

                    // 이미 있는지 확인
                    var checkData = await _queryFactory.Query("ItemData")
                        .Select("ItemId", "Amount")
                        .Where(new { AccountID = accountId, ItemCode = itemCode })
                        .GetAsync<ItemData>();

                    // check null
                    if (checkData is null)
                    {
                        _logger.ZLogError($"[GameDb.InsertOrUpdateItem] data doesn't exist, AccountId: {accountId}");
                        return await InsertItem(accountId, itemData);
                    }

                    // get first data
                    var refData = checkData.First();

                    // update info
                    var count = await _queryFactory.Query("ItemData")
                        .Where(new { ItemId = refData.ItemId })
                        .UpdateAsync(new { Amount = refData.Amount + itemData.Amount });

                    if (count != 1)
                    {
                        _logger.ZLogError($"[GameDb.InsertOrUpdateItem] ErrorCode : {ErrorCode.InsertOrUpdate__Item_Data_Fail_Exception}, AccountId: {accountId}");
                        return ErrorCode.InsertOrUpdate__Item_Data_Fail_Exception;
                    }
                }
                else
                {
                    // 겹쳐질 수 없음 => Insert
                    _logger.ZLogError($"[GameDb.InsertOrUpdateItem] data can't overlappe, AccountId: {accountId}");
                    return await InsertItem(accountId, itemData);
                }

                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.InsertOrUpdateItem] ErrorCode : {ErrorCode.InsertOrUpdate__Item_Data_Fail_Exception}, AccountId: {accountId}");
                return ErrorCode.InsertOrUpdate__Item_Data_Fail_Exception;
            }
        }

        async Task<ErrorCode> InsertItem(Int64 accountId, ItemData itemData)
        {
            try
            {
                var itemCode = itemData.ItemCode;

                var count = await _queryFactory.Query("ItemData").InsertAsync(new
                {
                    AccountId = accountId,
                    ItemCode = itemCode,
                    Amount = itemData.Amount,
                    Attack = _masterDb.Items[itemCode].Attack,
                    Defence = _masterDb.Items[itemCode].Defence,
                    Magic = _masterDb.Items[itemCode].Magic,
                });

                if (count != 1)
                {
                    _logger.ZLogError($"[GameDb.InsertItem] ErrorCode : {ErrorCode.Insert_Item_Data_Fail_Duplicate}, AccountId: {accountId}");
                    return ErrorCode.Insert_Item_Data_Fail_Duplicate;
                }

                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.InsertItem] ErrorCode : {ErrorCode.Insert_Item_Data_Fail_Exception}, AccountId: {accountId}");
                return ErrorCode.Insert_Item_Data_Fail_Exception;
            }
        }

        public async Task<Tuple<ErrorCode, List<ItemData>>> LoadItemData(Int64 accountId)
        {
            try
            {
                var ItemDataList = await _queryFactory.Query("ItemData")
                    .Select("ItemId", "ItemCode", "Amount", "EnchanceCount")
                    .Where("AccountId", accountId).GetAsync<ItemData>();

                return new Tuple<ErrorCode, List<ItemData>>(ErrorCode.None, ItemDataList.ToList<ItemData>());
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode : {ErrorCode.Get_GameDate_Fail_Not_Exist}, AccountId: {accountId}");
                return new Tuple<ErrorCode, List<ItemData>>(ErrorCode.Get_ItemData_Fail_Exception, null);
            }
        }

        public async Task<Tuple<ErrorCode, List<MailboxData>>> LoadMailboxData(Int64 accountId, Int32 page)
        {
            try
            {
                Int32 offset = 10 * (page - 1);
                var MailboxDataList = await _queryFactory.Query("Mailbox")
                    .Select("MailId", "Title", "ItemCode", "Amount", "IsRead")
                    .Where("AccountId", accountId)
                    .Limit(20).Offset(offset)
                    .GetAsync<MailboxData>();

                return new Tuple<ErrorCode, List<MailboxData>>(ErrorCode.None, MailboxDataList.ToList<MailboxData>());
            }
            catch
            {
                _logger.ZLogError($"[GameDb.LoadMailboxData] ErrorCode : {ErrorCode.LoadMailBoxData_Fail_Exception}, AccountId: {accountId}");
                return new Tuple<ErrorCode, List<MailboxData>>(ErrorCode.LoadMailBoxData_Fail_Exception, null);
            }
        }

        public async Task<Tuple<ErrorCode, ItemData>> LoadMailItemData(Int64 accountId, Int32 mailId)
        {
            try
            {
                var mailItemData = await _queryFactory.Query("Mailbox")
                    .Select("ItemCode", "Amount")
                    .Where(new { AccountId = accountId, MailId = mailId})
                    .GetAsync<ItemData>();

                if(mailItemData is null)
                {
                    return new Tuple<ErrorCode, ItemData>(ErrorCode.LoadMailItem_Fail_Item_Not_Exist, null);
                }

                return new Tuple<ErrorCode, ItemData>(ErrorCode.None, mailItemData.First());
            }
            catch
            {
                _logger.ZLogError($"[GameDb.LoadMailItemData] ErrorCode : {ErrorCode.LoadMailItem_Fail_Exception}, AccountId: {accountId}");
                return new Tuple<ErrorCode, ItemData>(ErrorCode.LoadMailItem_Fail_Exception, null);
            }
        }

        public async Task<ErrorCode> DeleteMail(Int64 accountId, Int32 mailId)
        {
            try
            {
                var result = await _queryFactory.Query("Mailbox").Where(new { AccountId = accountId, MailId = mailId }).DeleteAsync();
                _logger.ZLogInformation($"[GameDb.DeleteMail] Delete Result : {result}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.DeleteMail] ErrorCode : {ErrorCode.DeleteMail_Fail_Exception}, AccountId: {accountId}");
                return ErrorCode.DeleteMail_Fail_Exception;
            }
        }

        public async Task<ErrorCode> LoadAndDeleteItemFromMail(Int64 accountId, Int32 mailId)
        {
            try
            {
                // load item
                var (errorCode, mailItemData) = await LoadMailItemData(accountId, mailId);
                if(errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameDb.LoadAndDeleteItemFromMail] load Item Fail, AccountId: {accountId}");
                    return errorCode;
                }
                // insert item
                errorCode = await InsertOrUpdateItem(accountId, mailItemData);
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameDb.LoadAndDeleteItemFromMail] Insert Item Fail , AccountId: {accountId}");
                    return errorCode;
                }
                // delete mail
                errorCode = await DeleteMail(accountId, mailId);
                if(errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameDb.LoadAndDeleteItemFromMail] Insert Item Fail , AccountId: {accountId}");
                    return errorCode;
                }

                return errorCode;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.LoadAndDeleteItemFromMail] ErrorCode : {ErrorCode.LoadAndDeleteItemFromMail_Fail_Exception}, AccountId: {accountId}");
                return ErrorCode.LoadAndDeleteItemFromMail_Fail_Exception;
            }
        }

    }
}
