using System;
using System.Collections.Generic;
using Xunit;

namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame.Tests.xUnit;

public class SimpleDataFrameColumnTests
{
    [Fact]
    public void Constructor_InitializesEmptyColumn()
    {
        // Arrange & Act
        var column = new SimpleDataFrameColumn<int>("TestColumn");

        // Assert
        Assert.Empty(column.Data);
        Assert.Equal("TestColumn", column.Name);
    }

    [Fact]
    public void AddValue_NewValue_AddsSuccessfully()
    {
        // Arrange
        var column = new SimpleDataFrameColumn<int>("TestColumn");
        var dateIndex = DateTime.Now;
        var value = 10;

        // Act
        column.AddValue(dateIndex, value);

        // Assert
        Assert.True(column.ContainsIndex(dateIndex));
        Assert.Equal(value, column[dateIndex]);
    }

    [Fact]
    public void AddValue_ExistingValue_ThrowsException()
    {
        // Arrange
        var column = new SimpleDataFrameColumn<int>("TestColumn");
        var dateIndex = DateTime.Now;
        var value = 10;
        column.AddValue(dateIndex, value);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => column.AddValue(dateIndex, value));
        Assert.Contains($"Value already exists for dateIndex {dateIndex}.", ex.Message);
    }

    [Fact]
    public void DeleteValue_ExistingValue_DeletesSuccessfully()
    {
        // Arrange
        var column = new SimpleDataFrameColumn<int>("TestColumn");
        var dateIndex = DateTime.Now;
        var value = 10;
        column.AddValue(dateIndex, value);

        // Act
        column.DeleteValue(dateIndex);

        // Assert
        Assert.False(column.ContainsIndex(dateIndex));
    }

    [Fact]
    public void UpdateValue_ExistingValue_UpdatesSuccessfully()
    {
        // Arrange
        var column = new SimpleDataFrameColumn<int>("TestColumn");
        var dateIndex = DateTime.Now;
        var value = 10;
        column.AddValue(dateIndex, value);
        var newValue = 20;

        // Act
        column.UpdateValue(dateIndex, newValue);

        // Assert
        Assert.Equal(newValue, column[dateIndex]);
    }

    [Fact]
    public void UpdateValue_NonExistingValue_ThrowsException()
    {
        // Arrange
        var column = new SimpleDataFrameColumn<int>("TestColumn");
        var dateIndex = DateTime.Now;
        var newValue = 20;

        // Act & Assert
        var ex = Assert.Throws<KeyNotFoundException>(() => column.UpdateValue(dateIndex, newValue));

        if (ex is not null)
            Assert.Contains($"No value found for dateIndex {dateIndex}.", ex.Message);
    }

}
