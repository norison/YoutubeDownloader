using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.ConsoleUI;

public static class YoutubeLoader
{
    public static async Task DownloadVideoAsync(string videoId)
    {
        var youtube = new YoutubeClient();
        var videoUrl = $"https://www.youtube.com/watch?v={videoId}";
        
        var details = await youtube.Videos.GetAsync(videoUrl);
        var streamManifest = await youtube.Videos.Streams.GetManifestAsync(videoUrl);

        var audioStreamInfo = streamManifest.GetAudioStreams().GetWithHighestBitrate();
        var videoStreamInfo = streamManifest.GetVideoStreams().GetWithHighestVideoQuality();

        var fileName = $"video.{videoStreamInfo.Container}";
        
        var conversionRequest = new ConversionRequestBuilder(fileName)
            .SetPreset(ConversionPreset.VerySlow)
            .SetFFmpegPath(@"D:\Development\Personal\YoutubeDownloader\tools\ffmpeg.exe")
            .SetContainer(videoStreamInfo.Container)
            .Build();

        var streamInfos = new[] { audioStreamInfo, videoStreamInfo };
        
        var semaphore = new SemaphoreSlim(1, 1);

        Console.CursorVisible = false;
        Console.WriteLine("Progress: 0%");
        
        var progress = new Progress<double>(p =>
        {
            semaphore.Wait();
            Console.SetCursorPosition(10, 0);
            Console.WriteLine($"{p:P}     ");
            Console.WriteLine($"{p}                ");
            Console.WriteLine($"{Math.Round(p * 100, 2)}                ");
            semaphore.Release();
        });
        
        await youtube.Videos.DownloadAsync(streamInfos, conversionRequest, progress);
    }
}