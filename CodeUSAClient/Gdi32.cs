using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace CodeUSAClient
{
    public class Gdi32
    {
        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight,
            IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);

        public static Image CaptureWindow(IntPtr handle)
        {
            var windowDC = User32.GetWindowDC(handle);
            var rect = new RECT();
            User32.GetWindowRect(handle, ref rect);
            var nWidth = rect.right - rect.left;
            var nHeight = rect.bottom - rect.top;
            var hDC = CreateCompatibleDC(windowDC);
            var hObject = CreateCompatibleBitmap(windowDC, nWidth, nHeight);
            var ptr4 = SelectObject(hDC, hObject);
            BitBlt(hDC, 0, 0, nWidth, nHeight, windowDC, 0, 0, 0xcc0020);
            SelectObject(hDC, ptr4);
            DeleteDC(hDC);
            User32.ReleaseDC(handle, windowDC);
            Image image = Image.FromHbitmap(hObject);
            DeleteObject(hObject);
            return image;
        }

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
    }
}