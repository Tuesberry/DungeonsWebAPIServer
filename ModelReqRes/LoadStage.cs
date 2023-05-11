using System.ComponentModel.DataAnnotations;

namespace TuesberryAPIServer.ModelReqRes
{
    public class PkLoadStageRequest
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

    public class PkLoadStageResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;

        public Int32 FinalCompletedStageNum { get; set; } = 0;
    }
}
