using System;

using Microsoft.JSInterop;

public class KeepAliveService
{
    private readonly IJSRuntime _jsRuntime;
    private Timer? _timer;

    public KeepAliveService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task StartKeepAlive()
    {
        _timer = new Timer(async _ => 
        {
            await SendKeepAliveMessage();
        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(20));

        await _jsRuntime.InvokeVoidAsync("startKeepAliveActivities");
    }

    public async Task SendKeepAliveMessage()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("sendServiceWorkerMessage", 
                new { type = "MANUAL_PING" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка отправки keep-alive: {ex.Message}");
        }
    }

    public void StopKeepAlive()
    {
        _timer?.Dispose();
        _timer = null;
    }
}
