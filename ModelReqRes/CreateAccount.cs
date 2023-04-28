using System.ComponentModel.DataAnnotations;

namespace TuesberryAPIServer.ModelReqRes
{
    public class PkCreateAccountRequest
    {
        [Required(ErrorMessage = "Required")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Id length is not appropriate")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Pw length is not appropriate")]
        public string Pw { get; set; } = string.Empty;
    }

    public class PkCreateAccountResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
    }
}
