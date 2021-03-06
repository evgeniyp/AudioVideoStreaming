﻿using FFmpeg.AutoGen;
using Robodem.Streaming.Video;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace HelloWorld
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Init.Initialize();
        }

        private unsafe void Button_Test_Click(object sender, RoutedEventArgs e)
        {
            Bitmap bitmap = CreateTestBitmap();

            AVFrame* inFrame = FFmpegInvoke.avcodec_alloc_frame();
            if (inFrame == null) throw new Exception("Could not allocate video frame");
            inFrame->width = bitmap.Width;
            inFrame->height = bitmap.Height;
            inFrame->format = (int)AVPixelFormat.AV_PIX_FMT_BGR24;

            int ret1 = FFmpegInvoke.av_image_alloc(&inFrame->data_0, inFrame->linesize, bitmap.Width, bitmap.Height, AVPixelFormat.AV_PIX_FMT_BGR24, 32);
            if (ret1 < 0) throw new Exception("Could not allocate raw picture buffer");

            VideoHelper.UpdateFrame(inFrame, bitmap);
            VideoConverter converterToYuv = new VideoConverter(AVPixelFormat.AV_PIX_FMT_YUV420P);
            var data = converterToYuv.ConvertFrame(inFrame);

            var bitmap2 = VideoHelper.CreateBitmap(data, bitmap.Width, bitmap.Height);

            SetImageSource(bitmap2);
        }

        private Bitmap CreateTestBitmap()
        {
            Bitmap bitmap = new Bitmap(640, 480);
            Graphics g = Graphics.FromImage(bitmap);
            g.DrawLine(Pens.Blue, 0, 0, 640, 480);
            g.Flush();

            return bitmap;
        }

        private void SetImageSource(Bitmap bitmap)
        {
            MemoryStream ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Bmp);
            ms.Position = 0;
            BitmapImage bi = new BitmapImage();
            bi.BeginInit();
            bi.StreamSource = ms;
            bi.EndInit();

            Image.Source = bi;
        }
    }
}
