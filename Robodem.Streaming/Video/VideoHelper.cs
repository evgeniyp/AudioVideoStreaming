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
            UpdateBitmap(ref bitmap, bitmapData);
            return bitmap;
        }

        public static void UpdateBitmap(ref Bitmap bitmap, byte[] bitmapData)
        {
            var rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            BitmapData data = bitmap.LockBits(rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(bitmapData, 0, data.Scan0, bitmapData.Length);
            bitmap.UnlockBits(data);
        }
    }
}