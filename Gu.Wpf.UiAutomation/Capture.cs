// ReSharper disable RedundantNameQualifier
namespace Gu.Wpf.UiAutomation
{
    using System;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Media.Imaging;
    using Gu.Wpf.UiAutomation.WindowsAPI;

    /// <summary>
    /// Provides methods to capture screenshots or partially screenshots.
    /// </summary>
#pragma warning disable CA1060 // Move pinvokes to native methods class
    public static class Capture
#pragma warning restore CA1060 // Move pinvokes to native methods class
    {
        /// <summary>
        /// Captures the whole screen (all monitors).
        /// </summary>
        public static Bitmap Screen()
        {
            // https://stackoverflow.com/a/3072580/1069200
            var sz = new System.Drawing.Size((int)SystemParameters.VirtualScreenWidth, (int)SystemParameters.VirtualScreenHeight);
            var hDesk = User32.GetDesktopWindow();
            var hSrce = User32.GetWindowDC(hDesk);
            var hDest = CreateCompatibleDC(hSrce);
            var hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
            var hOldBmp = SelectObject(hDest, hBmp);
            //// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            _ = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, 0, 0, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            var bmp = Image.FromHbitmap(hBmp);
            _ = SelectObject(hDest, hOldBmp);
            _ = DeleteObject(hBmp);
            _ = DeleteDC(hDest);
            _ = ReleaseDC(hDesk, hSrce);
            return bmp;
        }

        public static BitmapImage ScreenWpf()
        {
            using var screen = Screen();
            return screen.ToWpf();
        }

        /// <summary>
        /// Captures the screen and saves it to a file.
        /// </summary>
        public static void ScreenToFile(string filePath)
        {
            using var bmp = Screen();
            bmp.Save(filePath, ImageFormat.Png);
        }

        /// <summary>
        /// Captures an element and returns the image.
        /// Note that a sleep may be required before if the control is newly loaded.
        /// </summary>
        public static Bitmap Element(UiElement element)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return Rectangle(element.Bounds);
        }

        /// <summary>
        /// Captures an element and saves it to a file.
        /// Note that a sleep may be required before if the control is newly loaded.
        /// </summary>
        public static void ElementToFile(UiElement element, string filePath)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (filePath is null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            using var bmp = Rectangle(element.Bounds);
            bmp.Save(filePath, ImageFormat.Png);
        }

        /// <summary>
        /// Captures a specific area and saves it to a file.
        /// </summary>
        public static void RectangleToFile(Rect bounds, string filePath)
        {
            using var bmp = Rectangle(bounds);
            bmp.Save(filePath, ImageFormat.Png);
        }

        public static BitmapImage RectangleWpf(Rect bounds)
        {
            using var rectangle = Rectangle(bounds);
            return rectangle.ToWpf();
        }

        /// <summary>
        /// Captures a specific area from the screen.
        /// </summary>
        public static Bitmap Rectangle(Rect bounds)
        {
            // https://stackoverflow.com/a/3072580/1069200
            var sz = new System.Drawing.Size((int)bounds.Width, (int)bounds.Height);
            var hDesk = User32.GetDesktopWindow();
            var hSrce = User32.GetWindowDC(hDesk);
            var hDest = CreateCompatibleDC(hSrce);
            var hBmp = CreateCompatibleBitmap(hSrce, sz.Width, sz.Height);
            var hOldBmp = SelectObject(hDest, hBmp);
            //// ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
            _ = BitBlt(hDest, 0, 0, sz.Width, sz.Height, hSrce, (int)bounds.X, (int)bounds.Y, CopyPixelOperation.SourceCopy | CopyPixelOperation.CaptureBlt);
            var bmp = Image.FromHbitmap(hBmp);
            _ = SelectObject(hDest, hOldBmp);
            _ = DeleteObject(hBmp);
            _ = DeleteDC(hDest);
            _ = ReleaseDC(hDesk, hSrce);
            return bmp;
        }

        /// <summary>
        /// Converts a WinForms bitmap to a WPF bitmap.
        /// </summary>
        public static BitmapImage ToWpf(this Bitmap bitmap)
        {
            if (bitmap is null)
            {
                throw new ArgumentNullException(nameof(bitmap));
            }

            using var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            _ = memory.Seek(0, SeekOrigin.Begin);
            bitmapImage.StreamSource = memory;
            bitmapImage.EndInit();
            bitmapImage.Freeze();
            return bitmapImage;
        }

        // P/Invoke declarations
        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int wDest, int hDest, IntPtr hdcSource, int xSrc, int ySrc, CopyPixelOperation rop);

        [DllImport("user32.dll")]
        private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteDC(IntPtr hDc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr DeleteObject(IntPtr hDc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr bmp);
    }
}
