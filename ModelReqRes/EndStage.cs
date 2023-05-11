using System.ComponentModel.DataAnnotations;
using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.ModelReqRes
{
    public class PkEndStageRequest
    {
        [Required(ErrorMessage = "Required")]
        public Int32 StageNum { get; set; } = 0;

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

    public class PkEndStageResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;

        public bool IsClear { get; set; } = true;

        public Int32 Level { get; set; } = 0;

        public Int32 Exp { get; set; } = 0;

        public List<ItemData> Items { get; set; } = new List<ItemData>();
    }
}
