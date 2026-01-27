using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Taiko_CS.Enums;

namespace Taiko_CS.Chart;

public class Chart
{
    private static Measure GetRollStartMeasure(ChartData data, Measure currentMeasure)
    {
        foreach (Note note in currentMeasure.GetNotes())
        {
            if (note.noteType == NoteType.Balloon || note.noteType == NoteType.Drumroll || note.noteType == NoteType.BigDrumroll)
            {
                return currentMeasure;
            }
        }
        for (int i = data.measures.Count - 1; i >= 0; i--)
        {
            foreach (Note note in data.measures[i].GetNotes())
            {
                if (note.noteType == NoteType.Balloon || note.noteType == NoteType.Drumroll || note.noteType == NoteType.BigDrumroll)
                {
                    return data.measures[i];
                }
            }
        }
        return null;
    }

    private static int GetNearestRollStart(Measure measure, int index)
    {
        for (int i = index; i >= 0; i--)
        {
            if (measure.GetNotes()[i].noteType == NoteType.Balloon ||
                measure.GetNotes()[i].noteType == NoteType.Drumroll ||
                measure.GetNotes()[i].noteType == NoteType.BigDrumroll)
            {
                return i;
            }
        }

        return -1;
    }
    
    public static ChartData Parse(string tjaFilePath, string fileName, Difficulty difficulty)
    {
        double audioOffset = 0;
        NumberFormatInfo decimalFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };
        string[] courseHeaderCommands = {"COURSE", "LEVEL", "BALLOON", "SCOREINIT", "SCOREDIFF"};
        string numbers = "0123456789";
        string fileData = "";
        try
        {
            using StreamReader reader = new(tjaFilePath + "/" + fileName);
            fileData = reader.ReadToEnd();
        }
        catch (IOException e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }

        string difficultyDataName = difficulty switch
        {
            Difficulty.EASY => "Easy",
            Difficulty.NORMAL => "Normal",
            Difficulty.HARD => "Hard",
            Difficulty.ONI => "Oni",
            Difficulty.URA => "Edit",
            _ => ""
        };

        ChartData chartData = new ChartData();
        chartData.SetField("LOCATION", tjaFilePath);

        Measure currentMeasure = null;
        bool canParseMeasure = false;
        bool canParseEvent = false;
        bool isMeasureComplete = false;
        bool showMeasureBar = true;
        bool isGoGoTime = false;
        bool isParsingFieldName = true;
        bool isParsingEvent = false;
        RollType currentRollType = RollType.NONE;

        string[] fileLines = fileData.Split("\n");
        string fieldName = "";
        string fieldValue = "";
        string measureContent = "";
        string eventContent = "";
        double currentTimeSignature = 1;
        double currentBPM = 120;
        double currentScrollSpeed = 1;
        double lastMillisecondInMeasure = 0;

        double offset = 0; // par défaut
        Note currentRollStart = null;
        List<double> secondsPerMeasures = new List<double>();

        foreach (string rawLine in fileLines)
        {
            if (rawLine == "")
            {
                continue;
            }
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] parts = line.Split(':');

            if (parts.Length >= 2 && parts[0] == "COURSE" && parts[1].Trim() == difficultyDataName)
            {
                canParseEvent = true;
                continue;
            }

            if (courseHeaderCommands.Contains(line.Split(":")[0]) && !canParseEvent)
            {
                continue;
            }

            if (canParseEvent && line.StartsWith("#START"))
            {
                canParseEvent = true;
                canParseMeasure = true;
                continue;
            }

            if (canParseEvent && line.StartsWith("#END"))
            {
                return chartData;
            }

            int lineIndex = 0;
            if (line[0] == ',' && measureContent == "" && canParseMeasure)
            {
                double measureStartTime = 0;
                if (chartData.measures.Count == 0)
                {
                    measureStartTime = -offset;
                }
                else
                {
                    var prev = chartData.measures[chartData.measures.Count - 1];
                    if (prev.timeLength <= 0)
                    {
                        measureStartTime = prev.songStartTime;
                    }
                    else
                    {
                        measureStartTime = prev.songStartTime + prev.timeLength;
                    }
                }
                

                Measure m = new Measure((60000 * currentTimeSignature * 4 / currentBPM) / 1000,
                    measureStartTime, currentTimeSignature, currentBPM);
                Note n = new Note(NoteType.None, 0, currentScrollSpeed, currentRollType, showMeasureBar, currentBPM);
                n.RollStart = currentRollStart;
                m.AddNote(n);
                chartData.AddMeasure(m);
                continue;
            }
            foreach (char c in line)
            {
                if (c == '/' && line[lineIndex + 1] == '/')
                    break;

                isParsingEvent = (line[0] == '#' && canParseEvent);

                if (isParsingEvent)
                    eventContent += c;

                if (!numbers.Contains(line[0]) && line[0] != '#')
                {
                    if (c == ':')
                        isParsingFieldName = false;
                    else if (isParsingFieldName)
                    {
                        fieldName += c;
                    }
                    else
                        fieldValue += c;
                }

                if (numbers.Contains(line[0]) && canParseMeasure)
                {
                    if (c == ',')
                    {
                        isMeasureComplete = true;
                        break;
                    }
                    if (currentMeasure == null)
                        currentMeasure = new Measure((60000 * currentTimeSignature * 4 / currentBPM) / 1000, 0, currentTimeSignature, currentBPM);

                    measureContent += c;
                }

                lineIndex++;
            }

            // Fields standards
            if (!numbers.Contains(line[0]) && line[0] != '#')
            {
                chartData.SetField(fieldName, fieldValue);
                if (fieldName == "BPM")
                {
                    currentBPM = double.Parse(fieldValue, decimalFormat);
                }

                if (fieldName == "OFFSET")
                {
                    offset = double.Parse(fieldValue, decimalFormat) - audioOffset;
                }
                fieldName = "";
                fieldValue = "";
                isParsingFieldName = true;
            }

            // Lignes commençant par #
            if (line[0] == '#' && canParseEvent)
            {
                eventContent = eventContent.Substring(1).Trim();
                string[] eventParts = eventContent.Split(" ");

                switch (eventParts[0])
                {
                    case "OFFSET":
                        chartData.SetField("OFFSET", eventParts[1]);
                        double.TryParse(eventParts[1], out offset);
                        break;
                    case "GOGOSTART":
                        isGoGoTime = true;
                        break;
                    case "GOGOEND":
                        isGoGoTime = false;
                        break;
                    case "MEASURE":
                        var measureParts = eventParts[1].Split('/');
                        currentTimeSignature = double.Parse(measureParts[0]) / double.Parse(measureParts[1]);
                        double secondsPerMeasure = (60000 * currentTimeSignature * 4 / currentBPM) / 1000;
                        currentMeasure = new Measure(secondsPerMeasure, 0, currentTimeSignature, currentBPM);
                        break;
                    case "SCROLL":
                        currentScrollSpeed = double.Parse(eventParts[1], decimalFormat);
                        break;
                    case "BPMCHANGE":
                        currentBPM = double.Parse(eventParts[1], decimalFormat);
                        break;
                    case "BARLINEOFF":
                        showMeasureBar = false;
                        break;
                    case "BARLINEON":
                        showMeasureBar = true;
                        break;
                }

                eventContent = "";
            }

            // Parsing notes
            if (numbers.Contains(line[0]))
            {
                Console.WriteLine(currentBPM);
                int i = 0;
                foreach (char c in measureContent)
                {
                    NoteType noteType = (NoteType)int.Parse(c.ToString());
                    double secondsPerMeasure = (60000 * currentTimeSignature * 4 / currentBPM) / 1000;
                    double timeInMeasure = (currentMeasure.GetNumberOfNotes() == 0)
                        ? 0
                        : lastMillisecondInMeasure + (secondsPerMeasure / measureContent.Length);

                    lastMillisecondInMeasure = timeInMeasure;
                    bool isNoteBarlined = (currentMeasure.GetNumberOfNotes() == 0 && showMeasureBar);
                    Note note = new Note(noteType, 0, currentScrollSpeed, currentRollType, isNoteBarlined, currentBPM);
                    secondsPerMeasures.Add(secondsPerMeasure);
                    note.RollStart = currentRollStart;
                    currentMeasure.AddNote(note);
                    if (noteType == NoteType.EndOfRoll)
                    {
                        currentRollType = RollType.NONE;
                        currentRollStart = null;
                        Measure startRollMeasure = GetRollStartMeasure(chartData, currentMeasure);
                        if (startRollMeasure == currentMeasure)
                        {
                            Console.WriteLine($"Processing EndOfRoll at index {currentMeasure.GetNotes().IndexOf(note)}, roll starts at index {GetNearestRollStart(currentMeasure,
                                currentMeasure.GetNotes().IndexOf(note))}");
                            foreach (Note n in currentMeasure.GetNotes())
                            {
                                if (currentMeasure.GetNotes().IndexOf(n) < GetNearestRollStart(currentMeasure,
                                        currentMeasure.GetNotes().IndexOf(note)) || currentMeasure.GetNotes().IndexOf(n) > currentMeasure.GetNotes().IndexOf((note)))
                                {
                                    continue;
                                }
                                n.RollEnd = note;
                                Console.WriteLine($"  Setting RollEnd on note at index {currentMeasure.GetNotes().IndexOf(n)} (type: {n.noteType}, rollType: {n.rollType})");
                            }
                        }
                        else
                        {
                            for (int mIndex = chartData.measures.IndexOf(startRollMeasure); mIndex < chartData.measures.Count; mIndex++)
                            {
                                foreach (Note n in chartData.measures[mIndex].GetNotes())
                                {
                                    if (chartData.measures[mIndex].GetNotes().IndexOf(n) < GetNearestRollStart(chartData.measures[mIndex], chartData.measures[mIndex].GetNumberOfNotes() - 1))
                                    {
                                        continue;
                                    }
                                    n.RollEnd = note;
                                }

                                foreach (Note n in currentMeasure.GetNotes())
                                {
                                    n.RollEnd = note;
                                }
                            }   
                        }
                        
                    }

                    if (noteType == NoteType.Drumroll)
                    {
                        currentRollType = RollType.NORMAL;
                        note.rollType = currentRollType;
                        note.RollStart = note;
                        currentRollStart = note;   
                    }
                    else if (noteType == NoteType.BigDrumroll)
                    {
                        currentRollType = RollType.BIG;
                        note.rollType = currentRollType;
                        note.RollStart = note;
                        currentRollStart = note;
                    } else if (noteType == NoteType.Balloon)
                    {
                        currentRollType = RollType.BALOON;
                        note.rollType = currentRollType;
                        note.RollStart = note;
                        currentRollStart = note;
                    }
 
                    i++;
                }

                measureContent = "";

                if (isMeasureComplete)
                {
                    double secondsPerMeasure = 0;
                    double lastMilInMeasure = 0;
                    int index = 0;
                    foreach (Note note in currentMeasure.GetNotes())
                    {
                        secondsPerMeasure = secondsPerMeasures[index]; 
                        Console.WriteLine(secondsPerMeasure);
                        note.timeInMeasure = lastMilInMeasure;
                        lastMilInMeasure += secondsPerMeasure / currentMeasure.GetNotes().Count;
                        index++;

                    }
                    secondsPerMeasures.Clear();
                    isMeasureComplete = false;

                    // Calcul du startTime avant d’ajouter la mesure
                    double measureStartTime = 0;
                    if (chartData.measures.Count == 0)
                        measureStartTime = -offset; // première mesure
                    else
                    {
                        var prev = chartData.measures[chartData.measures.Count - 1];
                        if (prev.timeLength <= 0)
                        {
                            measureStartTime = prev.songStartTime;
                        }
                        else
                        {
                            measureStartTime = prev.songStartTime + prev.timeLength;
                        }
                    }

                    currentMeasure.songStartTime = measureStartTime;
                    currentMeasure.timeLength = lastMilInMeasure;
                    Console.WriteLine($"mesureSartTime : {measureStartTime} - measureLength {currentMeasure.timeLength}");
                    chartData.AddMeasure(currentMeasure);

                    // Création de la nouvelle mesure
                    currentMeasure = new Measure(0, 0, currentTimeSignature, currentBPM);
                    lastMillisecondInMeasure = 0;
                }
            }
        }

        return chartData;
    }
}
