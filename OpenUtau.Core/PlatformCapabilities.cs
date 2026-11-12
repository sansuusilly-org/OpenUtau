using System;

namespace OpenUtau.Core {
    /// <summary>
    /// Describes host capabilities that differ from the desktop application.
    /// Sandboxed hosts can opt out of features that would otherwise load code or
    /// start a process from the user's project directory.
    /// </summary>
    public interface IPlatformCapabilities {
        bool SupportsExternalProcesses { get; }
        bool SupportsDynamicPlugins { get; }
    }

    public static class PlatformCapabilities {
        static IPlatformCapabilities? current;

        public static IPlatformCapabilities? Current => current;

        public static bool SupportsExternalProcesses => Current?.SupportsExternalProcesses ?? true;
        public static bool SupportsDynamicPlugins => Current?.SupportsDynamicPlugins ?? true;

        public static void Configure(IPlatformCapabilities capabilities) {
            current = capabilities ?? throw new ArgumentNullException(nameof(capabilities));
        }
    }
}
