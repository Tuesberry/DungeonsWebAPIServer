using System.ComponentModel.DataAnnotations;
using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.ModelReqRes
{
    public class PkOpenMailboxRequest
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

    public class PkOpenMailboxResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;

        public string MailboxTitle { get; set; } = string.Empty;

        public string MailboxComment { get; set; } = string.Empty;

        public List<MailboxData> MailboxDatum { get; set; } = new List<MailboxData>();
    } 
}
