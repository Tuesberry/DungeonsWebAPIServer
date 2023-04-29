namespace TuesberryAPIServer.ModelDb
{
    public class MailboxData
    {
        public Int32 MailId { get; set; } = 0;
        public string Title { get; set; } = string.Empty;   
        public Int32 ItemCode { get; set; } = 0;    
        public Int32 Amount { get; set; } = 0; 
        public bool IsRead { get; set; } = false;
    }
}
