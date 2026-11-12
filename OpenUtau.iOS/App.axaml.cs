using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using OpenUtau.Core;

namespace OpenUtau.iOS;

public sealed class App : Application {
    public App() {
        PlatformPaths.Configure(new IosPlatformPaths());
        PlatformCapabilities.Configure(new IosPlatformCapabilities());
    }

    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is ISingleViewApplicationLifetime lifetime) {
            PlaybackManager.Inst.AudioOutput = new IosAudioOutput();
            lifetime.MainView = new MainView();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
