using System;
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
    public void VariableValue_BooleanThrowsOnInvalidFloatAccess()
    {
      var value = new VariableValue(true);
      Assert.Throws<InvalidCastException>(() => value.FloatValue);
    }

    [Fact]
    public void VariableValue_FloatThrowsOnBooleanAccess()
    {
      var value = new VariableValue(1.0f);
      Assert.Throws<InvalidCastException>(() => value.BooleanValue);
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