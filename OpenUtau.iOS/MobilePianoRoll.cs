using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace OpenUtau.iOS;

/// <summary>
/// A deliberately small, touch-oriented project view for the first iPad milestone.
/// It renders notes from the current USTX document and makes them selectable; the
/// surrounding controls perform the first safe edit (semitone changes).
/// </summary>
public sealed class MobilePianoRoll : Control {
    const double LeftPadding = 40;
    const double TopPadding = 8;
    const double TickWidth = 0.25;
    const double KeyHeight = 18;
    MobileNoteRow? selected;

    public MobilePianoRoll() {
        ClipToBounds = true;
        PointerPressed += OnPointerPressed;
    }

    public override void Render(DrawingContext context) {
        base.Render(context);
        context.FillRectangle(new SolidColorBrush(Color.Parse("#18202B")), Bounds);
        var viewModel = DataContext as MobileProjectViewModel;
        if (viewModel is null) return;

        for (var midi = 48; midi <= 84; midi++) {
            var y = ToneToY(midi);
            var blackKey = new[] { 1, 3, 6, 8, 10 }.Contains(midi % 12);
            context.FillRectangle(
                new SolidColorBrush(blackKey ? Color.Parse("#202B38") : Color.Parse("#263443")),
                new Rect(0, y, Bounds.Width, KeyHeight - 1));
        }

        foreach (var row in viewModel.Notes) {
            var rect = NoteRect(row);
            var fill = row == viewModel.SelectedNote ? Color.Parse("#F5A623") : Color.Parse("#4D9DE0");
            context.DrawRectangle(new SolidColorBrush(fill), null, rect, 4, 4);
            var label = new FormattedText(
                row.Note.lyric,
                System.Globalization.CultureInfo.InvariantCulture,
                FlowDirection.LeftToRight,
                new Typeface("Arial"),
                12,
                Brushes.Black);
            context.DrawText(label, rect.TopLeft + new Point(5, 2));
        }
    }

    void OnPointerPressed(object? sender, PointerPressedEventArgs args) {
        if (DataContext is not MobileProjectViewModel viewModel) return;
        var point = args.GetPosition(this);
        selected = viewModel.Notes.LastOrDefault(row => NoteRect(row).Contains(point));
        viewModel.SelectedNote = selected;
        InvalidateVisual();
        args.Pointer.Capture(this);
    }

    static double ToneToY(int tone) => TopPadding + (84 - tone) * KeyHeight;

    static Rect NoteRect(MobileNoteRow row) => new(
        LeftPadding + (row.Part.position + row.Note.position) * TickWidth,
        ToneToY(row.Note.tone),
        Math.Max(24, row.Note.duration * TickWidth),
        KeyHeight - 2);
}
