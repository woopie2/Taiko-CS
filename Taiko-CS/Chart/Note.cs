using System.Numerics;
using Raylib_cs;
using Taiko_CS.Enums;

namespace Taiko_CS.Chart;

public class Note
{
    private bool Equals(Note other)
    {
        return noteType == other.noteType && timeInMeasure.Equals(other.timeInMeasure) && ScrollSpeed.Equals(other.ScrollSpeed) && rollType == other.rollType && X.Equals(other.X);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Note)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine((int)noteType, timeInMeasure, ScrollSpeed, (int)rollType, X);
    }

    public NoteType noteType;
    public double timeInMeasure;
    public double ScrollSpeed;
    public double BPM;
    public RollType rollType;
    public double X { get; set; }
    public Note RollStart { get; set; }
    public Note RollEnd { get; set; }
    public Texture2D Notes { get; set; }
    public double RollStartTime;
    public double RollEndTime;
    private bool showBarLine;

    public Note(NoteType noteType, double timeInMeasure, double scrollSpeed, RollType rollType, bool showBarLine, double BPM)
    {
        this.noteType = noteType;
        this.timeInMeasure = timeInMeasure;
        this.ScrollSpeed = scrollSpeed;
        this.rollType = rollType;
        this.showBarLine = showBarLine;
        this.BPM = BPM;
    }
    
    public Rectangle GetNoteTextureSrc()
    {
        switch (noteType)
        {
            case NoteType.Don:
                return new Rectangle(159, 30, 70, 70);
            case NoteType.Ka:
                return new Rectangle(290, 160, 70, 70);
            case NoteType.BigDon:
                return new Rectangle(401, 12, 105, 105);
            case NoteType.BigKa:
                return new Rectangle(530, 12, 105, 105);
            case NoteType.Drumroll:
                return new Rectangle(679, 30, 70, 70);
            case NoteType.BigDrumroll:
                return new Rectangle(1050, 142, 105, 105);
            case NoteType.Balloon: 
                return new Rectangle(1460, 30, 175, 70);
        }

        if (rollType == RollType.NORMAL && noteType !=  NoteType.EndOfRoll)
        {
            return new Rectangle(780, 30, 130, 69);
        }

        if (rollType == RollType.BIG &&  noteType !=  NoteType.EndOfRoll) 
        {
            return new  Rectangle(1170, 142, 130, 106);
        }
        
        if (noteType == NoteType.EndOfRoll)
        {
            if (rollType == RollType.NORMAL)
            {
                return new Rectangle(910, 30, 30, 70);
            }
            return new Rectangle(1303, 142, 130, 108);
        }
        
        return new Rectangle(0, 0, 0, 0);
    }

    public static Rectangle GetRollSrc(RollType rollType)
    {
        if (rollType == RollType.NORMAL )
        {
            return new Rectangle(780, 30, 130, 69);
        }

        if (rollType == RollType.BIG) 
        {
            return new  Rectangle(1170, 142, 130, 106);
        }
        return new Rectangle(0, 0, 0, 0);
    }
    
    public void Draw(int y, Texture2D measureBar)
    {
        if (showBarLine)
        {
            Raylib.DrawTexturePro(measureBar, new Rectangle(0, 0, 4, 270), new Rectangle((float)(X + GetNoteTextureSrc().Width / 2 * 1.3), y - 40, 4, 200 ), Vector2.Zero, 0, Color.White);
        }
        Raylib.DrawTexturePro(Notes, this.GetNoteTextureSrc(), new Rectangle((int) X, y - (float) (GetNoteTextureSrc().Height * 1.3 / 2) + 50 , (float) (GetNoteTextureSrc().Width * 1.3), (float) (GetNoteTextureSrc().Height * 1.3)), Vector2.Zero, 0, Color.White);
    }
}