using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RemoteDesktopClient.RemoteClient
{
    /// <summary>
    /// Klasse zum erzeugen beliebiger Screenshots
    /// </summary>
    public class Screenshot
    {
        public Screenshot()
        {
        }

        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        private static extern int GetForegroundWindow();

        /// <summary>
        /// Erzeugt ein Screenshot vom gesamten Desktop.
        /// </summary>
        /// <returns>Bitmap</returns>
        public Bitmap WholeDesktop()
        {
            return CreateScreenshot(0, 0, Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        }

        /// <summary>
        /// Erzeugt ein Screenshot vom übergebenen Bereich.
        /// </summary>
        /// <param name="topleft">Punkt des Bereich oben - links</param>
        /// <param name="bottomRight">Punkt des Bereich unten - rechts</param>
        /// <returns>Bitmap</returns>
        public Bitmap UserDefined(Point topleft, Point bottomRight)
        {
            return CreateScreenshot(topleft.X, topleft.Y, bottomRight.X, bottomRight.Y);
        }

        /// <summary>
        /// Erzeugt ein Screenshot vom Fenster des übergebenen Handels
        /// </summary>
        /// <param name="windowhandle"></param>
        /// <returns>Bitmap</returns>
        public Bitmap UserDefinedWindowHandle(IntPtr windowhandle)
        {
            return CreateScreenshot(windowhandle);
        }

        /// <summary>
        /// Erzeugt ein Screenshot vom aktiven Fenster.
        /// </summary>
        /// <returns>Bitmap</returns>
        public Bitmap ActiveWindow()
        {
            return CreateScreenshot((System.IntPtr) GetForegroundWindow());
        }

        private Bitmap CreateScreenshot(int left, int top, int width, int height)
        {
            Bitmap bmp = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(left, top, 0, 0, new Size(width, height));
            g.Dispose();
            return bmp;
        }

        private Bitmap CreateScreenshot(IntPtr windowhandle)
        {
            RECT windowRectangle;
            GetWindowRect(windowhandle, out windowRectangle);
            return CreateScreenshot(windowRectangle.Left, windowRectangle.Top,
                windowRectangle.Right - windowRectangle.Left, windowRectangle.Bottom - windowRectangle.Top);
        }

        [DllImport("gdi32.dll")]
        static extern bool BitBlt(IntPtr hdcDest, int nxDest, int nyDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int nHeight);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll")]
        static extern IntPtr DeleteObject(IntPtr hObject);

        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDc);

        [DllImport("gdi32.dll")]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hObject);

        const int SRCCOPY = 0x00CC0020;

        const int CAPTUREBLT = 0x40000000;

        public Bitmap CaptureRegion(Rectangle region)
        {
            IntPtr desktophWnd;
            IntPtr desktopDc;
            IntPtr memoryDc;
            IntPtr bitmap;
            IntPtr oldBitmap;
            bool success;
            Bitmap result;

            desktophWnd = GetDesktopWindow();
            desktopDc = GetWindowDC(desktophWnd);
            memoryDc = CreateCompatibleDC(desktopDc);
            bitmap = CreateCompatibleBitmap(desktopDc, region.Width, region.Height);
            oldBitmap = SelectObject(memoryDc, bitmap);

            success = BitBlt(memoryDc, 0, 0, region.Width, region.Height, desktopDc, region.Left, region.Top, SRCCOPY | CAPTUREBLT);

            try
            {
                if (!success)
                {
                    throw new Win32Exception();
                }

                result = Image.FromHbitmap(bitmap);
            }
            finally
            {
                SelectObject(memoryDc, oldBitmap);
                DeleteObject(bitmap);
                DeleteDC(memoryDc);
                ReleaseDC(desktophWnd, desktopDc);
            }

            return result;
        }

        public Bitmap CaptureDesktop()
        {
            return this.CaptureDesktop(false);
        }

        public Bitmap CaptureDesktop(bool workingAreaOnly)
        {
            Rectangle desktop;
            Screen[] screens;

            desktop = Rectangle.Empty;
            screens = Screen.AllScreens;

            for (int i = 0; i < screens.Length; i++)
            {
                Screen screen;

                screen = screens[i];

                desktop = Rectangle.Union(desktop, workingAreaOnly ? screen.WorkingArea : screen.Bounds);
            }

            return this.CaptureRegion(desktop);
        }

    }
}
