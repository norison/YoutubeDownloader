using System.Globalization;
using System.Windows.Data;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.DesktopUI.Converters;

public class AudioStreamToStringConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is IAudioStreamInfo audioStreamInfo)
        {
            return $"{audioStreamInfo.Bitrate}, {audioStreamInfo.Container} ({audioStreamInfo.AudioCodec}) - {audioStreamInfo.Size}";
        }

        return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}