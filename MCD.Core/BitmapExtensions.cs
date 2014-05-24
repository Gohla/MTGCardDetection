using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace MCD.Core
{
    public static class BitmapExtensions
    {
        public static BitmapImage ToBitmapImage(this Bitmap bitmap)
        {
            BitmapImage image = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.Save(stream, ImageFormat.Bmp);
                stream.Seek(0, SeekOrigin.Begin);
                image.StreamSource = stream;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
    }
}
