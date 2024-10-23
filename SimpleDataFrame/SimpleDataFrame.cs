using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using CsvHelper;

namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame;

public class SimpleDataFrame
{
    #region Members

    private readonly Dictionary<string, ISimpleDataFrameColumn> _columns;
    public int ColumnCount => this._columns.Count;
    public int RowCount => this._columns.Count > 0 ? this._columns.Values.Select(c => c.Length).Max() : 0;

    #endregion Members

    #region Constructor

    public SimpleDataFrame()
    {
        this._columns = new Dictionary<string, ISimpleDataFrameColumn>();
    }

    #endregion Constructor

    #region Methods

    public ISimpleDataFrameColumn this[string columnName]
    {
        get => this._columns[columnName];
        set => this._columns[columnName] = value;
    }

    #region Methods - Column

    #region Methods - Column - Add

    public ISimpleDataFrameColumn AddColumn(ISimpleDataFrameColumn columnToAdd, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw)
    {
        if (this._columns.ContainsKey(columnToAdd.Name))
        {
            if (this._columns[columnToAdd.Name].ValueType != columnToAdd.ValueType)
            {
                throw new ArgumentException($"Column {columnToAdd.Name} already exists with a different type.");
            }

            switch (ifExists)
            {
                case IfExistsBehaviour.Continue:
                    Debug.WriteLine($"Column of same name already exists.  Continuing as per {nameof(ifExists)} flag.  Returning existing column.");
                    return this._columns[columnToAdd.Name];
                case IfExistsBehaviour.Overwrite:
                    this._columns.Remove(columnToAdd.Name);
                    break;
                case IfExistsBehaviour.Throw:
                    throw new ArgumentException($"Column {columnToAdd.Name} already exists.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifExists), ifExists, null);
            }
        }

        this._columns[columnToAdd.Name] = columnToAdd;
        return columnToAdd;
    }

    public SimpleDataFrameColumn<T> AddColumn<T>(string columnName, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw)
    {
        if (this._columns.ContainsKey(columnName))
        {
            if (this._columns[columnName].ValueType != typeof(T))
            {
                throw new ArgumentException($"Column {columnName} already exists with a different type.");
            }

            switch (ifExists)
            {
                case IfExistsBehaviour.Continue:
                    Debug.WriteLine($"Column of same name already exists.  Continuing as per {nameof(ifExists)} flag.  Returning existing column.");
                    return (SimpleDataFrameColumn<T>)this._columns[columnName];
                case IfExistsBehaviour.Overwrite:
                    this._columns.Remove(columnName);
                    break;
                case IfExistsBehaviour.Throw:
                    throw new ArgumentException($"Column {columnName} already exists.");
            }
        }

        var newColumn = new SimpleDataFrameColumn<T>(columnName);
        this._columns[columnName] = newColumn;
        return newColumn;
    }

    public ISimpleDataFrameColumn AddColumn(string columnName, Type type, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw)
    {
        if (this._columns.ContainsKey(columnName))
        {
            if (this._columns[columnName].ValueType != type)
            {
                throw new ArgumentException($"Column {columnName} already exists with a different type.");
            }

            switch (ifExists)
            {
                case IfExistsBehaviour.Continue:
                    Debug.WriteLine($"Column of same name already exists.  Continuing as per {nameof(ifExists)} flag.  Returning existing column.");
                    return this._columns[columnName];
                case IfExistsBehaviour.Overwrite:
                    this._columns.Remove(columnName);
                    break;
                case IfExistsBehaviour.Throw:
                    throw new ArgumentException($"Column {columnName} already exists.");
            }
        }

        var genericType = typeof(SimpleDataFrameColumn<>).MakeGenericType(type);
        var newColumn = (ISimpleDataFrameColumn)Activator.CreateInstance(genericType)!;
        newColumn.Initialize(columnName);

        this._columns[columnName] = newColumn;
        return newColumn;
    }

    #endregion Methods - Column - Add

    #region Methods - Column - Get

    public bool TryGetColumn(string businessDateColumnName, out ISimpleDataFrameColumn? retrievedColumn)
    {
        if (this._columns.TryGetValue(businessDateColumnName, out retrievedColumn))
        {
            return true;
        }

        retrievedColumn = null;
        return false;
    }

    public bool TryGetColumn(string businessDateColumnName, Type expectedType, out ISimpleDataFrameColumn? retrievedColumn)
    {
        if (!this._columns.TryGetValue(businessDateColumnName, out retrievedColumn))
        {
            retrievedColumn = null;
            return false;
        }

        if (retrievedColumn.ValueType != expectedType)
        {
            throw new Exception($"Column type mismatch.  Expected '{expectedType.Name}' but found '{retrievedColumn.ValueType.Name}'.");
        }

        retrievedColumn = null;
        return false;
    }

    public bool TryGetColumn<T>(string businessDateColumnName, out ISimpleDataFrameColumn<T>? retrievedColumn)
    {
        if (!this._columns.TryGetValue(businessDateColumnName, out var candidateColumn))
        {
            retrievedColumn = null;
            return false;
        }

        retrievedColumn = (ISimpleDataFrameColumn<T>)candidateColumn;
        return true;
    }

    public SimpleDataFrameColumn<T> GetColumn<T>(string name, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        if (!this._columns.TryGetValue(name, out var column))
        {
            switch (ifMissing)
            {
                case IfMissingBehaviour.Create:
                    Debug.WriteLine($"Creating missing column {name} of type {typeof(T).Name}");
                    return this.AddColumn<T>(name);
                case IfMissingBehaviour.Continue:
                    Debug.WriteLine($"Column '{name}' does not exist in the DataFrame.  Continuing as per {nameof(ifMissing)} flag.");
                    return null!;
                case IfMissingBehaviour.Throw:
                    throw new ArgumentException($"Column '{name}' not found.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
            }
        }

        if (column.Name != name)
        {
            throw new Exception($"Column name mismatch.  Expected '{name}' but found '{column.Name}'.");
        }

        if (column.ValueType != typeof(T))
        {
            throw new Exception($"Column type mismatch.  Expected '{typeof(T).Name}' but found '{column.ValueType.Name}'.");
        }

        return (SimpleDataFrameColumn<T>)column;
    }

    #endregion Methods - Column - Get

    #region Methods - Column - Update

    public void UpdateColumn(string columnName, List<ISimpleDataFrameValue> newColumnData, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        // Check if the specified column exists
        if (!this._columns.ContainsKey(columnName))
        {
            switch (ifMissing)
            {
                case IfMissingBehaviour.Create:
                    var columnType = newColumnData.Select(d => d.ValueType).Distinct().Single();
                    Debug.WriteLine($"Creating missing column {columnName} of type {columnType.Name}");
                    this.AddColumn(columnName, columnType);
                    break;
                case IfMissingBehaviour.Continue:
                    Debug.WriteLine($"Column '{columnName}' does not exist in the DataFrame.  Continuing as per {nameof(ifMissing)} flag.");
                    return;
                case IfMissingBehaviour.Throw:
                    throw new ArgumentException($"Column '{columnName}' does not exist in the DataFrame.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
            }

            throw new ArgumentException($"Column '{columnName}' does not exist in the DataFrame.");
        }

        // Get the column
        var simpleDataFrameColumn = this._columns[columnName];

        // Update the column
        simpleDataFrameColumn.UpdateValuesUntyped(newColumnData, ifMissing);
    }

    #endregion Methods - Column - Update

    #region Methods - Column - Delete

    public void DeleteColumn(string columnName, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        // Check if the specified column exists
        if (!this._columns.ContainsKey(columnName))
        {
            switch (ifMissing)
            {
                case IfMissingBehaviour.Create:
                    Debug.WriteLine($"Column '{columnName}' does not exist in the DataFrame.  Ignoring {nameof(ifMissing)}.{ifMissing} flag.");
                    return;
                case IfMissingBehaviour.Continue:
                    Debug.WriteLine($"Column '{columnName}' does not exist in the DataFrame.  Continuing as per {nameof(ifMissing)} flag.");
                    return;
                case IfMissingBehaviour.Throw:
                    throw new ArgumentException($"Column '{columnName}' does not exist in the DataFrame.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
            }

            throw new ArgumentException($"Column '{columnName}' does not exist in the DataFrame.");
        }

        // Remove the column from the dictionary
        this._columns.Remove(columnName);
    }

    #endregion Methods - Column - Delete

    #endregion Methods - Column

    #region Methods - Row

    #region Methods - Row - Add

    public void AddRow(DateTime dateIndex, List<ISimpleDataFrameValue> rowData, IfExistsBehaviour valueBehaviour = IfExistsBehaviour.Throw)
    {
        foreach (var rowEnry in rowData)
        {
            if (!this._columns.ContainsKey(rowEnry.ColumnName))
            {
                throw new ArgumentException($"Column {rowEnry.ColumnName} does not exist.");
            }

            var simpleDataFrameColumn = this[rowEnry.ColumnName];
            simpleDataFrameColumn.AddValueUntyped(dateIndex, rowEnry.ValueUntyped, valueBehaviour);
        }
    }

    public void AddRow(DateTime dateIndex,
                       IEnumerable<ISimpleDataFrameValue> valuesToAdd,
                       IfExistsBehaviour valueBehaviour = IfExistsBehaviour.Throw,
                       IfMissingBehaviour columnBehaviour = IfMissingBehaviour.Create)
    {
        foreach (var value in valuesToAdd)
        {
            if (!this._columns.TryGetValue(value.ColumnName, out var column))
            {
                switch (columnBehaviour)
                {
                    case IfMissingBehaviour.Create:
                        column = this.AddColumn(value.ColumnName, value.ValueType, IfExistsBehaviour.Throw);
                        break;
                    case IfMissingBehaviour.Throw:
                        throw new ArgumentException($"Column '{value.ColumnName}' does not exist.");
                    case IfMissingBehaviour.Continue:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(columnBehaviour), columnBehaviour, null);
                }
            }

            if (value.DateIndex == dateIndex)
            {
                column.AddValueUntyped(dateIndex, value.ValueUntyped, valueBehaviour);
            }
            else
            {
                throw new ArgumentException("Row date does not match.");
            }
        }
    }

    #endregion Methods - Row - Add

    #region Methods - Row - Get

    public Dictionary<string, ISimpleDataFrameValue?> GetRow(DateTime dateIndex, IfMissingBehaviour ifMissing)
    {
        Dictionary<string, ISimpleDataFrameValue?> rowValues = new();

        foreach (var column in this._columns)
        {
            if (column.Value.ContainsIndex(dateIndex))
            {
                rowValues[column.Key] = column.Value.GetValueTyped(dateIndex);
            }
            else
            {
                switch (ifMissing)
                {
                    case IfMissingBehaviour.Throw:
                        throw new ArgumentException($"Missing data for column '{column.Key}' on date '{dateIndex}'.");
                    case IfMissingBehaviour.Create:
                        rowValues[column.Key] = null;
                        break;
                    case IfMissingBehaviour.Continue:
                        // Optionally, do not add the key to the dictionary.
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
                }
            }
        }

        return rowValues;
    }

    #endregion Methods - Row - Get

    #region Methods - Row - Update

    public void UpdateRow(DateTime dateIndex, IEnumerable<ISimpleDataFrameValue> values, IfMissingBehaviour ifMissing)
    {
        foreach (var value in values)
        {
            if (!this._columns.TryGetValue(value.ColumnName, out var column))
            {
                switch (ifMissing)
                {
                    case IfMissingBehaviour.Create:
                        column = this.AddColumn(value.ColumnName, value.ValueType, IfExistsBehaviour.Throw);
                        break;
                    case IfMissingBehaviour.Throw:
                        throw new ArgumentException($"Column '{value.ColumnName}' does not exist.");
                    case IfMissingBehaviour.Continue:
                        continue;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
                }
            }

            // Check if the column contains the dateIndex
            if (column.ContainsIndex(dateIndex))
            {
                column.UpdateValueUntyped(dateIndex, value, ifMissing);
            }
            else
            {
                switch (ifMissing)
                {
                    case IfMissingBehaviour.Create:
                        column.AddValueUntyped(dateIndex, value.ValueUntyped, IfExistsBehaviour.Throw);
                        break;
                    case IfMissingBehaviour.Throw:
                        // Optionally handle the case where the value does not exist and ifMissing is false
                        throw new ArgumentException($"Row with dateIndex {dateIndex} does not exist in column '{value.ColumnName}'.");
                    case IfMissingBehaviour.Continue:
                        // Do nothing
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
                }
            }
        }
    }

    #endregion Methods - Row - Update

    #region Methods - Row - Delete

    public void DeleteRow(DateTime dateIndex, IfMissingBehaviour ifMissing)
    {
        foreach (var column in this._columns.Values)
        {
            if (column.ContainsIndex(dateIndex))
            {
                column.DeleteValue(dateIndex);
            }
            else
            {
                switch (ifMissing)
                {
                    case IfMissingBehaviour.Throw:
                        throw new ArgumentException($"Row with dateIndex {dateIndex} does not exist in column '{column.Name}'.");
                    case IfMissingBehaviour.Continue:
                        Debug.WriteLine($"Row with dateIndex {dateIndex} does not exist in column '{column.Name}'.  Continuing as per {nameof(ifMissing)}.{ifMissing}");
                        break;
                    case IfMissingBehaviour.Create:
                        Debug.WriteLine($"Ignoring {IfMissingBehaviour.Create} flag in {nameof(this.DeleteRow)} as we");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
                }
            }
        }
    }

    #endregion Methods - Row - Delete

    #endregion Methods - Row

    #region Methods - Value

    #region Methods - Value - Add

    public void AddValue<T>(DateTime dateIndex, string columnName, T value, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw)
    {
        // Check if the specified column exists
        if (!this._columns.ContainsKey(columnName))
        {
            throw new ArgumentException($"Column '{columnName}' does not exist in the DataFrame.");
        }

        // Retrieve the column
        var column = this._columns[columnName];

        // Check if the column is of the expected type
        if (column is SimpleDataFrameColumn<T> typedColumn)
        {
            if (typedColumn.TryGetValue(dateIndex, out var existingValue))
            {
                switch (ifExists)
                {
                    case IfExistsBehaviour.Overwrite:
                        Debug.WriteLine($"Overwriting existing value for column '{columnName}' on date '{dateIndex}': {existingValue} with {value}.");
                        typedColumn.DeleteValue(dateIndex);
                        break;
                    case IfExistsBehaviour.Continue:
                        Debug.WriteLine($"Ignoring existing value for column '{columnName}' on date '{dateIndex}': {existingValue}.");
                        return;
                    case IfExistsBehaviour.Throw:
                        if (EqualityComparer<T>.Default.Equals(existingValue, value))
                        {
                            Debug.WriteLine($"Ignoring existing matching value for column '{columnName}' on date '{dateIndex}': {existingValue}.");
                            return;
                        }
                        var message = $"Cannot add new value {value} in column '{columnName}' as value {existingValue} already exists on date '{dateIndex}'.";
                        Debug.WriteLine(message);
                        throw new ArgumentException(message);
                    default:
                        throw new ArgumentOutOfRangeException(nameof(ifExists), ifExists, null);
                }
            }

            // Add the value to the column
            typedColumn.AddValue(dateIndex, value);
        }
        else
        {
            // The column exists but is of a different type
            throw new ArgumentException($"Column '{columnName}' exists but is not of type '{typeof(T).Name}'.");
        }
    }

    #endregion Methods - Value - Add

    #region Methods - Value - Get

    public T? GetValue<T>(DateTime dateIndex, string columnName, IfMissingBehaviour ifMissing)
    {
        if (!this._columns.ContainsKey(columnName))
        {
            switch (ifMissing)
            {
                case IfMissingBehaviour.Throw:
                    throw new ArgumentException($"Column '{columnName}' does not exist.");
                case IfMissingBehaviour.Create:
                    // Use this one wisely; GetValue should only result in AddColumn in rare cases
                    Debug.WriteLine($"Created missing column '{columnName}'.");
                    this.AddColumn<T>(columnName);
                    break;
                case IfMissingBehaviour.Continue:
                    Debug.WriteLine($"Column '{columnName}' does not exist.  Returning default value as per {nameof(ifMissing)}.{ifMissing}");
                    return default;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
            }

            throw new ArgumentException($"Column {columnName} does not exist.");
        }

        var simpleDataFrameColumn = this[columnName];
        if (simpleDataFrameColumn.ValueType != typeof(T))
        {
            throw new TypeAccessException($"Column '{columnName}' is of type '{simpleDataFrameColumn.ValueType.Name}' but the requested type is '{typeof(T).Name}'.");
        }

        var simpleDataFrameColumnT = (SimpleDataFrameColumn<T>)simpleDataFrameColumn;
        return simpleDataFrameColumnT.GetValue(dateIndex, ifMissing);
    }

    public object? GetValueUntyped(DateTime dateIndex, string columnName, IfMissingBehaviour ifMissing)
    {
        if (!this._columns.ContainsKey(columnName))
        {
            switch (ifMissing)
            {
                case IfMissingBehaviour.Throw:
                    throw new ArgumentException($"Column '{columnName}' does not exist.");
                case IfMissingBehaviour.Create:
                    // Use this one wisely; GetValue should only result in AddColumn in rare cases
                    throw new Exception($"Cannot create missing column '{columnName}' as required type is not known.");
                case IfMissingBehaviour.Continue:
                    Debug.WriteLine($"Column '{columnName}' does not exist.  Returning default value as per {nameof(ifMissing)}.{ifMissing}");
                    return default;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
            }

            throw new ArgumentException($"Column {columnName} does not exist.");
        }

        var simpleDataFrameColumn = this[columnName];
        var value = simpleDataFrameColumn.GetValueUntyped(dateIndex);
        return value;
    }

    public bool TryGetValue<T>(DateTime dateIndex, string columnName, out T? value)
    {
        value = default;

        if (!this._columns.TryGetValue(columnName, out var column))
        {
            return false;
        }

        if (column.ValueType != typeof(T))
        {
            return false;
        }

        var simpleDataFrameColumnT = (SimpleDataFrameColumn<T>)column;
        return simpleDataFrameColumnT.TryGetValue(dateIndex, out value);
    }

    public bool TryGetValueUntyped(DateTime dateIndex, string columnName, out object? value)
    {
        value = default;

        if (!this._columns.TryGetValue(columnName, out var column))
        {
            return false;
        }

        // Assuming GetValueUntyped() has a TryGetValue() equivalent in your implementation
        // If not, you'll need to handle it accordingly, potentially with reflection.
        return column.TryGetValueUntyped(dateIndex, out value);
    }

    #endregion Methods - Value - Get

    #region Methods - Value - Update

    public void UpdateValue<T>(DateTime dateIndex, string columnName, T newValue, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        if (!this._columns.ContainsKey(columnName))
        {
            switch (ifMissing)
            {
                case IfMissingBehaviour.Throw:
                    throw new ArgumentException($"Column '{columnName}' does not exist.");
                case IfMissingBehaviour.Create:
                    // Use this one wisely; UpdateValue should only result in AddColumn in rare cases
                    Debug.WriteLine($"Created missing column '{columnName}'.");
                    this.AddColumn<T>(columnName);
                    break;
                case IfMissingBehaviour.Continue:
                    Debug.WriteLine($"Column '{columnName}' does not exist.  Ignoring update as per {nameof(ifMissing)}.{ifMissing}");
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
            }
        }

        var simpleDataFrameColumn = this._columns[columnName];

        // Check if the column is of the expected type
        if (simpleDataFrameColumn is SimpleDataFrameColumn<T> typedColumn)
        {
            // Update the value if the types match
            typedColumn.UpdateValue(dateIndex, newValue);
        }
        else
        {
            // Throw an exception if the column is not of the expected type
            throw new InvalidCastException($"Column '{columnName}' is not of type '{typeof(T).Name}'.");
        }
    }

    public void UpdateValueUntyped(DateTime dateIndex, string columnName, object newValue, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        if (!this._columns.ContainsKey(columnName))
        {
            switch (ifMissing)
            {
                case IfMissingBehaviour.Throw:
                    throw new ArgumentException($"Column '{columnName}' does not exist.");
                case IfMissingBehaviour.Create:
                    // Use this one wisely; UpdateValue should only result in AddColumn in rare cases
                    Debug.WriteLine($"Created missing column '{columnName}'.");
                    var requiredType = newValue.GetType();
                    this.AddColumn(columnName, requiredType, IfExistsBehaviour.Throw);
                    break;
                case IfMissingBehaviour.Continue:
                    Debug.WriteLine($"Column '{columnName}' does not exist.  Ignoring update as per {nameof(ifMissing)}.{ifMissing}");
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
            }
        }

        var simpleDataFrameColumn = this._columns[columnName];
        simpleDataFrameColumn.UpdateValueUntyped(dateIndex, newValue);
    }

    #endregion Methods - Value - Update

    #region Methods - Value - Delete

    public void DeleteValue(DateTime dateIndex, string columnName, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        if (!this._columns.ContainsKey(columnName))
        {
            switch (ifMissing)
            {
                case IfMissingBehaviour.Throw:
                    throw new ArgumentException($"Column '{columnName}' does not exist.");
                case IfMissingBehaviour.Create:
                    // Use this one wisely; GetValue should only result in AddColumn in rare cases
                    Debug.WriteLine($"Cannot create missing column '{columnName}' as there is no point when deleting.");
                    return;
                case IfMissingBehaviour.Continue:
                    Debug.WriteLine($"Column '{columnName}' does not exist.  Continuing as per {nameof(ifMissing)}.{ifMissing}");
                    return;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
            }
        }

        var simpleDataFrameColumn = this._columns[columnName];
        simpleDataFrameColumn.DeleteValue(dateIndex);
    }

    #endregion Methods - Value - Delete

    #endregion Methods - Value

    #region Methods - Slicing

    public HashSet<DateTime> Indexes()
    {
        var uniquedDates = this._columns.Values.SelectMany(c => c.Indexes).Distinct().OrderBy(d => d).ToHashSet();
        return uniquedDates;
    }

    public Dictionary<string, ISimpleDataFrameValue?> this[DateTime dateIndex]
    {
        get
        {
            return this.GetRow(dateIndex, IfMissingBehaviour.Throw);
        }
    }

    public IEnumerable<Dictionary<string, object?>> IterateRows()
    {
        var dateIndices = this.Indexes();
        foreach (var dateIndex in dateIndices)
        {
            var row = new Dictionary<string, object?>();
            foreach (var column in this._columns)
            {
                row[column.Key] = column.Value.GetValueUntyped(dateIndex);
            }

            yield return row;
        }
    }

    /// <summary>
    /// Fingers crossed this works as an iterator
    /// </summary>
    public IEnumerable<SimpleDataFrameRow> Rows
    {
        get
        {
            var dateIndices = this.Indexes();
            foreach (var dateIndex in dateIndices)
            {
                // Nullable value to match expected nullability of the parameter
                var row = new Dictionary<string, ISimpleDataFrameValue?>();
                foreach (var kvp in this._columns)
                {
                    if (kvp.Value.TryGetSimpleValueUntyped(dateIndex, out var simpleValue))
                    {
                        if (simpleValue is null)
                        {
                            simpleValue = kvp.Value.CreateNewDateFrameValue(dateIndex, kvp.Key);
                        }

                        row[kvp.Key] = simpleValue;
                    }
                    else
                    {
                        simpleValue = kvp.Value.CreateNewDateFrameValue(dateIndex, kvp.Key);
                        row[kvp.Key] = simpleValue;
                    }
                }

                yield return new SimpleDataFrameRow(dateIndex, row);
            }
        }
    }

    public SimpleDataFrame GetSubFrameByDateRange<T>(DateTime startDate, DateTime endDate)
    {
        var subFrame = new SimpleDataFrame();
        foreach (var kvp in this._columns)
        {
            var subColumn = kvp.Value.GetSubColumn<T>(startDate, endDate);
            subFrame.AddColumn(subColumn);
        }

        return subFrame;
    }

    public SimpleDataFrame GetSubFrame<T>(string columnName, Func<T, bool> selectorFunction)
    {
        var subFrame = new SimpleDataFrame();
        if (this._columns.TryGetValue(columnName, out var selectorColumn))
        {
            var matchingRange = selectorColumn.GetIndexes(selectorFunction);
            foreach (var column in this._columns)
            {
                var subColumn = column.Value.GetSubColumn<T>(matchingRange);
                subFrame.AddColumn(subColumn);
            }
        }
        else
        {
            throw new ArgumentException($"Column {columnName} does not exist.");
        }

        return subFrame;
    }

    public SimpleDataFrame GetSubFrameByColumns(IEnumerable<string> columnNames)
    {
        var subFrame = new SimpleDataFrame();
        foreach (var columnName in columnNames)
        {
            if (this._columns.TryGetValue(columnName, out var column))
            {
                var cloneColumn = column.Clone();
                subFrame.AddColumn(cloneColumn);
            }
        }

        return subFrame;
    }

    #endregion Methods - Slicing

    #region Methods - Filtering

    public SimpleDataFrame Filter(Func<Dictionary<string, ISimpleDataFrameValue?>, bool> predicate)
    {
        var filteredDataFrame = new SimpleDataFrame();
        foreach (var dateIndex in this.Indexes())
        {
            var row = this[dateIndex];
            if (predicate(row))
            {
                foreach (var column in row.Keys)
                {
                    filteredDataFrame.AddValue(dateIndex, column, row[column]?.ValueUntyped);
                }
            }
        }

        return filteredDataFrame;
    }

    public SimpleDataFrame OrderBy<T>(string columnName)
    {
        if (!this._columns.TryGetValue(columnName, out var simpleDataFrameColumn))
        {
            throw new ArgumentException($"Column '{columnName}' does not exist.");
        }

        if (simpleDataFrameColumn.ValueType != typeof(T))
        {
            throw new ArgumentException($"Column '{columnName}' is not of type '{typeof(T).Name}'.");
        }

        if (simpleDataFrameColumn is not ISimpleDataFrameColumn<T> typedColumn)
        {
            throw new InvalidCastException($"Failed to cast column '{columnName}' to type '{typeof(ISimpleDataFrameColumn<T>).Name}'.");
        }

        var orderedIndexes = typedColumn.GetValues().OrderBy(kv => kv.Value).Select(kv => kv.DateIndex);
        return this.GetSubFrameByIndexes(orderedIndexes);
    }

    private SimpleDataFrame GetSubFrameByIndexes(IEnumerable<DateTime> indexes)
    {
        var subFrame = new SimpleDataFrame();
        foreach (var index in indexes)
        {
            foreach (var column in this._columns)
            {
                subFrame.AddValue(index, column.Key, column.Value.GetValueUntyped(index));
            }
        }

        return subFrame;
    }

    public Dictionary<T, SimpleDataFrame> GroupBy<T>(string columnName)
        where T : notnull
    {
        var groups = new Dictionary<T, List<DateTime>>();
        foreach (var index in this.Indexes())
        {
            // Pattern matching with 'is' to check and cast in one step
            if (this._columns[columnName].GetValueUntyped(index) is T typedValue)
            {
                if (!groups.ContainsKey(typedValue))
                {
                    groups[typedValue] = new List<DateTime>();
                }
                groups[typedValue].Add(index);
            }
            else
            {
                throw new InvalidOperationException($"Value in column '{columnName}' at index '{index}' is not of type '{typeof(T)}'.");
            }
        }

        var result = new Dictionary<T, SimpleDataFrame>();
        foreach (var group in groups)
        {
            result[group.Key] = this.GetSubFrameByIndexes(group.Value);
        }

        return result;
    }

    public Dictionary<string, double?> ApplyStatistic<T>(Func<IEnumerable<T>, double?> statisticFunction)
#if NET7_0_OR_GREATER
        // https://learn.microsoft.com/en-us/dotnet/standard/generics/math
        // https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters
        where T : INumber<T>
#else
        // https://stackoverflow.com/questions/3329576/generic-constraint-to-match-numeric-types
        // All the numeric types implement these 5 interfaces, but IFormattable is not implemented by bool;
        // and strings are a reference type, so they're not applicable.
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
#endif
    {
        var results = new Dictionary<string, double?>();
        foreach (var column in this._columns)
        {
            var valueType = column.Value.ValueType;
            if (valueType.IsNumericType())
            {
                // Using pattern matching to avoid casting issues
                if (column.Value is SimpleDataFrameColumn<T> typedColumn)
                {
                    // Filter out null values before applying the statistic function
                    var numericValues = typedColumn.GetValues()
                        .Where(v => v.Value != null)
                        .Select(v => v.Value!); // Force non-nullable values for the statistic function

                    // Applying the statistic function
                    var statistic = statisticFunction(numericValues);
                    results[column.Key] = statistic;
                }
                else
                {
                    // Handle the case where the column is not of the expected numeric type
                    throw new InvalidCastException($"Column '{column.Key}' is not of the expected numeric type '{typeof(T)}'.");
                }
            }
        }

        return results;
    }

    #endregion Methods - Filtering

    public void Output(string csvFilePath = "", bool toDebug = false, int head = 0)
    {
        try
        {
            var indexes = this.Indexes().ToList();
            if (head > 0)
            {
                indexes = indexes.Take(head).ToList();
            }

            var intSpacing = 2;
            var dateFormat = "yyyy-MM-dd";
            var dateColWidth = dateFormat.Length + intSpacing;

            var debugHeaderRow = toDebug ? new StringBuilder("Date".PadRight(dateColWidth)) : null;
            var csvHeaderRow = csvFilePath.Length > 0 ? new StringBuilder("Date") : null;
            var debugOutputRows = toDebug ? new Dictionary<DateTime, StringBuilder>() : null;
            var outputRows = csvFilePath.Length > 0 ? new Dictionary<DateTime, StringBuilder>() : null;

            foreach (var index in indexes)
            {
                if (toDebug)
                {
                    var asString = new StringBuilder(index.ToString(dateFormat).PadRight(dateColWidth));
                    debugOutputRows?.Add(index.Date, asString);
                }

                if (csvFilePath.Length > 0)
                {
                    if (outputRows is not null)
                        outputRows[index.Date] = new StringBuilder(index.ToString(dateFormat));
                }
            }

            foreach (var kvp in this._columns)
            {
                var valueType = kvp.Value.ValueType;
                var columnFormat = this.GetColumnFormat(valueType, kvp.Value.GetValuesUntyped());
                var columnWidth = this.CalculateColumnWidth(kvp.Value.GetValuesUntyped(), columnFormat, kvp.Value.Name.Length, intSpacing);

                if (toDebug)
                {
                    var asString = kvp.Key.PadLeft(columnWidth);
                    debugHeaderRow?.Append(asString);
                }

                if (csvFilePath.Length > 0)
                {
                    if (csvHeaderRow is not null)
                        csvHeaderRow.Append("," + kvp.Key);
                }

                foreach (var index in indexes)
                {
                    var rawValue = kvp.Value.TryGetValueUntyped(index, out var val) ? val : null;

                    if (toDebug)
                    {
                        var valueFormattedForDebug = rawValue != null
                                                         ? this.FormatValue(rawValue, valueType, columnFormat).PadLeft(columnWidth)
                                                         : new string(' ', columnWidth);
                        debugOutputRows?[index.Date].Append(valueFormattedForDebug);
                    }

                    if (csvFilePath.Length > 0)
                    {
                        var valueFormattedForCsv = rawValue != null ? rawValue.ToString() : "";
                        var newCsvField = "," + valueFormattedForCsv; // always prepend with comma as Date is already in StringBuilder above
                        if (outputRows is not null)
                            outputRows[index.Date].Append(newCsvField);
                    }
                }
            }

            if (toDebug)
            {
                var asString = debugHeaderRow?.ToString() ?? "";
                Debug.WriteLine(asString);
                if (debugOutputRows is not null)
                {
                    foreach (var row in debugOutputRows?.Values ?? Enumerable.Empty<StringBuilder>())
                    {
                        Debug.WriteLine(row.ToString());
                    }
                }
            }

            if (csvFilePath.Length > 0)
            {
                Debug.WriteLine($"Saving to {csvFilePath}");
                using (var textWriter = new StreamWriter(csvFilePath))
                using (var csvWriter = new CsvWriter(textWriter, CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteField(csvHeaderRow?.ToString() ?? string.Empty);
                    csvWriter.NextRecord();

                    if (outputRows is not null)
                    {
                        foreach (var row in outputRows?.Values ?? Enumerable.Empty<StringBuilder>())
                        {
                            csvWriter.WriteField(row.ToString());
                            csvWriter.NextRecord();
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private string FormatValue(object? value, Type valueType, string columnFormat)
    {
        try
        {
            if (value == null)
            {
                return "";
            }

            var useType = Nullable.GetUnderlyingType(valueType) ?? valueType;

            if (useType.IsNumericType())
            {
                var asDouble = Convert.ToDouble(value);
                return asDouble.ToString(columnFormat);
            }
            else if (useType == typeof(DateTime))
            {
                var asDateTime = (DateTime)value;
                return asDateTime.ToString(columnFormat);
            }
            else
            {
                return value.ToString() ?? "";
            }
        }
        catch (FormatException ex)
        {
            throw new FormatException($"Failed to format value '{value}' of type '{valueType}' using format '{columnFormat}'.", ex);
        }
        catch (InvalidCastException ex)
        {
            throw new InvalidCastException($"Invalid type cast for value '{value}' to '{valueType}'.", ex);
        }
    }

    private int CalculateColumnWidth(IEnumerable<ISimpleDataFrameValue> values, string columnFormat, int nameLength, int intSpacing)
    {
        if (!values.Any())
        {
            return nameLength + intSpacing;
        }

        var type = values.First().ValueType;
        var maxWidth = values.Select(v => this.FormatValue(v.ValueUntyped, type, columnFormat).Length).Max();
        return Math.Max(maxWidth, nameLength) + intSpacing;
    }

    private string GetColumnFormat(Type valueType, IEnumerable<ISimpleDataFrameValue> values)
    {
        var useType = Nullable.GetUnderlyingType(valueType) ?? valueType;

        if (useType.IsNumericType())
        {
            var format = this.GetNumericColumnFormat(values);
            return format;
        }
        else if (useType == typeof(DateTime))
        {
            return "yyyy-MM-dd";
        }
        else
        {
            // Handle unhandled types
            Debug.WriteLine($"Unhandled type {useType.FullName}");
            return "#,##0.00"; // Default format for unhandled types
        }
    }

    private string GetNumericColumnFormat(IEnumerable<ISimpleDataFrameValue> values)
    {
        var numericValues = values.Select(v => Convert.ToDouble(v.ValueUntyped)).ToList();
        if (!numericValues.Any())
        {
            return "";
        }

        var medianDecimalPlaces = this.GetSignificantDecimalPlaces(numericValues);
        return $"#,##0.{new string('0', medianDecimalPlaces)}";
    }

    /// <summary>
    /// Method to handle floating-point errors by identifying repeating zeroes
    /// e.g. 1323.2500001252 is converted to 1323.25
    /// e.g. 452439.45000000004 is converted to 452439.45
    /// The median value of the start of the repeating zeroes is returned.
    /// </summary>
    /// <param name="values"></param>
    /// <returns></returns>
    public int GetSignificantDecimalPlaces<T>(IEnumerable<T> values)
        where T : struct, IComparable, IComparable<T>, IConvertible, IEquatable<T>, IFormattable
    {
        try
        {
            var decimalPlacesList = new List<int>();

            foreach (var value in values)
            {
                if (decimal.TryParse(value.ToString(), NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out var decimalValue))
                {
                    // Convert the number to string with high precision
                    var stringValue = decimalValue.ToString("G29", CultureInfo.InvariantCulture);

                    if (stringValue.Contains('.'))
                    {
                        // Extract the decimal part depending on the floating-point error type
                        var decimalPart = stringValue.Split('.')[1];
                        if (decimalPart.Contains("00"))
                        {
                            var index = decimalPart.IndexOf("00", StringComparison.InvariantCulture);
                            decimalPlacesList.Add(index);
                        }

                        if (decimalPart.EndsWith("001"))
                        {
                            var index = decimalPart.IndexOf("00", StringComparison.InvariantCulture);
                            decimalPlacesList.Add(index);
                        }
                        else if (decimalPart.EndsWith("999"))
                        {
                            var index = decimalPart.IndexOf("999", StringComparison.InvariantCulture);
                            decimalPlacesList.Add(index);
                        }
                        else
                        {
                            decimalPlacesList.Add(decimalPart.Length);
                        }
                    }
                    else
                    {
                        // No decimal part
                        decimalPlacesList.Add(0);
                    }
                }
            }

            // Use the median to determine the most representative number of decimal places
            var decimalPlaces = 0;
            switch (decimalPlacesList.Count)
            {
                case 0:
                    decimalPlaces = 6;
                    break;
                case 1:
                    decimalPlaces = decimalPlacesList.First();
                    break;
                case 2:
                    decimalPlaces = (int)(decimalPlacesList.Average() + 0.5);
                    break;
                default:
                    decimalPlaces = decimalPlacesList.Median();
                    break;
            }

            return decimalPlaces;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return 0;
        }
    }

    #endregion Methods
}
