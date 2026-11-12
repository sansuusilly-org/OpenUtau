using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using OpenUtau.Core;

namespace OpenUtau.iOS;

public partial class MainView : UserControl {
    readonly MobileProjectViewModel viewModel = new();
    readonly ProjectDocumentStore documents = new();
    static readonly FilePickerFileType UstxFile = new("OpenUtau project") { Patterns = new[] { "*.ustx" } };

    public MainView() {
        InitializeComponent();
        DataContext = viewModel;
    }

    void NewProject(object? sender, RoutedEventArgs e) => viewModel.NewProject();
    void AddNote(object? sender, RoutedEventArgs e) => viewModel.AddNote();
    void LowerPitch(object? sender, RoutedEventArgs e) => viewModel.MoveSelectedPitch(-1);
    void RaisePitch(object? sender, RoutedEventArgs e) => viewModel.MoveSelectedPitch(1);
    void TestAudio(object? sender, RoutedEventArgs e) => PlaybackManager.Inst.PlayTestSound();

    async void OpenProject(object? sender, RoutedEventArgs e) {
        var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storage is null) return;
        var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions {
            Title = "Open OpenUtau project",
            AllowMultiple = false,
            FileTypeFilter = new[] { UstxFile },
        });
        var file = files.FirstOrDefault();
        if (file is null) return;
        try {
            viewModel.SetProject(await documents.OpenAsync(file), file.Name);
        } catch (Exception error) {
            viewModel.ReportError(error.Message);
        }
    }

    async void SaveProject(object? sender, RoutedEventArgs e) {
        var storage = TopLevel.GetTopLevel(this)?.StorageProvider;
        if (storage is null) return;
        var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions {
            Title = "Save OpenUtau project",
            SuggestedFileName = $"{viewModel.Project.name}.ustx",
            FileTypeChoices = new[] { UstxFile },
            ShowOverwritePrompt = true,
        });
        if (file is null) return;
        try {
            await documents.SaveAsAsync(viewModel.Project, file);
            viewModel.MarkSaved();
        } catch (Exception error) {
            viewModel.ReportError(error.Message);
        }
    }
}
