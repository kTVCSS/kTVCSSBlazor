using Microsoft.JSInterop;

namespace kTVCSSBlazor.Client.Services
{
    public interface IMobileDetectionService
    {
        Task<bool> IsMobileDeviceAsync();
    }

    public class MobileDetectionService : IMobileDetectionService
    {
        private readonly IJSRuntime _jsRuntime;

        public MobileDetectionService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<bool> IsMobileDeviceAsync()
        {
            return await _jsRuntime.InvokeAsync<bool>("isMobileDevice");
        }
    }
}
