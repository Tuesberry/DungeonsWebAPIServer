﻿using Microsoft.Extensions.Options;
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

        QueryFactory _queryFactory;
        IDbConnection _connection;
        SqlKata.Compilers.MySqlCompiler _compiler;

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
                    _logger.ZLogError($"[AccountDb.CreateAccount] ErrorCode : {ErrorCode.Create_Account_Fail_Duplicate}, Id: {id}");
                    return ErrorCode.Create_Account_Fail_Duplicate;
                }

                _logger.ZLogInformation($"[CreateAccount] Id: {id}, SaltValue: {saltValue} ,hashedPassword: {hashingPassword}");
                return ErrorCode.None;
            }
            catch (Exception ex)
            {
                _logger.ZLogError($"[AccountDb.CreateAccount] ErrorCode : {ErrorCode.Create_Account_Fail_Exception}, Id: {id}");
                return ErrorCode.Create_Account_Fail_Exception;
            }
        }
        public async Task<Tuple<ErrorCode, Int64>> VerifyAccount(string id, string pw)
        {
            try
            {
                var accountInfo = await _queryFactory.Query("account").Where("UserId", id).FirstOrDefaultAsync<Account>();

                if (accountInfo is null || accountInfo.AccountId == 0)
                {
                    _logger.ZLogError($"[AccountDb.VerifyAccount] ErrorCode : {ErrorCode.Login_Fail_Pw_Not_Match}, Id : {id}");
                    return new Tuple<ErrorCode, Int64>(ErrorCode.Login_Fail_User_Not_Exist, 0);
                }

                var hashingValue = Security.MakeHashingPassWord(accountInfo.SaltValue, pw);
                if (accountInfo.HashedPassword != hashingValue)
                {
                    _logger.ZLogError($"[AccountDb.VerifyAccount] ErrorCode : {ErrorCode.Login_Fail_Pw_Not_Match}, Id : {id}");
                    return new Tuple<ErrorCode, Int64>(ErrorCode.Login_Fail_Pw_Not_Match, 0);
                }

                _logger.ZLogInformation($"[AccountDb.VerifyAccount] id : {id}, pw : {pw}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.None, accountInfo.AccountId);
            }
            catch
            {
                _logger.ZLogError($"[AccountDb.VerifyAccount] ErrorCode : {ErrorCode.Login_Fail_Exception}, Id : {id}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.Login_Fail_Exception, 0);
            }
        }
    }
}