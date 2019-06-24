using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ReactorServer.Secure;
using ReactorServer.Utils.Json;

namespace RemoteDesktopServer.Server
{
    public class RemoteVirtualClient : ReactorSecureVirtualClient
    {
        
        public RemoteVirtualClient(RemoteServer server) : base(server)
        {
            
        }

        protected override void SendSecurePacket(byte[] content)
        {
            base.SendSecurePacket(content);
        }

        private int current = 0;
        private int max = 0;
        private List<byte[]> packets = new List<byte[]>();

        protected override void HandleSecurePacket(byte[] content)
        {
            JsonObject o = JsonObject.readFrom(Encoding.Unicode.GetString(content));
            current = o.get("current").asInt();
            max = o.get("of").asInt();
            packets.Add(Convert.FromBase64String(o.get("data").asString()));
            if (current == max)
            {
                byte[] array = packets
                    .SelectMany(a => a)
                    .ToArray();
                Bitmap b = ByteToBitmap(array);
                RemoteServer rs = (RemoteServer)Server;
                rs.mw.Dispatcher.Invoke(new Action((() =>
                {
                    rs.mw.SetRemoteDesktop(b);
                })));
                current = 0;
                max = 0;
                packets = new List<byte[]>();
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
