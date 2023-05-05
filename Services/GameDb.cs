using Microsoft.Extensions.Options;
using MySqlConnector;
using SqlKata.Compilers;
using SqlKata.Execution;
using System.Data;
using System.Reflection.PortableExecutable;
using TuesberryAPIServer.ModelDb;
using ZLogger;
using static Humanizer.In;

namespace TuesberryAPIServer.Services
{
    public class GameDb : IGameDb
    {
        readonly ILogger<GameDb> _logger;
        readonly IOptions<DbConfig> _dbConfig;
        readonly IMasterDb _masterDb;

        readonly QueryFactory _queryFactory;
        readonly IDbConnection _connection;
        readonly MySqlCompiler _compiler;

        public GameDb(ILogger<GameDb> logger, IOptions<DbConfig> options, IMasterDb masterDb)
        {
            _logger = logger;
            _dbConfig = options;
            _masterDb = masterDb;

            _connection = new MySqlConnection(_dbConfig.Value.GameDb);
            _connection.Open();

            _compiler = new MySqlCompiler();
            _queryFactory = new QueryFactory(_connection, _compiler);
        }

        public void Dispose()
        {
            _connection.Close();
        }

        public async Task<Tuple<ErrorCode, Int64>> CreateGameData(string userId)
        {
            try
            {
                Int64 accountId = await _queryFactory.Query("GameData").InsertGetIdAsync<Int64>(new
                {
                    UserId = userId,
                    Level = 1,
                    Exp = 0,
                    Hp = 20,
                    Ap = 10,
                    Mp = 10,
                    Money = 0,
                    Stage = 0
                });

                if (accountId == 0)
                {
                    _logger.ZLogError($"[AccountDb.CreateAccount] ErrorCode = {ErrorCode.Create_GameData_Fail_Duplicate}, UserId = {userId}");
                    return new Tuple<ErrorCode, Int64>(ErrorCode.Create_GameData_Fail_Duplicate, 0);
                }

                return new Tuple<ErrorCode, Int64>(ErrorCode.None, accountId);
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode = {ErrorCode.Create_GameData_Fail_Exception}, UserId = {userId}");
                return new Tuple<ErrorCode, Int64>(ErrorCode.Create_GameData_Fail_Exception, 0);
            }
        }

        public async Task<Tuple<ErrorCode, GameData, Int64>> LoadGameData(string userId)
        {
            try
            {
                var gameData = await _queryFactory.Query("GameData")
                    .Select("Level", "Exp", "Hp", "Ap", "Mp", "Money")
                    .Where("UserId", userId).FirstOrDefaultAsync<GameData>();

                var accountId = await _queryFactory.Query("GameData")
                    .Select("AccountId")
                    .Where("UserId", userId).FirstOrDefaultAsync<Int64>();

                if (accountId == 0)
                {
                    _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode = {ErrorCode.Load_GameDate_Fail_Not_Exist}, UserId = {userId}");
                    return new Tuple<ErrorCode, GameData, Int64>(ErrorCode.Load_GameDate_Fail_Not_Exist, null, 0);
                }

                _logger.ZLogDebug($"[GameDb.CreateGameData] Get Game Data Complete, UserId = {userId}");
                return new Tuple<ErrorCode, GameData, Int64>(ErrorCode.None, gameData, accountId);
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateGameData] ErrorCode =  {ErrorCode.Load_GameData_Fail_Exception} , UserId = {userId}");
                return new Tuple<ErrorCode, GameData, Int64>(ErrorCode.Load_GameData_Fail_Exception, null, 0);
            }
        }

        public async Task<ErrorCode> UpdateMoney(Int64 accountId, Int32 amount)
        {
            try
            {
                var money = await _queryFactory.Query("GameData")
                        .Select("Money")
                        .Where("AccountId", accountId)
                        .FirstAsync<Int32>();

                if(money + amount < 0)
                {
                    _logger.ZLogError($"[GameDb.UpdateMoney] ErrorCode = {ErrorCode.UpdateMoney_Invalid_Amount}, AccountId = {accountId}");
                    return ErrorCode.UpdateMoney_Invalid_Amount;
                }

                var count = await _queryFactory.Query("GameData")
                    .Where("AccountId", accountId)
                    .UpdateAsync(new { Money = money + amount });

                if (count != 1)
                {
                    _logger.ZLogError($"[GameDb.UpdateMoney] ErrorCode = {ErrorCode.UpdateMoney_Fail_Exception}, AccountId = {accountId}");
                    return ErrorCode.UpdateMoney_Fail_Exception;
                }

                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.UpdateMoney] ErrorCode = {ErrorCode.UpdateMoney_Fail_Exception}, AccountId = {accountId}");
                return ErrorCode.UpdateMoney_Fail_Exception;
            }
        }

        public async Task<ErrorCode> DeleteGameData(string userId)
        {
            try
            {
                var result = await _queryFactory.Query("GameData").Where("UserId", userId).DeleteAsync();
                _logger.ZLogDebug($"[GameDb.DeleteGameData] Delete Game Data Complete, UserId = {userId}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.DeleteGameData] ErrorCode = {ErrorCode.DeleteGameData_Fail_Exception}, UserId = {userId}");
                return ErrorCode.DeleteGameData_Fail_Exception;
            }

        }

        public async Task<ErrorCode> CreateDefaultItemData(Int64 accountId)
        {
            try
            {
                ItemData itemData = new ItemData { 
                    ItemCode = 1,
                    Amount = 20,
                    EnchanceCount = 0,
                    Attack = _masterDb.Items[1].Attack,
                    Defence = _masterDb.Items[1].Defence,
                    Magic = _masterDb.Items[1].Magic
                };

                var errorCode = await InsertItem(accountId, itemData);
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameDb.CreateDefaultItemData] ErrorCode = {errorCode}, AccountId = {accountId}");
                    return errorCode;
                }

                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.CreateDefaultItemData] ErrorCode = {ErrorCode.Create_Item_Data_Fail_Exception}, AccountId = {accountId}");
                return ErrorCode.Create_Item_Data_Fail_Exception;
            }
        }

        public async Task<Tuple<ErrorCode, List<ItemData>>> LoadItemData(Int64 accountId)
        {
            try
            {
                var itemDataList = await _queryFactory.Query("ItemData")
                    .Select("UserItemId", "ItemCode", "Amount", "EnchanceCount", "Attack", "Defence", "Magic")
                    .Where("AccountId", accountId).GetAsync<ItemData>();

                if (itemDataList is null)
                {
                    _logger.ZLogError($"[GameDb.LoadItemData] ErrorCode = {ErrorCode.Load_ItemData_Fail_Not_Exist}, AccountId = {accountId}");
                    return new Tuple<ErrorCode, List<ItemData>>(ErrorCode.Load_ItemData_Fail_Not_Exist, null);
                }

                return new Tuple<ErrorCode, List<ItemData>>(ErrorCode.None, itemDataList.ToList<ItemData>());
            }
            catch
            {
                _logger.ZLogError($"[GameDb.LoadItemData] ErrorCode = {ErrorCode.Load_ItemData_Fail_Exception}, AccountId = {accountId}");
                return new Tuple<ErrorCode, List<ItemData>>(ErrorCode.Load_ItemData_Fail_Exception, null);
            }
        }

        public async Task<ErrorCode> InsertItem(Int64 accountId, ItemData itemData)
        {
            try
            {
                var itemCode = itemData.ItemCode;

                // 새로운 아이템을 넣는다.
                var count = await _queryFactory.Query("ItemData").InsertAsync(new
                {
                    AccountId = accountId,
                    ItemCode = itemCode,
                    Amount = itemData.Amount,
                    EnchanceCount = itemData.EnchanceCount,
                    Attack = itemData.Attack,
                    Defence = itemData.Defence,
                    Magic = itemData.Magic,
                });

                if (count != 1)
                {
                    _logger.ZLogError($"[GameDb.InsertItem] ErrorCode = {ErrorCode.Insert_Item_Data_Fail_Duplicate}, AccountId = {accountId}");
                    return ErrorCode.Insert_Item_Data_Fail_Duplicate;
                }

                _logger.ZLogDebug($"[GameDb.InsertItem] Complete, AccountId = {accountId}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.InsertItem] ErrorCode : {ErrorCode.Insert_Item_Data_Fail_Exception}, AccountId: {accountId}");
                return ErrorCode.Insert_Item_Data_Fail_Exception;
            }
        }

        public async Task<ErrorCode> InsertOrUpdateItem(Int64 accountId, ItemData itemData)
        {
            try
            {
                Int32 itemCode = itemData.ItemCode;

                // 아이템 코드가 돈 => GameData에서 업데이트
                if(_masterDb.Items[itemCode].Attribute == _masterDb.ItemAttributes["Money"])
                {
                    var result = await UpdateMoney(accountId, itemData.Amount);
                    if (result != ErrorCode.None)
                    {
                        _logger.ZLogError($"[GameDb.InsertOrUpdateItem] InsertItem Error, AccountId = {accountId}");
                    }
                    return result;
                }

                // 겹침 가능 여부 확인
                if (_masterDb.Items[itemCode].IsOverlapped)
                {
                    // 겹쳐질 수 있고, 해당 아이템이 이미 존재 => update
                    // 겹쳐질 수 있는데, 해당 아이템이 없음 => Insert

                    // 이미 있는지 확인
                    var checkData = await _queryFactory.Query("ItemData")
                        .Select("UserItemId", "Amount")
                        .Where(
                            new { 
                                AccountID = accountId, 
                                ItemCode = itemCode, 
                                EnchanceCount = itemData.EnchanceCount,
                                Attack = itemData.Attack,
                                Defence = itemData.Defence,
                                Magic = itemData.Magic
                            })
                        .FirstOrDefaultAsync<ItemData>();

                    // 해당 데이터가 존재하는지 확인
                    if (checkData is null)
                    {
                        var result = await InsertItem(accountId, itemData);
                        if (result != ErrorCode.None)
                        {
                            _logger.ZLogError($"[GameDb.InsertOrUpdateItem] InsertItem Error, AccountId = {accountId}");
                        }
                        return result;
                    }

                    // Update Data
                    var count = await _queryFactory.Query("ItemData")
                        .Where(new { UserItemId = checkData.UserItemId })
                        .UpdateAsync(new { Amount = checkData.Amount + itemData.Amount });

                    if (count != 1)
                    {
                        _logger.ZLogError($"[GameDb.InsertOrUpdateItem] ErrorCode = {ErrorCode.InsertOrUpdate_Item_Data_Fail_Exception}, AccountId = {accountId}");
                        return ErrorCode.InsertOrUpdate_Item_Data_Fail_Exception;
                    }
                }
                else
                {
                    // 겹쳐질 수 없음 => Insert
                    var result =  await InsertItem(accountId, itemData);
                    if(result != ErrorCode.None)
                    {
                        _logger.ZLogError($"[GameDb.InsertOrUpdateItem] InsertItem Error, AccountId = {accountId}");
                    }
                }
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.InsertOrUpdateItem] ErrorCode : {ErrorCode.InsertOrUpdate_Item_Data_Fail_Exception}, AccountId: {accountId}");
                return ErrorCode.InsertOrUpdate_Item_Data_Fail_Exception;
            }
        }

        public async Task<Tuple<ErrorCode, List<MailboxData>>> LoadMailList(Int64 accountId, Int32 page)
        {
            try
            {
                if(page <= 0)
                {
                    _logger.ZLogError($"[GameDb.LoadMailboxData] ErrorCode = {ErrorCode.LoadMailList_Fail_Inappropriate_Page_Range}, AccountId = {accountId}");
                    return new Tuple<ErrorCode, List<MailboxData>>(ErrorCode.LoadMailList_Fail_Inappropriate_Page_Range, null); ;
                }

                Int32 offset = 10 * (page - 1);

                // 메일 리스트에서 20개를 가져온다.
                // 최신 메일 20개부터, ExpiryDate가 만료된 것은 제외
                var mailboxQuery = _queryFactory.Query("Mailbox")
                    .Select("MailId", "Title", "IsRead", "ExpiryDate")
                    .Where("AccountId", accountId)
                    .WhereFalse("IsRead")
                    .Where("ExpiryDate", ">", DateTime.UtcNow)
                    .OrderByDesc("MailId")
                    .Limit(20).Offset(offset);

                var mailList = await _queryFactory.Query("MailboxItem")
                    .Select(
                        "Mailbox.MailId", "Mailbox.Title", "Mailbox.IsRead", "Mailbox.ExpiryDate",
                        "MailboxItem.ItemCode", "MailboxItem.Amount", "MailboxItem.EnchanceCount",
                        "MailboxItem.Attack", "MailboxItem.Defence", "MailboxItem.Magic")
                    .Join(mailboxQuery.As("Mailbox"), j => j.On("Mailbox.MailId", "MailboxItem.MailId"))
                    .GetAsync<MailboxDataDb>();

                if (mailList is null)
                {
                    _logger.ZLogError($"[GameDb.LoadMailboxData] ErrorCode = {ErrorCode.LoadMailList_Fail_Not_Exist_In_This_Page}, AccountId = {accountId}");
                    return new Tuple<ErrorCode, List<MailboxData>>(ErrorCode.LoadMailList_Fail_Not_Exist_In_This_Page, null); 
                }

                // 메일 리스트
                List<MailboxData> mailboxDataList = new List<MailboxData>();
                Int32 lastInsertId = 0;
                foreach (var mail in mailList) 
                {
                    if(mail.MailId != lastInsertId) 
                    {
                        mailboxDataList.Add(new MailboxData
                        {
                            MailId = mail.MailId,
                            Title = mail.Title,
                            IsRead = mail.IsRead,
                            ExpiryDate = mail.ExpiryDate,
                        });
                        lastInsertId = mail.MailId;
                    }

                    mailboxDataList.Last().MailboxItemData.Add(new MailboxItemData
                    {
                        ItemCode = mail.ItemCode,
                        Amount = mail.Amount,
                        EnchanceCount = mail.EnchanceCount,
                        Attack = mail.Attack,
                        Defence = mail.Defence,
                        Magic = mail.Magic
                    });
                }

                _logger.ZLogDebug($"[GameDb.LoadMailboxData] Complete, AccountId = {accountId}");
                return new Tuple<ErrorCode, List<MailboxData>>(ErrorCode.None, mailboxDataList);
            }
            catch
            {
                _logger.ZLogError($"[GameDb.LoadMailboxData] ErrorCode = {ErrorCode.LoadMailList_Fail_Exception}, AccountId = {accountId}");
                return new Tuple<ErrorCode, List<MailboxData>>(ErrorCode.LoadMailList_Fail_Exception, null);
            }
        }

        public async Task<Tuple<ErrorCode, string>> LoadMailDetail(Int64 accountId, Int32 mailId)
        {
            try
            {
                // Check if the user's email is
                var result = await IsValidMailId(accountId, mailId);
                if (!result)
                {
                    _logger.ZLogError($"[GameDb.LoadMailDetail] ErrorCode = {ErrorCode.LoadMailDetail_Fail_Invalid_MailId}, AccountId = {accountId}, MailId = {mailId}");
                    return new Tuple<ErrorCode, string>(ErrorCode.LoadMailDetail_Fail_Invalid_MailId, string.Empty);
                }

                var detail = await _queryFactory.Query("Mailbox")
                    .Select("Comment")
                    .Where(new { AccountId = accountId, MailId = mailId })
                    .FirstAsync<string>();

                if(detail is null)
                {
                    _logger.ZLogError($"[GameDb.LoadMailDetail] ErrorCode = {ErrorCode.LoadMailDetail_Fail_Not_Exist}, AccountId = {accountId}, MailId = {mailId}");
                    return new Tuple<ErrorCode, string>(ErrorCode.LoadMailDetail_Fail_Not_Exist, string.Empty);
                }

                _logger.ZLogDebug($"[GameDb.LoadMailDetail] Complete, AccountId = {accountId}, MailId = {mailId}");
                return new Tuple<ErrorCode, string>(ErrorCode.None, detail);
            }
            catch
            {
                _logger.ZLogError($"[GameDb.LoadMailDetail] ErrorCode = {ErrorCode.LoadMailDetail_Fail_Exception}, AccountId = {accountId}, MailId = {mailId}");
                return new Tuple<ErrorCode, string>(ErrorCode.LoadMailDetail_Fail_Exception, string.Empty);
            }
        }

        public async Task<Tuple<ErrorCode, List<MailboxItemData>>> LoadMailItemList(Int32 mailId)
        {
            try
            {
                var mailItemData = await _queryFactory.Query("MailboxItem")
                    .Select("ItemCode", "Amount", "EnchanceCount", "Attack", "Defence", "Magic")
                    .Where("MailId", mailId)
                    .GetAsync<MailboxItemData>();

                if(mailItemData is null)
                {
                    _logger.ZLogError($"[GameDb.LoadMailItemList] ErrorCode = {ErrorCode.LoadMailItem_Fail_Item_Not_Exist}, MailId = {mailId}");
                    return new Tuple<ErrorCode, List<MailboxItemData>>(ErrorCode.LoadMailItem_Fail_Item_Not_Exist, null);
                }

                _logger.ZLogDebug($"[GameDb.LoadMailItemList] Complete, MailId = {mailId}");
                return new Tuple<ErrorCode, List<MailboxItemData>>(ErrorCode.None, mailItemData.ToList());
            }
            catch
            {
                _logger.ZLogError($"[GameDb.LoadMailItemList] ErrorCode = {ErrorCode.LoadMailItem_Fail_Exception}, MailId = {mailId}");
                return new Tuple<ErrorCode, List<MailboxItemData>>(ErrorCode.LoadMailItem_Fail_Exception, null);
            }
        }

        public async Task<ErrorCode> ReceiveMailItem(Int64 accountId, Int32 mailId)
        {
            try
            {
                // Check if the user's email is
                var result = await IsValidMailId(accountId, mailId);
                if(!result)
                {
                    _logger.ZLogError($"[GameDb.ReceiveMailItem] ErrorCode = {ErrorCode.ReceiveMailItem_Fail_Invalid_MailId}, AccountId = {accountId}");
                    return ErrorCode.ReceiveMailItem_Fail_Invalid_MailId;
                }

                // load mail item list
                var (errorCode, mailItemList) = await LoadMailItemList(mailId);
                if(errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameDb.ReceiveMailItem] ErrorCode = {ErrorCode.ReceiveMailItem_Fail_Item_Not_Exist}, AccountId = {accountId}");
                    return ErrorCode.ReceiveMailItem_Fail_Item_Not_Exist;
                }

                // add item in itemData table
                foreach(var mailItem in mailItemList)
                {
                    ItemData itemData = new ItemData
                    {
                        ItemCode = mailItem.ItemCode,
                        Amount = mailItem.Amount,
                        EnchanceCount = mailItem.EnchanceCount,
                        Attack = mailItem.Attack,
                        Defence = mailItem.Defence,
                        Magic = mailItem.Magic
                    };
                    errorCode = await InsertOrUpdateItem(accountId, itemData);
                    if(errorCode != ErrorCode.None)
                    {
                        _logger.ZLogError($"[GameDb.ReceiveMailItem] ErrorCode = {ErrorCode.ReceiveMailItem_Fail_Add_Item_Exception}, AccountId = {accountId}");
                        return ErrorCode.ReceiveMailItem_Fail_Add_Item_Exception;
                    }
                }

                // set mail read
                errorCode = await SetMailRead(accountId, mailId);
                if(errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[GameDb.ReceiveMailItem] ErrorCode = {ErrorCode.ReceiveMailItem_Fail_Set_Read_Exception}, AccountId = {accountId}");
                    return ErrorCode.ReceiveMailItem_Fail_Set_Read_Exception;
                }

                _logger.ZLogDebug($"[GameDb.ReceiveMailItem] Complete, AccountId = {accountId}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.ReceiveMailItem] ErrorCode = {ErrorCode.ReceiveMailItem_Fail_Exception}, AccountId = {accountId}");
                return ErrorCode.ReceiveMailItem_Fail_Exception;
            }
        }

        public async Task<ErrorCode> SetMailRead(Int64 accountId, Int32 mailId)
        {
            try
            {
                // valid check
                var result = await IsValidMailId(accountId, mailId);
                if (!result)
                {
                    _logger.ZLogError($"[GameDb.DeleteMail] ErrorCode = {ErrorCode.DeleteMail_Fail_Invalid_MailId}, AccountId = {accountId}");
                    return ErrorCode.DeleteMail_Fail_Invalid_MailId;
                }

                // just update "IsRead = true"
                var count = await _queryFactory.Query("Mailbox")
                    .Where(new {AccountId = accountId, MailId = mailId})
                    .UpdateAsync(new { IsRead = true });

                // count check
                if(count != 1)
                {
                    _logger.ZLogError($"[GameDb.DeleteMail] ErrorCode = {ErrorCode.DeleteMail_Fail_Exception}, AccountId = {accountId}");
                    return ErrorCode.DeleteMail_Fail_Exception;
                }

                _logger.ZLogDebug($"[GameDb.DeleteMail] Delete Complete, AccountId = {accountId}, MailId = {mailId}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.DeleteMail] ErrorCode = {ErrorCode.DeleteMail_Fail_Exception}, AccountId = {accountId}");
                return ErrorCode.DeleteMail_Fail_Exception;
            }
        }

        public async Task<ErrorCode> DeleteMail(Int64 accountId, Int32 mailId)
        {
            try
            {
                // valid check
                var result = await IsValidMailId(accountId, mailId);
                if (!result)
                {
                    _logger.ZLogError($"[GameDb.DeleteMail] ErrorCode = {ErrorCode.DeleteMail_Fail_Invalid_MailId}, AccountId = {accountId}");
                    return ErrorCode.DeleteMail_Fail_Invalid_MailId;
                }

                var count = await _queryFactory.Query("Mailbox")
                    .Where(new { AccountId = accountId, MailId = mailId })
                    .DeleteAsync();

                _logger.ZLogDebug($"[GameDb.DeleteMail] Delete Complete, AccountId = {accountId}, MailId = {mailId}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.DeleteMail] ErrorCode = {ErrorCode.DeleteMail_Fail_Exception}, AccountId = {accountId}");
                return ErrorCode.DeleteMail_Fail_Exception;
            }
        }


        public async Task<bool> IsValidMailId(Int64 accountId, Int32 mailId)
        {
            try
            {
                var result = await _queryFactory.Query("Mailbox")
                    .Where(new { AccountId = accountId, MailId = mailId })
                    .WhereFalse("IsRead")
                    .CountAsync<Int32>();

                if (result != 1)
                {
                    _logger.ZLogError($"[GameDb.ReceiveMailItem] Inappropriate Mail Id, AccountId = {accountId}");
                    return false;
                }

                return true;
            }
            catch
            {
                _logger.ZLogError($"[GameDb.ReceiveMailItem] ErrorCode = MailValidCheck_Fail_Exception , AccountId = {accountId}");
                return false;
            }
        }

        public async Task<Tuple<ErrorCode, Int32>> InsertMail(Int64 accountId, MailboxData mailData, string comment)
        {
            Int32 mailId = 0;

            try
            {
                // insert mailbox data
                mailId = await _queryFactory.Query("Mailbox")
                    .InsertGetIdAsync<Int32>(new
                    {
                        AccountId = accountId,
                        Title = mailData.Title,
                        ExpiryDate = mailData.ExpiryDate,
                        IsRead = false,
                        Comment = comment
                    });

                if (mailId == 0)
                {
                    _logger.ZLogError($"[InsertMail] ErrorCode = {ErrorCode.InsertMail_Fail_Mailbox_Insert_Fail}, AccountId = {accountId}");
                    return new Tuple<ErrorCode, Int32>(ErrorCode.InsertMail_Fail_Mailbox_Insert_Fail, 0);
                }

                // insert mail item data
                var cols = new[] { "MailId", "ItemCode", "Amount", "EnchanceCount", "Attack", "Defence", "Magic" };
                object[][] data = new object[mailData.MailboxItemData.Count()][];

                for(int i = 0; i < mailData.MailboxItemData.Count(); i++)
                {
                    data[i] = new object[]
                    {
                        mailId,
                        mailData.MailboxItemData[i].ItemCode,
                        mailData.MailboxItemData[i].Amount,
                        mailData.MailboxItemData[i].EnchanceCount,
                        mailData.MailboxItemData[i].Attack,
                        mailData.MailboxItemData[i].Defence,
                        mailData.MailboxItemData[i].Magic
                    };
                }

                // insert
                var result = await _queryFactory.Query("MailboxItem")
                    .InsertAsync(cols, data);

                if(result != mailData.MailboxItemData.Count())
                {
                    var errorCode = await DeleteMail(accountId, mailId);
                    if (errorCode != ErrorCode.None)
                    {
                        _logger.ZLogError($"[InsertMail] Rollback Mailbox Data Fail, AccountId = {accountId}, MailId = {mailId}");
                    }

                    _logger.ZLogError($"[InsertMail] ErrorCode = {ErrorCode.InsertMail_Fail_Exception}, AccountId = {accountId}");
                    return new Tuple<ErrorCode, Int32>(ErrorCode.InsertMail_Fail_Exception, 0);
                }

                return new Tuple<ErrorCode, Int32>(ErrorCode.None, mailId);
            }
            catch
            {
                // mailId != 0 이면, 넣은 메일을 삭제해줘야 함
                if(mailId != 0)
                {
                    var errorCode = await DeleteMail(accountId, mailId);
                    if(errorCode != ErrorCode.None)
                    {
                        _logger.ZLogError($"[InsertMail] Rollback Mailbox Data Fail, AccountId = {accountId}, MailId = {mailId}");
                    }
                }

                _logger.ZLogError($"[InsertMail] ErrorCode = {ErrorCode.InsertMail_Fail_Exception}, AccountId = {accountId}");
                return new Tuple<ErrorCode, Int32>(ErrorCode.InsertMail_Fail_Exception, 0);
            }
        }

        public async Task<ErrorCode> CreateAttendanceData(Int64 accountId)
        {
            try
            {
                var result = await _queryFactory.Query("Attendance")
                    .InsertAsync(new {
                        AccountId = accountId,
                        LastCheckDate = DateTime.Today.AddDays(-1),
                        ContinuousPeriod = 0
                    });

                if (result != 1)
                {
                    _logger.ZLogError($"[CreateAttendanceData] ErrorCode = {ErrorCode.CreateAttendanceData_Fail_Duplicate}, AccountId = {accountId}");
                    return ErrorCode.CreateAttendanceData_Fail_Duplicate;
                }

                _logger.ZLogDebug($"[CreateAttendanceData] Complete, AccountId = {accountId}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[CreateAttednaceData] ErrorCode = {ErrorCode.CreateAttendanceData_Fail_Exception}, AccountId = {accountId}");
                return ErrorCode.CreateAttendanceData_Fail_Exception;
            }
        }

        public async Task<Tuple<ErrorCode, AttendanceData>> LoadAttendanceData(Int64 accountId)
        {
            try
            {
                var attendanceData = await _queryFactory.Query("Attendance")
                    .Select("LastCheckDate", "ContinuousPeriod")
                    .Where("AccountId", accountId)
                    .GetAsync<AttendanceData>();

                if(attendanceData is null)
                {
                    _logger.ZLogError($"[LoadAttendanceData] ErrorCode = {ErrorCode.AttendanceCheck_Fail_AccountId_Not_Exist}, AccountId = {accountId}");
                    return new Tuple<ErrorCode, AttendanceData>(ErrorCode.AttendanceCheck_Fail_AccountId_Not_Exist, null);
                }

                _logger.ZLogDebug($"[LoadAttendanceData] Complete, AccountId = {accountId}");
                return new Tuple<ErrorCode, AttendanceData>(ErrorCode.None, attendanceData.First());
            }
            catch
            {
                _logger.ZLogError($"[LoadAttendanceData] ErrorCode = {ErrorCode.AttendanceCheck_Fail_Exception}, AccountId = {accountId}");
                return new Tuple<ErrorCode, AttendanceData>(ErrorCode.AttendanceCheck_Fail_Exception, null);
            }
        }

        public async Task<ErrorCode> UpdateAttendanceData(Int64 accountId, AttendanceData attendanceData)
        {
            try
            {
                var count = await _queryFactory.Query("Attendance")
                    .Where("AccountId", accountId)
                    .UpdateAsync(new
                    {
                        LastCheckDate = attendanceData.LastCheckDate,
                        ContinuousPeriod = attendanceData.ContinuousPeriod
                    });

                if(count != 1)
                {
                    _logger.ZLogError($"[UpdateAttendanceData] ErrorCode = {ErrorCode.UpdateAttendanceData_Fail_Duplicate}, AccountId = {accountId}");
                    return ErrorCode.UpdateAttendanceData_Fail_Duplicate;
                }

                _logger.ZLogDebug($"[UpdateAttendanceData] Complete, AccountId = {accountId}");
                return ErrorCode.None;
            }
            catch
            {
                _logger.ZLogError($"[UpdateAttendanceData] ErrorCode = {ErrorCode.UpdateAttendanceDate_Fail_Exceiption}, AccountId = {accountId}");
                return ErrorCode.UpdateAttendanceDate_Fail_Exceiption;
            }
        }

    }
}
