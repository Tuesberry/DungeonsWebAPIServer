using StackExchange.Redis;
using System.Net.WebSockets;
using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Services
{
    public interface IMemoryDb 
    {
        public void Init(string address);

        // --------- Auth --------- //

        public Task<ErrorCode> RegistUserAsync(string id, string authToken, Int64 accountId);
        
        public Task<ErrorCode> CheckUserAuthAsync(string id, string authToken);

        public Task<(bool, AuthUser)> GetUserAsync(string id);

        public Task<ErrorCode> DelUserAsync(string id);

        // --------- Req & Res Lock --------- //

        public Task<bool> SetUserReqLockAsync(string key);

        public Task<bool> DelUserReqLockAsync(string key);

        // --------- Notice --------- //
        public Task<Tuple<ErrorCode, string>> GetNotice();

        // --------- GameStage --------- //

        public Task<ErrorCode> SetPlayingStage(Int64 accountId, Int32 stageNum);

        public Task<Tuple<ErrorCode, Int32>> GetPlayingStage(Int64 accountId);

        public Task<ErrorCode> DelPlayingStage(Int64 accountId);

        public Task<ErrorCode> SetStageFoundItem(Int64 accountId, Int32 itemCode);

        public Task<ErrorCode> SetStageKilledNpc(Int64 accountId, Int32 npcCode);

        public Task<Tuple<ErrorCode, Int32>> LoadStageKilledNpcNum(Int64 accountId, Int32 npcCode);

        public Task<Tuple<ErrorCode, Dictionary<string, Int32>>> LoadPlayingStageInfo(Int64 accountId, Int32 stageNum);

        // --------- Channel --------- //

        public Task<Tuple<ErrorCode, Int32>> AllocateChannel();

        public Task<ErrorCode> AllocateChannel(Int32 channel);

        public Task<ErrorCode> LeaveChannel(Int32 channel);

        // --------- Chat --------- //

        public Task<ErrorCode> EnterChatRoom(Int32 channel, Action<RedisChannel, RedisValue> handler);

        public Task<ErrorCode> LeaveChatRoom(Int32 channel, Action<RedisChannel, RedisValue> handler);

        public Task<ErrorCode> SendChat(Int32 channel, string message);
    }
}
