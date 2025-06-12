using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;
using TodoApp.Application.Common.Interfaces;

namespace TodoApp.ReadApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebSocketController : ControllerBase
{
    private readonly IPubSubService _iPubSubService;

    public WebSocketController(IPubSubService iPubSubService)
    {
        _iPubSubService = iPubSubService;
    }

    [HttpGet("test-ws")] // 👈 Thêm dòng này
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            var cancellationToken = new CancellationTokenSource();
            var token = cancellationToken.Token;

            // Đăng ký subscriber 1 lần duy nhất
            await _iPubSubService.SubscribeAsync("chat-room", async (channel, message) =>
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    var msgBytes = Encoding.UTF8.GetBytes($"{channel}: {message}");
                    await webSocket.SendAsync(
                        new ArraySegment<byte>(msgBytes),
                        WebSocketMessageType.Text,
                        true,
                        token);
                }
            });

            // Vòng lặp giữ kết nối (nếu muốn duy trì hoặc làm heartbeat)
            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(2000);
            }

            cancellationToken.Cancel(); // Hủy nếu cần

            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure,
                "Connection closed by the server", CancellationToken.None);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}