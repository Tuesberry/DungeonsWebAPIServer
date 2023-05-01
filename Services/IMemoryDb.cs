using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Services
{
    public interface IMemoryDb 
    {
        public void Init(string address);

        public Task<ErrorCode> RegistUserAsync(string id, string authToken, Int64 accountId);
        
        public Task<ErrorCode> CheckUserAuthAsync(string id, string authToken);

        public Task<(bool, AuthUser)> GetUserAsync(string id);

        public Task<ErrorCode> DelUserAsync(string id); 

        public Task<bool> SetUserReqLockAsync(string key);

        public Task<bool> DelUserReqLockAsync(string key);
        
        // 공지 가져오기
        public Task<Tuple<ErrorCode, string>> GetNotice();

        // 페이지 번호 기록하기
        public Task<ErrorCode> SetPageRead(Int64 accountId, Int32 page);

        // 읽은 페이지인지 확인
        public Task<Tuple<ErrorCode, bool>> IsReadPage(Int64 accountId, Int32 page);

        // 읽은 페이지 정보 모두 삭제
        public Task<bool> DelPageReadInfo(Int64 accountId);
    }
}
