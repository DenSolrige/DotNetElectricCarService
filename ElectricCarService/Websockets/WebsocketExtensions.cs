using System.Net.WebSockets;
using System.Text;

namespace ElectricCarService.Websockets;

public static class WebsocketExtensions
{
    public static Task SendStringAsync(this WebSocket ws, string message, CancellationToken cancellationToken, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;

        return ws.SendAsync(encoding.GetBytes(message), WebSocketMessageType.Text, true, cancellationToken);
    }

    public static async Task<string> ReceiveStringAsync(this WebSocket ws, byte[] buffer, CancellationToken cancellationToken, Encoding? encoding = null)
    {
        encoding ??= Encoding.UTF8;

        var result = await ws.ReceiveAsync(buffer, cancellationToken);
        var str = encoding.GetString(buffer, 0, result.Count);

        return str;
    }
}