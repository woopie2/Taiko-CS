using System.ComponentModel.Design;
using System.Net.Cache;
using System.Numerics;
using System.Reflection;
using Raylib_cs;
using Taiko_CS.Enums;

namespace Taiko_CS.Chart;

public class Measure
{
    private List<Note> notes = new List<Note>();
    public List<Note> activeNotes = new List<Note>();
    public List<Note> removedNotes = new List<Note>();
    public double timeLength;
    public double songStartTime;

    public const double SPAWN_X = 1920;           // Notes apparaissent hors écran à droite
    public const double HIT_X = 509;              // Point de frappe
    public double timeSignature;
    public double pixelPerBeat = 318;

    public Measure(double timeLength, double songStartTime, double timeSignature, double BPM)
    {
        this.timeLength = timeLength;
        this.songStartTime = songStartTime;
        this.timeSignature = timeSignature;
    }

    public void AddNote(Note note)
    {
        notes.Add(note);
    }

    public int GetNumberOfNotes()
    {
        return notes.Count;
    }

    public List<Note> GetNotes()
    {
        return notes;
    }

    private Note? GetNearestEndOfRoll(int index)
    {
        for (int i = index; i < notes.Count; i++)
        {
            if (notes[i].noteType == NoteType.EndOfRoll)
            {
                return notes[i];
            }
        }

        return null;
    }

    private double GetMeasureEndX(float songTime, double scrollSpeed)
    {
        double measurePos = pixelPerBeat * (notes[0].BPM / 60) * ((songStartTime + timeLength) - songTime) * notes[0].ScrollSpeed;
        return measurePos;
    }

    public double GetMeasureStartX(float songTime)
    {
        double measurePos = pixelPerBeat * (notes[0].BPM / 60) * ((songStartTime) - songTime) * notes[0].ScrollSpeed;
        return measurePos;
    }
    
    public void Draw(int y, Texture2D notesTexture, float songTime, Texture2D barLine)
    {
        
        List<Note> drawLater = new List<Note>();
        foreach (Note note in activeNotes)
        {
            if (note is { rollType: RollType.BALOON, noteType: NoteType.EndOfRoll })
            {
                continue;
            }
            note.Notes = notesTexture;
            if (note.rollType is not( RollType.NONE or RollType.BALOON) && note.noteType != NoteType.EndOfRoll)
            {
                if (notes.Count == 1)
                {
                    double startX = note.noteType is NoteType.Drumroll or NoteType.BigDrumroll ?  note.X + note.GetNoteTextureSrc().Width / 2 * 1.5 : notes[0].X;
                    double endX = note.RollEnd.X is > 0 and <= SPAWN_X
                        ? note.RollEnd.X
                        : SPAWN_X + Note.GetRollSrc(note.rollType).Width * 1.5;
                    Raylib.DrawTexturePro(
                        note.Notes,
                        Note.GetRollSrc(note.rollType),
                        new Rectangle((float)startX,
                            y - (float)(Note.GetRollSrc(note.rollType).Height * 1.5 / 2) + 50,
                            (float)endX-(float)startX,
                            (float)(Note.GetRollSrc(note.rollType).Height * 1.5)),
                        Vector2.Zero, 0, Color.White
                    );
                }
                else if (notes.Count > 1)
                {
    
                    Note? endOfRoll = GetNearestEndOfRoll(notes.IndexOf(note));

                    double startX;
                    if (note.RollStart != null && note.RollStart.X > 0)
                    {
                        startX = (note.RollStart.X + note.RollStart.GetNoteTextureSrc().Width* 1.5);   
                    } else
                    {
                        startX = 0;
                    }

                    double endX = endOfRoll != null && activeNotes.Contains(endOfRoll)
                        ? endOfRoll.X - (endOfRoll.GetNoteTextureSrc().Width * 1.5 / 2) + 50
                        : GetMeasureEndX(songTime, note.ScrollSpeed);

                    if (startX < endX)
                    {
                        Raylib.DrawTexturePro(
                            note.Notes,
                            Note.GetRollSrc(note.rollType),
                            new Rectangle((float)startX,
                                y - (float)(Note.GetRollSrc(note.rollType).Height * 1.5 / 2) + 50,
                                (float)endX-(float)startX,
                                (float)(Note.GetRollSrc(note.rollType).Height * 1.5)),
                            Vector2.Zero, 0, Color.White
                        );
                    }
                    
                }
            }

            if (note.noteType is NoteType.Drumroll or NoteType.BigDrumroll)
            {
                drawLater.Add(note);
                continue;
            }
            if (note.noteType != NoteType.None)
            {
                note.Draw(y, barLine);
            }
                
        }

        foreach (Note note in drawLater)
        {
            note.Draw(y, barLine);
        }
    }

    public void Update(double songTime, int measureIndex, ref List<Note> removedNotesResult)
    {
        for (int i = 0; i < notes.Count; i++)
        {
            Note note = notes[i];
            double notePos = pixelPerBeat * (note.BPM / 60) * ((songStartTime + note.timeInMeasure) - songTime) * note.ScrollSpeed; 
            if (!activeNotes.Contains(note) && !removedNotes.Contains(note) && notePos <= SPAWN_X && notePos > 0)
            {
                activeNotes.Add(note);
            }

            if (!activeNotes.Contains(note))
                continue;

            note.X = notePos + HIT_X;
            if (note.noteType == NoteType.EndOfRoll && note.rollType != RollType.BALOON && note.RollStart != null)
            {
                double minRollLength = 130; // Minimum pixels for roll tail
                double calculatedEnd = notePos + HIT_X;
                double minEnd = note.RollStart.X + note.RollStart.GetNoteTextureSrc().Width * 1.5+ minRollLength;
    
                note.X = Math.Max(calculatedEnd, minEnd);
            }
            if (note.noteType is NoteType.Balloon && note.X <= HIT_X)
            {
                note.X = HIT_X;
            }

            // Remove note when past hit time
            if (songStartTime + note.timeInMeasure < songTime && note.rollType == RollType.NONE && note.noteType is not (NoteType.Drumroll or NoteType.BigDrumroll or NoteType.Balloon or NoteType.EndOfRoll))
            {
                note.X = 0;
                removedNotesResult.Add(note);
            }

            if (note.noteType is NoteType.Balloon && note.RollEnd.X <= HIT_X && note.RollEnd.X > 0)
            {
                note.X = 0;
                removedNotesResult.Add(note);
            }
        }
    }

    public void UpdateRemovedNote(List<Note> notesToRemove)
    {
        foreach (Note note in notesToRemove)
        {
            activeNotes.Remove(note);
            removedNotes.Add(note);
        }
    }



 


    public bool IsMeasureFinished(float songTime)
    {
        return songTime >= songStartTime + timeLength && notes[^1].X <= 0;
    }
}