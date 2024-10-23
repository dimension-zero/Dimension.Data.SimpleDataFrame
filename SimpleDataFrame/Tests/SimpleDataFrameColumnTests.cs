using NUnit.Framework;

namespace Dimension.Data.SimpleDataFrame.Tests;

[TestFixture]
public class SimpleDataFrameColumnTests
{
    [Test]
    public void Constructor_InitializesEmptyColumn()
    {
        // Arrange & Act
        var column = new SimpleDataFrameColumn<int>("TestColumn");

        // Assert
        Assert.Equals(column.Data.Count, 0);
        Assert.Equals("TestColumn", column.Name);
    }

    [Test]
    public void AddValue_NewValue_AddsSuccessfully()
    {
        // Arrange
        var column = new SimpleDataFrameColumn<int>("TestColumn");
        var dateIndex = DateTime.Now;
        var value = 10;

        // Act
        column.AddValue(dateIndex, value);

        // Assert
        Assert.That(column.ContainsIndex(dateIndex));
        Assert.Equals(value, column[dateIndex]);
    }

    [Test]
    public void AddValue_ExistingValue_ThrowsException()
    {
        // Arrange
        var column = new SimpleDataFrameColumn<int>("TestColumn");
        var dateIndex = DateTime.Now;
        var value = 10;
        column.AddValue(dateIndex, value);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => column.AddValue(dateIndex, value));
        Assert.That(ex.Message, Does.Contain($"Value already exists for dateIndex {dateIndex}."));
    }

    [Test]
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
        Assert.Equals(column.ContainsIndex(dateIndex), false);
    }

    [Test]
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
        Assert.Equals(newValue, column[dateIndex]);
    }

    [Test]
    public void UpdateValue_NonExistingValue_ThrowsException()
    {
        // Arrange
        var column = new SimpleDataFrameColumn<int>("TestColumn");
        var dateIndex = DateTime.Now;
        var newValue = 20;

        // Act & Assert
        var ex = Assert.Throws<KeyNotFoundException>(() => column.UpdateValue(dateIndex, newValue));
        Assert.That(ex.Message, Does.Contain($"No value found for dateIndex {dateIndex}."));
    }

    // Additional tests can be written to cover more scenarios
}
