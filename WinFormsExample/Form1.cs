using AForge.Video.DirectShow;
using System.Drawing;
using System.Windows.Forms;
using Robodem.Streaming;
using Robodem.Streaming.Video;
using FFmpeg.AutoGen;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System;

namespace WinFormsExample
{
    public partial class Form1 : Form
    {
        private VideoCaptureDevice _videoSource;
        private VideoEncoder _encoder;
        private BinaryWriter _writer;
        private Socket _socket;
        private IPEndPoint _endPoint;

        public Form1()
        {
            InitializeComponent();

            Init.Initialize();
            _encoder = new VideoEncoder(640, 480, 50);
            _writer = new BinaryWriter(File.Open("test.mpg", FileMode.Create));
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _endPoint = new IPEndPoint(IPAddress.Parse("192.168.2.255"), 1234);

            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            _videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);
            _videoSource.NewFrame += VideoSource_NewFrame;
            _videoSource.Start();
        }

        private void VideoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (pictureBox1.Image != null)
                    pictureBox1.Image.Dispose();
                pictureBox1.Image = eventArgs.Frame.Clone(new Rectangle(0, 0, eventArgs.Frame.Width, eventArgs.Frame.Height), System.Drawing.Imaging.PixelFormat.DontCare);

                var bytes = _encoder.EncodeFrame(BitmapPtr(eventArgs.Frame));
                if (bytes != null && bytes.Length > 0)
                {
                    _writer.Write(bytes);
                    _socket.SendTo(bytes, _endPoint);
                }
            });

        }

        private static IntPtr BitmapPtr(Bitmap bmp)
        {
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            IntPtr ptr = bmpData.Scan0;
            return ptr;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _videoSource.SignalToStop();
        }
    }
}
