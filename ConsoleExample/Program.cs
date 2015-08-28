using System;
using System.IO;
using System.Drawing;
using AForge.Video.DirectShow;
using System.Net.Sockets;
using System.Net;
using Robodem.Streaming;
using Robodem.Streaming.Video;
using FFmpeg.AutoGen;

namespace ConsoleExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Init.Initialize();
            using (VideoEncoder encoder = new VideoEncoder(640, 480, 50))
            using (BinaryWriter writer = new BinaryWriter(File.Open("test.mpg", FileMode.Create)))
            {

                Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                IPAddress broadcast = IPAddress.Parse("192.168.2.255");
                IPEndPoint endPoint = new IPEndPoint(broadcast, 1234);

                var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
                videoSource.NewFrame += (sender, eventArgs) =>
                {
                    var bytes = encoder.EncodeFrame(BitmapPtr(eventArgs.Frame));
                    if (bytes != null && bytes.Length > 0)
                    {
                        writer.Write(bytes);
                        s.SendTo(bytes, endPoint);
                    }
                };
                videoSource.Start();
                Console.ReadLine();
                videoSource.SignalToStop();
            }
        }

        // .\ffplay.exe -i -noinfbuf udp://192.168.2.255:1234

        private static IntPtr BitmapPtr(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            return ptr;
        }
    }
}
