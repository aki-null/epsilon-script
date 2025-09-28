using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Helper;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_Function : AstTestBase
  {
    [Fact]
    public void AST_Function_WithIntegerFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(10, node.IntegerValue);
      Assert.Equal(10.0f, node.FloatValue);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_Function_WithFloatFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (float x) => x * 2.5f);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeFloatNode(4.0f));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.Float, node.ValueType);
      Assert.Equal(10.0f, node.FloatValue, 6);
      Assert.Equal(10, node.IntegerValue);
    }

    [Fact]
    public void AST_Function_WithBooleanFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (bool x) => !x);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeBooleanNode(true));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue);
      Assert.Equal(0, node.IntegerValue);
      Assert.Equal(0.0f, node.FloatValue);
    }

    [Fact]
    public void AST_Function_WithStringFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (string x) => x.ToUpperInvariant());
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeStringNode("hello"));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.String, node.ValueType);
      Assert.Equal("HELLO", node.StringValue);
    }

    [Fact]
    public void AST_Function_WithTwoParameters_ReturnsCorrectValue()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "add";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (int x, int y) => x + y);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      // Create tuple parameter node
      var tupleNode = new TupleNode();
      var tupleRpn = CreateStack(new FakeIntegerNode(3), new FakeIntegerNode(7));
      var tupleElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      tupleNode.Build(tupleRpn, tupleElement, Compiler.Options.None, null, null);

      var node = new FunctionNode();
      var rpn = CreateStack(tupleNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(10, node.IntegerValue);
    }

    [Fact]
    public void AST_Function_UndefinedFunction_ThrowsParserException()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "undefinedFunc";

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      var exception = Assert.Throws<ParserException>(() =>
        node.Build(rpn, element, Compiler.Options.None, null, functions));

      Assert.Contains("Undefined function", exception.Message);
      Assert.Contains(functionName, exception.Message);
    }

    [Fact]
    public void AST_Function_WithoutParameters_ThrowsParserException()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (int x) => x);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(); // Empty stack
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      var exception = Assert.Throws<ParserException>(() =>
        node.Build(rpn, element, Compiler.Options.None, null, functions));

      Assert.Contains("Cannot find parameters for calling function", exception.Message);
    }

    [Fact]
    public void AST_Function_WithWrongParameterType_ThrowsRuntimeException()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = functionName.GetUniqueIdentifier();

      // Function expects int but we'll pass string
      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeStringNode("hello"));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);

      var exception = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("function with given type signature is undefined", exception.Message);
    }

    [Fact]
    public void AST_Function_IsConstant_WithConstantFunction_ReturnsTrue()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "constFunc";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2, true); // isConstant = true
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5)); // Constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);

      Assert.True(node.IsConstant); // Both function and parameters are constant
    }

    [Fact]
    public void AST_Function_IsConstant_WithNonConstantFunction_ReturnsFalse()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "nonConstFunc";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2, false); // isConstant = false
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5)); // Constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);

      Assert.False(node.IsConstant); // Function is not constant
    }

    [Fact]
    public void AST_Function_IsConstant_WithVariableParameter_ReturnsFalse()
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "constFunc";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2, true); // isConstant = true
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new TestVariableNode()); // Non-constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);

      Assert.False(node.IsConstant); // Parameter is not constant
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    public void AST_Function_WorksWithAllCompilerOptions(Compiler.Options options)
    {
      var functions = new Dictionary<uint, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = functionName.GetUniqueIdentifier();

      var customFunction = CustomFunction.Create(functionName, (int x) => x + 1);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, options, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(6, node.IntegerValue);
    }

    // Helper class for testing non-constant parameters
    private class TestVariableNode : Node
    {
      public override bool IsConstant => false; // Not constant

      public TestVariableNode()
      {
        ValueType = ValueType.Integer;
        IntegerValue = 5;
        FloatValue = 5.0f;
        BooleanValue = true;
      }

      public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
        IVariableContainer variables, IDictionary<uint, CustomFunctionOverload> functions)
      {
        throw new NotImplementedException("Test node should not be built from RPN");
      }

      public override void Execute(IVariableContainer variablesOverride)
      {
        // No-op for testing
      }
    }
  }
}