using CloudStructures;
using CloudStructures.Structures;
using Microsoft.Extensions.Options;
using TuesberryAPIServer.ModelDb;
using ZLogger;

namespace TuesberryAPIServer.Services
{
    public class RedisDb : IMemoryDb
    {
        readonly ILogger<RedisDb> _logger;
        public RedisConnection _redisCon;

        public RedisDb(ILogger<RedisDb> logger) 
        { 
            _logger = logger;
        }

        public void Init(string address)
        {
            var config = new RedisConfig("default", address);
            _redisCon = new RedisConnection(config);

            _logger.ZLogDebug($"UserDbAddress:{address}");
        }

        public async Task<ErrorCode> RegistUserAsync(string id, string authToken, Int64 accountId)
        {
            var key = MemoryDbKeyMaker.MakeUIDKey(id);
            var result = ErrorCode.None;

            var user = new AuthUser
            {
                AccountId = accountId,
                UserId = id,
                AuthToken = authToken,
                State = UserState.Default.ToString()
            };

            try
            {
                var redis = new RedisString<AuthUser>(_redisCon, key, null);
                if(await redis.SetAsync(user, null) == false)
                {
                    _logger.ZLogError($"[RedisDb.RegisterUserAsync] Redis String set error, id = {id}");
                    result = ErrorCode.Login_Fail_Add_Redis;
                    return result;
                }
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.RegisterUserAsync] Redis connection error, id = {id}");
                result = ErrorCode.Login_Fail_Add_Redis;
                return result;
            }

            return result;
        }

        public async Task<ErrorCode> CheckUserAuthAsync(string id, string authToken)
        {
            var key = MemoryDbKeyMaker.MakeUIDKey(id);
            var result = ErrorCode.None;

            try
            {
                var redis = new RedisString<AuthUser>(_redisCon, key, null);
                var user = await redis.GetAsync();

                if (!user.HasValue)
                {
                    _logger.ZLogError($"[RedisDb.CheckUserAuthAsync] Id doesn't exist, id = {id}");
                    result = ErrorCode.Check_Auth_Fail_Not_Exist;
                    return result;
                }

                if (user.Value.UserId != id || user.Value.AuthToken != authToken)
                {
                    _logger.ZLogError($"[RedisDb.CheckUserAuthAsync] Wrong id or token, id = {id}");
                    result = ErrorCode.Check_Auth_Fail_Not_Match;
                    return result;
                }
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.CheckUserAuthAsync] RedisConnection Error, id = {id}");
                result = ErrorCode.Check_Auth_Fail_Exception;
                return result;
            }

            return result;
        }

        public async Task<(bool, AuthUser)> GetUserAsync(string id)
        {
            var key = MemoryDbKeyMaker.MakeUIDKey(id);

            try
            {
                var redis = new RedisString<AuthUser>(_redisCon, key, null);
                var user = await redis.GetAsync();

                if(!user.HasValue)
                {
                    _logger.ZLogError($"[RedisDb.GetUserAsync] Not assigned user, id = {id}");
                    return (false, null);
                }

                return (true, user.Value);
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.GetUserAsync] Id doesn't exist, id = {id}");
                return (false, null);
            }
        }

        public async Task<Tuple<ErrorCode, string>> GetNotice()
        {
            try
            {
                var redis = new RedisString<string>(_redisCon, "Notice", null);
                var notice = await redis.GetAsync();

                if (!notice.HasValue)
                {
                    _logger.ZLogError($"[RedisDb.GetNotice] Notice doesn't exist");
                    return new Tuple<ErrorCode, string>(ErrorCode.Get_Notice_Fail_Exception, null);
                }

                return new Tuple<ErrorCode, string>(ErrorCode.None, notice.Value);
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.GetNotice] Redis error");
                return new Tuple<ErrorCode, string>(ErrorCode.Get_Notice_Fail_Exception, null);
            }
        }
    }
}
