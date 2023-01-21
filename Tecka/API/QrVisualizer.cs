using Windows.UI.Xaml.Media;
using ZXing;

namespace Tecka
{
    public static class QrVisualizer
    {
        public static ImageSource ToQrCode(string data, int width = 640, int height = 640)
        {
            var options = new ZXing.Common.EncodingOptions() { Width = width, Height = height };
            options.Hints.Add(EncodeHintType.ERROR_CORRECTION, ZXing.QrCode.Internal.ErrorCorrectionLevel.Q);
            options.Hints.Add(EncodeHintType.MARGIN, 0);
            IBarcodeWriter writer = new BarcodeWriter { Format = BarcodeFormat.QR_CODE, Options = options };
            var result = writer.Write(data);
            return result;
        }
    }
}
