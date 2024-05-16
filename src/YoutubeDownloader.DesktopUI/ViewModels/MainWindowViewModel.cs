using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Services;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace YoutubeDownloader.DesktopUI.ViewModels;

// ReSharper disable once ClassNeverInstantiated.Global
public partial class MainWindowViewModel : ObservableObject
{
    private readonly IVideoServiceFactory _videoServiceFactory;
    private IVideoService? _videoService;
    private CancellationTokenSource? _cancellationTokenSource;

    [ObservableProperty] private string? _videoUrlOrId;
    [ObservableProperty] private Video? _video;
    [ObservableProperty] private ObservableCollection<IAudioStreamInfo> _audioStreamInfos = [];
    [ObservableProperty] private ObservableCollection<IVideoStreamInfo> _videoStreamInfos = [];
    [ObservableProperty] private IAudioStreamInfo? _selectedAudioStreamInfo;
    [ObservableProperty] private IVideoStreamInfo? _selectedVideoStreamInfo;
    [ObservableProperty] private string? _folderPath;
    [ObservableProperty] private bool _isDownloadingVideo;
    [ObservableProperty] private double _downloadProgress;
    [ObservableProperty] private bool _isDownloadEnabled;
    [ObservableProperty] private bool _isDownloadDisabled;

    public MainWindowViewModel(IVideoServiceFactory videoServiceFactory)
    {
        _videoServiceFactory = videoServiceFactory;

        FolderPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
    }

    [RelayCommand]
    private async Task FindVideoAsync()
    {
        if (string.IsNullOrEmpty(VideoUrlOrId))
        {
            return;
        }

        IsDownloadingVideo = true;

        try
        {
            _videoService = _videoServiceFactory.GetVideoService(VideoUrlOrId);

            var video = await _videoService.GetVideoAsync();
            var videoStreamInfos = await _videoService.GetVideoStreamsAsync();
            var audioStreamInfos = await _videoService.GetAudioStreamsAsync();

            var filteredVideoStreamInfos = videoStreamInfos.OrderByDescending(x => x.VideoResolution.Area);

            var filteredAudioStreamInfos = audioStreamInfos
                .Where(x => x.Container == Container.Mp4)
                .OrderByDescending(x => x.Bitrate);

            Video = video;
            VideoStreamInfos = new ObservableCollection<IVideoStreamInfo>(filteredVideoStreamInfos);
            AudioStreamInfos = new ObservableCollection<IAudioStreamInfo>(filteredAudioStreamInfos);

            VideoUrlOrId = "";

            SelectedVideoStreamInfo = VideoStreamInfos.FirstOrDefault();
            SelectedAudioStreamInfo = AudioStreamInfos.FirstOrDefault();
            
            IsDownloadEnabled = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            IsDownloadingVideo = false;
        }
    }

    [RelayCommand]
    private void BrowseFolder()
    {
        var dialog = new FolderBrowserDialog();
        if (dialog.ShowDialog() == DialogResult.OK)
        {
            FolderPath = dialog.SelectedPath;
        }
    }

    [RelayCommand]
    private async Task DownloadAsync()
    {
        if (_videoService is null
            || string.IsNullOrEmpty(FolderPath)
            || SelectedVideoStreamInfo is null
            || SelectedAudioStreamInfo is null)
        {
            return;
        }
        
        IsDownloadDisabled = true;

        DownloadProgress = 0;
        var progress = new Progress<double>();
        progress.ProgressChanged += UpdateDownloadProgress;

        try
        {
            _cancellationTokenSource = new CancellationTokenSource();

            await _videoService.DownloadVideoAsync(
                SelectedAudioStreamInfo,
                SelectedVideoStreamInfo,
                FolderPath,
                progress,
                _cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
        finally
        {
            _cancellationTokenSource?.Dispose();
            
            progress.ProgressChanged -= UpdateDownloadProgress;
            
            DownloadProgress = 0;
            IsDownloadDisabled = false;
        }
    }
    
    [RelayCommand]
    private void CancelDownload()
    {
        _cancellationTokenSource?.Cancel();
    }

    private void UpdateDownloadProgress(object? sender, double progress)
    {
        DownloadProgress = Math.Round(progress * 100, 2);
    }
}