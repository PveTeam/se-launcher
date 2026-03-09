using FFmpeg.AutoGen.Abstractions;
using static FFmpeg.AutoGen.Abstractions.ffmpeg;

namespace CringeLauncher.Platform.Xplat;

public sealed unsafe class FFmpegStream : IDisposable
{
    private readonly AVFormatContext* _formatContext;
    private readonly AVCodecContext* _codecContext;
    private readonly SwrContext* _swrContext;
    private readonly AVFrame* _frame;
    private readonly AVPacket* _packet;
    private readonly MemoryStream _decodedStream;
    private readonly int _audioStreamIndex;

    public int SampleRate { get; }
    public int Channels { get; }
    public int BitsPerSample => 16;

    static FFmpegStream()
    {
        FFmpeg.AutoGen.Bindings.DynamicallyLoaded.DynamicallyLoadedBindings.Initialize();
    }

    public FFmpegStream(string filePath)
    {
        _formatContext = avformat_alloc_context();
        var formatContext = _formatContext;
        if (avformat_open_input(&formatContext, filePath, null, null) != 0)
            throw new FFmpegException($"Could not open input file: {filePath}");

        if (avformat_find_stream_info(_formatContext, null) < 0)
            throw new FFmpegException("Could not find stream information");

        _audioStreamIndex = av_find_best_stream(_formatContext, AVMediaType.AVMEDIA_TYPE_AUDIO, -1, -1, null, 0);
        if (_audioStreamIndex < 0)
            throw new FFmpegException("Could not find any audio stream in the file");

        var audioStream = _formatContext->streams[_audioStreamIndex];
        var codecParams = audioStream->codecpar;
        var decoder = avcodec_find_decoder(codecParams->codec_id);
        if (decoder == null)
            throw new FFmpegException("Unsupported codec");

        _codecContext = avcodec_alloc_context3(decoder);
        avcodec_parameters_to_context(_codecContext, codecParams);
        if (avcodec_open2(_codecContext, decoder, null) < 0)
            throw new FFmpegException("Could not open codec");
        
        _codecContext->pkt_timebase = audioStream->time_base;

        SampleRate = _codecContext->sample_rate;
        Channels = _codecContext->ch_layout.nb_channels;

        _packet = av_packet_alloc();
        _frame = av_frame_alloc();

        // Setup resampler
        AVChannelLayout outLayout = default;
        av_channel_layout_from_mask(&outLayout, _codecContext->ch_layout.nb_channels == 2 ? AV_CH_LAYOUT_STEREO : AV_CH_LAYOUT_MONO);
        av_channel_layout_default(&_codecContext->ch_layout, _codecContext->ch_layout.nb_channels);
        fixed (SwrContext** ctx = &_swrContext)
            if (swr_alloc_set_opts2(ctx,
                    &outLayout, AVSampleFormat.AV_SAMPLE_FMT_S16, SampleRate,
                    &_codecContext->ch_layout, _codecContext->sample_fmt, _codecContext->sample_rate, 
                    0, null
                ) != 0)
                throw new FFmpegException("Could not allocate swr");
        
        if (swr_init(_swrContext) != 0)
            throw new FFmpegException("Could not init swr");

        _decodedStream = new();
        DecodeAll();
    }
        
    private void DecodeAll()
    {
        while (av_read_frame(_formatContext, _packet) >= 0)
        {
            if (_packet->stream_index == _audioStreamIndex)
            {
                if (avcodec_send_packet(_codecContext, _packet) >= 0)
                {
                    while (avcodec_receive_frame(_codecContext, _frame) >= 0)
                    {
                        byte* output;
                        var outSamples = (int)av_rescale_rnd(swr_get_delay(_swrContext, SampleRate) +
                                                         _frame->nb_samples, SampleRate, _codecContext->sample_rate, AVRounding.AV_ROUND_UP);
                        av_samples_alloc(&output, null, Channels, outSamples, AVSampleFormat.AV_SAMPLE_FMT_S16, 0);
                        outSamples = swr_convert(_swrContext, &output, outSamples,
                            _frame->extended_data, _frame->nb_samples);
                        _decodedStream.Write(new(output, outSamples * Channels * sizeof(short)));
                        av_freep(&output);
                    }
                }
            }
            av_packet_unref(_packet);
        }
        _decodedStream.Position = 0;
    }

    public ReadOnlySpan<byte> Data => _decodedStream.GetBuffer().AsSpan(0, (int)_decodedStream.Length);

    public void Dispose()
    {
        fixed (AVPacket** packet = &_packet)
            av_packet_free(packet);
        fixed (AVFrame** frame = &_frame)
            av_frame_free(frame);

        fixed (SwrContext** swr = &_swrContext) 
            swr_free(swr);
            
        fixed (AVCodecContext** ctx = &_codecContext)
            avcodec_free_context(ctx);
        fixed (AVFormatContext** ctx = &_formatContext)
            avformat_close_input(ctx);

        _decodedStream?.Dispose();
    }
}

public class FFmpegException(string message) : Exception(message);
