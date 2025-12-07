using Taiko_CS.Enums;

namespace Taiko_CS.Chart;

public class Chart
{
    public static ChartData Parse(string tjaFilePath, Difficulty difficulty)
    {
        string numbers = "0123456789";
        string fileData = "";
        try
        {
            using StreamReader reader = new(tjaFilePath);
            
            fileData = reader.ReadToEnd();
        }
        catch (IOException e)
        {
            Console.WriteLine("The file could not be read:");
            Console.WriteLine(e.Message);
        }

        ChartData chartData = new ChartData();
        Measure currentMeasure = null;
        bool showMeasureBar;
        bool isGoGoTime;
        bool isParsingFieldName = true;
        String[] fileLines = fileData.Split("\n");
        string fieldName = "";
        string fieldValue = "";
        string measureContent ="";
        string eventContent = "";
        double currentTimeSignature = 1;
        double currentBPM = 120;
        foreach (string line in fileLines)
        {
            int lineIndex = 0;
            foreach (char c in line)
            {
                if (c == '/' && line[lineIndex + 1] == '/')
                {
                    break;
                }
                if (!numbers.Contains(line[0]) && line[0] != '#')
                {
                    if (isParsingFieldName)
                    {
                        fieldName += c.ToString();   
                    }
                    else
                    {
                        fieldValue += c.ToString();
                    }
                    if (c == ':')
                    {
                        isParsingFieldName = false;
                    }
                }

                if (numbers.Contains(line[0]))
                {
                    if (c == ',')
                    {
                        break;
                    }
                    if (currentMeasure == null)
                    {
                        currentMeasure = new Measure();
                    }

                    measureContent += c.ToString();
                }
                
                
                lineIndex++;
            }

            if (!numbers.Contains(line[0]) && line[0] != '#')
            {
               chartData.SetField(fieldName, fieldValue);
            }

            if (numbers.Contains(line[0]))
            {
                double lastTimeInMeasure = 0;
                foreach (char c in measureContent)
                {
                    NoteType noteType = (NoteType) int.Parse(c.ToString());
                    double millisecondsPerMeasure = 60000 * currentTimeSignature * 4 / currentBPM;
                    double timeInMeasure = currentMeasure.GetNumberOfNotes() == 0 ? 0 : lastTimeInMeasure + (millisecondsPerMeasure / currentMeasure.GetNumberOfNotes());
                    Note note = new Note(noteType,  timeInMeasure);
                    currentMeasure.AddNote(note);
                }
            }
        } 
        return chartData;
    }
}