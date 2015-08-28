using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FFmpeg.AutoGen
{
    public static class Init
    {
        public static void Initialize()
        {
            string ffmpegPath = string.Format(@"ffmpeg/{0}", Environment.Is64BitProcess ? "x64" : "x86");
            InteropHelper.RegisterLibrariesSearchPath(ffmpegPath);

            FFmpegInvoke.av_register_all();
            FFmpegInvoke.avcodec_register_all();
            FFmpegInvoke.avformat_network_init();
        }
    }
}
