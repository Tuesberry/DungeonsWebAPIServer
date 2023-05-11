using System.ComponentModel.DataAnnotations;

namespace TuesberryAPIServer.ModelReqRes
{
    public class PkCheckAttendanceRequest
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

    public class PkCheckAttendanceResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;

        public Int32 ContinuousPeriod { get; set; } = 0;
    }
}
