using System.Net.Cache;
using System.Numerics;
using System.Reflection;
using Raylib_cs;
using Taiko_CS.Enums;

namespace Taiko_CS.Chart;

public class Measure
{
    private List<Note> notes = new List<Note>();
    private List<Note> activeNotes = new List<Note>();
    private List<Note> removedNotes = new List<Note>();
    public double timeLength;
    public double songStartTime;

    public const double SPAWN_X = 1920;           // Notes apparaissent hors écran à droite
    public const double HIT_X = 509;              // Point de frappe
    public const double ANTICIPATION_TIME = 2.5;  // Notes apparaissent 2.5s avant d'être frappées
    private const double MIN_ANTICIPATION_TIME = 0;
    public double timeSignature;
    public const double BASE_PIXELS_PER_SECOND = 600  * 1.3; 

    public Measure(double timeLength, double songStartTime, double timeSignature)
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
        double hitTime = songStartTime + timeLength;
        double distance = Math.Abs(SPAWN_X - HIT_X);
        double travelTime = distance / (BASE_PIXELS_PER_SECOND * scrollSpeed);
        double spawnTime = hitTime - travelTime;
        double timeSinceSpawn = songTime - spawnTime;
        double pixelsTraveled = (BASE_PIXELS_PER_SECOND * scrollSpeed) * Math.Max(0, timeSinceSpawn);
        return SPAWN_X - pixelsTraveled;
    }

    private double GetMeasureStartX(float songTime)
    {
        double hitTime = songStartTime;
        double distance = Math.Abs(SPAWN_X - HIT_X);
        double travelTime = distance / (BASE_PIXELS_PER_SECOND * notes[0].ScrollSpeed);
        double spawnTime = hitTime - travelTime;
        double timeSinceSpawn = songTime - spawnTime;
        double pixelsTraveled = (BASE_PIXELS_PER_SECOND * notes[^1].ScrollSpeed) * Math.Max(0, timeSinceSpawn);
        return SPAWN_X - pixelsTraveled;
    }
    
    public void Draw(int y, Texture2D notesTexture, float songTime)
    {
        List<Note> drawLater = new List<Note>();
        foreach (Note note in activeNotes)
        {
            if (note is { rollType: RollType.BALOON, noteType: NoteType.EndOfRoll })
            {
                continue;
            }
            note.Notes = notesTexture;
            if (note.rollType != RollType.NONE && note.noteType != NoteType.EndOfRoll)
            {
                if (notes.Count == 1)
                {
                    for (double x = GetMeasureStartX(songTime);
                         x < GetMeasureEndX(songTime, note.ScrollSpeed);
                         x++)
                    {
                        Raylib.DrawTexturePro(note.Notes, Note.GetRollSrc(note.rollType),
                            new Rectangle((int)x, y - (float)(Note.GetRollSrc(note.rollType).Height * 1.3 / 2) + 50,
                                (float)(Note.GetRollSrc(note.rollType).Width * 1.3),
                                (float)(Note.GetRollSrc(note.rollType).Height * 1.3)), Vector2.Zero, 0, Color.White);
                    }
                }
                else if (notes.Count > 1 && note.noteType == NoteType.None)
                {
                    // Check if this is the first note of a new roll
                    int currentIndex = notes.IndexOf(note);
                    bool isFirstOfRoll = currentIndex == 0 || 
                                         notes[currentIndex - 1].noteType != NoteType.None || 
                                         notes[currentIndex - 1].rollType != note.rollType;
    
                    // Only draw for the first roll body note of each roll
                    if (!isFirstOfRoll)
                        continue;
    
                    Note? endOfRoll = GetNearestEndOfRoll(currentIndex);
                    double rollWidth = Note.GetRollSrc(note.rollType).Width * 1.3;

                    double startX;
                    if (note.RollStart != null && note.RollStart.X > 0)
                    {
                        startX = note.RollStart.X + note.RollStart.GetNoteTextureSrc().Width / 2 * 1.3;
                    }
                    else
                    {
                        startX = 0;
                    }

                    double endX = endOfRoll != null && activeNotes.Contains(endOfRoll) 
                        ? endOfRoll.X 
                        : 1920 + rollWidth;

                    // Draw from startX all the way to endX
                    for (double x = startX; x < endX; x += rollWidth)
                    {
                        double currentWidth = Math.Min(rollWidth, endX - x);
        
                        Raylib.DrawTexturePro(
                            note.Notes,
                            Note.GetRollSrc(note.rollType),
                            new Rectangle((int)x,
                                y - (float)(Note.GetRollSrc(note.rollType).Height * 1.3 / 2) + 50,
                                (float)currentWidth,
                                (float)(Note.GetRollSrc(note.rollType).Height * 1.3)),
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
                    note.Draw(y);
                }
                
        }

        foreach (Note note in drawLater)
        { 
            note.Draw(y);
        }
    }

    public void Update(double songTime, int measureIndex)
    {
        for (int i = 0; i < notes.Count; i++)
        {
            Note note = notes[i];

            double hitTime = songStartTime + note.timeInMeasure;
            double distance = Math.Abs(SPAWN_X - HIT_X);
            double travelTime = distance / (BASE_PIXELS_PER_SECOND * note.ScrollSpeed);
            double spawnTime = hitTime - travelTime;
            double timeSinceSpawn = songTime - spawnTime;
            if (!activeNotes.Contains(note) && !removedNotes.Contains(note) && timeSinceSpawn >= 0)
            {
                activeNotes.Add(note);
            }

            if (!activeNotes.Contains(note))
                continue;
                // ALL notes start at SPAWN_X and move toward HIT_X
                // Calculate how far the note should have traveled
                double pixelsTraveled = (BASE_PIXELS_PER_SECOND * note.ScrollSpeed) * Math.Max(0, timeSinceSpawn);
            
                // Position = start position + distance traveled
                // (For right-to-left movement: SPAWN_X - pixelsTraveled)
                note.X = SPAWN_X - pixelsTraveled;
                if (note.noteType is NoteType.Balloon && note.X <= HIT_X)
                {
                    note.X = HIT_X;
                }

            // Remove note when past hit time
            if (songTime >= hitTime && note.rollType == RollType.NONE && note.noteType is not (NoteType.Drumroll or NoteType.BigDrumroll or NoteType.Balloon))
            {
                note.X = 0;
                activeNotes.Remove(note);
                removedNotes.Add(note);
            }

            if (note.noteType is NoteType.Balloon && activeNotes.Contains(note.RollEnd) && note.RollEnd.X < HIT_X)
            {
                note.X = 0;
                activeNotes.Remove(note);
                removedNotes.Add(note);
            }
            {
                
            }
        }
    }





 


    public bool IsMeasureFinished(float songTime)
    {
        return songTime >= songStartTime + timeLength && GetMeasureEndX(songTime, notes[^1].ScrollSpeed) < 0;
    }
}
