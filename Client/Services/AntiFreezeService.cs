using Microsoft.JSInterop;

namespace kTVCSSBlazor.Client.Services
{
    public class AntiFreezeService : IAsyncDisposable
    {
        private readonly IJSRuntime _js;
        private readonly HttpClient _http;
        private Timer? _serverPingTimer;
        private Timer? _internalTimer;
        private DotNetObjectReference<AntiFreezeService>? _objRef;
        private bool _isInitialized;

        public AntiFreezeService(IJSRuntime js, HttpClient http)
        {
            _js = js;
            _http = http;
        }

        public async Task StartAsync()
        {
            // Проверка что мы в браузере
            if (_js is IJSInProcessRuntime)
            {
                _objRef = DotNetObjectReference.Create(this);

                try
                {
                    await _js.InvokeVoidAsync("antiFreeze.setDotNetRef", _objRef);
                    await _js.InvokeVoidAsync("antiFreeze.start");
                    Console.WriteLine("Called antiFreeze.setDotNetRef && antiFreeze.start");
                    _isInitialized = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AntiFreeze JS init failed: {ex.Message}");
                }
            }

            // Пинг сервера каждые 3 минуты
            _serverPingTimer = new Timer(async _ =>
            {
                try
                {
                    await _http.GetAsync($"api/ping?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds()}");
                }
                catch { }
            }, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));

            // Внутренний .NET таймер каждые 30 секунд
            _internalTimer = new Timer(_ =>
            {
                _ = DateTime.UtcNow.Ticks;
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
        }

        [JSInvokable]
        public Task OnTabActivated()
        {
            Console.WriteLine($"Tab activated at {DateTime.Now}");
            return Task.CompletedTask;
        }

        public async ValueTask DisposeAsync()
        {
            // Останавливаем таймеры сразу
            _serverPingTimer?.Dispose();
            _internalTimer?.Dispose();

            // JS вызов только если был инициализирован
            if (_isInitialized && _js is IJSInProcessRuntime)
            {
                try
                {
                    await _js.InvokeVoidAsync("antiFreeze.stop");
                }
                catch
                {
                    // Игнорируем ошибки при dispose
                }
            }

            _objRef?.Dispose();
        }
    }
}
