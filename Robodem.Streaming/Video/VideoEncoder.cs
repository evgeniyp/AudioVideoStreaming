using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;

namespace Robodem.Streaming.Video
{
    public unsafe sealed class VideoEncoder : DisposableBase
    {
        private const AVCodecID CodecId = AVCodecID.AV_CODEC_ID_MPEG2VIDEO;

        private AVCodecContext* codec_context;
        private readonly AVFrame* avFrame;
        private AVPacket pkt;
        int i;

        public VideoEncoder(int width, int height, int fps)
        {
            AVCodec* codec = FFmpegInvoke.avcodec_find_encoder(CodecId);
            if (codec == null) throw new Exception("Codec not found");

            codec_context = FFmpegInvoke.avcodec_alloc_context3(codec);
            if (codec_context == null) throw new Exception("Could not allocate video codec context");

            codec_context->bit_rate = 50000;
            codec_context->width = width;
            codec_context->height = height;
            codec_context->time_base = new AVRational() { num = 1, den = fps };
            codec_context->gop_size = 10; // emit one intra frame every ten frames
            codec_context->max_b_frames = 1;
            codec_context->pix_fmt = AVPixelFormat.AV_PIX_FMT_YUV420P;
            FFmpegInvoke.av_opt_set(codec_context->priv_data, "preset", "fast", 0);
            if (FFmpegInvoke.avcodec_open2(codec_context, codec, null) < 0) throw new Exception("Could not open codec");

            avFrame = FFmpegInvoke.avcodec_alloc_frame();
            if (avFrame == null) throw new Exception("Could not allocate video frame");
            avFrame->format = (int)codec_context->pix_fmt;
            avFrame->width = codec_context->width;
            avFrame->height = codec_context->height;

            var ret1 = FFmpegInvoke.av_image_alloc(&avFrame->data_0, avFrame->linesize, codec_context->width, codec_context->height, codec_context->pix_fmt, 32);
            if (ret1 < 0) throw new Exception("Could not allocate raw picture buffer");
        }

        public byte[] EncodeFrame(IntPtr rgb)
        {
            fixed (AVPacket* packet = &pkt)
            {
                i++;

                FFmpegInvoke.av_init_packet(packet);
                pkt.data = null;
                pkt.size = 0;

                // taking only red component
                for (int y = 0; y < codec_context->height; y++)
                {
                    for (int x = 0; x < codec_context->width; x++)
                    {
                        var value = ((byte*)rgb)[3 * (640 * y + x)];
                        avFrame->data_0[y * avFrame->linesize[0] + x] = value;
                    }
                }

                for (int y = 0; y < codec_context->height / 2; y++)
                    for (int x = 0; x < codec_context->width / 2; x++)
                    {
                        avFrame->data_1[y * avFrame->linesize[1] + x] = 128;
                        avFrame->data_2[y * avFrame->linesize[2] + x] = 128;
                    }

                int got_output;
                var ret = FFmpegInvoke.avcodec_encode_video2(codec_context, packet, avFrame, &got_output);
                if (ret < 0) throw new Exception("Error encoding frame");

                if (got_output != 0)
                {
                    byte[] arr = new byte[pkt.size];
                    Marshal.Copy((IntPtr)pkt.data, arr, 0, pkt.size);
                    FFmpegInvoke.av_free_packet(packet);
                    return arr;
                }
                else { return null; }
            }
        }

        protected override void DisposeManaged() { }

        protected override void DisposeUnmanaged()
        {
            fixed (AVPacket* p = &pkt) { FFmpegInvoke.av_free_packet(p); }
            FFmpegInvoke.avcodec_close(codec_context);
            FFmpegInvoke.av_free(codec_context);
            FFmpegInvoke.av_freep(&avFrame->data_0);

            AVFrame* yuvFrameOnStack = avFrame;
            FFmpegInvoke.avcodec_free_frame(&yuvFrameOnStack);
        }
    }
}
