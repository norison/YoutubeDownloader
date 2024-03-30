namespace YoutubeDownloader.Core.Services;

public interface IVideoServiceFactory
{
    IVideoService GetVideoService(string videoId);
}