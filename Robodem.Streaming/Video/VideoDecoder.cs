using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;

namespace Robodem.Streaming.Video
{
    public unsafe class VideoDecoder : DisposableBase
    {
        private const AVCodecID CodecId = AVCodecID.AV_CODEC_ID_MPEG2VIDEO;

        private readonly AVCodecContext* codec_context;
        private readonly AVFrame* avFrame;

        public VideoDecoder()
        {   
            AVCodec* codec = FFmpegInvoke.avcodec_find_decoder(CodecId);
            if (codec == null) throw new Exception("Codec not found");

            codec_context = FFmpegInvoke.avcodec_alloc_context3(codec);
            if (codec_context == null) throw new Exception("Could not allocate video codec context");

            if (FFmpegInvoke.avcodec_open2(codec_context, codec, null) < 0) throw new Exception("Could not open codec");

            avFrame = FFmpegInvoke.avcodec_alloc_frame();
            if (avFrame == null) throw new Exception("Could not allocate video frame");
        }

        public bool TryDecode(ref AVPacket packet, out AVFrame* pFrame)
        {
            int gotPicture;
            fixed (AVPacket* pPacket = &packet)
            {
                int decodedSize = FFmpegInvoke.avcodec_decode_video2(codec_context, avFrame, &gotPicture, pPacket);
                if (decodedSize < 0) { Console.WriteLine("Error while decoding frame."); }
            }
            pFrame = avFrame;
            return gotPicture == 1;
        }

        protected override void DisposeManaged() { }

        protected override void DisposeUnmanaged()
        {
            FFmpegInvoke.avcodec_close(codec_context);

            AVFrame* frameOnStack = avFrame;
            FFmpegInvoke.avcodec_free_frame(&frameOnStack);
        }
    }
}
