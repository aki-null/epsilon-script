using Xunit;
using EpsilonScript.Function;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptContextualFunctionTests : ScriptTestBase
  {
    // =========================================================================
    // Pure Contextual Functions (1 Context Variable, 0 Parameters)
    // =========================================================================

    [Fact]
    public void ContextualFunction_SingleVariable_NoParameters_ExecutesCorrectly()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "IsMon",
          "day",
          (int day) => day % 7 == 1));

      var variables = new DictionaryVariableContainer
      {
        ["day"] = new VariableValue(1) // Monday
      };

      var script = compiler.Compile("IsMon()", Compiler.Options.Immutable, variables);
      script.Execute();

      Assert.True(script.BooleanValue);
    }

    [Fact]
    public void ContextualFunction_SingleVariable_UpdatedBetweenExecutions()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "IsWeekend",
          "day",
          (int day) => day == 0 || day == 6)); // Sunday or Saturday

      var variables = new DictionaryVariableContainer
      {
        ["day"] = new VariableValue(1) // Monday
      };

      var script = compiler.Compile("IsWeekend()", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.False(script.BooleanValue);

      // Update variable
      variables["day"] = new VariableValue(0); // Sunday
      script.Execute();
      Assert.True(script.BooleanValue);

      // Update again
      variables["day"] = new VariableValue(6); // Saturday
      script.Execute();
      Assert.True(script.BooleanValue);
    }

    [Fact]
    public void ContextualFunction_MissingVariable_ThrowsException()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "GetDay",
          "day",
          (int day) => day));

      var variables = new DictionaryVariableContainer(); // No 'day' variable

      var script = compiler.Compile("GetDay()", Compiler.Options.Immutable, variables);

      var ex = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("day", ex.Message);
    }

    [Fact]
    public void ContextualFunction_WorksWithImmutableFlag()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "GetValue",
          "val",
          (int val) => val * 2));

      var variables = new DictionaryVariableContainer
      {
        ["val"] = new VariableValue(10)
      };

      // Should compile and execute with Immutable flag
      var script = compiler.Compile("GetValue()", Compiler.Options.Immutable, variables);
      script.Execute();

      Assert.Equal(20, script.IntegerValue);
    }

    // =========================================================================
    // Hybrid Functions (1 Context Variable + 1-3 Parameters)
    // =========================================================================

    [Fact]
    public void ContextualFunction_OneContextOneParam_ExecutesCorrectly()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "IsAfter",
          "currentDay",
          (int current, int target) => current > target));

      var variables = new DictionaryVariableContainer
      {
        ["currentDay"] = new VariableValue(10)
      };

      var script = compiler.Compile("IsAfter(5)", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.True(script.BooleanValue);

      script = compiler.Compile("IsAfter(15)", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.False(script.BooleanValue);
    }

    [Fact]
    public void ContextualFunction_OneContextTwoParams_ExecutesCorrectly()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "InRange",
          "value",
          (int value, int min, int max) => value >= min && value <= max));

      var variables = new DictionaryVariableContainer
      {
        ["value"] = new VariableValue(50)
      };

      var script = compiler.Compile("InRange(1, 100)", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.True(script.BooleanValue);

      script = compiler.Compile("InRange(60, 100)", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.False(script.BooleanValue);
    }

    [Fact]
    public void ContextualFunction_OneContextThreeParams_ExecutesCorrectly()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "ScaleAndOffset",
          "baseValue",
          (float baseValue, float scale, float offset, float multiplier) =>
            (baseValue * scale + offset) * multiplier));

      var variables = new DictionaryVariableContainer
      {
        ["baseValue"] = new VariableValue(10.0f)
      };

      var script = compiler.Compile("ScaleAndOffset(2.0, 5.0, 1.5)", Compiler.Options.Immutable, variables);
      script.Execute();

      // (10 * 2 + 5) * 1.5 = 37.5
      AssertNearlyEqual(37.5f, script.FloatValue);
    }

    // =========================================================================
    // Multiple Context Variables
    // =========================================================================

    [Fact]
    public void ContextualFunction_TwoContexts_NoParams_ExecutesCorrectly()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "IsBusinessDay",
          "dayOfWeek",
          "isHoliday",
          (int day, bool holiday) => day >= 1 && day <= 5 && !holiday));

      var variables = new DictionaryVariableContainer
      {
        ["dayOfWeek"] = new VariableValue(3), // Wednesday
        ["isHoliday"] = new VariableValue(false)
      };

      var script = compiler.Compile("IsBusinessDay()", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.True(script.BooleanValue);

      // Test with holiday
      variables["isHoliday"] = new VariableValue(true);
      script.Execute();
      Assert.False(script.BooleanValue);

      // Test with weekend
      variables["dayOfWeek"] = new VariableValue(0); // Sunday
      variables["isHoliday"] = new VariableValue(false);
      script.Execute();
      Assert.False(script.BooleanValue);
    }

    [Fact]
    public void ContextualFunction_TwoContextsOneParam_ExecutesCorrectly()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "CheckAccess",
          "userLevel",
          "isAdmin",
          (int userLevel, bool isAdmin, int requiredLevel) =>
            userLevel >= requiredLevel || isAdmin));

      var variables = new DictionaryVariableContainer
      {
        ["userLevel"] = new VariableValue(5),
        ["isAdmin"] = new VariableValue(false)
      };

      var script = compiler.Compile("CheckAccess(3)", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.True(script.BooleanValue); // 5 >= 3

      script = compiler.Compile("CheckAccess(10)", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.False(script.BooleanValue); // 5 < 10 and not admin

      // Test with admin
      variables["isAdmin"] = new VariableValue(true);
      script.Execute();
      Assert.True(script.BooleanValue); // Admin bypasses level check
    }

    [Fact]
    public void ContextualFunction_TwoContextsTwoParams_ExecutesCorrectly()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "CalculatePrice",
          "baseCost",
          "taxRate",
          (float baseCost, float taxRate, float discount, float shipping) =>
            (baseCost * (1 - discount) * (1 + taxRate)) + shipping));

      var variables = new DictionaryVariableContainer
      {
        ["baseCost"] = new VariableValue(100.0f),
        ["taxRate"] = new VariableValue(0.1f) // 10% tax
      };

      var script = compiler.Compile("CalculatePrice(0.2, 10.0)", Compiler.Options.Immutable, variables);
      script.Execute();

      // (100 * (1 - 0.2) * (1 + 0.1)) + 10 = 98
      AssertNearlyEqual(98.0f, script.FloatValue);
    }

    [Fact]
    public void ContextualFunction_ThreeContexts_NoParams_ExecutesCorrectly()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "IsValidState",
          "x",
          "y",
          "z",
          (int x, int y, int z) => x > 0 && y > 0 && z > 0));

      var variables = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(5),
        ["y"] = new VariableValue(10),
        ["z"] = new VariableValue(15)
      };

      var script = compiler.Compile("IsValidState()", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.True(script.BooleanValue);

      variables["z"] = new VariableValue(-1);
      script.Execute();
      Assert.False(script.BooleanValue);
    }

    [Fact]
    public void ContextualFunction_ThreeContextsOneParam_ExecutesCorrectly()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "CheckDistance",
          "playerX",
          "playerY",
          "playerZ",
          (float playerX, float playerY, float playerZ, float maxDistance) =>
          {
            var distSq = playerX * playerX + playerY * playerY + playerZ * playerZ;
            return distSq <= maxDistance * maxDistance;
          }));

      var variables = new DictionaryVariableContainer
      {
        ["playerX"] = new VariableValue(3.0f),
        ["playerY"] = new VariableValue(4.0f),
        ["playerZ"] = new VariableValue(0.0f)
      };

      // Distance is 5 (3-4-5 triangle)
      var script = compiler.Compile("CheckDistance(10.0)", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.True(script.BooleanValue);

      script = compiler.Compile("CheckDistance(4.0)", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.False(script.BooleanValue);
    }

    // =========================================================================
    // Type System Integration
    // =========================================================================

    [Fact]
    public void ContextualFunction_SupportsDifferentTypes()
    {
      var compiler = new Compiler();

      // Integer context
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "IntFunc",
          "intVal",
          (int val) => val));

      // Float context
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "FloatFunc",
          "floatVal",
          (float val) => val));

      // String context
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "StrFunc",
          "strVal",
          (string val) => val));

      // Boolean context
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "BoolFunc",
          "boolVal",
          (bool val) => val));

      var variables = new DictionaryVariableContainer
      {
        ["intVal"] = new VariableValue(42),
        ["floatVal"] = new VariableValue(3.14f),
        ["strVal"] = new VariableValue("test"),
        ["boolVal"] = new VariableValue(true)
      };

      var script1 = compiler.Compile("IntFunc()", Compiler.Options.Immutable, variables);
      script1.Execute();
      Assert.Equal(42, script1.IntegerValue);

      var script2 = compiler.Compile("FloatFunc()", Compiler.Options.Immutable, variables);
      script2.Execute();
      AssertNearlyEqual(3.14f, script2.FloatValue);

      var script3 = compiler.Compile("StrFunc()", Compiler.Options.Immutable, variables);
      script3.Execute();
      Assert.Equal("test", script3.StringValue);

      var script4 = compiler.Compile("BoolFunc()", Compiler.Options.Immutable, variables);
      script4.Execute();
      Assert.True(script4.BooleanValue);
    }

    // =========================================================================
    // Complex Scenarios
    // =========================================================================

    [Fact]
    public void ContextualFunction_CanBeUsedInComplexExpressions()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "GetBase",
          "baseValue",
          (int baseValue) => baseValue));

      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "Multiply",
          "multiplier",
          (int multiplier, int value) => multiplier * value));

      var variables = new DictionaryVariableContainer
      {
        ["baseValue"] = new VariableValue(10),
        ["multiplier"] = new VariableValue(3)
      };

      var script = compiler.Compile("GetBase() + Multiply(5) * 2", Compiler.Options.Immutable, variables);
      script.Execute();

      // 10 + (3 * 5) * 2 = 10 + 30 = 40
      Assert.Equal(40, script.IntegerValue);
    }

    [Fact]
    public void ContextualFunction_CanBeNested()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "Double",
          "val",
          (int val) => val * 2));

      compiler.AddCustomFunction(CustomFunction.Create("Add", (int a, int b) => a + b));

      var variables = new DictionaryVariableContainer
      {
        ["val"] = new VariableValue(5)
      };

      var script = compiler.Compile("Add(Double(), 10)", Compiler.Options.Immutable, variables);
      script.Execute();

      // Double(5) + 10 = 10 + 10 = 20
      Assert.Equal(20, script.IntegerValue);
    }

    [Fact]
    public void ContextualFunction_WorksWithVariableOverride()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "GetValue",
          "value",
          (int value) => value));

      var compileTimeVars = new DictionaryVariableContainer
      {
        ["value"] = new VariableValue(10)
      };

      var script = compiler.Compile("GetValue()", Compiler.Options.Immutable, compileTimeVars);
      script.Execute();
      Assert.Equal(10, script.IntegerValue);

      // Override at execution time
      var runtimeVars = new DictionaryVariableContainer
      {
        ["value"] = new VariableValue(20)
      };

      script.Execute(runtimeVars);
      Assert.Equal(20, script.IntegerValue);
    }

    [Fact]
    public void ContextualFunction_CacheIsInvalidatedOnContainerChange()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "GetVal",
          "x",
          (int x) => x));

      var vars1 = new DictionaryVariableContainer { ["x"] = new VariableValue(10) };
      var vars2 = new DictionaryVariableContainer { ["x"] = new VariableValue(20) };

      var script = compiler.Compile("GetVal()", Compiler.Options.Immutable, vars1);

      script.Execute(vars1);
      Assert.Equal(10, script.IntegerValue);

      script.Execute(vars2);
      Assert.Equal(20, script.IntegerValue);

      script.Execute(vars1);
      Assert.Equal(10, script.IntegerValue);
    }

    [Fact]
    public void ContextualFunction_FallsBackToCompileTimeContainer()
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "GetContext",
          "contextVar",
          (int val) => val * 2));

      // Compile-time variables contain the context variable
      var compileTimeVars = new DictionaryVariableContainer
      {
        ["contextVar"] = new VariableValue(10),
        ["other"] = new VariableValue(100)
      };

      var script = compiler.Compile("GetContext() + other", Compiler.Options.Immutable, compileTimeVars);
      script.Execute();
      Assert.Equal(120, script.IntegerValue); // (10 * 2) + 100

      // Override container has 'other' but NOT 'contextVar' - should fall back
      var overrideVars = new DictionaryVariableContainer
      {
        ["other"] = new VariableValue(50) // Override 'other' but not 'contextVar'
      };

      script.Execute(overrideVars);
      Assert.Equal(70, script.IntegerValue); // (10 * 2) + 50 - contextVar from compile-time, other from override

      // Override container has 'contextVar' - should use override, not compile-time
      var overrideWithContext = new DictionaryVariableContainer
      {
        ["contextVar"] = new VariableValue(5),
        ["other"] = new VariableValue(30)
      };

      script.Execute(overrideWithContext);
      Assert.Equal(40, script.IntegerValue); // (5 * 2) + 30 - both from override
    }

    // =========================================================================
    // Precision Tests
    // =========================================================================

    [Fact]
    public void ContextualFunction_WorksWithLongPrecision()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "GetLong",
          "bigVal",
          (long val) => val));

      var variables = new DictionaryVariableContainer
      {
        ["bigVal"] = new VariableValue(9999999999L)
      };

      var script = compiler.Compile("GetLong()", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.Equal(9999999999L, script.LongValue);
    }

    [Fact]
    public void ContextualFunction_WorksWithDoublePrecision()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "GetDouble",
          "preciseVal",
          (double val) => val));

      var variables = new DictionaryVariableContainer
      {
        ["preciseVal"] = new VariableValue(3.141592653589793)
      };

      var script = compiler.Compile("GetDouble()", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.Equal(3.141592653589793, script.DoubleValue, 10);
    }

    [Fact]
    public void ContextualFunction_WorksWithDecimalPrecision()
    {
      var compiler = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      compiler.AddCustomFunction(
        CustomFunction.CreateContextual(
          "GetDecimal",
          "preciseVal",
          (decimal val) => val));

      var variables = new DictionaryVariableContainer
      {
        ["preciseVal"] = new VariableValue(0.1m + 0.2m)
      };

      var script = compiler.Compile("GetDecimal()", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.Equal(0.3m, script.DecimalValue);
    }
  }
}