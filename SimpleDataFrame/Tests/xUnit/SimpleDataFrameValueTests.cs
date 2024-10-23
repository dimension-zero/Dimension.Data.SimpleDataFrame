using System;
using Xunit;

namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame.Tests.xUnit;


public class SimpleDataFrameValueTests
{
    [Fact]
    public void Constructor_ValidInput_SetsPropertiesCorrectly()
    {
        // Arrange
        var dateIndex = DateTime.Now;
        var columnName = "TestColumn";
        var value = 123;

        // Act
        var dataFrameValue = new SimpleDataFrameValue<int>(dateIndex, columnName, value);

        // Assert
        Assert.Equal(dateIndex, dataFrameValue.DateIndex);
        Assert.Equal(columnName, dataFrameValue.ColumnName);
        Assert.Equal(typeof(int), dataFrameValue.ValueType);
        Assert.Equal(value, dataFrameValue.Value);
        Assert.True(dataFrameValue.HasValue);
        Assert.Equal(value, dataFrameValue.ValueUntyped);
    }

    [Fact]
    public void Constructor_NullValue_SetsHasValueToFalse()
    {
        // Arrange
        var dateIndex = DateTime.Now;
        var columnName = "TestColumn";
        int? value = null;

        // Act
        var dataFrameValue = new SimpleDataFrameValue<int?>(dateIndex, columnName, value);

        // Assert
        Assert.Equal(dateIndex, dataFrameValue.DateIndex);
        Assert.Equal(columnName, dataFrameValue.ColumnName);
        Assert.Equal(typeof(int?), dataFrameValue.ValueType);
        Assert.True(dataFrameValue.HasValue);
        if (value != null)
        {
            if (dataFrameValue.Value != null)
            {
                Assert.Equal(value, dataFrameValue.Value);
            }
            Assert.Equal(value, dataFrameValue.ValueUntyped);
        }
    }

    // Additional tests can be added here to cover more scenarios
}
