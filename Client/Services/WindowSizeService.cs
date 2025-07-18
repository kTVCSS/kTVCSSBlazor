using Microsoft.JSInterop;

namespace kTVCSSBlazor.Client.Services
{
    public class WindowSizeService : IAsyncDisposable
    {
        private DotNetObjectReference<WindowSizeService> _dotNetRef;
        private IJSObjectReference _resizer;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public event Action<int, int> OnResized;

        public async ValueTask InitializeAsync(IJSRuntime js)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _resizer = await js.InvokeAsync<IJSObjectReference>(
              "registerResizeCallback",
              _dotNetRef,
              nameof(NotifyResize)
            );
        }

        [JSInvokable]
        public void NotifyResize(int width, int height)
        {
            Width = width;
            Height = height;
            OnResized?.Invoke(width, height);
        }

        public int GetWidth() => Width;
        public int GetHeight() => Height;

        public async ValueTask DisposeAsync()
        {
            if (_resizer != null)
            {
                await _resizer.InvokeVoidAsync("dispose");
                await _resizer.DisposeAsync();
            }

            _dotNetRef?.Dispose();
        }
    }
}
