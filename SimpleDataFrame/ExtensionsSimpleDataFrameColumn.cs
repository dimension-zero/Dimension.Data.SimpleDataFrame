using System;
using System.Linq;
using System.Numerics;

namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame;

public static class ExtensionsSimpleDataFrameColumn
{
    public static T? Mean<T>(this ISimpleDataFrameColumn<T> column) 
        where T : INumber<T>
    {
        if (!column.Data.Values.Any())
            return default;

        T sum = T.Zero;
        foreach (var value in column.Data.Values)
        {
            sum += value;
        }
        return sum / T.CreateChecked(column.Data.Count);
    }

    public static T? Sum<T>(this ISimpleDataFrameColumn<T> column) 
        where T : INumber<T>
    {
        if (!column.Data.Values.Any())
            return default;

        T sum = T.Zero;
        foreach (var value in column.Data.Values)
        {
            sum += value;
        }
        return sum;
    }

    public static double? StandardDeviation<T>(this ISimpleDataFrameColumn<T> column)
        where T : INumber<T>
    {
        if (!column.Data.Values.Any())
            return default;

        T? mean = column.Mean();
        if (mean == null)
            return default;

        T sumOfSquares = T.Zero;
        foreach (var value in column.Data.Values)
        {
            var diff = value - mean;
            sumOfSquares += diff * diff;
        }

        double avgSquares = Convert.ToDouble(sumOfSquares) / column.Data.Count;
        return Math.Sqrt(avgSquares);
    }

    public static double? Variance<T>(this ISimpleDataFrameColumn<T> column)
        where T : INumber<T>
    {
        var stdDev = column.StandardDeviation();
        return stdDev.HasValue ? stdDev * stdDev : null;
    }
}