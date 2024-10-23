using System;
using NUnit.Framework;

namespace Dimension.Data.SimpleDataFrame.SimpleDataFrame.Tests.NUnit;

public class SimpleDataFrameTests
{
    [Test]
    public void Constructor_InitializesEmptyDataFrame()
    {
        // Arrange & Act
        var dataFrame = new SimpleDataFrame();

        // Assert
        Assert.That(dataFrame.ColumnCount, Is.EqualTo(0));
        Assert.That(dataFrame.RowCount, Is.EqualTo(0));
    }

    [Test]
    public void AddColumn_NewColumn_AddsSuccessfully()
    {
        // Arrange
        var dataFrame = new SimpleDataFrame();
        var columnName = "TestColumn";

        // Act
        dataFrame.AddColumn(new SimpleDataFrameColumn<int>(columnName));

        // Assert
        Assert.That(dataFrame.ColumnCount, Is.EqualTo(1));
        Assert.That(dataFrame.TryGetColumn(columnName, out var retrievedColumn), Is.EqualTo(true));
        Assert.That(retrievedColumn, Is.Not.Null); // Corrected Assertion
    }

    [Test]
    public void AddColumn_ExistingColumn_ThrowsException()
    {
        // Arrange
        var dataFrame = new SimpleDataFrame();
        var columnName = "TestColumn";
        dataFrame.AddColumn(new SimpleDataFrameColumn<int>(columnName));

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => dataFrame.AddColumn(new SimpleDataFrameColumn<int>(columnName)));
        Assert.That(ex!.Message, Does.Contain($"Column {columnName} already exists."));
    }

    [Test]
    public void DeleteColumn_ExistingColumn_DeletesSuccessfully()
    {
        // Arrange
        var dataFrame = new SimpleDataFrame();
        var columnName = "TestColumn";
        dataFrame.AddColumn(new SimpleDataFrameColumn<int>(columnName));

        // Act
        dataFrame.DeleteColumn(columnName);

        // Assert
        Assert.That(dataFrame.ColumnCount, Is.EqualTo(0));
        Assert.That(dataFrame.TryGetColumn(columnName, out var retrievedColumn), Is.False);
    }
}
