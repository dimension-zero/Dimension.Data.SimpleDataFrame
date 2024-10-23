namespace Dimension.Library.Structures.SimpleDataFrame;

public record SimpleDataFrameRow(DateTime DateIndex, Dictionary<string, ISimpleDataFrameValue?> Values)
{
    public ISimpleDataFrameValue? this[string fieldName]
    {
        get
        {
            if (this.Values.TryGetValue(fieldName, out var value))
            {
                return value;
            }

            return null;
        }
    }
}
