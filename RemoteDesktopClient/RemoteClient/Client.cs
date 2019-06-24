using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ReactorClient.Secure;
using ReactorClient.Utils.Json;

namespace RemoteDesktopClient.RemoteClient
{
    public class Client : ReactorSecureClient
    {
        private Thread captureThread;
        private bool capture = true;
        
        protected override void HandleSecurePacket(byte[] data)
        {
            base.HandleSecurePacket(data);
        }

        protected override void ConnectionSecured()
        {
            captureThread = new Thread(RemoteDesktop);
            captureThread.Start();
        }

        private void RemoteDesktop()
        {
            Screenshot s = new Screenshot();

            while (capture)
            {
                Bitmap b = s.WholeDesktop();
                byte[] toSend = ImageToByte2(b);

                List<byte[]> packets = ArraySplit(toSend, 5000);
                for(int i = 0;i<packets.Count();i++)
                {
                    JsonObject data = new JsonObject();
                    data.set("current", i+1);
                    data.set("of", packets.Count());
                    data.set("data", Convert.ToBase64String(packets[i]));
                    SendSecurePacket(Encoding.Unicode.GetBytes(data.ToString()));
                }
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

        private static List<byte[]> ArraySplit(byte[] bArray, int intBufforLengt)
        {
            List<byte[]> toRet = new List<byte[]>();
            int bArrayLenght = bArray.Length;
            byte[] bReturn = null;

            int i = 0;
            for (; bArrayLenght > (i + 1) * intBufforLengt; i++)
            {
                bReturn = new byte[intBufforLengt];
                Array.Copy(bArray, i * intBufforLengt, bReturn, 0, intBufforLengt);
                toRet.Add(bReturn);
            }

            int intBufforLeft = bArrayLenght - i * intBufforLengt;
            if (intBufforLeft > 0)
            {
                bReturn = new byte[intBufforLeft];
                Array.Copy(bArray, i * intBufforLengt, bReturn, 0, intBufforLeft);
                toRet.Add(bReturn);
            }
            return toRet;
        }

    }
}
