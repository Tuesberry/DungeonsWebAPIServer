namespace TuesberryAPIServer.ModelDb
{
    public class AttendanceData
    {
        public DateTime LastCheckDate { get; set; } = DateTime.MinValue;
        public Int32 ContinuousPeriod { get; set; } = 0;
    }
}
