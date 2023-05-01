using System.ComponentModel.DataAnnotations;
using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.ModelReqRes
{
    public class PkOpenMailRequest
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

    public class PkOpenMailResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;

        public string MailboxTitle { get; set; } = string.Empty;

        public string MailboxComment { get; set; } = string.Empty;

        public List<MailboxData> MailboxDatum { get; set; } = new List<MailboxData>();
    }

    public class PkLoadMailRequest
    {
        [Required(ErrorMessage = "Required")]
        public Int32 PageNum { get; set; } = 0;

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

    public class PkLoadMailResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;

        public string MailboxTitle { get; set; } = string.Empty;

        public string MailboxComment { get; set; } = string.Empty;

        public List<MailboxData> MailboxDatum { get; set; } = new List<MailboxData>();
    }

    public class PkGetMailItemRequest
    {
        [Required(ErrorMessage = "Required")]
        public Int32 MailId { get; set; } = 0;

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

    public class PkGetMailItemResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
    }
}
