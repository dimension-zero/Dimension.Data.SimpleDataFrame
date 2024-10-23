using System;
using System.Collections.Generic;
using System.Numerics;

namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame;

public static class MathExtensions
{
    public static T Median<T>(this List<T> values)
        where T : INumber<T>
    {
        values.Sort();
        int count = values.Count;
        if (INumber<T>.Equals(count, 0))
        {
            throw new InvalidOperationException("List is empty.");
        }
        if (count % 2 == 0)
        {
            int mid = count / 2;
            T val1 = values[mid - 1];
            T val2 = values[mid];
            return (val1 + val2) / T.CreateChecked(2);
        }
        else
        {
            return values[count / 2];
        }
    }
}