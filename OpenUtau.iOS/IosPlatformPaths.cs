using OpenUtau.Core;

namespace OpenUtau.iOS;

/// <summary>Paths inside the iOS application container; never exposes desktop paths to the host.</summary>
internal sealed class IosPlatformPaths : IPlatformPaths {
    public IosPlatformPaths() {
        RootPath = AppContext.BaseDirectory;
        DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OpenUtau");
        CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Caches", "OpenUtau");
        Directory.CreateDirectory(DataPath);
        Directory.CreateDirectory(CachePath);
    }

    public string RootPath { get; }
    public string DataPath { get; }
    public string CachePath { get; }
}
