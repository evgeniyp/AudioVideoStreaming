using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFmpeg.AutoGen;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;
using AForge.Video.DirectShow;

namespace ConsoleExample
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            string ffmpegPath = string.Format(@"ffmpeg/{0}", Environment.Is64BitProcess ? "x64" : "x86");
            InteropHelper.RegisterLibrariesSearchPath(ffmpegPath);

            FFmpegInvoke.av_register_all();
            FFmpegInvoke.avcodec_register_all();
            FFmpegInvoke.avformat_network_init();

            H264Encoder encoder = new H264Encoder(640, 480, 25);

            BinaryWriter writer = new BinaryWriter(File.Open("test.h264", FileMode.Create));

            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            VideoCaptureDevice videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource.NewFrame += (sender, eventArgs) =>
            {
                eventArgs.Frame.Save("frame.jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
                writer.Write(encoder.EncodeFrame(BitmapPtr(eventArgs.Frame)));
            };
            videoSource.Start();
            Console.ReadLine();
            videoSource.SignalToStop();
            encoder.Free();
        }

        static IntPtr BitmapPtr(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            return ptr;
        }
    }
}
