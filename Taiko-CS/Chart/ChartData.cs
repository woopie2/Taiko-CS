using Taiko_CS.Enums;

namespace Taiko_CS.Chart;

public class ChartData
{
    private Difficulty difficulty;
    public List<Measure> measures = new List<Measure>();
    private Dictionary<string, string> fields = new Dictionary<string, string>();

    public void AddMeasure(Measure measure)
    {
        measures.Add(measure);
    }

    public void SetField(string fieldName, string fieldValue)
    {
        fields[fieldName] = fieldValue;
    }

    public string GetField(string fieldName)
    {
        return fields[fieldName];
    }

    public bool ContainsField(string fieldName)
    {
        return this.fields.ContainsKey(fieldName);
    }
}