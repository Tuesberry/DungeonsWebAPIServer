namespace TuesberryAPIServer
{
    public enum ErrorCode : int
    {
        None  = 0,

        Create_Account_Fail_Duplicate = 11,
        Create_Account_Fail_Exception = 12,

        Login_Fail_Exception = 16,
        Login_Fail_Pw_Not_Match = 17,
        Login_Fail_Add_Redis = 18,
        Login_Fail_User_Not_Exist = 19,
        Login_Fail_Set_Auth_Token = 20,

        Check_Auth_Fail_Not_Exist = 21,
        Check_Auth_Fail_Not_Match = 22,
        Check_Auth_Fail_Exception = 23,
    }
}
