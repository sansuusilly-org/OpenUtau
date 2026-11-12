using OpenUtau.Core;

namespace OpenUtau.iOS;

/// <summary>iOS app containers cannot execute project-supplied programs or load arbitrary plug-ins.</summary>
internal sealed class IosPlatformCapabilities : IPlatformCapabilities {
    public bool SupportsExternalProcesses => false;
    public bool SupportsDynamicPlugins => false;
}
