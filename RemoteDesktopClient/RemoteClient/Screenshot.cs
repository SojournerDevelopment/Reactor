using System;
using System.Collections.Generic;
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
    }
}
