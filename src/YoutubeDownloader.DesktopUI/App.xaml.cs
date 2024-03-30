using System.Windows;
using Prism.Ioc;
using YoutubeDownloader.Core.Services;
using YoutubeDownloader.DesktopUI.Views;

namespace YoutubeDownloader.DesktopUI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IVideoServiceFactory, VideoServiceFactory>();
    }

    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }
}