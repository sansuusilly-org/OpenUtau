using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenUtau.Core.Format;
using OpenUtau.Core.Ustx;

namespace OpenUtau.iOS;

internal sealed class MobileProjectViewModel : INotifyPropertyChanged {
    UProject project = Ustx.Create();
    MobileNoteRow? selectedNote;
    string status = "Create a project or open a .ustx file from Files.";

    public ObservableCollection<MobileNoteRow> Notes { get; } = new();
    public string ProjectName => project.name;
    public string Status { get => status; private set => SetField(ref status, value); }
    public MobileNoteRow? SelectedNote { get => selectedNote; set => SetField(ref selectedNote, value); }
    public UProject Project => project;

    public void SetProject(UProject value, string sourceName) {
        project = value;
        RefreshNotes();
        Status = $"Opened {sourceName}.";
        OnPropertyChanged(nameof(ProjectName));
    }

    public void NewProject() {
        project = Ustx.Create();
        RefreshNotes();
        Status = "New project.";
        OnPropertyChanged(nameof(ProjectName));
    }

    public void AddNote() {
        var part = project.parts.OfType<UVoicePart>().FirstOrDefault();
        if (part is null) {
            part = new UVoicePart { name = "Verse", trackNo = 0, position = 0, Duration = project.resolution * 4 };
            project.parts.Add(part);
        }
        var position = part.notes.Count == 0 ? 0 : part.notes.Max(note => note.End);
        var note = project.CreateNote(60, position, project.resolution);
        note.lyric = "a";
        part.notes.Add(note);
        part.Duration = Math.Max(part.Duration, note.End);
        project.Saved = false;
        RefreshNotes(note);
        Status = "Added a note. Select it, then use Pitch +/− to edit.";
    }

    public void MoveSelectedPitch(int semitones) {
        if (SelectedNote is null) {
            Status = "Select a note first.";
            return;
        }
        SelectedNote.Note.tone = Math.Clamp(SelectedNote.Note.tone + semitones, 0, 127);
        project.Saved = false;
        RefreshNotes(SelectedNote.Note);
        Status = "Edited note pitch.";
    }

    public void MarkSaved() {
        project.Saved = true;
        Status = "Saved a copy to Files.";
    }

    public void ReportError(string message) => Status = message;

    void RefreshNotes(UNote? keepSelected = null) {
        keepSelected ??= SelectedNote?.Note;
        Notes.Clear();
        foreach (var part in project.parts.OfType<UVoicePart>()) {
            foreach (var note in part.notes) {
                Notes.Add(new MobileNoteRow(part, note));
            }
        }
        SelectedNote = Notes.FirstOrDefault(row => row.Note == keepSelected);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    void SetField<T>(ref T field, T value, [CallerMemberName] string? name = null) {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(name);
    }
}

internal sealed class MobileNoteRow {
    public MobileNoteRow(UVoicePart part, UNote note) { Part = part; Note = note; }
    public UVoicePart Part { get; }
    public UNote Note { get; }
    public string Label => $"{Note.lyric}  ·  MIDI {Note.tone}  ·  tick {Part.position + Note.position}  ·  {Note.duration} ticks";
}
