using Microsoft.AspNetCore.Mvc;
using SqlKata.Execution;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateAccountController : ControllerBase
    {
        [HttpPost]
        public async Task<PkCreateAccountResponse> Post([FromBody]PkCreateAccountRequest request)
        {
            var response = new PkCreateAccountResponse { Result = ErrorCode.None };

            var saltValue = Security.SaltString();
            var hashingPassword = Security.MakeHashingPassWord(saltValue, request.Password);

            using(var db = await DBManager.GetDBQuery())
            {
                try
                {
                    var count = await db.Query("account").InsertAsync(new
                    {
                        Email = request.Email,
                        SaltValue = saltValue,
                        HashedPassword = hashingPassword
                    });

                    if (count != 1)
                    {
                        response.Result = ErrorCode.Create_Account_Fail_Duplicate;
                    }

                    Console.WriteLine($"[Request CreateAccount] Email:{request.Email}, saltValue:{saltValue}, hashingPassword:{hashingPassword}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    response.Result = ErrorCode.Create_Account_Fail_Exception;
                    return response;
                }
                finally
                {
                    db.Dispose();
                }
            }

            return response;
        }
    }
}
