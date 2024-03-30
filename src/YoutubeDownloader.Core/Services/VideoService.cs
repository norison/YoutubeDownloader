using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Services;

public class VideoService : IVideoService
{
    private readonly VideoClient _videoClient;
    private readonly string _videoId;

    private Video? _video;
    private StreamManifest? _streamManifest;

    public VideoService(VideoClient videoClient, string videoId)
    {
        _videoClient = videoClient;
        _videoId = videoId;
    }

    public async Task<Video> GetVideoAsync(CancellationToken cancellationToken = default)
    {
        return _video ??= await _videoClient.GetAsync(_videoId, cancellationToken);
    }

    public async Task<IEnumerable<IAudioStreamInfo>> GetAudioStreamsAsync(CancellationToken cancellationToken = default)
    {
        var manifest = await GetStreamManifestAsync(cancellationToken);
        return manifest.GetAudioOnlyStreams();
    }

    public async Task<IEnumerable<IVideoStreamInfo>> GetVideoStreamsAsync(CancellationToken cancellationToken = default)
    {
        var manifest = await GetStreamManifestAsync(cancellationToken);
        return manifest.GetVideoOnlyStreams();
    }

    public async Task DownloadVideoAsync(
        IAudioStreamInfo audioStreamInfo,
        IVideoStreamInfo videoStreamInfo,
        string outputDirectory,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(outputDirectory))
        {
            throw new DirectoryNotFoundException($"Output directory '{outputDirectory}' does not exist.");
        }

        var video = await GetVideoAsync(cancellationToken);
        var fileName = $"{video.Id}.{videoStreamInfo.Container}";
        var filePath = Path.Combine(outputDirectory, fileName);

        var conversionRequest = new ConversionRequestBuilder(filePath)
            .SetPreset(ConversionPreset.VerySlow)
            .SetContainer(videoStreamInfo.Container)
            .Build();

        var streamInfos = new List<IStreamInfo> { audioStreamInfo, videoStreamInfo };
        await _videoClient.DownloadAsync(streamInfos, conversionRequest, progress, cancellationToken);
    }

    private async Task<StreamManifest> GetStreamManifestAsync(CancellationToken cancellationToken = default)
    {
        return _streamManifest ??= await _videoClient.Streams.GetManifestAsync(_videoId, cancellationToken);
    }
}