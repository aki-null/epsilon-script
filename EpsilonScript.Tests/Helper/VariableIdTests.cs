using Xunit;
using EpsilonScript.Helper;

namespace EpsilonScript.Tests.Helper
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Helper")]
  public class VariableIdTests
  {
    [Fact]
    public void ImplicitConversion_FromString_CreatesCorrectId()
    {
      // Arrange
      const string variableName = "testVariable";
      var expectedId = variableName.GetUniqueIdentifier();

      // Act
      VariableId variableId = variableName;

      // Assert
      Assert.Equal(expectedId, variableId.Value);
      Assert.Equal(expectedId, (uint)variableId);
    }

    [Fact]
    public void ImplicitConversion_FromUint_CreatesCorrectId()
    {
      // Arrange
      const uint expectedId = 42;

      // Act
      VariableId variableId = expectedId;

      // Assert
      Assert.Equal(expectedId, variableId.Value);
      Assert.Equal(expectedId, (uint)variableId);
    }

    [Fact]
    public void ImplicitConversion_ToUint_ReturnsCorrectValue()
    {
      // Arrange
      const string variableName = "testVariable";
      var expectedId = variableName.GetUniqueIdentifier();
      VariableId variableId = variableName;

      // Act
      uint result = variableId;

      // Assert
      Assert.Equal(expectedId, result);
    }

    [Fact]
    public void Equals_WithSameVariableId_ReturnsTrue()
    {
      // Arrange
      VariableId id1 = "testVariable";
      VariableId id2 = "testVariable";

      // Act & Assert
      Assert.True(id1.Equals(id2));
      Assert.True(id1 == id2);
      Assert.False(id1 != id2);
    }

    [Fact]
    public void Equals_WithDifferentVariableId_ReturnsFalse()
    {
      // Arrange
      VariableId id1 = "variable1";
      VariableId id2 = "variable2";

      // Act & Assert
      Assert.False(id1.Equals(id2));
      Assert.False(id1 == id2);
      Assert.True(id1 != id2);
    }

    [Fact]
    public void Equals_WithMatchingUint_ReturnsTrue()
    {
      // Arrange
      const string variableName = "testVariable";
      VariableId variableId = variableName;
      uint expectedId = variableName.GetUniqueIdentifier();

      // Act & Assert
      Assert.True(variableId.Equals(expectedId));
      Assert.True(variableId == expectedId);
      Assert.True(expectedId == variableId);
      Assert.False(variableId != expectedId);
      Assert.False(expectedId != variableId);
    }

    [Fact]
    public void Equals_WithNonMatchingUint_ReturnsFalse()
    {
      // Arrange
      VariableId variableId = "testVariable";
      const uint differentId = 999999;

      // Act & Assert
      Assert.False(variableId.Equals(differentId));
      Assert.False(variableId == differentId);
      Assert.False(differentId == variableId);
      Assert.True(variableId != differentId);
      Assert.True(differentId != variableId);
    }

    [Fact]
    public void Equals_WithObject_ReturnsCorrectResult()
    {
      // Arrange
      VariableId variableId = "testVariable";
      object sameAsVariableId = (VariableId)"testVariable";
      object sameAsUint = (uint)variableId;
      object differentObject = "not a variable id";
      object nullObject = null;

      // Act & Assert
      Assert.True(variableId.Equals(sameAsVariableId));
      Assert.True(variableId.Equals(sameAsUint));
      Assert.False(variableId.Equals(differentObject));
      Assert.False(variableId.Equals(nullObject));
    }

    [Fact]
    public void GetHashCode_SameVariableIds_ReturnSameHashCode()
    {
      // Arrange
      VariableId id1 = "testVariable";
      VariableId id2 = "testVariable";

      // Act & Assert
      Assert.Equal(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_DifferentVariableIds_ReturnDifferentHashCodes()
    {
      // Arrange
      VariableId id1 = "variable1";
      VariableId id2 = "variable2";

      // Act & Assert
      Assert.NotEqual(id1.GetHashCode(), id2.GetHashCode());
    }

    [Fact]
    public void ToString_WithValidId_ReturnsOriginalString()
    {
      // Arrange
      const string originalName = "testVariable";
      VariableId variableId = originalName;

      // Act
      var result = variableId.ToString();

      // Assert
      Assert.Equal(originalName, result);
    }

    [Fact]
    public void ToString_WithDirectUintConstruction_ReturnsNull()
    {
      // Arrange
      VariableId variableId = 999999u; // Direct uint that doesn't correspond to any string

      // Act
      var result = variableId.ToString();

      // Assert
      Assert.Null(result);
    }

    [Fact]
    public void Value_Property_ReturnsCorrectUint()
    {
      // Arrange
      const string variableName = "testVariable";
      var expectedId = variableName.GetUniqueIdentifier();
      VariableId variableId = variableName;

      // Act
      var result = variableId.Value;

      // Assert
      Assert.Equal(expectedId, result);
    }

    [Fact]
    public void SameStringCreation_ProducesSameId()
    {
      // Arrange
      const string variableName = "consistentVariable";

      // Act
      VariableId id1 = variableName;
      VariableId id2 = variableName;

      // Assert
      Assert.Equal(id1, id2);
      Assert.Equal(id1.Value, id2.Value);
    }

    [Fact]
    public void ConversionRoundTrip_MaintainsValue()
    {
      // Arrange
      const string originalName = "roundTripTest";

      // Act
      VariableId variableId = originalName; // string -> VariableId
      uint uintValue = variableId; // VariableId -> uint
      VariableId backToVariableId = uintValue; // uint -> VariableId

      // Assert
      Assert.Equal(variableId, backToVariableId);
      Assert.Equal(originalName, backToVariableId.ToString());
    }
  }
}