using CloudStructures;
using CloudStructures.Structures;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using TuesberryAPIServer.ModelDb;
using ZLogger;

namespace TuesberryAPIServer.Services
{
    public class RediskeyExpireTime
    {
        public const ushort NxKeyExpireSecond = 3;
        public const ushort LoginKeyExpireMin = 60;
        public const ushort PlayingInfoKeyExpireMin = 3000; //test 용도
    }

    public class Channel
    {
        public const ushort ChannelCount = 100;
        public const ushort MaxCountPerChannel = 10;
    }

    public class RedisDb : IMemoryDb
    {
        readonly ILogger<RedisDb> _logger;
        public RedisConnection _redisCon;

        public RedisDb(ILogger<RedisDb> logger)
        {
            _logger = logger;
        }

        public TimeSpan LoginTimeSpan()
        {
            return TimeSpan.FromMinutes(RediskeyExpireTime.LoginKeyExpireMin);
        }

        public TimeSpan PlayingInfoTimeSpan()
        {
            return TimeSpan.FromMinutes(RediskeyExpireTime.PlayingInfoKeyExpireMin);
        }

        public TimeSpan NxKeyTimeSpan()
        {
            return TimeSpan.FromSeconds(RediskeyExpireTime.NxKeyExpireSecond);
        }

        public void Init(string address)
        {
            var config = new RedisConfig("default", address);
            _redisCon = new RedisConnection(config);

            _logger.ZLogDebug($"UserDbAddress = {address}");
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
                var redis = new RedisString<AuthUser>(_redisCon, key, LoginTimeSpan());
                if (await redis.SetAsync(user, LoginTimeSpan()) == false)
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

                if (!user.HasValue)
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

        public async Task<ErrorCode> DelUserAsync(string id)
        {
            var key = MemoryDbKeyMaker.MakeUIDKey(id);

            try
            {
                var redis = new RedisString<AuthUser>(_redisCon, key, null);
                if (!await redis.DeleteAsync())
                {
                    return ErrorCode.Logout_Fail_Id_Not_Exist;
                }

                return ErrorCode.None;
            }
            catch
            {
                return ErrorCode.Logout_Fail_Exception;
            }
        }

        public async Task<bool> SetUserReqLockAsync(string key)
        {
            try
            {
                var redis = new RedisString<AuthUser>(_redisCon, key, null);
                if (await redis.SetAsync(new AuthUser(), NxKeyTimeSpan(), When.NotExists) == false)
                {
                    _logger.ZLogError($"[RedisDb.SetUserReqLockAsync] Error = Key Duplicate, key = {key}");
                    return false;
                }
                _logger.ZLogDebug($"[RedisDb.SetUserReqLock] complete, key = {key}");
                return true;
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.SetUserReqLockAsync] Redis Connection Error, key = {key}");
                return false;
            }
        }

        public async Task<bool> DelUserReqLockAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            try
            {
                var redis = new RedisString<AuthUser>(_redisCon, key, null);
                var redisResult = await redis.DeleteAsync();
                return redisResult;
            }
            catch
            {
                return false;
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

        public async Task<ErrorCode> SetPlayingStage(Int64 accountId, Int32 stageNum)
        {
            var key = MemoryDbKeyMaker.MakePlayingInfoKey(accountId);
            var stageKey = MemoryDbKeyMaker.StageKey;

            try
            {
                var redis = new RedisDictionary<string, Int32>(_redisCon, key, PlayingInfoTimeSpan());
                if (await redis.SetAsync(stageKey, stageNum, null, When.NotExists) == false)
                {
                    _logger.ZLogError($"[RedisDb.SetPlayingStage] ErrorCode = {ErrorCode.SetPlayingStage_Fail_Key_Duplicate}, AccountId = {accountId}, StageNum = {stageNum}");
                    return ErrorCode.SetPlayingStage_Fail_Key_Duplicate;
                }

                _logger.ZLogDebug($"[RedisDb.SetPlayingStage] Complete, AccountId = {accountId}, StageNum = {stageNum}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.SetPlayingStage] ErrorCode = {ErrorCode.SetPlayingStage_Fail_Exception}, AccountId = {accountId}, StageNum = {stageNum}");
                return ErrorCode.SetPlayingStage_Fail_Exception;
            }
        }

        public async Task<Tuple<ErrorCode, Int32>> GetPlayingStage(Int64 accountId)
        {
            var key = MemoryDbKeyMaker.MakePlayingInfoKey(accountId);
            var stageKey = MemoryDbKeyMaker.StageKey;

            try
            {
                var redis = new RedisDictionary<string, Int32>(_redisCon, key, null);
                var value = await redis.GetAsync(stageKey);

                if (!value.HasValue)
                {
                    _logger.ZLogError($"[RedisDb.GetPlayingStage] ErrorCode = {ErrorCode.GetPlayingStage_Fail_Key_Not_Exist}, AccountId = {accountId}");
                    return new Tuple<ErrorCode, Int32>(ErrorCode.GetPlayingStage_Fail_Key_Not_Exist, 0);
                }

                _logger.ZLogDebug($"[RedisDb.GetPlayingStage] Complete, AccountId = {accountId}");
                return new Tuple<ErrorCode, Int32>(ErrorCode.None, value.Value);
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.GetPlayingStage] ErrorCode = {ErrorCode.GetPlayingStage_Fail_Exception}, AccountId = {accountId}");
                return new Tuple<ErrorCode, Int32>(ErrorCode.GetPlayingStage_Fail_Exception, 0);
            }
        }

        public async Task<ErrorCode> DelPlayingStage(Int64 accountId)
        {
            var key = MemoryDbKeyMaker.MakePlayingInfoKey(accountId);

            try
            {
                var redis = new RedisDictionary<string, Int32>(_redisCon, key, null);
                if (!await redis.DeleteAsync())
                {
                    _logger.ZLogError($"[RedisDb.DelPlayingStage] ErrorCode = {ErrorCode.DelPlayingStage_Fail_Key_Not_Exist}, AccountId = {accountId}");
                    return ErrorCode.DelPlayingStage_Fail_Key_Not_Exist;
                }

                _logger.ZLogDebug($"[RedisDb.DelPlayingStage] Complete, AccountId = {accountId}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.DelPlayingStage] ErrorCode = {ErrorCode.DelPlayingStage_Fail_Exception}, AccountId = {accountId}");
                return ErrorCode.DelPlayingStage_Fail_Exception;
            }
        }

        public async Task<ErrorCode> SetStageFoundItem(Int64 accountId, Int32 itemCode)
        {
            var key = MemoryDbKeyMaker.MakePlayingInfoKey(accountId);
            var itemKey = MemoryDbKeyMaker.MakeStageItemKey(itemCode);

            try
            {
                var redis = new RedisDictionary<string, Int32>(_redisCon, key, null);
                var result = await redis.IncrementAsync(itemKey, 1);

                _logger.ZLogDebug($"[RedisDb.SetStageFoundItem] Complete, AccountId = {accountId}, Result = {result}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.SetStageFoundItem] ErrorCode = {ErrorCode.SetStageFountItem_Fail_Exception}, AccountId = {accountId}");
                return ErrorCode.SetStageFountItem_Fail_Exception;
            }
        }

        public async Task<ErrorCode> SetStageKilledNpc(Int64 accountId, Int32 npcCode)
        {
            var key = MemoryDbKeyMaker.MakePlayingInfoKey(accountId);
            var npcKey = MemoryDbKeyMaker.MakeStageNpcKey(npcCode);

            try
            {
                var redis = new RedisDictionary<string, Int32>(_redisCon, key, null);
                var result = await redis.IncrementAsync(npcKey, 1);

                _logger.ZLogDebug($"[RedisDb.SetStageKilledNpc] Complete, AccountId = {accountId}, Result = {result}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.SetStageKilledNpc] ErrorCode = {ErrorCode.SetStageKilledNpc_Fail_Exception}, AccountId = {accountId}");
                return ErrorCode.SetStageKilledNpc_Fail_Exception;
            }
        }

        public async Task<Tuple<ErrorCode, Int32>> LoadStageKilledNpcNum(Int64 accountId, Int32 npcCode)
        {
            var key = MemoryDbKeyMaker.MakePlayingInfoKey(accountId);
            var npcKey = MemoryDbKeyMaker.MakeStageNpcKey(npcCode);

            try
            {
                var redis = new RedisDictionary<string, Int32>(_redisCon, key, null);
                var npcNum = await redis.GetAsync(npcKey);

                if (!npcNum.HasValue)
                {
                    _logger.ZLogError($"[RedisDb.LoadStageKilledNpcNum] ErrorCode = {ErrorCode.LoadStageKilledNpcNum_Fail_Not_Exist}, AccountId = {accountId}, NpcCode = {npcCode}");
                    return new Tuple<ErrorCode, Int32>(ErrorCode.LoadStageKilledNpcNum_Fail_Not_Exist, 0);
                }

                _logger.ZLogDebug($"[RedisDb.LoadStageKilledNpcNum] Complete, AccountId = {accountId}, NpcCode = {npcCode}");
                return new Tuple<ErrorCode, Int32>(ErrorCode.None, npcNum.Value);
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.LoadStageKilledNpcNum] ErrorCode = {ErrorCode.LoadStageKilledNpcNum_Fail_Exception}, AccountId = {accountId}, NpcCode = {npcCode}");
                return new Tuple<ErrorCode, Int32>(ErrorCode.LoadStageKilledNpcNum_Fail_Exception, 0);
            }
        }

        public async Task<Tuple<ErrorCode, Dictionary<string, Int32>>> LoadPlayingStageInfo(Int64 accountId, Int32 stageNum)
        {
            var key = MemoryDbKeyMaker.MakePlayingInfoKey(accountId);

            try
            {
                var redis = new RedisDictionary<string, Int32>(_redisCon, key, null);
                var stageData = await redis.GetAllAsync();

                if (stageData is null)
                {
                    _logger.ZLogError($"[RedisDb.LoadPlayingStageInfo] ErrorCode = {ErrorCode.LoadPlayingStageInfo_Fail_Not_Exist}, AccountId = {accountId}, StageNum = {stageNum}");
                    return new Tuple<ErrorCode, Dictionary<string, Int32>>(ErrorCode.LoadPlayingStageInfo_Fail_Not_Exist, null);
                }

                _logger.ZLogDebug($"[RedisDb.LoadPlayingStageInfo] Complete, AccountId = {accountId}, StageNum = {stageNum}");
                return new Tuple<ErrorCode, Dictionary<string, Int32>>(ErrorCode.None, stageData);
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.LoadPlayingStageInfo] ErrorCode = {ErrorCode.LoadPlayingStageInfo_Fail_Exception}, AccountId = {accountId}, StageNum = {stageNum}");
                return new Tuple<ErrorCode, Dictionary<string, Int32>>(ErrorCode.LoadPlayingStageInfo_Fail_Exception, null);
            }
        }

        public async Task<Tuple<ErrorCode, Int32>> AllocateChannel()
        {
            var key = MemoryDbKeyMaker.ChannelKey;

            try
            {
                var redis = new RedisDictionary<Int32, Int32>(_redisCon, key, null);
                for(int i =1; i <= Channel.ChannelCount; i++)
                {
                    var channelCnt = await redis.GetAsync(i);

                    if(channelCnt.HasValue)
                    {
                        if(channelCnt.Value >= Channel.MaxCountPerChannel)
                        {
                            continue;
                        }
                    }

                    var result = await redis.IncrementAsync(i, 1);

                    _logger.ZLogInformation($"[RedisDb.AllocateChannel] Complete, Channel = {i}, Result = {result}");
                    return new Tuple<ErrorCode, Int32>(ErrorCode.None, i);
                }

                _logger.ZLogError($"[RedisDb.AllocateChannel] ErrorCode = {ErrorCode.AllocateChannel_Fail_All_Channel_Full}");
                return new Tuple<ErrorCode, Int32>(ErrorCode.AllocateChannel_Fail_All_Channel_Full, 0);
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.AllocateChannel] ErrorCode = {ErrorCode.AllocateChannel_Fail_Exception}");
                return new Tuple<ErrorCode, Int32>(ErrorCode.AllocateChannel_Fail_Exception, 0);
            }
        }

        bool IsInvalidChannel(Int32 channel)
        {
            if (channel <= 0 || channel > Channel.ChannelCount)
            {
                return true;
            }
            return false;
        }

        public async Task<ErrorCode> AllocateChannel(Int32 channel)
        {
            if(IsInvalidChannel(channel))
            {
                _logger.ZLogError($"[RedisDb.AllocateChannel] ErrorCode = {ErrorCode.AllocateChannel_Fail_Invalid_ChannelNum}, Channel = {channel}");
                return ErrorCode.AllocateChannel_Fail_Invalid_ChannelNum;
            }

            var key = MemoryDbKeyMaker.ChannelKey;

            try
            {
                var redis = new RedisDictionary<Int32, Int32>(_redisCon, key, null);

                var channelCnt = await redis.GetAsync(channel);

                if(channelCnt.HasValue)
                {
                    if (channelCnt.Value >= Channel.MaxCountPerChannel)
                    {
                        _logger.ZLogError($"[RedisDb.AllocateChannel] ErrorCode = {ErrorCode.AllocateChannel_Fail_Channel_Full}, Channel = {channel}");
                        return ErrorCode.AllocateChannel_Fail_Channel_Full;
                    }
                }

                await redis.IncrementAsync(channel, 1);

                _logger.ZLogDebug($"[RedisDb.AllocateChannel] Complete, Channel = {channel}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.AllocateChannel] ErrorCode = {ErrorCode.AllocateChannel_Fail_Exception}, Channel = {channel}");
                return ErrorCode.AllocateChannel_Fail_Exception;
            }
        }

        public async Task<ErrorCode> LeaveChannel(Int32 channel)
        {
            if(IsInvalidChannel(channel))
            {
                _logger.ZLogError($"[RedisDb.LeaveChannel] ErrorCode = {ErrorCode.LeaveChannel_Fail_Invalid_Channel}, Channel = {channel}");
                return ErrorCode.LeaveChannel_Fail_Invalid_Channel;
            }

            var key = MemoryDbKeyMaker.ChannelKey;
            
            try
            {
                var redis = new RedisDictionary<Int32, Int32>(_redisCon, key, null);

                var channelCnt = await redis.GetAsync(channel);

                if (!channelCnt.HasValue)
                {
                    _logger.ZLogError($"[RedisDb.LeaveChannel] ErrorCode = {ErrorCode.LeaveChannel_Fail_Channel_Not_Exist}, Channel = {channel}");
                    return ErrorCode.LeaveChannel_Fail_Channel_Not_Exist;
                }
                if(channelCnt.Value <= 0)
                {
                    _logger.ZLogError($"[RedisDb.LeaveChannel] ErrorCode = {ErrorCode.LeaveChannel_Fail_Invalid_Channel}, Channel = {channel}");
                    return ErrorCode.LeaveChannel_Fail_Invalid_Channel;
                }

                await redis.DecrementAsync(channel, 1);

                _logger.ZLogDebug($"[RedisDb.LeaveChannel] Complete, Channel = {channel}");
                return ErrorCode.None;

            }
            catch
            {
                _logger.ZLogError($"[RedisDb.LeaveChannel] ErrorCode = {ErrorCode.LeaveChannel_Fail_Exception}, Channel = {channel}");
                return ErrorCode.LeaveChannel_Fail_Exception;
            }
        }

        public async Task<ErrorCode> EnterChatRoom(Int32 channel, Action<RedisChannel, RedisValue> handler)
        {
            if(IsInvalidChannel(channel))
            {
                _logger.ZLogError($"[RedisDb.EnterChatRoom] ErrorCode = {ErrorCode.EnterChatRoom_Fail_Invalid_Channel}, Channel = {channel}");
                return ErrorCode.EnterChatRoom_Fail_Invalid_Channel;
            }

            var channelKey = MemoryDbKeyMaker.MakeChannelKey(channel);

            try
            {
                await _redisCon.GetConnection().GetSubscriber().SubscribeAsync(channelKey, handler);

                _logger.ZLogDebug($"[RedisDb.EnterChatRoom] Complete, Channel = {channel}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.EnterChatRoom] ErrorCode = {ErrorCode.EnterChatRoom_Fail_Exception}, Channel = {channel}");
                return ErrorCode.EnterChatRoom_Fail_Exception;
            }
        }

        public async Task<ErrorCode> LeaveChatRoom(Int32 channel, Action<RedisChannel, RedisValue> handler)
        {
            if (IsInvalidChannel(channel))
            {
                _logger.ZLogError($"[RedisDb.LeaveChatRoom] ErrorCode = {ErrorCode.LeaveChatRoom_Fail_Invalid_Channel}, Channel = {channel}");
                return ErrorCode.LeaveChatRoom_Fail_Invalid_Channel;
            }

            var channelKey = MemoryDbKeyMaker.MakeChannelKey(channel);

            try
            {
                await _redisCon.GetConnection().GetSubscriber().UnsubscribeAsync(channelKey, handler);

                _logger.ZLogDebug($"[RedisDb.EnterChatRoom] Complete, Channel = {channel}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.EnterChatRoom] ErrorCode = {ErrorCode.EnterChatRoom_Fail_Exception}, Channel = {channel}");
                return ErrorCode.EnterChatRoom_Fail_Exception;
            }
        }

        public async Task<ErrorCode> SendChat(Int32 channel, string message)
        {
            if (IsInvalidChannel(channel))
            {
                _logger.ZLogError($"[RedisDb.SendChat] ErrorCode = {ErrorCode.SendChat_Fail_Invalid_Channel}, Channel = {channel}");
                return ErrorCode.SendChat_Fail_Invalid_Channel;
            }

            var channelKey = MemoryDbKeyMaker.MakeChannelKey(channel);

            try
            {
                await _redisCon.GetConnection().GetSubscriber().PublishAsync(channelKey, message);

                _logger.ZLogDebug($"[RedisDb.SendChat] Complete, Channel = {channel}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[RedisDb.SendChat] ErrorCode = {ErrorCode.SendChat_Fail_Exception}, Channel = {channel}");
                return ErrorCode.SendChat_Fail_Exception;
            }
        }
    }
}
