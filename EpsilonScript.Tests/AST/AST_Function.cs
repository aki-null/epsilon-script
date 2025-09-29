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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "add";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "constFunc";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "nonConstFunc";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "constFunc";
      var functionId = (VariableId)functionName;

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
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

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

    [Fact]
    public void AST_Function_WithZeroParameterIntegerFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getAnswer";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => 42);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), Compiler.Options.None, null, null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.Integer, node.ValueType);
      Assert.Equal(42, node.IntegerValue);
      Assert.Equal(42.0f, node.FloatValue);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_Function_WithZeroParameterFloatFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getPi";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => 3.14159f);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), Compiler.Options.None, null, null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.Float, node.ValueType);
      Assert.Equal(3.14159f, node.FloatValue, 5);
      Assert.Equal(3, node.IntegerValue);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    public void AST_Function_WithZeroParameterBooleanFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "isReady";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => true);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), Compiler.Options.None, null, null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
      Assert.Equal(1, node.IntegerValue);
      Assert.Equal(1.0f, node.FloatValue);
    }

    [Fact]
    public void AST_Function_WithZeroParameterStringFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getVersion";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => "v1.2.0");
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), Compiler.Options.None, null, null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.Equal(ValueType.String, node.ValueType);
      Assert.Equal("v1.2.0", node.StringValue);
    }

    [Fact]
    public void AST_Function_WithZeroParameterConstantFunction_IsConstant()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getConstant";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => 100, true);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), Compiler.Options.None, null, null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);
      node.Execute(null);

      Assert.True(node.IsConstant);
      Assert.Equal(100, node.IntegerValue);
    }

    [Fact]
    public void AST_Function_WithZeroParameterNonConstantFunction_IsNotConstant()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getTimestamp";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => DateTime.Now.Millisecond, false);
      var overload = new CustomFunctionOverload(customFunction);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), Compiler.Options.None, null, null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element, Compiler.Options.None, null, functions);

      Assert.False(node.IsConstant);
    }


    [Fact]
    public void AST_Function_WithZeroParameterFunctionMixedWithParameterizedFunction_WorksCorrectly()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();

      // Add a zero-parameter function
      var zeroParamName = "getZero";
      var zeroParamId = (VariableId)zeroParamName;
      var zeroParamFunction = CustomFunction.Create(zeroParamName, () => 0);
      var zeroParamOverload = new CustomFunctionOverload(zeroParamFunction);
      functions[zeroParamId] = zeroParamOverload;

      // Add a one-parameter function with the same name (overload)
      var oneParamFunction = CustomFunction.Create(zeroParamName, (int x) => x + 100);
      zeroParamOverload.Add(oneParamFunction);

      // Test zero-parameter call
      var node1 = new FunctionNode();
      var nullNode1 = new NullNode();
      nullNode1.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), Compiler.Options.None, null, null);
      var rpn1 = CreateStack(nullNode1);
      var token1 = new Token(zeroParamName, TokenType.Identifier);
      var element1 = new Element(token1, ElementType.Function);

      node1.Build(rpn1, element1, Compiler.Options.None, null, functions);
      node1.Execute(null);

      Assert.Equal(0, node1.IntegerValue); // Zero-parameter version

      // Test one-parameter call
      var node2 = new FunctionNode();
      var rpn2 = CreateStack(new FakeIntegerNode(5)); // One parameter
      var token2 = new Token(zeroParamName, TokenType.Identifier);
      var element2 = new Element(token2, ElementType.Function);

      node2.Build(rpn2, element2, Compiler.Options.None, null, functions);
      node2.Execute(null);

      Assert.Equal(105, node2.IntegerValue); // One-parameter version (5 + 100)
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
        IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions)
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