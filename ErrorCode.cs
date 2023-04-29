namespace TuesberryAPIServer
{
    public enum ErrorCode : int
    {
        None  = 0,

        Version_Fail = 1,

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
    }
}
