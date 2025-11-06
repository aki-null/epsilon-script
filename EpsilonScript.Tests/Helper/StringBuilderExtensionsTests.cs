using System.Collections.Generic;
using System.Text;
using Xunit;
using EpsilonScript.Function;

namespace EpsilonScript.Tests.Helper
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Helper")]
  public class StringBuilderExtensionsTests
  {
    private CompilerContext CreateContext()
    {
      return new CompilerContext(
        Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float,
        new Dictionary<VariableId, CustomFunctionOverload>());
    }

    [Fact]
    public void AppendFloatInvariant_WithNormalValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();
      var value = 3.14159f;

      // Act
      sb.AppendFloatInvariant(value, context);

      // Assert
      Assert.Equal("3.14159", sb.ToString());
    }

    [Fact]
    public void AppendDoubleInvariant_WithNormalValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();
      var value = 3.141592653589793;

      // Act
      sb.AppendDoubleInvariant(value, context);

      // Assert
      Assert.Equal("3.141592653589793", sb.ToString());
    }

    [Fact]
    public void AppendDecimalInvariant_WithNormalValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();
      var value = 3.141592653589793238462643383279m;

      // Act
      sb.AppendDecimalInvariant(value, context);

      // Assert - Decimal has precision limits, so the value is rounded
      Assert.Equal("3.1415926535897932384626433833", sb.ToString());
    }

    [Fact]
    public void AppendFloatInvariant_WithMaxValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendFloatInvariant(float.MaxValue, context);

      // Assert
      Assert.Equal("3.4028235E+38", sb.ToString());
    }

    [Fact]
    public void AppendFloatInvariant_WithMinValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendFloatInvariant(float.MinValue, context);

      // Assert
      Assert.Equal("-3.4028235E+38", sb.ToString());
    }

    [Fact]
    public void AppendDoubleInvariant_WithMaxValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDoubleInvariant(double.MaxValue, context);

      // Assert
      Assert.Equal("1.7976931348623157E+308", sb.ToString());
    }

    [Fact]
    public void AppendDoubleInvariant_WithMinValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDoubleInvariant(double.MinValue, context);

      // Assert
      Assert.Equal("-1.7976931348623157E+308", sb.ToString());
    }

    [Fact]
    public void AppendDecimalInvariant_WithMaxValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDecimalInvariant(decimal.MaxValue, context);

      // Assert
      Assert.Equal("79228162514264337593543950335", sb.ToString());
    }

    [Fact]
    public void AppendDecimalInvariant_WithMinValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDecimalInvariant(decimal.MinValue, context);

      // Assert
      Assert.Equal("-79228162514264337593543950335", sb.ToString());
    }

    [Fact]
    public void AppendFloatInvariant_WithZero_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendFloatInvariant(0f, context);

      // Assert
      Assert.Equal("0", sb.ToString());
    }

    [Fact]
    public void AppendDoubleInvariant_WithZero_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDoubleInvariant(0.0, context);

      // Assert
      Assert.Equal("0", sb.ToString());
    }

    [Fact]
    public void AppendDecimalInvariant_WithZero_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDecimalInvariant(0m, context);

      // Assert
      Assert.Equal("0", sb.ToString());
    }

    [Fact]
    public void AppendFloatInvariant_WithNegativeZero_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendFloatInvariant(-0f, context);

      // Assert - .NET formats negative zero as "-0" for float types
      Assert.Equal("-0", sb.ToString());
    }

    [Fact]
    public void AppendFloatInvariant_WithNaN_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendFloatInvariant(float.NaN, context);

      // Assert
      Assert.Equal("NaN", sb.ToString());
    }

    [Fact]
    public void AppendFloatInvariant_WithPositiveInfinity_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendFloatInvariant(float.PositiveInfinity, context);

      // Assert
      Assert.Equal("Infinity", sb.ToString());
    }

    [Fact]
    public void AppendFloatInvariant_WithNegativeInfinity_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendFloatInvariant(float.NegativeInfinity, context);

      // Assert
      Assert.Equal("-Infinity", sb.ToString());
    }

    [Fact]
    public void AppendDoubleInvariant_WithNaN_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDoubleInvariant(double.NaN, context);

      // Assert
      Assert.Equal("NaN", sb.ToString());
    }

    [Fact]
    public void AppendDoubleInvariant_WithPositiveInfinity_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDoubleInvariant(double.PositiveInfinity, context);

      // Assert
      Assert.Equal("Infinity", sb.ToString());
    }

    [Fact]
    public void AppendDoubleInvariant_WithNegativeInfinity_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDoubleInvariant(double.NegativeInfinity, context);

      // Assert
      Assert.Equal("-Infinity", sb.ToString());
    }

    [Fact]
    public void AppendFloatInvariant_WithVerySmallValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendFloatInvariant(float.Epsilon, context);

      // Assert - Verify it formats without error (exact format may vary by .NET version)
      var result = sb.ToString();
      Assert.True(result.Contains("E-") || result.Contains("e-"), $"Expected scientific notation, got: {result}");
      Assert.True(result.Length > 0 && result.Length <= 64);
    }

    [Fact]
    public void AppendDoubleInvariant_WithVerySmallValue_AppendsSuccessfully()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder();

      // Act
      sb.AppendDoubleInvariant(double.Epsilon, context);

      // Assert - Verify it formats without error (exact format may vary by .NET version)
      var result = sb.ToString();
      Assert.True(result.Contains("E-") || result.Contains("e-"), $"Expected scientific notation, got: {result}");
      Assert.True(result.Length > 0 && result.Length <= 64);
    }

    [Fact]
    public void AppendFloatInvariant_MultipleAppends_WorksCorrectly()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder("Value: ");

      // Act
      sb.AppendFloatInvariant(1.5f, context);
      sb.Append(", ");
      sb.AppendFloatInvariant(2.5f, context);

      // Assert
      Assert.Equal("Value: 1.5, 2.5", sb.ToString());
    }

    [Fact]
    public void AppendDoubleInvariant_MultipleAppends_WorksCorrectly()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder("Value: ");

      // Act
      sb.AppendDoubleInvariant(1.5, context);
      sb.Append(", ");
      sb.AppendDoubleInvariant(2.5, context);

      // Assert
      Assert.Equal("Value: 1.5, 2.5", sb.ToString());
    }

    [Fact]
    public void AppendDecimalInvariant_MultipleAppends_WorksCorrectly()
    {
      // Arrange
      var context = CreateContext();
      var sb = new StringBuilder("Value: ");

      // Act
      sb.AppendDecimalInvariant(1.5m, context);
      sb.Append(", ");
      sb.AppendDecimalInvariant(2.5m, context);

      // Assert
      Assert.Equal("Value: 1.5, 2.5", sb.ToString());
    }
  }
}