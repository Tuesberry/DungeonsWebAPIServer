using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Services
{
    public interface IGameDb : IDisposable
    {
        // --------- Game Data --------- //
        public Task<Tuple<ErrorCode, Int64>> CreateGameData(string userId);
        
        public Task<Tuple<ErrorCode, GameData, Int64>> LoadGameData(string userId);

        public Task<ErrorCode> UpdateMoney(Int64 accountId, Int32 amount);

        public Task<ErrorCode> DeleteGameData(string userId);

        // --------- Item Data --------- //
        public Task<ErrorCode> CreateDefaultItemData(Int64 accountId);  

        public Task<ErrorCode> InsertOrUpdateItem(Int64 accountId, ItemData itemData);

        public Task<Tuple<ErrorCode, List<ItemData>>> LoadItemData(Int64 accountId);

        //public Task<ErrorCode> DeleteOrUpdateItemData(Int64 accountId, ItemData itemData);

        // --------- Mailbox --------- //

        public Task<Tuple<ErrorCode, List<MailboxData>>> LoadMailList(Int64 accountId, Int32 page);

        public Task<Tuple<ErrorCode, string>> LoadMailDetail(Int64 accountId, Int32 mailId);

        public Task<Tuple<ErrorCode, List<MailboxItemData>>> LoadMailItemList(Int32 mailId);

        public Task<ErrorCode> ReceiveMailItem(Int64 accountId, Int32 mailId);

        public Task<ErrorCode> DeleteMail(Int64 accountId, Int32 mailId);

        public Task<bool> IsValidMailId(Int64 accountId, Int32 mailId);
        
        public Task<ErrorCode> InsertMail(Int64 account, List<MailboxData> itemData);

    }
}
