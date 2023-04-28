using System.ComponentModel.DataAnnotations;
using TuesberryAPIServer.ModelDb;

namespace TuesberryAPIServer.ModelReqRes
{
    public class PKLoginRequest
    {
        [Required(ErrorMessage = "Required")]
        [StringLength(15, MinimumLength = 4, ErrorMessage = "Id length is not appropriate")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Required")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Pw length is not appropriate")]
        public string Pw { get; set; } = string.Empty;
    }

    public class PKLoginResponse
    {
        public ErrorCode Result { get; set; } = ErrorCode.None;
        
        public string Authtoken { get; set; } = string.Empty;

        public GameData GameData { get; set; } = new();

        public List<ItemData> ItemDatum { get; set; } = new();
    }


}
