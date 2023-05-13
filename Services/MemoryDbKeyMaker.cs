using System.Text;

namespace TuesberryAPIServer.Services
{
    public class MemoryDbKeyMaker
    {
        const string loginUID = "UID_";
        const string userLockKey = "ULock_";

        const string playingInfoKey = "UPlayingInfo_";

        const string itemKey = "UItem_";
        const string npcKey = "UNpc_";

        const string channelKey = "Channel_";

        public static string StageKey { get; } = "Stage";

        public static string ChannelKey { get; } = "Channel";

        public static string MakeUIDKey(string id)
        {
            return loginUID + id;
        }

        public static string MakeUserLockKey(string id) 
        {  
            return userLockKey + id; 
        }

        public static string MakePlayingInfoKey(Int64 accountId)
        {
            return playingInfoKey + accountId.ToString();
        }

        public static string MakeStageItemKey(Int32 itemCode)
        {
            return itemKey + itemCode.ToString();
        }

        public static string MakeStageNpcKey(Int32 npcCode)
        {
            return npcKey + npcCode.ToString();
        }

        public static string MakeChannelKey(Int32 channel)
        {
            return ChannelKey + channel.ToString();
        }
    }
}
