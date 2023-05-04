namespace TuesberryAPIServer.ModelDb
{
    public class GameData
    {
        public Int32 Level { get; set; } = 0;
        public Int32 Exp { get; set; } = 0;
        public Int32 Hp { get; set; } = 0;
        public Int32 Ap { get; set; } = 0;
        public Int32 Mp { get; set; } = 0;
        public Int32 Money { get; set; } = 0;
    }

    public class ItemData
    {
        public Int32 UserItemId { get; set; } = 0;
        public Int32 ItemCode { get; set; } = 0;
        public Int32 Amount { get; set; } = 0;   
        public Int32 EnchanceCount { get; set; } = 0;
        public Int32 Attack { get; set; } = 0;
        public Int32 Defence { get; set; } = 0;
        public Int32 Magic { get; set; } = 0;
    }

}
