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

        QueryFactory _queryFactory;
        IDbConnection _connection;
        MySqlCompiler _compiler;

        public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> options) 
        { 
            _logger = logger;
            _dbConfig = options;

            _connection = new MySqlConnection(_dbConfig.Value.GameDb);
            _connection.Open();

            _compiler = new MySqlCompiler();
            _queryFactory = new QueryFactory(_connection, _compiler);
        }

        public void Dispose()
        {
            _connection.Close();    
        }

        public async Task<ErrorCode> CreateGameData(Int64 accountId)
        {
            try
            {
                var count = await _queryFactory.Query("GameData").InsertAsync(new
                {
                    AccountId = accountId,
                    Level = 1,
                    Exp = 0,
                    Hp = 1,
                    Ap = 1,
                    Mp = 1,
                    LastLoginDate = DateTime.Today
                });

                if(count != 1)
                {
                    _logger.ZLogError($"[AccountDb.CreateAccount] ErrorCode : {ErrorCode.Create_GameData_Fail_Duplicate}, AccountId: {accountId}");
                    return ErrorCode.Create_GameData_Fail_Duplicate;
                }
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode : {ErrorCode.Create_GameData_Fail_Exception}, AccountId: {accountId}");
                return ErrorCode.Create_GameData_Fail_Exception;
            }

            return ErrorCode.None;
        }

        public async Task<Tuple<ErrorCode, GameData>> GetGameData(Int64 accountId)
        {
            try
            {
                var gameData = await _queryFactory.Query("GameData")
                    .Select("Level", "Exp", "Hp", "Ap", "Mp")
                    .Where("AccountId", accountId).FirstOrDefaultAsync<GameData>();

                if(gameData is null)
                {
                    _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode : {ErrorCode.Get_GameDate_Fail_Not_Exist}, AccountId: {accountId}");
                    return new Tuple<ErrorCode, GameData>(ErrorCode.Get_GameDate_Fail_Not_Exist, null);
                }

                _logger.ZLogInformation($"[GameDb.CreateGameData] Get Game Data Complete, AccountId: {accountId}");
                return new Tuple<ErrorCode, GameData>(ErrorCode.None, gameData);
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode : {ErrorCode.Get_GameDate_Fail_Not_Exist}, AccountId: {accountId}");
                return new Tuple<ErrorCode, GameData>(ErrorCode.Get_GameData_Fail_Exception, null);
            }
        }
        /*
        public Task<ErrorCode> InsertCharacterItem(Int64 accountId, ItemData itemData)
        {

        }
        */
        public async Task<Tuple<ErrorCode, IEnumerable<ItemData>>> GetItemData(Int64 accountId)
        {
            try
            {
                var ItemDataList = await _queryFactory.Query("ItemData")
                    .Select("ItemCode", "Amount", "EnchanceCount")
                    .Where("AccountId", accountId).GetAsync<ItemData>();

                return new Tuple<ErrorCode, IEnumerable<ItemData>>(ErrorCode.None, ItemDataList);   
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode : {ErrorCode.Get_GameDate_Fail_Not_Exist}, AccountId: {accountId}");
                return new Tuple<ErrorCode, IEnumerable<ItemData>>(ErrorCode.Get_ItemData_Fail_Exception, null);
            }
        }
        
    }
}
