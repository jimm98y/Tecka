using Windows.Foundation.Metadata;

namespace Tecka.Utility
{
    public static class PlatformDetectionUtils
    {
        public static bool IsWindowsMobile()
        {
            return ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons");
        }
    }
}
