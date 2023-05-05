using System.ComponentModel.DataAnnotations;

namespace TuesberryAPIServer.ModelReqRes
{
    public class PkPaymentRequest
    {
        [Required(ErrorMessage = "Required")]
        [StringLength(20, MinimumLength = 10, ErrorMessage = "OrderNumber length is not appropriate")]
        public string OrderNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        public DateTime PurchaseDate { get; set; } = DateTime.MinValue;

        [Required(ErrorMessage = "Required")]
        public Int32 ProductCode { get; set; } = 0;

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

    public class PkPaymentResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
    }

}
