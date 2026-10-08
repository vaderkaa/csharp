using GeometryTools.Lib;

namespace GeometryTools.Tests;

[TestClass]
public sealed class RectangleUtilsTests
{
    [TestMethod]
    public void CalculateArea_ValidDimensions_ReturnsCorrectArea()
    {
        //Arrange
        double width = 5;
        double height = 4;

        //Act
        double result = RectangleUtils.CalculateArea(width, height);

        //Assert
        Assert.AreEqual(20, result, 1e-7);
    }
}