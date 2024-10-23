namespace Dimension.Library.Structures.SimpleDataFrame;

public class SimpleDataFrameValue<T>
    : ISimpleDataFrameValue<T>
{
    public DateTime DateIndex { get; private set; }
    public string ColumnName { get; private set; }
    public Type ValueType => typeof(T);

    public T      Value        { get; private set; }
    public bool   HasValue     => this.Value != null;
    public object ValueUntyped => this.Value!;

    public SimpleDataFrameValue()
    {
        this.DateIndex  = DateTime.MinValue;
        this.ColumnName = string.Empty;
        this.Value         = default!;
    }

    public SimpleDataFrameValue(DateTime dateIndex, string columnName, T value)
        : this()
    {
        this.DateIndex  = dateIndex;
        this.ColumnName = columnName;
        this.Value         = value;
    }

    public bool IsNull => this.Value is null;

    public bool IsNullOrZero
    {
        get
        {
            if (this.Value is null)
            {
                return true;
            }

            if (typeof(T).IsNumericType())
            {
                return this.Value.Equals(0);
            }

            return false;
        }
    }
}
