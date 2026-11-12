using Avalonia.Platform.Storage;
using OpenUtau.Core.Format;
using OpenUtau.Core.Ustx;

namespace OpenUtau.iOS;

/// <summary>
/// Bridges Files-provider documents to OpenUtau's path-based serializer. Imported
/// documents live in the app container; save always writes a user-selected file.
/// </summary>
internal sealed class ProjectDocumentStore {
    readonly string documentsPath;

    public ProjectDocumentStore() {
        documentsPath = Path.Combine(OpenUtau.Core.PathManager.Inst.DataPath, "Documents");
        Directory.CreateDirectory(documentsPath);
    }

    public async Task<UProject> OpenAsync(IStorageFile file) {
        var fileName = Path.GetFileName(file.Name);
        if (!fileName.EndsWith(".ustx", StringComparison.OrdinalIgnoreCase)) {
            throw new NotSupportedException("The iPad milestone currently opens .ustx projects only.");
        }
        var localPath = Path.Combine(documentsPath, $"{Guid.NewGuid():N}-{fileName}");
        await using var source = await file.OpenReadAsync();
        await using var destination = File.Create(localPath);
        await source.CopyToAsync(destination);
        return Ustx.Load(localPath);
    }

    public async Task SaveAsAsync(UProject project, IStorageFile destination) {
        var localPath = Path.Combine(documentsPath, $"{Guid.NewGuid():N}-{SanitizeFileName(project.name)}.ustx");
        Ustx.Save(localPath, project);
        await using var source = File.OpenRead(localPath);
        await using var target = await destination.OpenWriteAsync();
        await source.CopyToAsync(target);
    }

    static string SanitizeFileName(string name) {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }
}
