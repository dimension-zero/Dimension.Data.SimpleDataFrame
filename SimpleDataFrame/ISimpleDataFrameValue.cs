namespace Dimension.Library.Structures.SimpleDataFrame;

public interface ISimpleDataFrameValue
{
    string ColumnName { get; }
    DateTime DateIndex { get; }
    object ValueUntyped { get; }
    Type ValueType { get; }
    bool IsNullOrZero { get; }
}

public interface ISimpleDataFrameValue<out T>
    : ISimpleDataFrameValue
{
    T Value { get; }
}
