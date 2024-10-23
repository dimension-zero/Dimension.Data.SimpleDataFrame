namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame;

public static class ExtensionsType
{
    public static bool IsNumericType<T>(this T type)
    {
        return type is byte or sbyte or short or ushort or int or uint or long or ulong or float or double or decimal;
    }
}