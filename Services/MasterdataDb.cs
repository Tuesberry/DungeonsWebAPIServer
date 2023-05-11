using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using TuesberryAPIServer.ModelDb;
using ZLogger;

namespace TuesberryAPIServer.Services
{
    public class MasterdataDb : IMasterDb
    {
        readonly ILogger<MasterdataDb> _logger;
        readonly IOptions<DbConfig> _dbConfig;

        QueryFactory _queryFactory;
        IDbConnection _connection;
        MySqlCompiler _compiler;

        public MasterdataDb(ILogger<MasterdataDb> logger, IOptions<DbConfig> options)
        {
            _logger = logger;
            _dbConfig = options;

            _connection = new MySqlConnection(_dbConfig.Value.GameDb);
            _connection.Open();

            _compiler = new MySqlCompiler();
            _queryFactory = new QueryFactory(_connection, _compiler);
        }

        public void Init()
        {
            // init master data
            try
            {
                // item data
                var itemData = _queryFactory.Query("ItemMasterData").Get<ItemMasterData>();
                foreach (var item in itemData)
                {
                    Items.Add(item.ItemCode, item);
                }
                // item attribute data
                var itemAttrData = _queryFactory.Query("ItemAttributeMasterData").Get<ItemAttributeMasterData>();
                foreach( var itemAttr in itemAttrData)
                {
                    ItemAttributes.Add(itemAttr.Name, itemAttr.ItemCode);
                }
                // attenance master data
                var attendanceData = _queryFactory.Query("AttendanceMasterData").Get<AttendanceMasterData>();
                foreach(var attendance in  attendanceData)
                {
                    AttendanceRewards.Add(attendance.Code, attendance);
                }
                // products
                var productData = _queryFactory.Query("ProductMasterData").Get<ProductMasterData>();
                foreach (var product in productData)
                {
                    if(BundleProducts.ContainsKey(product.Code))
                    {
                        BundleProducts[product.Code].Add(product);
                    }
                    else
                    {
                        BundleProducts.Add(product.Code, new List<ProductMasterData>(new ProductMasterData[] {product}));
                    }
                }
                // stage item
                var stageItemData = _queryFactory.Query("StageItemMasterData").Get<StageItemMasterData>();
                foreach (var stageItem in stageItemData)
                {
                    if(StageItems.ContainsKey(stageItem.Code))
                    {
                        StageItems[stageItem.Code].Add(stageItem.ItemCode);
                    }
                    else
                    {
                        StageItems.Add(stageItem.Code, new List<Int32>(new Int32[] {stageItem.ItemCode}));   
                    }
                }
                // npc data
                var npcData = _queryFactory.Query("StageNpcMasterData").Get<NpcMasterDataDb>();
                foreach (var npc in npcData)
                {
                    if (StageNpc.ContainsKey(npc.Code))
                    {
                        StageNpc[npc.Code].Add(new NpcMasterData {
                            NpcCode = npc.NpcCode,
                            Count = npc.Count,
                            Exp = npc.Exp
                        });
                    }
                    else
                    {
                        StageNpc.Add(npc.Code, new List<NpcMasterData>(new NpcMasterData[] {
                            new NpcMasterData {
                                NpcCode = npc.NpcCode,
                                Count = npc.Count,
                                Exp = npc.Exp
                        }}));  
                    }
                }
                // level up exp
                var levelUpExpData = _queryFactory.Query("LevelUpExpMasterData").Get<LevelUpExpData>();
                foreach(var levelUpExp in levelUpExpData)
                {
                    LevelUpExp.Add(levelUpExp.Level, levelUpExp.Exp);
                }

                _logger.ZLogDebug($"[MasterdataDb.Init] Init Master Data Complete");
                _connection.Close();
            }
            catch
            {
                _logger.ZLogError("[MasterdataDb.Init] Init Master Data Error");
                _connection.Close();
            }
        }

        public string MasterDataVersion { get; } = "1.0";

        public string AppVersion { get; } = "1.0";

        public string MailboxTitle { get; } = "우편함";

        public string MailboxComment { get; } = "선물은 7일 동안 보관됩니다";

        public Dictionary<Int32, ItemMasterData> Items { get; } = new Dictionary<Int32, ItemMasterData>();

        public Dictionary<string, Int32> ItemAttributes { get; } = new Dictionary<string, Int32>();

        public Dictionary<Int32, AttendanceMasterData> AttendanceRewards { get; } = new Dictionary<Int32, AttendanceMasterData>();

        public Dictionary<Int32, List<ProductMasterData>> BundleProducts { get; } = new Dictionary<Int32, List<ProductMasterData>>();

        public Dictionary<Int32, List<Int32>> StageItems { get; } = new Dictionary<Int32, List<Int32>>();

        public Dictionary<Int32, List<NpcMasterData>> StageNpc { get; } = new Dictionary<Int32, List<NpcMasterData>>();

        public Dictionary<Int32, Int32> LevelUpExp { get; } = new Dictionary<Int32, Int32>();

    }
}
