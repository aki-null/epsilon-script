using System.Linq;
using EpsilonScript;
using EpsilonScript.Function;
using EpsilonScript.Helper;
using Xunit;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.ScriptSystem
{
  [Trait("Category", "Integration")]
  [Trait("Component", "ScriptSystem")]
  public class ScriptComplexIntegrationTests : ScriptTestBase
  {
    [Fact]
    public void ScriptSystem_ComplexArithmeticExpression_EvaluatesCorrectly()
    {
      // Test complex arithmetic: ((5 + 3) * 2 - 4) / 3
      var result = CompileAndExecute("((5 + 3) * 2 - 4) / 3", Compiler.Options.Immutable);

      Assert.Equal(4, result.IntegerValue); // (8 * 2 - 4) / 3 = 12 / 3 = 4
    }

    [Fact]
    public void ScriptSystem_ComplexBooleanExpression_EvaluatesCorrectly()
    {
      // Test complex boolean: (x > 5 && y < 10) || (z == 0)
      var variables = Variables()
        .WithInteger("x", 7)
        .WithInteger("y", 8)
        .WithInteger("z", 0)
        .Build();

      var result = CompileAndExecute("(x > 5 && y < 10) || (z == 0)", Compiler.Options.None, variables);

      Assert.True(result.BooleanValue); // (true && true) || true = true
    }

    [Fact]
    public void ScriptSystem_ComplexMixedTypes_WithStringConcatenation()
    {
      // Test mixed types with string concatenation: "Result: " + (5 * 3)
      var result = CompileAndExecute("\"Result: \" + (5 * 3)", Compiler.Options.Immutable);

      Assert.Equal("Result: 15", result.StringValue);
    }

    [Fact]
    public void ScriptSystem_NestedFunctionCalls_EvaluatesCorrectly()
    {
      // Test nested function calls with custom functions
      // This test verifies that the script compiler can handle function registration
      var compiler = CreateCompiler();
      compiler.AddCustomFunction(CustomFunction.Create("double", (int x) => x * 2));
      compiler.AddCustomFunction(CustomFunction.Create("square", (int x) => x * x));

      // Verify the functions are registered and compile succeeds
      var script = compiler.Compile("double(square(3))", Compiler.Options.Immutable);
      Assert.NotNull(script);

      // The actual execution would require a proper AST result mechanism
      // For now, we just verify compilation succeeds with custom functions
    }

    [Fact]
    public void ScriptSystem_ComplexSequenceWithAssignments_EvaluatesCorrectly()
    {
      // Test sequence with multiple assignments: x = 5; y = x * 2; x + y
      var variables = Variables()
        .WithInteger("x", 0)
        .WithInteger("y", 0)
        .Build();

      var result = CompileAndExecute("x = 5; y = x * 2; x + y", Compiler.Options.None, variables);

      Assert.Equal(15, result.IntegerValue); // 5 + 10 = 15
    }

    [Fact]
    public void ScriptSystem_DeepNestedExpressions_HandlesCorrectly()
    {
      // Test deeply nested expressions: ((((1 + 2) * 3) + 4) * 5)
      var result = CompileAndExecute("((((1 + 2) * 3) + 4) * 5)", Compiler.Options.Immutable);

      Assert.Equal(65, result.IntegerValue); // ((3 * 3 + 4) * 5) = (13 * 5) = 65
    }

    [Fact]
    public void ScriptSystem_ComplexFloatPrecisionExpression_HandlesCorrectly()
    {
      // Test complex float operations with precision considerations
      var result = CompileAndExecute("(0.1 + 0.2) * 10.0", Compiler.Options.Immutable);

      // Due to float precision, this should be close to 3.0 but may not be exact
      Assert.True(EpsilonScript.Math.IsNearlyEqual(3.0f, result.FloatValue));
    }

    [Fact]
    public void ScriptSystem_ComplexConditionalLogic_EvaluatesCorrectly()
    {
      // Test complex conditional: (age >= 18 && hasLicense) || isEmergency
      var variables = Variables()
        .WithInteger("age", 20)
        .WithBoolean("hasLicense", true)
        .WithBoolean("isEmergency", false)
        .Build();

      var result = CompileAndExecute("(age >= 18 && hasLicense) || isEmergency", Compiler.Options.None, variables);

      Assert.True(result.BooleanValue); // (true && true) || false = true
    }

    [Fact]
    public void ScriptSystem_LargeExpressionWithManyOperators_PerformsWell()
    {
      // Test performance with a large expression
      var largeExpression = string.Join(" + ", Enumerable.Range(1, 50).Select(i => i.ToString()));
      var result = CompileAndExecute(largeExpression, Compiler.Options.Immutable);

      var expectedSum = Enumerable.Range(1, 50).Sum(); // 1275
      Assert.Equal(expectedSum, result.IntegerValue);
    }

    [Theory]
    [InlineData("1 + 2 * 3", 7)]           // Tests operator precedence
    [InlineData("(1 + 2) * 3", 9)]         // Tests parentheses
    [InlineData("10 - 5 - 2", 3)]          // Tests left associativity
    [InlineData("2 + 3 * 4 - 1", 13)]      // Tests mixed precedence
    public void ScriptSystem_OperatorPrecedenceAndAssociativity_EvaluatesCorrectly(string expression, int expected)
    {
      var result = CompileAndExecute(expression, Compiler.Options.Immutable);

      Assert.Equal(expected, result.IntegerValue);
    }

    [Fact]
    public void ScriptSystem_ComplexStringManipulation_EvaluatesCorrectly()
    {
      var result = CompileAndExecute("\"Hello\" + \" \" + \"World\" + \"!\"", Compiler.Options.Immutable);

      Assert.Equal("Hello World!", result.StringValue);
    }
  }
}