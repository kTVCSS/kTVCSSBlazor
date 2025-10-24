using Microsoft.JSInterop;

namespace kTVCSSBlazor
{
    public static class DateTimeExtensions
    {
        private static IJSRuntime? _jsRuntime;

        public static void Initialize(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public static DateTime ToLocalTimeFromUtc(this DateTime utcDateTime)
        {
            if (_jsRuntime == null)
                throw new InvalidOperationException("DateTimeExtensions not initialized. Call Initialize() first.");

            if (utcDateTime.Kind != DateTimeKind.Utc)
            {
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);
            }

            var offsetMinutes = ((IJSInProcessRuntime)_jsRuntime).Invoke<int>("getTimezoneOffset");
            return utcDateTime.AddMinutes(-offsetMinutes);
        }

        public static DateTime ToLocalFromMoscow(this DateTime moscowTime)
        {
            if (_jsRuntime == null)
                throw new InvalidOperationException("DateTimeExtensions not initialized. Call Initialize() first.");

            var isoString = moscowTime.ToString("yyyy-MM-ddTHH:mm:ss");
            var localIsoString = ((IJSInProcessRuntime)_jsRuntime).Invoke<string>("convertMoscowToLocal", isoString);
            return DateTime.Parse(localIsoString);
        }
    }
}
