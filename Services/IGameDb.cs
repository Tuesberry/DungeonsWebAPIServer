using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Services
{
    public interface IGameDb : IDisposable
    {
        public Task<Tuple<ErrorCode, Int64>> CreateGameData(string userId);
        
        public Task<Tuple<ErrorCode, GameData, Int64>> LoadGameData(string userId);
        
        public Task<ErrorCode> CreateDefaultItemData(Int64 accountId);  

        public Task<ErrorCode> InsertOrUpdateItem(Int64 accountId, ItemData itemData);

        public Task<Tuple<ErrorCode, List<ItemData>>> LoadItemData(Int64 accountId);

        public Task<Tuple<ErrorCode, List<MailboxData>>> LoadMailboxData(Int64 accountId, Int32 page);

        public Task<Tuple<ErrorCode, ItemData>> LoadMailItemData(Int64 accountId, Int32 mailId);

    }
}
