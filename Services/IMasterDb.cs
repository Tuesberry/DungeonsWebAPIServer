using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Services
{
    public interface IMasterDb
    {
        public void Init();

        public string MasterDataVersion { get; }

        public string AppVersion { get; }

        public string MailboxTitle { get; }

        public string MailboxComment { get; }
        
        public Dictionary<Int32, ItemMasterData> Items { get; }
        
        public Dictionary<string, Int32> ItemAttributes { get; }

        public Dictionary<Int32, AttendanceMasterData> AttendanceRewards { get; }

        public Dictionary<Int32, List<ProductMasterData>> BundleProducts { get; }

        public Dictionary<Int32, List<Int32>> StageItems { get; }

        public Dictionary<Int32, List<NpcMasterData>> StageNpc { get; }

        public Dictionary<Int32, Int32> LevelUpExp { get; }

        public Dictionary<Int32, Int32> DefaultItem { get; }

    }
}
