namespace TuesberryAPIServer
{
    public class Item
    {
        public string _name = string.Empty;
        public Int32 _attribute;
        public Int32 _sell;
        public Int32 _buy;
        public Int32 _useLv;
        public Int32 _attack;
        public Int32 _defence;
        public Int32 _magic;
        public Int32 _enchanceMaxCount;
        public bool _bOverlapped;

        public Item(string name, Int32 attribute, Int32 sell, Int32 buy, Int32 useLv, Int32 attack, Int32 defence, Int32 magic, Int32 enchanceMaxCount, bool bOverlapped)
        {
            _name = name;
            _attribute = attribute;
            _sell = sell;
            _buy = buy;
            _useLv = useLv;
            _attack = attack;
            _defence = defence;
            _magic = magic;
            _enchanceMaxCount = enchanceMaxCount;
            _bOverlapped = bOverlapped;
        }
    }

    public class Reward
    {
        public Int32 _itemCode;
        public Int32 _count;

        public Reward(Int32 itemCode, Int32 count)
        {
            _itemCode = itemCode;
            _count = count;
        }
    }

    public class Product
    {
        public Int32 _itemCode;
        public string _itemName = string.Empty;
        public Int32 _itemCount;

        public Product(int itemCode, string itemName, int itemCount)
        {
            _itemCode = itemCode;
            _itemName = itemName;
            _itemCount = itemCount;
        }
    }

    public class NpcInfo
    {
        public Int32 _npcCode;
        public Int32 _count;
        public Int32 _exp;

        public NpcInfo(int npcCode, int count, int exp)
        {
            _npcCode = npcCode;
            _count = count;
            _exp = exp;
        }
    }

    public static class MasterData
    {
        public static string MasterDataVersion { get; } = "1.0";

        public static string AppVersion { get; } = "1.0";

        // 아이템( Code, (Name, Attribute, Sell, Buy, UseLv, Attack, Defence, Magic, EnchanceCount, bOverlapped))
        public static Dictionary<Int32, Item> Items { get; } = new Dictionary<Int32, Item>()
        {
            { 1, new Item("돈", 5, 0, 0, 0, 0, 0, 0, 0, true ) },
            { 2, new Item("작은 칼", 1, 10, 20, 1, 10, 5, 1, 10, false) },
            { 3, new Item("도금 칼", 1, 100, 200, 5, 29, 12, 10, 10, false) },
            { 4, new Item("나무 방패", 2, 7, 15, 1, 3, 10, 1, 10, false) },
            { 5, new Item("보통 모자", 3, 5, 8, 1, 1, 1, 1, 10, false)},
            { 6, new Item("포션", 4, 3, 6, 1, 0, 0, 0, 0, true) }
        };

        // 아이템 특성( Code, Name )
        public static Dictionary<Int32, string> ItemAttributes { get; } = new Dictionary<Int32, string>()
        {
            { 1, "무기" },
            { 2, "방어구" },
            { 3, "복장" },
            { 4, "마법도구" },
            { 5, "돈" }
        };

        // 출석부 보상( Code, ( ItemCode, Count ))
        public static Dictionary<Int32, Reward> AttendanceRewards { get; } = new Dictionary<Int32, Reward>()
        {
            { 1, new Reward(1, 100) },
            { 2, new Reward(1, 100) },
            { 3, new Reward(1, 100) },
            { 4, new Reward(1, 200) },
            { 5, new Reward(1, 200) },
            { 6, new Reward(1, 200) },
            { 7, new Reward(2, 1) },
            { 8, new Reward(1, 100) },
            { 9, new Reward(1, 100) },
            { 10, new Reward(1, 100) },
            { 11, new Reward(6, 5) },
            { 12, new Reward(1, 150) },
            { 13, new Reward(1, 150) },
            { 14, new Reward(1, 150) },
            { 15, new Reward(1, 150) },
            { 16, new Reward(1, 150) },
            { 17, new Reward(1, 150) },
            { 18, new Reward(4, 1) },
            { 19, new Reward(1, 200) },
            { 20, new Reward(1, 200) },
            { 21, new Reward(1, 200) },
            { 22, new Reward(1, 200) },
            { 23, new Reward(1, 200) },
            { 24, new Reward(5, 1) },
            { 25, new Reward(1, 250) },
            { 26, new Reward(1, 250) },
            { 27, new Reward(1, 250) },
            { 28, new Reward(1, 250) },
            { 29, new Reward(1, 250) },
            { 30, new Reward(3, 1) },
        };

        // 인앱상품(Code, (ItemCode, ItemName, ItemCount))
        public static Dictionary<Int32, List<Product>> BundleProducts { get; } = new Dictionary<int, List<Product>>()
        {
            {1, new List<Product>(
                new Product[]{
                    new Product(1, "돈", 1000),
                    new Product(2, "작은 칼",1),
                    new Product(3, "도금 칼",1)
                })
            },
            {2, new List<Product>(
                new Product[]{
                    new Product(4, "나무 방패",1),
                    new Product(5, "보통 모자",1),
                    new Product(6, "포션",10)
                })
            },
            {3, new List<Product>(
                new Product[]{
                    new Product(1, "돈", 2000),
                    new Product(2, "작은 칼",1),
                    new Product(3, "나무 방패",1),
                    new Product(5, "보통 모자",1)
                })
            }
        };

        // 스테이지 아이템(Code, ItemCodes)
        public static Dictionary<Int32, List<Int32>> StageItems { get; } = new Dictionary<Int32, List<Int32>>()
        {
            { 1, new List<Int32>( new Int32[]{1,2})},
            { 2, new List<Int32>( new Int32[]{3,3})}
        };

        // 스테이지 공격 NPC(Code, (NpcCode, Count, Exp))
        public static Dictionary<Int32, List<NpcInfo>> NpcItems { get; } = new Dictionary<int, List<NpcInfo>>()
        {
            {1, new List<NpcInfo>(
                new NpcInfo[]{ 
                    new NpcInfo(101, 10,10), 
                    new NpcInfo(110, 12,15)}
                )
            },
            {2, new List<NpcInfo>(
                new NpcInfo[]{ 
                    new NpcInfo(201, 40,20),
                    new NpcInfo(211, 20,35), 
                    new NpcInfo(221, 1,50)}
                )
            }
        };
    }
}
