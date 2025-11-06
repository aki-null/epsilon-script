using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Unit")]
  [Trait("Component", "SupportComponents")]
  public class ScriptSupportComponentTests : ScriptTestBase
  {
    [Fact]
    public void VariableValue_IntegerConversionsBehaveAsExpected()
    {
      var value = new VariableValue(10);
      Assert.Equal(10, value.IntegerValue);
      AssertNearlyEqual(10.0f, value.FloatValue);
      Assert.True(value.BooleanValue);
    }

    [Fact]
    public void VariableValue_FloatConversionsBehaveAsExpected()
    {
      var value = new VariableValue(3.5f);
      AssertNearlyEqual(3.5f, value.FloatValue);
      Assert.Equal(3, value.IntegerValue);
    }

    [Fact]
    public void VariableValue_BooleanToFloat_Converts()
    {
      var valueTrue = new VariableValue(true);
      var valueFalse = new VariableValue(false);
      Assert.Equal(1.0f, valueTrue.FloatValue);
      Assert.Equal(0.0f, valueFalse.FloatValue);
    }

    [Fact]
    public void VariableValue_FloatToBoolean_Converts()
    {
      var valueNonZero = new VariableValue(1.0f);
      var valueZero = new VariableValue(0.0f);
      Assert.True(valueNonZero.BooleanValue);
      Assert.False(valueZero.BooleanValue);
    }

    [Fact]
    public void VariableValue_CopyFromCopiesUnderlyingData()
    {
      var source = new VariableValue("hello");
      var destination = new VariableValue(Type.String);
      destination.CopyFrom(source);
      Assert.Equal("hello", destination.StringValue);
    }

    [Fact]
    public void DictionaryVariableContainer_TryGetValueReturnsStoredVariable()
    {
      var container = new DictionaryVariableContainer();
      var id = (VariableId)"val";
      container[id] = new VariableValue(5);
      Assert.True(container.ContainsKey(id));
      Assert.Equal(5, container[id].IntegerValue);
    }

    [Fact]
    public void DictionaryVariableContainer_IndexerUpdatesExistingEntry()
    {
      var container = new DictionaryVariableContainer();
      var id = (VariableId)"val";
      container[id] = new VariableValue(1);
      container[id] = new VariableValue(2);
      Assert.Equal(2, container[id].IntegerValue);
    }
  }
}