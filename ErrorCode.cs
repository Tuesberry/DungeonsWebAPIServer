namespace TuesberryAPIServer
{
    public enum ErrorCode : int
    {
        None  = 0,

        Invalid_Request_Http_Body = 1,
        Invalid_Version_Fail_Wrong_Keyword = 2,
        Invalid_AppVersion = 3,
        Invalid_MasterDataVersion = 4,
        Invalid_AppVersion_And_MasterDataVersion = 5,

        AuthToken_Fail_Wrong_keyword = 6,
        AuthToken_Fail_Wrong_AuthToken = 7,
        AuthToken_Fail_SetNx = 8,
        AuthToken_Access_Error = 9,

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

        Create_GameData_Fail_Exception = 24,
        Create_GameData_Fail_Duplicate = 25,

        Get_GameDate_Fail_Not_Exist = 26,
        Get_GameData_Fail_Exception = 27,

        Create_Item_Data_Fail_Duplicate= 28,
        Create_Item_Data_Fail_Exception= 29,

        Insert_Item_Data_Fail_Duplicate = 30,
        Insert_Item_Data_Fail_Exception = 31,

        InsertOrUpdate_Item_Data_Fail_Duplicate = 32,
        InsertOrUpdate__Item_Data_Fail_Exception = 33,

        Get_ItemData_Fail_Exception = 34,

        Get_Notice_Fail_Exception = 35,

        Logout_Fail_Id_Not_Exist = 36,
        Logout_Fail_Exception = 37,
    }
}
