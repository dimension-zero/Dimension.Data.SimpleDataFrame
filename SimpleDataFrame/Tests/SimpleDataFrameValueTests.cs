using System;
using NUnit.Framework;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame.Tests;

[TestFixture]
public class SimpleDataFrameValueTests
{
    [Test] [Fact]
    public void Constructor_ValidInput_SetsPropertiesCorrectly()
    {
        // Arrange
        var dateIndex = DateTime.Now;
        var columnName = "TestColumn";
        var value = 123;

        // Act
        var dataFrameValue = new SimpleDataFrameValue<int>(dateIndex, columnName, value);

        // Assert
        Assert.Equals(dateIndex, dataFrameValue.DateIndex);
        Assert.Equals(columnName, dataFrameValue.ColumnName);
        Assert.Equals(typeof(int), dataFrameValue.ValueType);
        Assert.Equals(value, dataFrameValue.Value);
        Assert.That(dataFrameValue.HasValue);
        Assert.Equals(value, dataFrameValue.ValueUntyped);
    }

    [Test] [Fact]
    public void Constructor_NullValue_SetsHasValueToFalse()
    {
        // Arrange
        var dateIndex = DateTime.Now;
        var columnName = "TestColumn";
        int? value = null;

        // Act
        var dataFrameValue = new SimpleDataFrameValue<int?>(dateIndex, columnName, value);

        // Assert
        Assert.Equals(dateIndex, dataFrameValue.DateIndex);
        Assert.Equals(columnName, dataFrameValue.ColumnName);
        Assert.Equals(typeof(int?), dataFrameValue.ValueType);
        Assert.That(dataFrameValue.HasValue);
        if (value != null)
        {
            if (dataFrameValue.Value != null)
            {
                Assert.Equals(value, dataFrameValue.Value);
            }
            Assert.Equals(value, dataFrameValue.ValueUntyped);
        }
    }

    // Additional tests can be added here to cover more scenarios
}
