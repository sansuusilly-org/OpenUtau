using System;

namespace OpenUtau.Core {
    /// <summary>
    /// Supplies writable application locations for hosts that are sandboxed or do not
    /// follow the desktop operating-system conventions used by <see cref="PathManager"/>.
    /// Configure this before the first access to <see cref="PathManager.Inst"/>.
    /// </summary>
    public interface IPlatformPaths {
        string RootPath { get; }
        string DataPath { get; }
        string CachePath { get; }
    }

    public static class PlatformPaths {
        static IPlatformPaths? current;

        public static IPlatformPaths? Current => current;

        public static void Configure(IPlatformPaths paths) {
            current = paths ?? throw new ArgumentNullException(nameof(paths));
        }
    }
}
