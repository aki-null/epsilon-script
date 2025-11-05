using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  /// <summary>
  /// Tests for edge cases in variable system
  /// Covers period-qualified names, length limits, unicode, and container edge cases
  /// </summary>
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  [Trait("Priority", "Medium")]
  public class ScriptVariableEdgeCaseTests : ScriptTestBase
  {
    #region Period-Qualified Variable Names

    [Fact]
    public void Variable_SimpleDottedName_Works()
    {
      var variables = Variables()
        .WithInteger("user.id", 42)
        .Build();

      var result = CompileAndExecute("user.id", Compiler.Options.Immutable, variables);
      Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void Variable_DeeplyNestedDots_Works()
    {
      var variables = Variables()
        .WithInteger("config.server.database.port", 5432)
        .Build();

      var result = CompileAndExecute("config.server.database.port",
        Compiler.Options.Immutable, variables);
      Assert.Equal(5432, result.IntegerValue);
    }

    [Fact]
    public void Variable_DottedName_Assignment_Works()
    {
      var variables = Variables()
        .WithInteger("obj.prop.value", 10)
        .Build();

      CompileAndExecute("obj.prop.value = 20", Compiler.Options.None, variables);
      Assert.Equal(20, variables["obj.prop.value"].IntegerValue);
    }

    [Fact]
    public void Variable_DottedName_WithUnderscore_Works()
    {
      var variables = Variables()
        .WithString("_private.internal.value", "secret")
        .Build();

      var result = CompileAndExecute("_private.internal.value",
        Compiler.Options.Immutable, variables);
      Assert.Equal("secret", result.StringValue);
    }

    [Fact]
    public void Variable_DottedName_WithNumbers_Works()
    {
      var variables = Variables()
        .WithInteger("server1.db2.port3", 8080)
        .Build();

      var result = CompileAndExecute("server1.db2.port3",
        Compiler.Options.Immutable, variables);
      Assert.Equal(8080, result.IntegerValue);
    }

    [Fact]
    public void Variable_ManyDots_Works()
    {
      var variables = Variables()
        .WithInteger("a.b.c.d.e.f.g.h.i.j", 999)
        .Build();

      var result = CompileAndExecute("a.b.c.d.e.f.g.h.i.j",
        Compiler.Options.Immutable, variables);
      Assert.Equal(999, result.IntegerValue);
    }

    #endregion

    #region Variable Name Length Limits

    [Fact]
    public void Variable_VeryLongName_100Chars_Works()
    {
      var longName = new string('x', 100);
      var variables = Variables()
        .WithInteger(longName, 42)
        .Build();

      var result = CompileAndExecute(longName, Compiler.Options.Immutable, variables);
      Assert.Equal(42, result.IntegerValue);
    }

    [Fact]
    public void Variable_VeryLongName_1000Chars_Works()
    {
      var longName = new string('a', 1000);
      var variables = Variables()
        .WithInteger(longName, 123)
        .Build();

      var result = CompileAndExecute(longName, Compiler.Options.Immutable, variables);
      Assert.Equal(123, result.IntegerValue);
    }

    [Fact]
    public void Variable_LongNameWithDots_Works()
    {
      var longName = string.Join(".", Enumerable.Repeat("part", 50));
      var variables = Variables()
        .WithString(longName, "value")
        .Build();

      var result = CompileAndExecute(longName, Compiler.Options.Immutable, variables);
      Assert.Equal("value", result.StringValue);
    }

    #endregion

    #region Variable Container Edge Cases

    [Fact]
    public void VariableContainer_ManyVariables_100_Works()
    {
      var container = new DictionaryVariableContainer();
      for (var i = 0; i < 100; i++)
      {
        container[$"var{i}"] = new VariableValue(i);
      }

      var result = CompileAndExecute("var50 + var99", Compiler.Options.Immutable, container);
      Assert.Equal(149, result.IntegerValue);
    }

    [Fact]
    public void VariableContainer_ManyVariables_1000_Works()
    {
      var container = new DictionaryVariableContainer();
      for (var i = 0; i < 1000; i++)
      {
        container[$"variable{i}"] = new VariableValue(i * 2);
      }

      var result = CompileAndExecute("variable500", Compiler.Options.Immutable, container);
      Assert.Equal(1000, result.IntegerValue);
    }

    [Fact]
    public void VariableContainer_Override_FallsBackToBase()
    {
      var baseVars = Variables()
        .WithInteger("base", 10)
        .WithInteger("shared", 20)
        .Build();

      var overrideVars = Variables()
        .WithInteger("override", 30)
        .WithInteger("shared", 999)
        .Build();

      var result = CompileAndExecute("base + shared + override",
        Compiler.Options.Immutable, baseVars, overrideVars);

      Assert.Equal(1039, result.IntegerValue); // 10 + 999 + 30
    }

    [Fact]
    public void VariableContainer_NestedOverride_Works()
    {
      var baseVars = Variables()
        .WithInteger("x", 1)
        .WithInteger("y", 2)
        .WithInteger("z", 3)
        .Build();

      var overrideVars = Variables()
        .WithInteger("y", 20)
        .Build();

      var result = CompileAndExecute("x + y + z",
        Compiler.Options.Immutable, baseVars, overrideVars);

      Assert.Equal(24, result.IntegerValue); // 1 + 20 + 3
    }

    [Fact]
    public void VariableContainer_UpdateBetweenExecutions_ReflectsChanges()
    {
      var variables = Variables()
        .WithInteger("value", 10)
        .Build();

      var script = Compile("value * 2", Compiler.Options.Immutable, variables);

      script.Execute();
      Assert.Equal(20, script.IntegerValue);

      variables["value"] = new VariableValue(25);
      script.Execute();
      Assert.Equal(50, script.IntegerValue);
    }

    #endregion

    #region Single Character Variable Names

    [Theory]
    [InlineData("a")]
    [InlineData("x")]
    [InlineData("z")]
    [InlineData("_")]
    [InlineData("A")]
    [InlineData("Z")]
    public void Variable_SingleCharacter_Works(string varName)
    {
      var variables = Variables()
        .WithInteger(varName, 42)
        .Build();

      var result = CompileAndExecute(varName, Compiler.Options.Immutable, variables);
      Assert.Equal(42, result.IntegerValue);
    }

    #endregion

    #region Variable Name Patterns

    [Theory]
    [InlineData("_var")]
    [InlineData("__private")]
    [InlineData("___internal")]
    [InlineData("_")]
    [InlineData("__")]
    public void Variable_UnderscorePrefix_Works(string varName)
    {
      var variables = Variables()
        .WithInteger(varName, 100)
        .Build();

      var result = CompileAndExecute(varName, Compiler.Options.Immutable, variables);
      Assert.Equal(100, result.IntegerValue);
    }

    [Theory]
    [InlineData("camelCase")]
    [InlineData("PascalCase")]
    [InlineData("snake_case")]
    [InlineData("SCREAMING_SNAKE_CASE")]
    [InlineData("mixedCASE_With_123")]
    public void Variable_VariousNamingConventions_Work(string varName)
    {
      var variables = Variables()
        .WithString(varName, "test")
        .Build();

      var result = CompileAndExecute(varName, Compiler.Options.Immutable, variables);
      Assert.Equal("test", result.StringValue);
    }

    #endregion

    #region Variable Type Consistency

    [Fact]
    public void Variable_ChangingType_BetweenExecutions_Works()
    {
      var variables = new DictionaryVariableContainer
      {
        ["dynamic"] = new VariableValue(42)
      };

      var script = Compile("dynamic", Compiler.Options.Immutable, variables);

      script.Execute();
      Assert.Equal(Type.Integer, script.ValueType);
      Assert.Equal(42, script.IntegerValue);

      // Change to string
      variables["dynamic"] = new VariableValue("now a string");
      script.Execute();
      Assert.Equal(Type.String, script.ValueType);
      Assert.Equal("now a string", script.StringValue);

      // Change to boolean
      variables["dynamic"] = new VariableValue(true);
      script.Execute();
      Assert.Equal(Type.Boolean, script.ValueType);
      Assert.True(script.BooleanValue);
    }

    [Fact]
    public void Variable_DifferentTypesInOverride_Works()
    {
      var baseVars = Variables()
        .WithInteger("x", 10)
        .Build();

      var overrideVars = Variables()
        .WithFloat("x", 10.5f)
        .Build();

      var result = CompileAndExecute("x", Compiler.Options.Immutable, baseVars, overrideVars);
      Assert.Equal(Type.Float, result.Type);
      AssertNearlyEqual(10.5f, result.FloatValue);
    }

    #endregion

    #region Complex Variable Expressions

    [Fact]
    public void Variable_InComplexExpression_WithMultipleDots_Works()
    {
      var variables = Variables()
        .WithInteger("a.b.c", 10)
        .WithInteger("x.y.z", 20)
        .WithInteger("m.n.o", 30)
        .Build();

      var result = CompileAndExecute("(a.b.c + x.y.z) * m.n.o",
        Compiler.Options.Immutable, variables);

      Assert.Equal(900, result.IntegerValue); // (10 + 20) * 30
    }

    [Fact]
    public void Variable_MultipleAssignments_InSequence_Work()
    {
      var variables = Variables()
        .WithInteger("x.a", 1)
        .WithInteger("x.b", 2)
        .WithInteger("x.c", 3)
        .Build();

      CompileAndExecute("x.a = 10; x.b = 20; x.c = 30",
        Compiler.Options.None, variables);

      Assert.Equal(10, variables["x.a"].IntegerValue);
      Assert.Equal(20, variables["x.b"].IntegerValue);
      Assert.Equal(30, variables["x.c"].IntegerValue);
    }

    #endregion
  }
}