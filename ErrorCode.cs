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

        Load_GameDate_Fail_Not_Exist = 26,
        Load_GameData_Fail_Exception = 27,

        Create_Item_Data_Fail_Duplicate= 28,
        Create_Item_Data_Fail_Exception= 29,

        Insert_Item_Data_Fail_Duplicate = 30,
        Insert_Item_Data_Fail_Exception = 31,

        InsertOrUpdate_Item_Data_Fail_Duplicate = 32,
        InsertOrUpdate_Item_Data_Fail_Exception = 33,

        Load_ItemData_Fail_Exception = 34,

        Get_Notice_Fail_Exception = 35,

        Logout_Fail_Id_Not_Exist = 36,
        Logout_Fail_Exception = 37,

        SetPageRead_Fail_Exception = 38,
        SetPageRead_Fail_Duplicate = 39,
        ReadPage_Fail_Exception = 40,
        DelPageReadInfo_Fail_Exception = 41,

        OpenMail_Fail_Exception = 42,
        OpenMail_Fail_Request_Duplicate = 43,

        LoadMailList_Fail_Exception = 44,
        LoadMailList_Fail_Inappropriate_Page_Range = 45,
        LoadMailList_Fail_Not_Exist_In_This_Page = 46,

        LoadMail_Fail_Exception = 47,
        LoadMail_Fail_Request_Duplicate = 48,

        LoadMailItem_Fail_Exception = 49,
        LoadMailItem_Fail_Item_Not_Exist = 50,

        ReceiveMailItem_Fail_Item_Not_Exist = 51,
        ReceiveMailItem_Fail_Exception = 52,
        ReceiveMailItem_Fail_Inappropriate_MailId = 53,
        ReceiveMailItem_Fail_Add_Item_Exception = 54,
        ReceiveMailItem_Fail_Set_Read_Exception = 55,

        DeleteMail_Fail_Not_Exist = 56,
        DeleteMail_Fail_Exception = 57,
        DeleteMail_Fail_Inappropriate_MailId = 58,

        DeleteGameData_Fail_Exception = 59,
        DeleteAccountData_Fail_Exception = 60,

        UpdateMoney_Fail_Exception = 61,

        LoadMailDetail_Fail_Not_Exist = 62,
        LoadMailDetail_Fail_Exception = 63,
    }
}
