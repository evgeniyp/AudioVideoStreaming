using FFmpeg.AutoGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleExample
{
    public unsafe sealed class H264Encoder
    {
        private AVCodec* codec;
        private AVCodecContext* codec_context;
        private AVFrame* frame;
        private AVPacket pkt;
        private SwsContext* sws_context;
        private int i;

        public H264Encoder(int width, int height, int fps)
        {
            codec = FFmpegInvoke.avcodec_find_encoder(AVCodecID.AV_CODEC_ID_H264);
            if (codec == null) throw new Exception("Codec not found");

            codec_context = FFmpegInvoke.avcodec_alloc_context3(codec);
            if (codec_context == null) throw new Exception("Could not allocate video codec context");

            codec_context->bit_rate = 400000;
            codec_context->width = width;
            codec_context->height = height;
            codec_context->time_base = new AVRational() { num = 1, den = fps };
            codec_context->gop_size = 10; // emit one intra frame every ten frames
            codec_context->max_b_frames = 1;
            codec_context->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            FFmpegInvoke.av_opt_set(codec_context->priv_data, "preset", "fast", 0);
            if (FFmpegInvoke.avcodec_open2(codec_context, codec, null) < 0) throw new Exception("Could not open codec");

            frame = FFmpegInvoke.avcodec_alloc_frame();
            if (frame == null) throw new Exception("Could not allocate video frame");

            frame->format = (int)codec_context->pix_fmt;
            frame->width = codec_context->width;
            frame->height = codec_context->height;

            var ret = FFmpegInvoke.av_image_alloc(&frame->data_0, frame->linesize, codec_context->width, codec_context->height, codec_context->pix_fmt, 32);
            if (ret < 0) throw new Exception("Could not allocate raw picture buffer");

            sws_context = FFmpegInvoke.sws_getContext(width, height, AVPixelFormat.AV_PIX_FMT_RGB24, width, height, AVPixelFormat.AV_PIX_FMT_YUV420P, 0, null, null, null);
        }

        public byte[] EncodeFrame(byte[] rgb)
        {
            fixed (AVPacket* packet = &pkt)
            {
                int got_output;

                FFmpegInvoke.av_init_packet(packet);
                pkt.data = null;    // packet data will be allocated by the encoder
                pkt.size = 0;

                // TODO: convert rgb to YCbCr

                #region dummy image
                i++;
                /* Y */
                for (int y = 0; y < codec_context->height; y++)
                    for (int x = 0; x < codec_context->width; x++)
                        frame->data_0[y * frame->linesize[0] + x] = (byte)(x + y + i * 3);

                /* Cb and Cr */
                for (int y = 0; y < codec_context->height / 2; y++)
                    for (int x = 0; x < codec_context->width / 2; x++)
                    {
                        frame->data_1[y * frame->linesize[1] + x] = (byte)(128 + y + i * 2);
                        frame->data_2[y * frame->linesize[2] + x] = (byte)(64 + x + i * 5);
                    }
                #endregion

                var ret = FFmpegInvoke.avcodec_encode_video2(codec_context, packet, frame, &got_output);
                if (ret < 0) throw new Exception("Error encoding frame");

                if (got_output != 0)
                {
                    Console.WriteLine("Write frame {0}, size={1}", i, pkt.size);
                    byte[] arr = new byte[pkt.size];
                    Marshal.Copy((IntPtr)pkt.data, arr, 0, pkt.size);
                    FFmpegInvoke.av_free_packet(packet);
                    return arr;
                }
                else { return new byte[0]; }
            }
        }

        // TODO: refactor this to IDisposable pattern
        public void Free()
        {
            fixed (AVPacket* p = &pkt) { FFmpegInvoke.av_free_packet(p); }
            FFmpegInvoke.avcodec_close(codec_context);
            FFmpegInvoke.av_free(codec_context);
            FFmpegInvoke.av_freep(&frame->data_0);
            fixed (AVFrame** p = &frame) { FFmpegInvoke.avcodec_free_frame(p); }
        }
    }
}
