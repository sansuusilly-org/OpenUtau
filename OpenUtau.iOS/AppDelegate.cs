using Avalonia;
using Avalonia.iOS;
using Foundation;

namespace OpenUtau.iOS;

[Register("AppDelegate")]
public sealed class AppDelegate : AvaloniaAppDelegate<App> {
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder) {
        return base.CustomizeAppBuilder(builder).With(new iOSPlatformOptions {
            RenderingMode = new[] { iOSRenderingMode.Metal, iOSRenderingMode.OpenGl },
        });
    }
}
