using StackExchange.Redis;
using System.Net.WebSockets;
using System.Text;
using TuesberryAPIServer.Services;
using ZLogger;

namespace TuesberryAPIServer.Middleware
{
    public class EnterChatting
    {
        readonly RequestDelegate _next;
        readonly ILogger<EnterChatting> _logger;
        readonly IMemoryDb _memoryDb;

        public EnterChatting(RequestDelegate next, ILogger<EnterChatting> logger, IMemoryDb memoryDb)
        {
            _next = next;
            _logger = logger;
            _memoryDb = memoryDb;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (string.Compare(path, "/Chat", StringComparison.OrdinalIgnoreCase) != 0)
            {
                // call the next delegate/middleware in the pipeline
                await _next(context);
                return;
            }

            if (context.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await context.WebSockets.AcceptWebSocketAsync();

                // 채널 할당
                var (errorCode, channel) = await _memoryDb.AllocateChannel();
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[EnterChatting] Allocate Channel Error");
                    return;
                }

                // 채팅방 입장
                errorCode = await EnterChat(webSocket, channel);
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[EnterChatting] Enter Chat Error");
                    return;
                }

                // 채널 해제
                errorCode = await _memoryDb.LeaveChannel(channel);
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[EnterChatting] Leave Channel Error");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 400;
            }
            
        }

        async Task<ErrorCode> EnterChat(WebSocket webSocket, Int32 channel)
        {
            ErrorCode errorCode = ErrorCode.None;

            Action<RedisChannel, RedisValue> handler = async (channel, value) =>
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    var message = Encoding.UTF8.GetBytes(value.ToString());
                    await webSocket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            };

            // 채팅방 입장
            if (webSocket.State == WebSocketState.Open)
            {
                // Subscribe socket callback to a Redis Channel
                errorCode = await _memoryDb.EnterChatRoom(channel, handler);
                if (errorCode != ErrorCode.None)
                {
                    _logger.ZLogError($"[EnterChatting.EnterChat] Enter Chat Romm Error");
                    return errorCode;
                }
            }

            // Reserve 4KB buffer
            var buffer = new byte[1024 * 4];

            // 채팅
            while (webSocket.State == WebSocketState.Open)
            {
                // non-blocking event loop
                // receive from client
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                // decode message as UTF-8 string, publish to Redis Channel
                if (result.CloseStatus.HasValue == false)
                {
                    errorCode = await _memoryDb.SendChat(channel, Encoding.UTF8.GetString(buffer, 0, result.Count));
                    if (errorCode != ErrorCode.None)
                    {
                        _logger.ZLogError($"[EnterChatting.EnterChat] Send Chat Error");
                        return errorCode;
                    }
                }
            }

            // leave chatting room
            errorCode = await _memoryDb.LeaveChatRoom(channel, handler);
            if (errorCode != ErrorCode.None)
            {
                _logger.ZLogError($"[EnterChatting.EnterChat] Leave Chat Romm Error");
                return errorCode;
            }

            // close web socket
            await webSocket.CloseAsync(webSocket.CloseStatus.Value, webSocket.CloseStatusDescription, CancellationToken.None);

            return ErrorCode.None;
        }
    }
}
