namespace Dimension.Data.SimpleDataFrame.Library.Structures.DataFrame;

public interface ISimpleDataFrameColumn
{
    string Name { get; }
    int Length { get; }
    bool ContainsIndex(DateTime dateIndex);
    List<DateTime> Indexes { get; }
    Type ValueType { get; }

    #region Methods

    void Initialize(string columnName);

    #region Methods - Factory

    /// <summary>
    /// Factory method - does NOT add to the column; just ensures the right type internally when the type is not known in the calling scope.
    /// </summary>
    /// <param name="simpleValueDateIndex"></param>
    /// <param name="simpleValueColumnName"></param>
    /// <returns></returns>
    ISimpleDataFrameValue CreateNewDateFrameValue(DateTime simpleValueDateIndex, string simpleValueColumnName);

    #endregion Methods - Factory

    #region Methods - Single

    void AddValueUntyped(DateTime dateIndex, object? newValue, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw);
    void AddValueUntyped(DateTime dateIndex, ISimpleDataFrameValue newValue, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw);

    object? GetValueUntyped(DateTime dateIndex, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw);
    ISimpleDataFrameValue GetValueTyped(DateTime dateIndex, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw);
    bool TryGetValueUntyped(DateTime dateIndex, out object? value);
    bool TryGetSimpleValueUntyped(DateTime dateIndex, out ISimpleDataFrameValue? simpleDataFrameValue);

    void UpdateValueUntyped(DateTime dateIndex, object? newValue, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw);

    void DeleteValue(DateTime dateIndex, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw);

    #endregion Methods - Single

    #region Methods - Bulk

    void AddValuesUntyped(List<ISimpleDataFrameValue> newData, IfExistsBehaviour ifExists);
    List<ISimpleDataFrameValue> GetValuesUntyped();
    List<ISimpleDataFrameValue> GetValuesUntyped(List<DateTime> dateIndices, IfMissingBehaviour ifMissing);
    void UpdateValuesUntyped(List<ISimpleDataFrameValue> newData, IfMissingBehaviour ifMissing);
    void DeleteValues(List<DateTime> dateIndices, IfMissingBehaviour ifMissing);

    #endregion Methods - Bulk

    #region Methods - Slicing

    List<DateTime> GetIndexes<T1>(Func<T1, bool> selectorFunction);
    ISimpleDataFrameColumn Clone();
    ISimpleDataFrameColumn<T1> GetSubColumn<T1>(DateTime startDate, DateTime endDate);
    ISimpleDataFrameColumn<T1> GetSubColumn<T1>(List<DateTime> matchingRange);

    #endregion Methods - Slicing

    #endregion Methods
}

public interface ISimpleDataFrameColumn<T>
    : ISimpleDataFrameColumn
{
    Dictionary<DateTime, T> Data { get; }
    T? this[DateTime dateIndex] { get; set; }

    #region Methods

    #region Methods - Factory

    /// <summary>
    /// Factory method - does NOT add to the column; just ensures the right type internally
    /// </summary>
    /// <param name="simpleValueDateIndex"></param>
    /// <param name="simpleValueColumnName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    SimpleDataFrameValue<T> CreateNewDateFrameValue(DateTime simpleValueDateIndex, string simpleValueColumnName, T value);

    #endregion Methods - Factory

    #region Methods - Single

    void AddValue(DateTime dateIndex, T newValue, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw);

    T? GetValue(DateTime dateIndex, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw);
    bool TryGetValue(DateTime index, out T? value);
    bool TryGetSimpleValue(DateTime dateIndex, out ISimpleDataFrameValue<T>? simpleDataFrameValue);

    void UpdateValue(DateTime dateIndex, T newValue, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw);

    #endregion Methods - Single

    #region Methods - Bulk

    void AddValues(List<ISimpleDataFrameValue<T>> newData, IfExistsBehaviour ifExists);
    List<ISimpleDataFrameValue<T?>> GetValues();
    List<ISimpleDataFrameValue<T?>> GetValues(List<DateTime> dateIndices, IfMissingBehaviour ifMissing);
    void UpdateValues(List<ISimpleDataFrameValue<T>> newData, IfMissingBehaviour ifMissing);

    #endregion Methods - Bulk

    #region Methods - Slicing

    List<DateTime> GetIndexes(Func<T, bool> selectorFunction);
    ISimpleDataFrameColumn<T> GetSubColumn(DateTime startDate, DateTime endDate);
    ISimpleDataFrameColumn<T> GetSubColumn(List<DateTime> matchingRange);

    #endregion Methods - Slicing

    #endregion Methods
}
