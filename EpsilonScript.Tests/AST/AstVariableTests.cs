using System;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AstVariableTests : AstTestBase
  {
    [Theory]
    [InlineData(Type.Integer, 42)]
    [InlineData(Type.Float, 3.14f)]
    [InlineData(Type.Boolean, true)]
    internal void Variable_WithValidVariable_ReturnsCorrectValue(Type variableType, object value)
    {
      var variableName = "testVar";
      var variableId = (VariableId)variableName;
      var variableValue = CreateVariableValue(variableType, value);
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, variables);
      node.Execute(null);

      Assert.Equal(GetExpectedValueType(variableType), node.ValueType);
      Assert.Equal(variableValue, node.Variable);
      AssertVariableValue(node, variableType, value);
    }

    [Fact]
    internal void Variable_UndefinedVariable_ThrowsRuntimeException()
    {
      var variableName = "undefinedVar";
      var variables = new DictionaryVariableContainer(); // Empty

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, variables);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, "Undefined variable");
    }

    [Theory]
    [InlineData(Type.Integer, 10)]
    [InlineData(Type.Float, 5.5f)]
    [InlineData(Type.Boolean, false)]
    internal void Variable_WithVariableOverride_UsesOverrideValue(Type variableType, object overrideValue)
    {
      var variableName = "testVar";
      var variableId = (VariableId)variableName;
      var originalValue = CreateVariableValue(variableType, GetDefaultValue(variableType));
      var overrideVariableValue = CreateVariableValue(variableType, overrideValue);

      var variables = new DictionaryVariableContainer { [variableId] = originalValue };
      var variableOverride = new DictionaryVariableContainer { [variableId] = overrideVariableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, variables);
      node.Execute(variableOverride);

      Assert.Equal(GetExpectedValueType(variableType), node.ValueType);
      Assert.Equal(overrideVariableValue, node.Variable);
      AssertVariableValue(node, variableType, overrideValue);
    }

    [Fact]
    internal void Variable_WithNullVariablesAndOverride_ThrowsRuntimeException()
    {
      var variableName = "testVar";

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, "Undefined variable");
    }

    [Theory]
    [InlineData("var1")]
    [InlineData("_var")]
    [InlineData("VAR")]
    [InlineData("variable123")]
    [InlineData("camelCase")]
    [InlineData("PascalCase")]
    [InlineData("snake_case")]
    internal void Variable_WithValidIdentifierNames_Works(string variableName)
    {
      var variableId = (VariableId)variableName;
      var variableValue = new VariableValue(42);
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, variables);
      node.Execute(null);

      Assert.Equal(ExtendedType.Integer, node.ValueType);
      Assert.Equal(42, node.IntegerValue);
    }

    [Fact]
    internal void Variable_IsPrecomputable_ReturnsFalse()
    {
      var variableName = "testVar";
      var variableId = (VariableId)variableName;
      var variableValue = new VariableValue(42);
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, variables);

      Assert.False(node.IsPrecomputable); // Variables are never constant
    }

    [Theory]
    [InlineData(Type.Integer, 100)]
    [InlineData(Type.Float, 2.5f)]
    [InlineData(Type.Boolean, true)]
    internal void Variable_MultipleExecutions_ReflectsVariableChanges(Type variableType, object newValue)
    {
      var variableName = "testVar";
      var variableId = (VariableId)variableName;
      var variableValue = CreateVariableValue(variableType, GetDefaultValue(variableType));
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, variables);

      // First execution
      node.Execute(null);
      AssertVariableValue(node, variableType, GetDefaultValue(variableType));

      // Change variable value
      SetVariableValue(variableValue, variableType, newValue);

      // Second execution should reflect new value
      node.Execute(null);
      AssertVariableValue(node, variableType, newValue);
    }

    [Fact]
    internal void Variable_UnsupportedVariableType_ThrowsArgumentOutOfRangeException()
    {
      var variableName = "testVar";
      var variableId = (VariableId)variableName;
      var variableValue = new VariableValue(Type.Undefined); // Unsupported type
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, variables);

      ErrorTestHelper.ExecuteNodeExpectingError<ArgumentOutOfRangeException>(node);
    }

    private static VariableValue CreateVariableValue(Type type, object value)
    {
      return type switch
      {
        Type.Integer => new VariableValue((int)value),
        Type.Float => new VariableValue((float)value),
        Type.Boolean => new VariableValue((bool)value),
        _ => throw new ArgumentOutOfRangeException(nameof(type))
      };
    }

    private static void SetVariableValue(VariableValue variable, ValueType type, object value)
    {
      switch (type)
      {
        case Type.Integer:
          variable.IntegerValue = (int)value;
          break;
        case Type.Float:
          variable.FloatValue = (float)value;
          break;
        case Type.Boolean:
          variable.BooleanValue = (bool)value;
          break;
      }
    }

    private static object GetDefaultValue(Type type)
    {
      return type switch
      {
        Type.Integer => 0,
        Type.Float => 0.0f,
        Type.Boolean => false,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
      };
    }

    private static ValueType GetExpectedValueType(Type type)
    {
      return type switch
      {
        Type.Integer => ExtendedType.Integer,
        Type.Float => ExtendedType.Float,
        Type.Boolean => ExtendedType.Boolean,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
      };
    }

    private static void AssertVariableValue(VariableNode node, ValueType type, object expectedValue)
    {
      switch (type)
      {
        case Type.Integer:
          Assert.Equal((int)expectedValue, node.IntegerValue);
          break;
        case Type.Float:
          Assert.Equal((float)expectedValue, node.FloatValue, 6);
          break;
        case Type.Boolean:
          Assert.Equal((bool)expectedValue, node.BooleanValue);
          break;
      }
    }
  }
}