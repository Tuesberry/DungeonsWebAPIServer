using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using ZLogger;
using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Services
{
    public class AccountDb : IAccountDb
    {
        readonly ILogger _logger;
        readonly IOptions<DbConfig> _dbConfig;

        readonly QueryFactory _queryFactory;
        readonly IDbConnection _connection;
        readonly MySqlCompiler _compiler;

        public AccountDb(ILogger<AccountDb> logger, IOptions<DbConfig> options) 
        {
            _logger = logger;
            _dbConfig = options;

            _connection = new MySqlConnection(_dbConfig.Value.AccountDb);
            _connection.Open();

            _compiler = new MySqlCompiler();
            _queryFactory = new QueryFactory(_connection, _compiler);
        }  
        public void Dispose()
        {
           _connection.Close();
        }

        public async Task<ErrorCode> CreateAccount(string id, string pw)
        {
            try
            {
                var saltValue = Security.SaltString();
                var hashingPassword = Security.MakeHashingPassWord(saltValue, pw);

                var count = await _queryFactory.Query("account").InsertAsync(new
                {
                    UserId = id,
                    SaltValue = saltValue,
                    HashedPassword = hashingPassword
                });

                if(count != 1)
                {
                    _logger.ZLogError($"[AccountDb.CreateAccount] ErrorCode = {ErrorCode.Create_Account_Fail_Duplicate}, UserId = {id}");
                    return ErrorCode.Create_Account_Fail_Duplicate;
                }

                _logger.ZLogDebug($"[AccountDb.CreateAccount] UserId = {id}, SaltValue = {saltValue} ,hashedPassword = {hashingPassword}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[AccountDb.CreateAccount] ErrorCode = {ErrorCode.Create_Account_Fail_Exception}, UserId = {id}");
                return ErrorCode.Create_Account_Fail_Exception;
            }
        }
        public async Task<ErrorCode> VerifyAccount(string id, string pw)
        {
            try
            {
                var accountInfo = await _queryFactory.Query("account").Where("UserId", id).FirstOrDefaultAsync<Account>();

                if (accountInfo is null )
                {
                    _logger.ZLogError($"[AccountDb.VerifyAccount] ErrorCode = {ErrorCode.Login_Fail_Pw_Not_Match}, UserId = {id}");
                    return ErrorCode.Login_Fail_User_Not_Exist;
                }

                var hashingValue = Security.MakeHashingPassWord(accountInfo.SaltValue, pw);
                if (accountInfo.HashedPassword != hashingValue)
                {
                    _logger.ZLogError($"[AccountDb.VerifyAccount] ErrorCode = {ErrorCode.Login_Fail_Pw_Not_Match}, UserId = {id}");
                    return ErrorCode.Login_Fail_Pw_Not_Match;
                }

                _logger.ZLogDebug($"[AccountDb.VerifyAccount] UserId = {id}, Pw = {pw}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[AccountDb.VerifyAccount] ErrorCode = {ErrorCode.Login_Fail_Exception}, UserId = {id}");
                return ErrorCode.Login_Fail_Exception;
            }
        }

        public async Task<ErrorCode> DeleteAccount(string id)
        {
            try
            {
                var result = await _queryFactory.Query("account").Where("UserId", id).DeleteAsync();
                _logger.ZLogDebug($"[AccountDb.DeleteAccount] UserId = {id}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"AccountDb.DeleteAccount] ErrorCode = {ErrorCode.DeleteAccountData_Fail_Exception}, UserId = {id}");
                return ErrorCode.DeleteAccountData_Fail_Exception;
            }
        }

    }
}
