using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReactorServer.Secure;
using ReactorServer.Utils.Json;

namespace RemoteDesktopServer.Server
{
    public class RemoteVirtualClient : ReactorSecureVirtualClient
    {
        
        public RemoteVirtualClient(RemoteServer server) : base(server)
        {
            StartTimer();
        }

        protected override void SendSecurePacket(byte[] content)
        {
            base.SendSecurePacket(content);
        }

        public int frames = 0;
        public string start = "";
        public string end = "";

        protected void StartTimer()
        {
            // Timer timer = new Timer(Tick,frames,0,1000);
        }

        protected void Tick(object info)
        {
            Debug.WriteLine("FPS: "+frames);
            frames = 0;
        }

        protected override void HandleSecurePacket(byte[] content)
        {
            if (frames == 0)
            {
                start = DateTime.Now.ToString("HH:mm:ss.ffffff");
            }
            frames++;
            Bitmap b = ByteToBitmap(content);
            RemoteServer rs = (RemoteServer)Server;
            
            rs.mw.Dispatcher.Invoke(new Action((() =>
            {
                rs.mw.SetRemoteDesktop(b);
            })));

            if (frames > 10)
            {
                end = DateTime.Now.ToString("HH:mm:ss.ffffff");
                Debug.WriteLine("FPS: "+start+" END: "+end+" FRAMES: "+frames);
                frames = 0;
            }

        }

        public static byte[] ImageToByte2(Image img)
        {
            using (var stream = new MemoryStream())
            {
                img.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static Bitmap ByteToBitmap(byte[] bitmap)
        {
            ImageConverter ic = new ImageConverter();
            Image img = (Image)ic.ConvertFrom(bitmap);
            return new Bitmap(img);
        }

    }
}
