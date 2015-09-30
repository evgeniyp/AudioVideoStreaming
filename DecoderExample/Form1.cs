using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using Robodem.Streaming.Video;
using FFmpeg.AutoGen;

namespace DecoderExample
{
    public partial class Form1 : Form
    {
        private VideoDecoder _decoder;
        private AVPacket _avPacket;
        private VideoConverter _converter;

        private Socket _socket;
        private EndPoint _endPoint;
        private Thread _socketThread;
        private byte[] _receiveBuffer = new byte[65536];

        public Form1()
        {
            InitializeComponent();

            Init.Initialize();

            _decoder = new VideoDecoder();
            _avPacket = new AVPacket();
            _converter = new VideoConverter(AVPixelFormat.AV_PIX_FMT_RGB24);

            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _endPoint = new IPEndPoint(IPAddress.Any, 1234);
            _socket.Bind(_endPoint);
            _socketThread = new Thread(SocketThread) { IsBackground = true };
            _socketThread.Start();
        }

        private unsafe void SocketThread()
        {
            while (true)
            {
                var bytesReceived = _socket.Receive(_receiveBuffer);
                if (bytesReceived == 0) { Thread.Sleep(1); continue; }
                var arr = new byte[bytesReceived];
                Array.Copy(_receiveBuffer, arr, bytesReceived);

                fixed (byte* pData = &arr[0])
                {
                    _avPacket.data = pData;
                    _avPacket.size = bytesReceived;
                    AVFrame* pFrame;
                    if (_decoder.TryDecode(ref _avPacket, out pFrame))
                    {
                        byte[] data = _converter.ConvertFrame(pFrame);
                        var bitmap = VideoHelper.CreateBitmap(data, pFrame->width, pFrame->height);
                        UpdateImage(bitmap);
                    }
                }
            }
        }

        private void UpdateImage(Bitmap bitmap)
        {
            this.Invoke((MethodInvoker)delegate
            {
                if (pictureBox1.Image != null) { pictureBox1.Image.Dispose(); }
                Bitmap clone = (Bitmap)bitmap.Clone();
                pictureBox1.Image = clone;
            });
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _socketThread.Abort();
        }
    }
}
