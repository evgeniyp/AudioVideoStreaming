using FFmpeg.AutoGen;
using System;
using System.Runtime.InteropServices;

namespace Robodem.Streaming.Video
{
    public unsafe sealed class VideoEncoder : DisposableBase
    {
        private const AVCodecID CODEC_ID = AVCodecID.AV_CODEC_ID_MPEG2VIDEO;
        private const AVPixelFormat CODEC_PIXEL_FORMAT = AVPixelFormat.AV_PIX_FMT_YUV420P;
        private const AVPixelFormat INPUT_PIXEL_FORMAT = AVPixelFormat.AV_PIX_FMT_BGR24;

        private AVCodecContext* _codec_context;
        private AVFrame* _avFrameYUV;
        private AVFrame* _avFrameBGR;
        private AVPacket _pkt;

        private VideoConverter _converter;

        public VideoEncoder(int width, int height, int fps)
        {
            _converter = new VideoConverter(CODEC_PIXEL_FORMAT);

            AVCodec* codec = FFmpegInvoke.avcodec_find_encoder(CODEC_ID);
            if (codec == null) throw new Exception("Codec not found");

            _codec_context = FFmpegInvoke.avcodec_alloc_context3(codec);
            if (_codec_context == null) throw new Exception("Could not allocate video codec context");

            _codec_context->bit_rate = 50000;
            _codec_context->width = width;
            _codec_context->height = height;
            _codec_context->time_base = new AVRational() { num = 1, den = fps };
            _codec_context->gop_size = 10; // emit one intra frame every ten frames
            _codec_context->max_b_frames = 1;
            _codec_context->pix_fmt = CODEC_PIXEL_FORMAT;
            FFmpegInvoke.av_opt_set(_codec_context->priv_data, "preset", "fast", 0);
            if (FFmpegInvoke.avcodec_open2(_codec_context, codec, null) < 0) throw new Exception("Could not open codec");

            _avFrameYUV = FFmpegInvoke.avcodec_alloc_frame();
            if (_avFrameYUV == null) throw new Exception("Could not allocate video frame");
            _avFrameYUV->format = (int)CODEC_PIXEL_FORMAT;
            _avFrameYUV->width = width;
            _avFrameYUV->height = height;

            var ret1 = FFmpegInvoke.av_image_alloc(&_avFrameYUV->data_0, _avFrameYUV->linesize, width, height, CODEC_PIXEL_FORMAT, 32);
            if (ret1 < 0) throw new Exception("Could not allocate raw picture buffer");

            _avFrameBGR = FFmpegInvoke.avcodec_alloc_frame();
            if (_avFrameBGR == null) throw new Exception("Could not allocate video frame");
            _avFrameBGR->format = (int)INPUT_PIXEL_FORMAT;
            _avFrameBGR->width = width;
            _avFrameBGR->height = height;

            var ret2 = FFmpegInvoke.av_image_alloc(&_avFrameBGR->data_0, _avFrameBGR->linesize, width, height, INPUT_PIXEL_FORMAT, 32);
            if (ret2 < 0) throw new Exception("Could not allocate raw picture buffer");
        }

        public byte[] EncodeFrame(IntPtr rgb)
        {
            fixed (AVPacket* packet = &_pkt)
            {
                FFmpegInvoke.av_init_packet(packet);
                _pkt.data = null;
                _pkt.size = 0;

                FFmpegInvoke.avpicture_fill((AVPicture*)_avFrameBGR, (byte*)rgb, INPUT_PIXEL_FORMAT, _avFrameBGR->width, _avFrameBGR->height);
                var convertedBytes = _converter.ConvertFrame(_avFrameBGR);
                fixed  (byte* yuv = &convertedBytes[0])
                {
                    FFmpegInvoke.avpicture_fill((AVPicture*)_avFrameYUV, yuv, CODEC_PIXEL_FORMAT, _avFrameYUV->width, _avFrameYUV->height);
                }


                int got_output;
                var ret = FFmpegInvoke.avcodec_encode_video2(_codec_context, packet, _avFrameYUV, &got_output);
                if (ret < 0) throw new Exception("Error encoding frame");

                if (got_output != 0)
                {
                    byte[] arr = new byte[_pkt.size];
                    Marshal.Copy((IntPtr)_pkt.data, arr, 0, _pkt.size);
                    FFmpegInvoke.av_free_packet(packet);
                    return arr;
                }
                else { return null; }
            }
        }

        protected override void DisposeManaged() { }

        protected override void DisposeUnmanaged()
        {
            fixed (AVPacket* p = &_pkt) { FFmpegInvoke.av_free_packet(p); }

            FFmpegInvoke.avcodec_close(_codec_context);
            FFmpegInvoke.av_free(_codec_context);

            FFmpegInvoke.av_freep(&_avFrameYUV->data_0);
            fixed (AVFrame** p = &_avFrameYUV) { FFmpegInvoke.avcodec_free_frame(p); }

            FFmpegInvoke.av_freep(&_avFrameBGR->data_0);
            fixed (AVFrame** p = &_avFrameBGR) { FFmpegInvoke.avcodec_free_frame(p); }
        }
    }
}
