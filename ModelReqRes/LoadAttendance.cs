using System.ComponentModel.DataAnnotations;

namespace TuesberryAPIServer.ModelReqRes
{
    public class PkLoadAttendanceRequest
    {
        [Required(ErrorMessage = "Required")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Id length is not appropriate")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        public string AuthToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        public string AppVersion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        public string MasterDataVersion { get; set; } = string.Empty;
    }

    public class PkLoadAttendanceResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;

        public DateTime LastCheckDate { get; set; } = DateTime.MinValue;

        public Int32 ContinuousPeriod { get; set; } = 0;
    }
}
