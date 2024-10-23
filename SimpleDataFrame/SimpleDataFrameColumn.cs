using System;
using System.Collections.Generic;
using System.Linq;

namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame;

public class SimpleDataFrameColumn<T>
    : ISimpleDataFrameColumn<T>
{
    #region Members

    public string Name { get; private set; } = string.Empty;
    public Dictionary<DateTime, T> Data { get; }
    public List<DateTime> Indexes => this.Data.Keys.ToList();

    public int Length => this.Data.Count;
    public Type ValueType => typeof(T);

    #endregion Members

    #region Constructor

    public SimpleDataFrameColumn()
    {
        this.Data = new Dictionary<DateTime, T>();
    }

    public SimpleDataFrameColumn(string name)
        : this()
    {
        this.Name = name;
    }

    #endregion Constructor

    #region Methods

    public void Initialize(string name)
    {
        this.Name = name;
    }

    #region Methods - Factory

    public ISimpleDataFrameValue CreateNewDateFrameValue(DateTime dateIndex, string columnName)
    {
        var newValue = this.CreateNewDateFrameValue(dateIndex, columnName, default!);
        return newValue;
    }

    public SimpleDataFrameValue<T> CreateNewDateFrameValue(DateTime dateIndex, string columnName, T value)
    {
        var newValue = new SimpleDataFrameValue<T>(dateIndex, columnName, value);
        return newValue;
    }

    #endregion Methods - Factory

    /// <summary>
    /// Indexer
    /// </summary>
    /// <param name="dateIndex"></param>
    /// <returns></returns>
    public T? this[DateTime dateIndex]
    {
        get => this.Data.ContainsKey(dateIndex) ? this.Data[dateIndex] : default;
        set => this.Data[dateIndex] = value ?? throw new ArgumentNullException(nameof(value), "Null values cannot be assigned to the data frame.");
    }

    public bool ContainsIndex(DateTime dateIndex)
    {
        return this.Data.ContainsKey(dateIndex);
    }

    #region Methods - AGUD

    #region Methods - Add

    public void AddValue(DateTime dateIndex, T newValue, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw)
    {
        if (this.Data.ContainsKey(dateIndex))
        {
            switch (ifExists)
            {
                case IfExistsBehaviour.Overwrite:
                    this.Data[dateIndex] = newValue;
                    break;
                case IfExistsBehaviour.Throw:
                    throw new ArgumentException($"Value already exists for dateIndex {dateIndex}.");
                case IfExistsBehaviour.Continue:
                default:
                    // For IfExistsBehaviour.Continue, do nothing
                    break;
            }
        }
        else
        {
            this.Data[dateIndex] = newValue;
        }
    }

    public void AddValueTyped(DateTime dateIndex, ISimpleDataFrameValue newValue, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw)
    {
        if (newValue.ValueType is T)
        {
            var typedValue = (ISimpleDataFrameValue<T>)newValue;
            this.AddValue(dateIndex, typedValue.Value, ifExists);
        }
        else
        {
            throw new ArgumentException("Value type mismatch.");
        }
    }

    public void AddValueUntyped(DateTime dateIndex, object? newValue, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw)
    {
        if (newValue is T typedValue)
        {
            this.AddValue(dateIndex, typedValue, ifExists);
        }
        else
        {
            throw new ArgumentException("Value type mismatch.");
        }
    }

    public void AddValueUntyped(DateTime dateIndex, ISimpleDataFrameValue newValue, IfExistsBehaviour ifExists = IfExistsBehaviour.Throw)
    {
        if (newValue.ValueType == typeof(T))
        {
            var typedValue = (ISimpleDataFrameValue<T>)newValue;
            this.AddValue(dateIndex, typedValue.Value, ifExists);
        }
        else
        {
            throw new ArgumentException("Value type mismatch.");
        }
    }


    public void AddValues(List<ISimpleDataFrameValue<T>> newData, IfExistsBehaviour ifExists)
    {
        foreach (var value in newData)
        {
            this.AddValue(value.DateIndex, value.Value, ifExists);
        }
    }

    public void AddValuesUntyped(List<ISimpleDataFrameValue> newData, IfExistsBehaviour ifExists)
    {
        foreach (var value in newData)
        {
            this.AddValueUntyped(value.DateIndex, value, ifExists);
        }
    }

    #endregion Methods - Add

    #region Methods - Get

    public T? GetValue(DateTime dateIndex, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        if (this.Data.TryGetValue(dateIndex, out var value))
        {
            return value;
        }

        switch (ifMissing)
        {
            case IfMissingBehaviour.Create:
                this.AddValue(dateIndex, default(T) ?? throw new ArgumentNullException(nameof(value), "Null value cannot be created."), IfExistsBehaviour.Throw);
                return default;
            case IfMissingBehaviour.Continue:
                return default;
            case IfMissingBehaviour.Throw:
                throw new KeyNotFoundException($"No value found for dateIndex {dateIndex}.");
            default:
                throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
        }

    }

    public ISimpleDataFrameValue GetValueTyped(DateTime dateIndex, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        var value = this.GetValue(dateIndex, ifMissing);
        return new SimpleDataFrameValue<T?>(dateIndex, this.Name, value);
    }

    public object? GetValueUntyped(DateTime dateIndex, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        return this.GetValue(dateIndex, ifMissing);
    }

    public List<ISimpleDataFrameValue<T?>> GetValues(List<DateTime> dateIndices, IfMissingBehaviour ifMissing)
    {
        var values = this.GetValuesInternal(dateIndices, ifMissing)
            .Select(v => new SimpleDataFrameValue<T?>(v.DateIndex, this.Name, v.Value));
        return values.Cast<ISimpleDataFrameValue<T?>>().ToList();
    }

    public List<ISimpleDataFrameValue<T?>> GetValues()
    {
        var values = this.Data.Select(v => new SimpleDataFrameValue<T?>(v.Key, this.Name, v.Value));
        return values.Cast<ISimpleDataFrameValue<T?>>().ToList();
    }

    public List<ISimpleDataFrameValue> GetValuesUntyped(List<DateTime> dateIndices, IfMissingBehaviour ifMissing)
    {
        var values = this.GetValuesInternal(dateIndices, ifMissing)
            .Select(v => new SimpleDataFrameValue<T>(v.DateIndex, this.Name, v.Value!)) // Use the null-forgiving operator to suppress the warning
            .Cast<ISimpleDataFrameValue>()
            .ToList();

        return values;
    }

    public List<ISimpleDataFrameValue> GetValuesUntyped()
    {
        List<ISimpleDataFrameValue> values = new();
        foreach (var dateIndex in this.Indexes)
        {
            if (this.TryGetValue(dateIndex, out var foundValue) && foundValue is T value)
            {
                values.Add(new SimpleDataFrameValue<T>(dateIndex, this.Name, value));
            }
        }

        return values;
    }

    private IEnumerable<(DateTime DateIndex, T? Value)> GetValuesInternal(List<DateTime> dateIndices, IfMissingBehaviour ifMissing)
    {
        var values = new List<(DateTime DateIndex, T? Value)>();
        foreach (var dateIndex in dateIndices)
        {
            var value = this.GetValue(dateIndex, IfMissingBehaviour.Continue); // Bypass exception for missing values
            var isValuePresent = value != null;

            if (isValuePresent)
            {
                values.Add((dateIndex, value));
            }
            else
            {
                switch (ifMissing)
                {
                    case IfMissingBehaviour.Continue:
                        break;
                    case IfMissingBehaviour.Create:
                        values.Add((dateIndex, default));
                        break;
                    case IfMissingBehaviour.Throw:
                        throw new KeyNotFoundException($"No value found for dateIndex {dateIndex}.");
                }
            }
        }

        return values;
    }

    public bool TryGetValueUntyped(DateTime dateIndex, out object? foundValue)
    {
        if (this.TryGetValue(dateIndex, out var value))
        {
            foundValue = value;
            return true;
        }
        foundValue = null;
        return false;
    }

    public bool TryGetValue(DateTime dateIndex, out T? foundValue)
    {
        // Ensure that 'foundValue' can hold null regardless of what T is.
        // This requires 'T' to be a reference type or a nullable value type.
        if (this.Data.TryGetValue(dateIndex, out var value))
        {
            foundValue = value; // Safely cast 'value' to 'T?' which is always legal.
            return true;
        }
        else
        {
            foundValue = default; // Assigns null for reference types, or the nullable default for value types.
            return false;
        }
    }

    public bool TryGetSimpleValue(DateTime dateIndex, out ISimpleDataFrameValue<T>? simpleDataFrameValue)
    {
        if (this.TryGetValue(dateIndex, out var foundValue))
        {
            // Handle possible null value using the null-forgiving operator (!)
            simpleDataFrameValue = new SimpleDataFrameValue<T>(dateIndex, this.Name, foundValue!);
            return true;
        }

        simpleDataFrameValue = default;
        return false;
    }

    public bool TryGetSimpleValueUntyped(DateTime dateIndex, out ISimpleDataFrameValue? simpleDataFrameValue)
    {
        if (this.TryGetValue(dateIndex, out var foundValue))
        {
            simpleDataFrameValue = new SimpleDataFrameValue<T?>(dateIndex, this.Name, foundValue);
            return true;
        }

        simpleDataFrameValue = default;
        return false;
    }

    #region Methods - Get - Slicing

    /// <summary>
    /// Implements the generic method in the non-generic base interface
    /// </summary>
    /// <typeparam name="T1"></typeparam>
    /// <param name="selectorFunction"></param>
    /// <returns></returns>
    /// <exception cref="InvalidCastException"></exception>
    /// <exception cref="FormatException"></exception>
    public List<DateTime> GetIndexes<T1>(Func<T1, bool> selectorFunction)
    {
        var matchingDates = new List<DateTime>();
        foreach (var (dateIndex, value) in this.Data)
        {
            // Safely try to convert value to type T1
            if (value is T1 valueAsT1)
            {
                if (selectorFunction(valueAsT1))
                {
                    matchingDates.Add(dateIndex);
                }
            }
            else
            {
                // If the cast fails, we don't attempt to convert to avoid exceptions
                continue;
            }
        }

        return matchingDates;
    }

    public List<DateTime> GetIndexes(Func<T, bool> selectorFunction)
    {
        var matchingDates = new List<DateTime>();
        foreach (var (dateIndex, value) in this.Data)
        {
            if (selectorFunction(value))
            {
                matchingDates.Add(dateIndex);
            }
        }

        return matchingDates;
    }

    public ISimpleDataFrameColumn<T> GetSubColumn(DateTime startDate, DateTime endDate)
    {
        if (startDate > endDate)
        {
            throw new ArgumentException("Start date must be earlier than end date.");
        }

        var subColumn = new SimpleDataFrameColumn<T>(this.Name + "_SubColumn");
        foreach (var kvp in this.Data.Where(kvp => kvp.Key >= startDate && kvp.Key <= endDate))
        {
            subColumn.Data.Add(kvp.Key, kvp.Value);
        }

        return subColumn;
    }

    public ISimpleDataFrameColumn<T> GetSubColumn(List<DateTime> matchingRange)
    {
        var subColumn = new SimpleDataFrameColumn<T>(this.Name + "_SubColumn");
        foreach (var date in matchingRange)
        {
            if (this.Data.ContainsKey(date))
            {
                subColumn.Data.Add(date, this.Data[date]);
            }
        }

        return subColumn;
    }

    public ISimpleDataFrameColumn<T1> GetSubColumn<T1>(DateTime startDate, DateTime endDate)
    {
        var subColumn = new SimpleDataFrameColumn<T1>(this.Name + "_SubColumn");
        foreach (var kvp in this.Data.Where(kvp => kvp.Key >= startDate && kvp.Key <= endDate))
        {
            if (kvp.Value is T1 valueAsT1)
            {
                subColumn.Data.Add(kvp.Key, valueAsT1);
            }
            else
            {
                throw new InvalidCastException($"Cannot cast value of type {kvp.Value?.GetType()} to type {typeof(T1)}.");
            }
        }

        return subColumn;
    }

    public ISimpleDataFrameColumn<T1> GetSubColumn<T1>(List<DateTime> matchingRange)
    {
        var subColumn = new SimpleDataFrameColumn<T1>(this.Name + "_SubColumn");
        foreach (var date in matchingRange)
        {
            if (this.Data.ContainsKey(date))
            {
                if (this.Data[date] is T1 valueAsT1)
                {
                    subColumn.Data.Add(date, valueAsT1);
                }
                else
                {
                    var asString = this.Data[date]?.GetType();
                    throw new InvalidCastException($"Cannot cast value of type {asString} to type {typeof(T1)}.");
                }
            }
        }

        return subColumn;
    }

    #endregion Methods - Get - Slicing

    #endregion Methods - Get

    #region Methods - Update

    public void UpdateValue(DateTime dateIndex, T newValue, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        if (this.Data.ContainsKey(dateIndex))
        {
            this.Data[dateIndex] = newValue;
        }
        else
        {
            switch (ifMissing)
            {
                case IfMissingBehaviour.Throw:
                    throw new KeyNotFoundException($"No value found for dateIndex {dateIndex}.");
                case IfMissingBehaviour.Create:
                    this.Data.Add(dateIndex, newValue);
                    break;
                case IfMissingBehaviour.Continue:
                    // Do nothing
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(ifMissing), ifMissing, null);
            }
        }
    }

    public void UpdateValueUntyped(DateTime dateIndex, object? newValue, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        if (newValue is T typedValue)
        {
            this.UpdateValue(dateIndex, typedValue, ifMissing);
        }
        else
        {
            throw new ArgumentException($"Value type mismatch for column '{this.Name}'. Expected type '{typeof(T)}'.");
        }
    }

    public void UpdateValues(List<ISimpleDataFrameValue<T>> newData, IfMissingBehaviour ifMissing)
    {
        foreach (var value in newData)
        {
            this.UpdateValue(value.DateIndex, value.Value, ifMissing);
        }
    }

    public void UpdateValuesUntyped(List<ISimpleDataFrameValue> newData, IfMissingBehaviour ifMissing)
    {
        foreach (var value in newData)
        {
            this.UpdateValueUntyped(value.DateIndex, value.ValueUntyped, ifMissing);
        }
    }

    #endregion Methods - Update

    #region Methods - Delete

    public void DeleteValue(DateTime dateIndex, IfMissingBehaviour ifMissing = IfMissingBehaviour.Throw)
    {
        var keyExists = this.Data.ContainsKey(dateIndex);
        if (!keyExists && ifMissing == IfMissingBehaviour.Throw)
        {
            throw new ArgumentException($"Row with dateIndex {dateIndex} does not exist in column {this.Name}.");
        }

        this.Data.Remove(dateIndex);
    }

    public void DeleteValues(List<DateTime> dateIndices, IfMissingBehaviour ifMissing)
    {
        foreach (var dateIndex in dateIndices)
        {
            this.DeleteValue(dateIndex, ifMissing);
        }
    }

    #endregion Methods - Delete

    #endregion Methods - AGUD

    #region Methods - Statistics

    public T? Max() => this.Data.Values.Max();
    public T? Min() => this.Data.Values.Min();
    public IEnumerable<T> UniqueValues() => this.Data.Values.Distinct();


    #endregion Methods - Statistics

    public ISimpleDataFrameColumn Clone()
    {
        var clonedColumn = new SimpleDataFrameColumn<T>(this.Name);
        foreach (var entry in this.Data)
        {
            clonedColumn.AddValue(entry.Key, entry.Value);
        }

        return clonedColumn;
    }

    #endregion Methods
}
