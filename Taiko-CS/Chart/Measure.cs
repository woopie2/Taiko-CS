namespace Taiko_CS.Chart;

public class Measure
{
    private List<Note> notes;

    public void AddNote(Note note)
    {
        notes.Add(note);
    }

    public int GetNumberOfNotes()
    {
        return notes.Count;
    }
} 