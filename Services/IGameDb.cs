using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Services
{
    public interface IGameDb : IDisposable
    {
        public Task<ErrorCode> CreateGameData(Int64 accountId);
        
        public Task<Tuple<ErrorCode, GameData>> GetGameData(Int64 accountId);
        
        //public Task<ErrorCode> InsertCharacterItem(Int64 accountId, ItemData itemData);

        public Task<Tuple<ErrorCode, IEnumerable<ItemData>>> GetItemData(Int64 accountId);
    }
}
