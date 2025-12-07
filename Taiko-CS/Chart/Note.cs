using Raylib_cs;
using Taiko_CS.Enums;

namespace Taiko_CS.Chart;

public class Note
{
    private NoteType noteType;
    private double timeInMeasure;

    public Note(NoteType noteType, double timeInMeasure)
    {
        this.noteType = noteType;
        this.timeInMeasure = timeInMeasure;
    }

    public void Draw(int x, int y)
    {
        throw new NotImplementedException();
    }

    public double GetTimeInMeasure()
    {
        throw new NotImplementedException();
    }

    public NoteType GetType()
    {
        throw new NotImplementedException();
    }
}