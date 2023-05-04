namespace TuesberryAPIServer.ModelDb
{
    public class MailboxDataDb
    {
        public Int32 MailId { get; set; } = 0;
        public string Title { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime ExpiryDate { get; set; } = DateTime.MinValue;
        public Int32 ItemCode { get; set; } = 0;
        public Int32 Amount { get; set; } = 0;
        public Int32 EnchanceCount { get; set; } = 0;
        public Int32 Attack { get; set; } = 0;
        public Int32 Defence { get; set; } = 0;
        public Int32 Magic { get; set; } = 0;
    }

    public class MailboxData
    {
        public Int32 MailId { get; set; } = 0;
        public string Title { get; set; } = string.Empty;
        public bool IsRead { get; set; } = false;
        public DateTime ExpiryDate { get; set; } = DateTime.MinValue;
        public List<MailboxItemData> MailboxItemData { get; set; } = new List<MailboxItemData>();
    }

    public class MailboxItemData
    {
        public Int32 ItemCode { get; set; } = 0;
        public Int32 Amount { get; set; } = 0;
        public Int32 EnchanceCount { get; set; } = 0;
        public Int32 Attack { get; set; } = 0;
        public Int32 Defence { get; set; } = 0;
        public Int32 Magic { get; set; } = 0;
    }
}
