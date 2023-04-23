namespace TuesberryAPIServer
{
    public class PKLoginRequest
    {
        public string? ID { get; set; }
        public string? PW { get; set; }
    }

    public class PKLoginResponse
    {
        public int Result { get; set; }
    }
}
