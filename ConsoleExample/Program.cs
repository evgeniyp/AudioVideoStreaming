using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FFmpeg.AutoGen;
using System.IO;
using System.Runtime.InteropServices;
using System.Drawing;

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
            for (int i = 0; i < 250; i++)
            {
                var bytes = encoder.EncodeFrame(new byte[0]);
                writer.Write(bytes);
            }
            encoder.Free();

            //AVCodec* codec = FFmpegInvoke.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
            //if (codec == null) throw new Exception("Codec not found");

            //AVCodecContext* context = FFmpegInvoke.avcodec_alloc_context3(codec);
            //if (context == null) throw new Exception("Could not allocate video codec context");

            //context->bit_rate = 400000;
            //context->width = 352;
            //context->height = 288;
            //context->time_base = new AVRational() { num = 1, den = 25 }; // 25 fps
            //context->gop_size = 10; // emit one intra frame every ten frames
            //context->max_b_frames = 1;
            //context->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            //FFmpegInvoke.av_opt_set(context->priv_data, "preset", "slow", 0);
            //if (FFmpegInvoke.avcodec_open2(context, codec, null) < 0) throw new Exception("Could not open codec");



            //AVFrame* frame = FFmpegInvoke.avcodec_alloc_frame();
            //if (frame == null) throw new Exception("Could not allocate video frame");

            //frame->format = (int)context->pix_fmt;
            //frame->width = context->width;
            //frame->height = context->height;

            ///* the image can be allocated by any means and av_image_alloc() is
            // * just the most convenient way if av_malloc() is to be used */
            //var ret = FFmpegInvoke.av_image_alloc(&frame->data_0, frame->linesize, context->width, context->height, context->pix_fmt, 32);
            //if (ret < 0) throw new Exception("Could not allocate raw picture buffer\n");

            //BinaryWriter writer = new BinaryWriter(File.Open("test.h264", FileMode.Create));

            //AVPacket pkt;
            //int i, got_output;
            //for (i = 0; i < 250; i++)
            //{
            //    FFmpegInvoke.av_init_packet(&pkt);
            //    pkt.data = null;    // packet data will be allocated by the encoder
            //    pkt.size = 0;


            //    /* Y */
            //    for (int y = 0; y < context->height; y++)
            //        for (int x = 0; x < context->width; x++)
            //            frame->data_0[y * frame->linesize[0] + x] = (byte)(x + y + i * 3);

            //    /* Cb and Cr */
            //    for (int y = 0; y < context->height / 2; y++)
            //        for (int x = 0; x < context->width / 2; x++)
            //        {
            //            frame->data_1[y * frame->linesize[1] + x] = (byte)(128 + y + i * 2);
            //            frame->data_2[y * frame->linesize[2] + x] = (byte)(64 + x + i * 5);
            //        }

            //    /* encode the image */
            //    ret = FFmpegInvoke.avcodec_encode_video2(context, &pkt, frame, &got_output);
            //    if (ret < 0) throw new Exception("Error encoding frame\n");

            //    if (got_output != 0)
            //    {
            //        Console.WriteLine("Write frame {0}, size={1}", i, pkt.size);
            //        byte[] arr = new byte[pkt.size];
            //        Marshal.Copy((IntPtr)pkt.data, arr, 0, pkt.size);
            //        writer.Write(arr);
            //        FFmpegInvoke.av_free_packet(&pkt);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Empty packet");
            //    }
            //}

            //// get the delayed frames
            //for (got_output = 1; got_output != 0; i++)
            //{
            //    ret = FFmpegInvoke.avcodec_encode_video2(context, &pkt, null, &got_output);
            //    if (ret < 0) throw new Exception("Error encoding frame\n");
            //    if (got_output != 0)
            //    {
            //        Console.WriteLine("Write frame {0}, size={1}", i, pkt.size);
            //        byte[] arr = new byte[pkt.size];
            //        Marshal.Copy((IntPtr)pkt.data, arr, 0, pkt.size);
            //        writer.Write(arr);
            //        FFmpegInvoke.av_free_packet(&pkt);
            //    }
            //    else
            //    {
            //        Console.WriteLine("Empty packet");
            //    }
            //}

            //writer.Close();

            //FFmpegInvoke.av_free_packet(&pkt);
            //FFmpegInvoke.avcodec_close(context);
            //FFmpegInvoke.av_free(context);
            //FFmpegInvoke.av_freep(&frame->data_0);
            //FFmpegInvoke.avcodec_free_frame(&frame);


            //var ctx = FFmpegInvoke.sws_getContext(imgWidth, imgHeight, AVPixelFormat.AV_PIX_FMT_RGB24, imgWidth, imgHeight, AVPixelFormat.AV_PIX_FMT_YUV420P, 0, 0, 0, 0);
            //uint8_t* inData[1] = { rgb24Data }; // RGB24 have one plane
            //int inLinesize[1] = { 3 * imgWidth }; // RGB stride
            //FFmpegInvoke.sws_scale(ctx, inData, inLinesize, 0, imgHeight, dst_picture.data, dst_picture.linesize)
        }

        static byte[] BitmapBytes(Bitmap bmp)
        {
            // Lock the bitmap's bits.  
            Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bmpData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Unlock the bits.
            bmp.UnlockBits(bmpData);

            return rgbValues;
        }
    }
}
