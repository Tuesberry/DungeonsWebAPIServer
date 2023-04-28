using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Services
{
    public interface IMemoryDb 
    {
        public void Init(string address);

        public Task<ErrorCode> RegistUserAsync(string id, string authToken, Int64 accountId);
        
        public Task<ErrorCode> CheckUserAuthAsync(string id, string authToken);

        public Task<(bool, AuthUser)> GetUserAsync(string id);

        public Task<Tuple<ErrorCode, string>> GetNotice();
    }
}
