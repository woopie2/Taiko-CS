using Taiko_CS.Enums;

namespace Taiko_CS.Chart;

public class ChartData
{
    private Difficulty difficulty;
    List<Measure> measures;
    private Dictionary<string, string> fields;

    public void AddMeasure(Measure measure)
    {
        measures.Add(measure);
    }

    public void SetField(string fieldName, string fieldValue)
    {
        fields[fieldName] = fieldValue;
    }
}