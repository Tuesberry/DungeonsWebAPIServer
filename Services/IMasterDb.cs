using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.Services
{
    public interface IMasterDb
    {
        public void Init();

        public string MasterDataVersion { get; set; }

        public string AppVersion { get; set; }

        public string MailboxTitle { get; set; }

        public string MailboxComment { get; set; }
        
        public Dictionary<Int32, ItemMasterData> Items { get; set; }
        
        public Dictionary<Int32, string> ItemAttributes { get; set; }

        public Dictionary<Int32, AttendanceMasterData> AttendanceRewards { get; set; }

        public Dictionary<Int32, List<ProductMasterData>> BundleProducts { get; set; }

        public Dictionary<Int32, List<Int32>> StageItems { get; set; }

        public Dictionary<Int32, List<NpcMasterData>>  Npc { get; set; }
    }
}
