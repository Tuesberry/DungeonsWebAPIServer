namespace TuesberryAPIServer
{
    public enum ErrorCode : Int32
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

        Rollback_Delete_Account_Fail = 13,
        Rollback_Delete_GameData_Fail = 14,

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
        Load_ItemData_Fail_Not_Exist = 35,

        Get_Notice_Fail_Exception = 36,

        Logout_Fail_Id_Not_Exist = 37,
        Logout_Fail_Exception = 38,

        SetPageRead_Fail_Exception = 39,
        SetPageRead_Fail_Duplicate = 40,
        ReadPage_Fail_Exception = 41,
        DelPageReadInfo_Fail_Exception = 42,

        LoadMailList_Fail_Exception = 44,
        LoadMailList_Fail_Inappropriate_Page_Range = 45,
        LoadMailList_Fail_Not_Exist_In_This_Page = 46,

        LoadMailItem_Fail_Exception = 49,
        LoadMailItem_Fail_Item_Not_Exist = 50,

        ReceiveMailItem_Fail_Item_Not_Exist = 51,
        ReceiveMailItem_Fail_Exception = 52,
        ReceiveMailItem_Fail_Invalid_MailId = 53,
        ReceiveMailItem_Fail_Add_Item_Exception = 54,
        ReceiveMailItem_Fail_Set_Read_Exception = 55,

        DeleteMail_Fail_Not_Exist = 56,
        DeleteMail_Fail_Exception = 57,
        DeleteMail_Fail_Invalid_MailId = 58,

        DeleteGameData_Fail_Exception = 59,
        DeleteAccountData_Fail_Exception = 60,

        UpdateMoney_Fail_Exception = 61,
        UpdateMoney_Invalid_Amount = 62,

        LoadMailDetail_Fail_Not_Exist = 63,
        LoadMailDetail_Fail_Exception = 64,
        LoadMailDetail_Fail_Invalid_MailId = 65,

        InsertMail_Fail_Exception = 66,
        InsertMail_Fail_Mailbox_Insert_Fail = 67,
        InsertMail_Fail_MailboxItem_Insert_Fail = 68,

        CreateAttendanceData_Fail_Duplicate = 69,
        CreateAttendanceData_Fail_Exception = 70,

        AttendanceCheck_Fail_AccountId_Not_Exist = 71,
        AttendanceCheck_Fail_Exception = 72,

        Attendance_Fail_Already_Check = 73,

        UpdateAttendanceDate_Fail_Exceiption = 74,
        UpdateAttendanceData_Fail_Duplicate = 75,

        Payment_Fail_Duplicate_Data = 76,

        IsDuplicatePayment_Fail_Exception = 77,

        InsertPaymentData_Fail_Exception = 78,
        InsertPaymentData_Fail_Duplicate = 79,

        Enchance_Fail_Enchance_Count_Exceed = 80,

        UpdateItemData_Fail_Exception = 81,
        UpdateItemData_Fail_Item_Not_Exist = 82,

        DeleteItemData_Fail_Exception = 83,
        DeleteItemData_Fail_Invalid_Data = 84,

        InsertItems_Fail_Exception = 85,

        DeleteItemDatum_Fail_Exception = 86,
        DeleteItemDatum_Fail_Not_Complete = 87,

        UpdateItemAmount_Fail_Exception = 88,
        UpdateItemAmount_Fail_Not_Exist = 89,
        UpdateItemAmount_Fail_Duplicate_Update = 90,
        UpdateItemAmount_Fail_Invalid_Amount = 91,

        DeleteOrUpdateItem_Fail_Exception = 92,

        SelectStage_Fail_Invalid_SelectedStageNum = 93,

        SetPlayingStage_Fail_Key_Duplicate = 94,
        SetPlayingStage_Fail_Exception = 95,

        GetPlayingStage_Fail_Key_Not_Exist = 96,
        GetPlayingStage_Fail_Exception = 97,

        DelPlayingStage_Fail_Key_Not_Exist = 98,
        DelPlayingStage_Fail_Exception = 99,

        SaveFoundItem_Fail_Invalid_StageNum = 100,
        SaveFoundItem_Fail_Invalid_ItemCode = 101,

        SaveKilledNpc_Fail_Invalid_StageNum = 102,
        SaveKilledNpc_Fail_Invalid_ItemCode = 103,
        SaveKilledNpc_Fail_Exceed_Number_Can_Appear = 104,

        LoadLevelAndExp_Fail_Not_Exist = 105,
        LoadLevelAndExp_Fail_Exception = 106,

        UpdateLevelAndExp_Fail_AccountId_Not_Exist = 107,
        UpdateLevelAndExp_Fail_Exception = 108,

        LoadFinalCompletedStageNum_Fail_Exception = 109,

        LoadPlayingStageInfo_Fail_Not_Exist = 110,
        LoadPlayingStageInfo_Fail_Exception = 111,

        SetStageFountItem_Fail_Exception = 112,
        SetStageKilledNpc_Fail_Exception = 113,

        SetMailRead_Fail_Invalid_MailId = 114,
        SetMailRead_Fail_Exception = 115,

        SetMailReceived_Fail_Invalid_MailId = 116,
        SetMailReceived_Fail_Exception = 117,

        LoadStageKilledNpcNum_Fail_Exception = 118,
        LoadStageKilledNpcNum_Fail_Not_Exist = 119,

        UpdateStage_Fail_Exception = 120,
        UpdateStage_Fail_AccountId_Not_Exist = 121,

        AllocateChannel_Fail_Exception = 122,
        AllocateChannel_Fail_All_Channel_Full = 123,
        AllocateChannel_Fail_Invalid_ChannelNum = 124,
        AllocateChannel_Fail_Channel_Full = 125,

        LeaveChannel_Fail_Exception = 126,
        LeaveChannel_Fail_Invalid_Channel = 127,
        LeaveChannel_Fail_Channel_Not_Exist = 128,

        EnterChatRoom_Fail_Exception = 126,
        EnterChatRoom_Fail_Invalid_Channel = 127,

        LeaveChatRoom_Fail_Exception = 126,
        LeaveChatRoom_Fail_Invalid_Channel = 127,

        SendChat_Fail_Exception = 126,
        SendChat_Fail_Invalid_Channel = 127,
    }
}
