using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Services;

public class VideoServiceFactory : IVideoServiceFactory
{
    public IVideoService GetVideoService(string videoId)
    {
        return new VideoService(new VideoClient(new HttpClient()), videoId);
    }
}