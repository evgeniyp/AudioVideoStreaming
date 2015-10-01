using FFmpeg.AutoGen;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace Robodem.Streaming.Video
{
    public static class VideoHelper
    {
        public static Bitmap CreateBitmap(byte[] bitmapData, int width, int height)
        {
            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            UpdateBitmap(bitmap, bitmapData);
            return bitmap;
        }

        public static void UpdateBitmap(Bitmap bitmap, byte[] bitmapData)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(bitmapData, 0, data.Scan0, bitmapData.Length);
            bitmap.UnlockBits(data);
        }

        public unsafe static void UpdateFrame(AVFrame* avFrame, Bitmap bitmap)
        {
            Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData bmpData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
            FFmpegInvoke.avpicture_fill((AVPicture*)avFrame, (byte*)bmpData.Scan0, (AVPixelFormat)avFrame->format, bitmap.Width, bitmap.Height);
            bitmap.UnlockBits(bmpData);
        }
    }
}