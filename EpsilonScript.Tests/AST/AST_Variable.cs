using System;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_Variable : AstTestBase
  {
    [Theory]
    [InlineData(Type.Integer, 42)]
    [InlineData(Type.Float, 3.14f)]
    [InlineData(Type.Boolean, true)]
    public void AST_Variable_WithValidVariable_ReturnsCorrectValue(Type variableType, object value)
    {
      var variableName = "testVar";
      var variableId = (VariableId)variableName;
      var variableValue = CreateVariableValue(variableType, value);
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element, Compiler.Options.None, variables, null);
      node.Execute(null);

      Assert.Equal(GetExpectedValueType(variableType), node.ValueType);
      Assert.Equal(variableValue, node.Variable);
      AssertVariableValue(node, variableType, value);
    }

    [Fact]
    public void AST_Variable_UndefinedVariable_ThrowsRuntimeException()
    {
      var variableName = "undefinedVar";
      var variables = new DictionaryVariableContainer(); // Empty

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element, Compiler.Options.None, variables, null);

      ErrorTestHelper.ExecuteNodeExpectingError<RuntimeException>(node, null, "Undefined variable");
    }

    [Theory]
    [InlineData(Type.Integer, 10)]
    [InlineData(Type.Float, 5.5f)]
    [InlineData(Type.Boolean, false)]
    public void AST_Variable_WithVariableOverride_UsesOverrideValue(Type variableType, object overrideValue)
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

      node.Build(rpn, element, Compiler.Options.None, variables, null);
      node.Execute(variableOverride);

      Assert.Equal(GetExpectedValueType(variableType), node.ValueType);
      Assert.Equal(overrideVariableValue, node.Variable);
      AssertVariableValue(node, variableType, overrideValue);
    }

    [Fact]
    public void AST_Variable_WithNullVariablesAndOverride_ThrowsRuntimeException()
    {
      var variableName = "testVar";

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element, Compiler.Options.None, null, null);

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
    public void AST_Variable_WithValidIdentifierNames_Works(string variableName)
    {
      var variableId = (VariableId)variableName;
      var variableValue = new VariableValue(42);
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element, Compiler.Options.None, variables, null);
      node.Execute(null);

      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(42, node.IntegerValue);
    }

    [Fact]
    public void AST_Variable_IsConstant_ReturnsFalse()
    {
      var variableName = "testVar";
      var variableId = (VariableId)variableName;
      var variableValue = new VariableValue(42);
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element, Compiler.Options.None, variables, null);

      Assert.False(node.IsConstant); // Variables are never constant
    }

    [Theory]
    [InlineData(Type.Integer, 100)]
    [InlineData(Type.Float, 2.5f)]
    [InlineData(Type.Boolean, true)]
    public void AST_Variable_MultipleExecutions_ReflectsVariableChanges(Type variableType, object newValue)
    {
      var variableName = "testVar";
      var variableId = (VariableId)variableName;
      var variableValue = CreateVariableValue(variableType, GetDefaultValue(variableType));
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element, Compiler.Options.None, variables, null);

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
    public void AST_Variable_UnsupportedVariableType_ThrowsArgumentOutOfRangeException()
    {
      var variableName = "testVar";
      var variableId = (VariableId)variableName;
      var variableValue = new VariableValue(Type.Undefined); // Unsupported type
      var variables = new DictionaryVariableContainer { [variableId] = variableValue };

      var node = new VariableNode();
      var rpn = CreateStack();
      var token = new Token(variableName, TokenType.Identifier);
      var element = new Element(token, ElementType.Variable);

      node.Build(rpn, element, Compiler.Options.None, variables, null);

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

    private static void SetVariableValue(VariableValue variable, Type type, object value)
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
        Type.Integer => ValueType.Integer,
        Type.Float => ValueType.Float,
        Type.Boolean => ValueType.Boolean,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
      };
    }

    private static void AssertVariableValue(VariableNode node, Type type, object expectedValue)
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