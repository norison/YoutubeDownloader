using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.Core.Services;

public interface IVideoService
{
    Task<Video> GetVideoAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<IAudioStreamInfo>> GetAudioStreamsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<IVideoStreamInfo>> GetVideoStreamsAsync(CancellationToken cancellationToken = default);

    Task DownloadVideoAsync(
        IAudioStreamInfo audioStreamInfo,
        IVideoStreamInfo videoStreamInfo,
        string outputDirectory,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default);
}