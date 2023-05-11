using Microsoft.AspNetCore.Mvc;
using TuesberryAPIServer.ModelDb;
using TuesberryAPIServer.ModelReqRes;
using TuesberryAPIServer.Services;
using ZLogger;

namespace TuesberryAPIServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttendanceController : ControllerBase
    {
        readonly IHttpContextAccessor _httpContextAccessor;
        readonly ILogger<AttendanceController> _logger;
        readonly IGameDb _gameDb;
        readonly IMasterDb _masterDb;

        public AttendanceController(IHttpContextAccessor httpContextAccessor, ILogger<AttendanceController> logger, IGameDb gameDb, IMasterDb masterDb)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _gameDb = gameDb;
            _masterDb = masterDb;
        }

        [HttpPost("LoadAttendance")]
        public async Task<PkLoadAttendanceResponse> LoadAttendance([FromBody] PkLoadAttendanceRequest request)
        {
            var response = new PkLoadAttendanceResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // 출석 데이터 가져오기
            var (errorCode, attendanceData) = await _gameDb.LoadAttendanceData(userInfo.AccountId);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[LoadAttendance] Load Attendance Data Fail, UserId = {request.Id}, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }

            response.LastCheckDate = attendanceData.LastCheckDate;
            response.ContinuousPeriod = attendanceData.ContinuousPeriod;

            return response;
        }

        [HttpPost("CheckAttendance")]
        public async Task<PkCheckAttendanceResponse> CheckAttendance([FromBody] PkCheckAttendanceRequest request)
        {
            var response = new PkCheckAttendanceResponse();

            // userInfo 가져오기
            AuthUser userInfo = _httpContextAccessor.HttpContext.Items[nameof(AuthUser)] as AuthUser;
            if (userInfo is null)
            {
                response.Result = ErrorCode.AuthToken_Access_Error;
                return response;
            }

            // 출석 데이터 가져오기
            var(errorCode, attendanceData) = await _gameDb.LoadAttendanceData(userInfo.AccountId);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[CheckAttendance] Load Attendance Data Fail, UserId = {request.Id}, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }

            // 출석 체크
            errorCode = AttendanceChack(attendanceData);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[CheckAttendance] ErrorCode = {errorCode}, UserId = {request.Id}, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }

            // 출석 체크 보상 메일 데이터 생성
            var(mailData, comment) = CreateAttendanceRewardMail(attendanceData);

            // 메일 추가하기
            (errorCode, var mailId) = await _gameDb.InsertMail(userInfo.AccountId, mailData, comment);
            if(errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[CheckAttendance] Insert Attendance Reward Mail Fail, UserId = {request.Id}, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }

            // 출석 데이터 업데이트 하기
            errorCode = await _gameDb.UpdateAttendanceData(userInfo.AccountId, attendanceData);
            if (errorCode != ErrorCode.None)
            {
                // 출석 보상 메일 롤백
                var result = await _gameDb.DeleteMail(userInfo.AccountId, mailId);
                if(result != ErrorCode.None)
                {
                    _logger.ZLogError($"[CheckAttendance] Rollback Reward Mail Fail, UserId = {request.Id}, AccountId = {userInfo.AccountId}");
                }
                _logger.ZLogError($"[CheckAttendance] Update AttendanceData Fail, UserId = {request.Id}, AccountId = {userInfo.AccountId}");
                response.Result = errorCode;
                return response;
            }

            _logger.ZLogDebug($"[CheckAttendance] Complete, UserId = {request.Id}, AccountId = {userInfo.AccountId}");

            response.ContinuousPeriod = attendanceData.ContinuousPeriod;
            return response;
        }

        ErrorCode AttendanceChack(AttendanceData attendanceData) 
        {
            if (attendanceData.LastCheckDate.Date == DateTime.Today)
            {
                // 이미 출석함
                return ErrorCode.Attendance_Fail_Already_Check;
            }

            if ((attendanceData.ContinuousPeriod >= 30) || (attendanceData.LastCheckDate.Date != DateTime.Today.AddDays(-1)))
            {
                // 30일이 넘었음 or 어제 출석체크 안함 => 다시 1일 부터
                attendanceData.ContinuousPeriod = 1;
                attendanceData.LastCheckDate = DateTime.Today;
            }
            else
            {
                // 어제 출석 체크 했고, 아직 30일이 넘지 않았음 => 연속 출석
                attendanceData.ContinuousPeriod++;
                attendanceData.LastCheckDate = DateTime.Today;
            }

            return ErrorCode.None;
        }

        Tuple<MailboxData, string> CreateAttendanceRewardMail(AttendanceData attendanceData)
        {
            MailboxData mailData = new MailboxData
            {
                Title = "Daily Attendance Chack Reward havs Arrived.",
                ExpiryDate = DateTime.Today.AddDays(100),
            };

            Int32 itemCode = _masterDb.AttendanceRewards[attendanceData.ContinuousPeriod].ItemCode;

            mailData.MailboxItemData.Add(new MailboxItemData
            {
                ItemCode = itemCode,
                Amount = _masterDb.AttendanceRewards[attendanceData.ContinuousPeriod].Count
            });

            string comment = $"{attendanceData.ContinuousPeriod}days Attendance Check. Would you like to receive your reward?";

            return new Tuple<MailboxData, string>(mailData, comment);
        }
    }
}
