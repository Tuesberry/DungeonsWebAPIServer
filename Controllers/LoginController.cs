using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        [HttpPost]
        public async Task<PKLoginResponse> Post([FromBody]PKLoginRequest request)
        {
            var response = new PKLoginResponse();
            response.Result = ErrorCode.None;
            
            using(var db = await DBManager.GetDBQuery())
            {
                var userInfo = await db.Query("account").Where("Email", request.Email).FirstOrDefaultAsync<DBUserInfo>();
            
                if(userInfo == null || string.IsNullOrEmpty(userInfo.HashedPassword))
                {
                    response.Result = ErrorCode.Login_Fail_NotUser;
                    return response;
                }

                var hashingPassword = Security.MakeHashingPassWord(userInfo.SaltValue, request.Password);
                if(userInfo.HashedPassword != hashingPassword) 
                {
                    response.Result = ErrorCode.Login_Fail_PW;
                    return response;
                }

                db.Dispose();
            }
            
            return response;
        }
    }
}
