using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public async Task<PKLoginResponse> Post([FromBody]PKLoginRequest request)
        {
            Console.WriteLine($"[Request Login] ID: {request.ID}, PW: {request.PW}");
            var response = new PKLoginResponse();

            return response;
        }
    }
}
