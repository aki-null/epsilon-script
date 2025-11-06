using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.AST.Literal;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AstFunctionTests : AstTestBase
  {
    [Fact]
    internal void Function_WithIntegerFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Integer, node.ValueType);
      Assert.Equal(10, node.IntegerValue);
      Assert.Equal(10.0f, node.FloatValue);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    internal void Function_WithFloatFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (float x) => x * 2.5f);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeFloatNode(4.0f));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Float, node.ValueType);
      Assert.Equal(10.0f, node.FloatValue, 6);
      Assert.Equal(10, node.IntegerValue);
    }

    [Fact]
    internal void Function_WithBooleanFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (bool x) => !x);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeBooleanNode(true));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.False(node.BooleanValue);
      Assert.Equal(0, node.IntegerValue);
      Assert.Equal(0.0f, node.FloatValue);
    }

    [Fact]
    internal void Function_WithStringFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (string x) => x.ToUpperInvariant());
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeStringNode("hello"));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.String, node.ValueType);
      Assert.Equal("HELLO", node.StringValue);
    }

    [Fact]
    internal void Function_WithTwoParameters_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "add";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x, int y) => x + y);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      // Create tuple parameter node
      var tupleNode = new TupleNode();
      var tupleRpn = CreateStack(new FakeIntegerNode(3), new FakeIntegerNode(7));
      var tupleElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      var tupleContext = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      tupleNode.Build(tupleRpn, tupleElement, tupleContext, Compiler.Options.None, null);

      var node = new FunctionNode();
      var rpn = CreateStack(tupleNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Integer, node.ValueType);
      Assert.Equal(10, node.IntegerValue);
    }

    [Fact]
    internal void Function_UndefinedFunction_ThrowsParserException()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "undefinedFunc";

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      var exception = Assert.Throws<ParserException>(() =>
        node.Build(rpn, element,
          new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float,
            new Dictionary<VariableId, CustomFunctionOverload>()),
          Compiler.Options.None, null));

      Assert.Contains("Undefined function", exception.Message);
      Assert.Contains(functionName, exception.Message);
    }

    [Fact]
    internal void Function_WithoutParameters_ThrowsParserException()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x) => x);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(); // Empty stack
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      var exception = Assert.Throws<ParserException>(() =>
        node.Build(rpn, element,
          new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
          Compiler.Options.None, null));

      Assert.Contains("Cannot find parameters for calling function", exception.Message);
    }

    [Fact]
    internal void Function_WithWrongParameterType_ThrowsRuntimeException()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

      // Function expects int but we'll pass string
      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeStringNode("hello"));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      var exception = Assert.Throws<RuntimeException>(() => node.Execute(null));
      Assert.Contains("No overload of function", exception.Message);
      Assert.Contains("matches parameter types", exception.Message);
    }

    [Fact]
    internal void Function_IsPrecomputable_WithConstantFunction_ReturnsTrue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "constFunc";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2, isDeterministic: true);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5)); // Constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      Assert.True(node.IsPrecomputable); // Both function and parameters are constant
    }

    [Fact]
    internal void Function_IsPrecomputable_WithNonConstantFunction_ReturnsFalse()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "nonConstFunc";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2, false); // isConstant = false
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5)); // Constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      Assert.False(node.IsPrecomputable); // Function is not constant
    }

    [Fact]
    internal void Function_IsPrecomputable_WithVariableParameter_ReturnsFalse()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "constFunc";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2, isDeterministic: true);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new TestVariableNode()); // Non-constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      Assert.False(node.IsPrecomputable); // Parameter is not constant
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    internal void Function_WorksWithAllCompilerOptions(Compiler.Options options)
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "testFunc";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x) => x + 1);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      var optionsContext = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float,
        functions);
      node.Build(rpn, element, optionsContext, options, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Integer, node.ValueType);
      Assert.Equal(6, node.IntegerValue);
    }

    [Fact]
    internal void Function_WithZeroParameterIntegerFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getAnswer";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => 42);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      var nullContext = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), nullContext, Compiler.Options.None,
        null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Integer, node.ValueType);
      Assert.Equal(42, node.IntegerValue);
      Assert.Equal(42.0f, node.FloatValue);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    internal void Function_WithZeroParameterFloatFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getPi";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => 3.14159f);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      var nullContext2 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), nullContext2, Compiler.Options.None,
        null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Float, node.ValueType);
      Assert.Equal(3.14159f, node.FloatValue, 5);
      Assert.Equal(3, node.IntegerValue);
      Assert.True(node.BooleanValue);
    }

    [Fact]
    internal void Function_WithZeroParameterBooleanFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "isReady";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => true);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      var nullContext3 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), nullContext3, Compiler.Options.None,
        null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Boolean, node.ValueType);
      Assert.True(node.BooleanValue);
      Assert.Equal(1, node.IntegerValue);
      Assert.Equal(1.0f, node.FloatValue);
    }

    [Fact]
    internal void Function_WithZeroParameterStringFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getVersion";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => "v1.2.0");
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      var nullContext4 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), nullContext4, Compiler.Options.None,
        null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.String, node.ValueType);
      Assert.Equal("v1.2.0", node.StringValue);
    }

    [Fact]
    internal void Function_WithZeroParameterConstantFunction_IsPrecomputable()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getConstant";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => 100, true);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      var nullContext5 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), nullContext5, Compiler.Options.None,
        null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.True(node.IsPrecomputable);
      Assert.Equal(100, node.IntegerValue);
    }

    [Fact]
    internal void Function_WithZeroParameterNonConstantFunction_IsNotConstant()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "getTimestamp";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, () => DateTime.Now.Millisecond, false);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var nullNode = new NullNode();
      var nullContext6 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      nullNode.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), nullContext6, Compiler.Options.None,
        null);
      var rpn = CreateStack(nullNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      Assert.False(node.IsPrecomputable);
    }


    [Fact]
    internal void Function_WithZeroParameterFunctionMixedWithParameterizedFunction_WorksCorrectly()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();

      // Add a zero-parameter function
      var zeroParamName = "getZero";
      var zeroParamId = (VariableId)zeroParamName;
      var zeroParamFunction = CustomFunction.Create(zeroParamName, () => 0);
      var zeroParamOverload = new CustomFunctionOverload(zeroParamFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[zeroParamId] = zeroParamOverload;

      // Add a one-parameter function with the same name (overload)
      var oneParamFunction = CustomFunction.Create(zeroParamName, (int x) => x + 100);
      zeroParamOverload.Add(oneParamFunction);

      // Test zero-parameter call
      var node1 = new FunctionNode();
      var nullNode1 = new NullNode();
      var nullContext7 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      nullNode1.Build(new Stack<Node>(), new Element(new Token(), ElementType.None), nullContext7,
        Compiler.Options.None, null);
      var rpn1 = CreateStack(nullNode1);
      var token1 = new Token(zeroParamName, TokenType.Identifier);
      var element1 = new Element(token1, ElementType.Function);

      var context1 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions);
      node1.Build(rpn1, element1, context1, Compiler.Options.None, null);
      node1.Execute(null);

      Assert.Equal(0, node1.IntegerValue); // Zero-parameter version

      // Test one-parameter call
      var node2 = new FunctionNode();
      var rpn2 = CreateStack(new FakeIntegerNode(5)); // One parameter
      var token2 = new Token(zeroParamName, TokenType.Identifier);
      var element2 = new Element(token2, ElementType.Function);

      var context2 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions);
      node2.Build(rpn2, element2, context2, Compiler.Options.None, null);
      node2.Execute(null);

      Assert.Equal(105, node2.IntegerValue); // One-parameter version (5 + 100)
    }

    [Fact]
    internal void Function_Optimize_WithConstantFunctionAndParameters_ReturnsValueNode()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "square";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x) => x * x, true); // Constant function
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5)); // Constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      // Optimize should evaluate the constant function and return a value node
      var optimized = node.Optimize();

      Assert.NotSame(node, optimized); // Should return a different node
      Assert.IsType<IntegerNode>(optimized);
      Assert.Equal(25, optimized.IntegerValue);
    }

    [Fact]
    internal void Function_Optimize_WithNonConstantFunction_ReturnsSelf()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "random";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2, false); // Non-constant function
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeIntegerNode(5)); // Constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      // Optimize should not fold non-constant functions
      var optimized = node.Optimize();

      Assert.Same(node, optimized); // Should return the same node
    }

    [Fact]
    internal void Function_Optimize_WithNonConstantParameter_ReturnsSelf()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "double";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x) => x * 2, true); // Constant function
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new TestVariableNode()); // Non-constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      // Optimize should not fold if parameters are not constant
      var optimized = node.Optimize();

      Assert.Same(node, optimized); // Should return the same node
    }

    [Fact]
    internal void Function_Optimize_WithConstantFloatFunction_ReturnsFloatNode()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "half";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (float x) => x / 2.0f, true); // Constant function
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeFloatNode(10.0f)); // Constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      // Optimize should evaluate and return a float node
      var optimized = node.Optimize();

      Assert.IsType<FloatNode>(optimized);
      Assert.Equal(5.0f, optimized.FloatValue, 6);
    }

    [Fact]
    internal void Function_Optimize_WithConstantBooleanFunction_ReturnsBooleanNode()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "not";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (bool x) => !x, true); // Constant function
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeBooleanNode(true)); // Constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      // Optimize should evaluate and return a boolean node
      var optimized = node.Optimize();

      Assert.IsType<BooleanNode>(optimized);
      Assert.False(optimized.BooleanValue);
    }

    [Fact]
    internal void Function_Optimize_WithConstantStringFunction_ReturnsStringNode()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "upper";
      var functionId = (VariableId)functionName;

      var customFunction =
        CustomFunction.Create(functionName, (string x) => x.ToUpperInvariant(), true); // Constant function
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeStringNode("hello")); // Constant parameter
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      // Optimize should evaluate and return a string node
      var optimized = node.Optimize();

      Assert.IsType<StringNode>(optimized);
      Assert.Equal("HELLO", optimized.StringValue);
    }

    [Fact]
    internal void Function_Optimize_WithMultipleParameters_EvaluatesCorrectly()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "add";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (int x, int y) => x + y, true); // Constant function
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      // Create tuple parameter node
      var tupleNode = new TupleNode();
      var tupleRpn = CreateStack(new FakeIntegerNode(3), new FakeIntegerNode(7));
      var tupleElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      var tupleContext = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null);
      tupleNode.Build(tupleRpn, tupleElement, tupleContext, Compiler.Options.None, null);

      var node = new FunctionNode();
      var rpn = CreateStack(tupleNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);

      // Optimize should evaluate and return an integer node
      var optimized = node.Optimize();

      Assert.IsType<IntegerNode>(optimized);
      Assert.Equal(10, optimized.IntegerValue);
    }

    #region Precision Type Tests (Long, Double, Decimal)

    [Fact]
    internal void Function_WithLongFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "multiplyLong";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (long x) => x * 2L);
      var overload =
        new CustomFunctionOverload(customFunction, Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeLongNode(3000000000L));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Long, node.ValueType);
      Assert.Equal(6000000000L, node.LongValue);
    }

    [Fact]
    internal void Function_WithDoubleFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "multiplyDouble";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (double x) => x * 2.5);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var rpn = CreateStack(new FakeDoubleNode(3.141592653589793));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Double, node.ValueType);
      Assert.Equal(7.85398163397448, node.DoubleValue, precision: 10);
    }

    [Fact]
    internal void Function_WithDecimalFunction_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "multiplyDecimal";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (decimal x) => x * 2.5m);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      functions[functionId] = overload;

      var node = new FunctionNode();
      var inputValue = 1.23456789012345678901234567m;
      var rpn = CreateStack(new FakeDecimalNode(inputValue));
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Decimal, node.ValueType);
      var expected = inputValue * 2.5m;
      Assert.Equal(expected, node.DecimalValue);
    }

    [Fact]
    internal void Function_WithLongParameters_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "addLong";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (long x, long y) => x + y, true);
      var overload =
        new CustomFunctionOverload(customFunction, Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
      functions[functionId] = overload;

      var tupleNode = new TupleNode();
      var tupleRpn = CreateStack(new FakeLongNode(5000000000L), new FakeLongNode(3000000000L));
      var tupleElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      var tupleContext2 = new CompilerContext(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float, null);
      tupleNode.Build(tupleRpn, tupleElement, tupleContext2, Compiler.Options.None, null);

      var node = new FunctionNode();
      var rpn = CreateStack(tupleNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Long, node.ValueType);
      Assert.Equal(8000000000L, node.LongValue);
    }

    [Fact]
    internal void Function_WithDoubleParameters_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "addDouble";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (double x, double y) => x + y, true);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
      functions[functionId] = overload;

      var tupleNode = new TupleNode();
      var tupleRpn =
        CreateStack(new FakeDoubleNode(3.141592653589793), new FakeDoubleNode(2.718281828459045));
      var tupleElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      var tupleContext3 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double, null);
      tupleNode.Build(tupleRpn, tupleElement, tupleContext3, Compiler.Options.None, null);

      var node = new FunctionNode();
      var rpn = CreateStack(tupleNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Double, node.ValueType);
      Assert.Equal(5.859874482048838, node.DoubleValue, precision: 10);
    }

    [Fact]
    internal void Function_WithDecimalParameters_ReturnsCorrectValue()
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>();
      var functionName = "addDecimal";
      var functionId = (VariableId)functionName;

      var customFunction = CustomFunction.Create(functionName, (decimal x, decimal y) => x + y, true);
      var overload = new CustomFunctionOverload(customFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
      functions[functionId] = overload;

      var tupleNode = new TupleNode();
      var tupleRpn = CreateStack(new FakeDecimalNode(0.1m), new FakeDecimalNode(0.2m));
      var tupleElement = new Element(new Token(",", TokenType.Comma), ElementType.Comma);
      var tupleContext4 = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal, null);
      tupleNode.Build(tupleRpn, tupleElement, tupleContext4, Compiler.Options.None, null);

      var node = new FunctionNode();
      var rpn = CreateStack(tupleNode);
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions),
        Compiler.Options.None, null);
      node.Execute(null);

      Assert.Equal(ExtendedType.Decimal, node.ValueType);
      Assert.Equal(0.3m, node.DecimalValue); // Exact decimal arithmetic
    }

    [Fact]
    internal void Function_AddingIntFunctionToLongPrecisionCompiler_ThrowsArgumentException()
    {
      var functionName = "overloaded";
      var intFunction = CustomFunction.Create(functionName, (int x) => x * 2);

      var ex = Assert.Throws<ArgumentException>(() =>
        new CustomFunctionOverload(intFunction, Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float));

      Assert.Contains("Cannot add function with Integer parameter", ex.Message);
      Assert.Contains("Long precision", ex.Message);
    }

    [Fact]
    internal void Function_AddingLongFunctionToIntegerPrecisionCompiler_ThrowsArgumentException()
    {
      var functionName = "overloaded";
      var longFunction = CustomFunction.Create(functionName, (long x) => x * 3L);

      var ex = Assert.Throws<ArgumentException>(() =>
        new CustomFunctionOverload(longFunction, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float));

      Assert.Contains("Cannot add function with Long parameter", ex.Message);
      Assert.Contains("Integer precision", ex.Message);
    }

    [Fact]
    internal void Function_AddingLongOverloadToIntFunction_ThrowsArgumentException()
    {
      var functionName = "overloaded";
      var intFunction = CustomFunction.Create(functionName, (int x) => x * 2);
      var longFunction = CustomFunction.Create(functionName, (long x) => x * 3L);

      var intOverload = new CustomFunctionOverload(intFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<ArgumentException>(() => intOverload.Add(longFunction));

      Assert.Contains("Cannot add function with Long parameter", ex.Message);
    }

    [Fact]
    internal void Function_AddingFloatFunctionToDoublePrecisionCompiler_ThrowsArgumentException()
    {
      var functionName = "overloaded";
      var floatFunction = CustomFunction.Create(functionName, (float x) => x * 2.0f);

      var ex = Assert.Throws<ArgumentException>(() =>
        new CustomFunctionOverload(floatFunction, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double));

      Assert.Contains("Cannot add function with Float parameter", ex.Message);
      Assert.Contains("Double precision", ex.Message);
    }

    [Fact]
    internal void Function_AddingDoubleFunctionToFloatPrecisionCompiler_ThrowsArgumentException()
    {
      var functionName = "overloaded";
      var doubleFunction = CustomFunction.Create(functionName, (double x) => x * 3.0);

      var ex = Assert.Throws<ArgumentException>(() =>
        new CustomFunctionOverload(doubleFunction, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float));

      Assert.Contains("Cannot add function with Double parameter", ex.Message);
      Assert.Contains("Float precision", ex.Message);
    }

    [Fact]
    internal void Function_AddingDecimalFunctionToFloatPrecisionCompiler_ThrowsArgumentException()
    {
      var functionName = "overloaded";
      var decimalFunction = CustomFunction.Create(functionName, (decimal x) => x * 3.0m);

      var ex = Assert.Throws<ArgumentException>(() =>
        new CustomFunctionOverload(decimalFunction, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float));

      Assert.Contains("Cannot add function with Decimal parameter", ex.Message);
      Assert.Contains("Float precision", ex.Message);
    }

    [Fact]
    internal void Function_AddingDoubleOverloadToFloatFunction_ThrowsArgumentException()
    {
      var functionName = "overloaded";
      var floatFunction = CustomFunction.Create(functionName, (float x) => x * 2.0f);
      var doubleFunction = CustomFunction.Create(functionName, (double x) => x * 3.0);

      var floatOverload = new CustomFunctionOverload(floatFunction,
        Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var ex = Assert.Throws<ArgumentException>(() => floatOverload.Add(doubleFunction));

      Assert.Contains("Cannot add function with Double parameter", ex.Message);
    }

    #endregion

    // Helper class for testing non-constant parameters
    private class TestVariableNode : Node
    {
      public override bool IsPrecomputable => false; // Not constant

      public TestVariableNode()
      {
        IntegerValue = 5;
        FloatValue = 5.0f;
        BooleanValue = true;
      }

      protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
        Compiler.Options options, IVariableContainer variables)
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