using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.AST.Literal;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using EpsilonScript.Tests.TestInfrastructure.Fakes;
using Xunit;

namespace EpsilonScript.Tests.Function
{
  /// <summary>
  /// Tests for function overload cache invalidation correctness.
  ///
  /// Architecture:
  /// - Layer 1: FunctionNode instance cache (_cachedFunction, _cachedPackedTypes, _cachedVersion)
  /// - Layer 2: CustomFunctionOverload lookup cache (_lookupCache Dictionary)
  /// - Layer 3: CustomFunctionOverloadNode tree (permanent structure)
  /// </summary>
  [Trait("Category", "Unit")]
  [Trait("Component", "FunctionCache")]
  public class FunctionOverloadCacheTests
  {
    #region Test Infrastructure

    /// <summary>
    /// Helper to create a FunctionNode for direct AST testing.
    /// Note: Only works for single-parameter functions due to tuple complexity.
    /// </summary>
    private FunctionNode CreateFunctionNode(string functionName, CustomFunctionOverload overload, Node parameterNode)
    {
      var functions = new Dictionary<VariableId, CustomFunctionOverload>
      {
        [(VariableId)functionName] = overload
      };

      var node = new FunctionNode();
      var token = new Token(functionName, TokenType.Identifier);
      var element = new Element(token, ElementType.Function);

      var rpnStack = new Stack<Node>();
      rpnStack.Push(parameterNode);

      var context = new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, functions);
      node.Build(rpnStack, element, context, Compiler.Options.None, null);

      return node;
    }

    #endregion

    #region Version Tracking & Invalidation

    [Fact]
    public void VersionTracking_StartsAtZero()
    {
      var func = CustomFunction.Create("test", (int x) => x * 2);
      var overload = new CustomFunctionOverload(func, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      Assert.Equal(0, overload.Version);
    }

    [Fact]
    public void VersionTracking_IncrementsOnOverloadAdd()
    {
      var func = CustomFunction.Create("test", (int x) => x * 2);
      var overload = new CustomFunctionOverload(func, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var initialVersion = overload.Version;

      overload.Add(CustomFunction.Create("test", (float x) => x * 3.0f));

      Assert.Equal(initialVersion + 1, overload.Version);
    }

    [Fact]
    public void VersionTracking_IncrementsOnMultipleAdds()
    {
      var func = CustomFunction.Create("test", (int x) => x * 2);
      var overload = new CustomFunctionOverload(func, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      // Add 5 different overloads
      overload.Add(CustomFunction.Create("test", (float x) => x * 2.0f));
      overload.Add(CustomFunction.Create("test", (string x) => x.Length));
      overload.Add(CustomFunction.Create("test", (int x, int y) => x + y));
      overload.Add(CustomFunction.Create("test", (bool x) => x ? 1 : 0));
      overload.Add(CustomFunction.Create("test", (float x, float y) => x + y));

      Assert.Equal(5, overload.Version);
    }

    [Fact]
    public void Invalidation_AfterOverloadAdd_NewOverloadSelected()
    {
      // This is the KEY test: verify that after adding an overload,
      // a call that should use the new overload actually does.

      var compiler = new Compiler();
      compiler.AddCustomFunction(CustomFunction.Create("test", (int x) => x * 2));

      var script = compiler.Compile("test(5)");

      // With only int overload
      script.Execute();
      Assert.Equal(10, script.IntegerValue); // 5 * 2 = 10

      // Add float overload - now float arguments should use it
      compiler.AddCustomFunction(CustomFunction.Create("test", (float x) => x * 10.0f));
      script = compiler.Compile("test(5.5)");

      script.Execute();
      Assert.Equal(Type.Float, script.ValueType);
      Assert.True(System.Math.Abs(55.0f - script.FloatValue) < 0.001f); // 5.5 * 10 = 55
    }

    [Fact]
    public void Invalidation_MultipleCallSites_AllSeeNewOverload()
    {
      // Verify that multiple call sites all see the new overload after addition.
      // This tests that the version mechanism works across different FunctionNode instances.

      var compiler = new Compiler();
      compiler.AddCustomFunction(CustomFunction.Create("calc", (int x) => x * 2));

      var script1 = compiler.Compile("calc(10)");
      var script2 = compiler.Compile("calc(20) + calc(30)");

      script1.Execute();
      Assert.Equal(20, script1.IntegerValue);

      script2.Execute();
      Assert.Equal(100, script2.IntegerValue); // (20*2) + (30*2) = 100

      // Add float overload
      compiler.AddCustomFunction(CustomFunction.Create("calc", (float x) => x * 3.0f));
      script1 = compiler.Compile("calc(10.5)");
      script2 = compiler.Compile("calc(20.5) + calc(30.5)");

      // After recompilation with float args, should use float overload
      script1.Execute();
      Assert.True(System.Math.Abs(31.5f - script1.FloatValue) < 0.001f); // 10.5 * 3

      script2.Execute();
      Assert.True(System.Math.Abs(153.0f - script2.FloatValue) < 0.001f); // (20.5*3) + (30.5*3) = 153
    }

    [Fact]
    public void Invalidation_ValidationPhase_DoesNotPreventNewOverloadSelection()
    {
      // Verify that Validate() doesn't lock in a stale resolution.
      // After adding overload, Execute() should see the new overload.

      var func = CustomFunction.Create("test", (int x) => x * 2);
      var overload = new CustomFunctionOverload(func, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var paramNode = new FakeIntegerNode(5);
      var functionNode = CreateFunctionNode("test", overload, paramNode);

      // Validate resolves and caches
      functionNode.Validate();
      Assert.Equal(ExtendedType.Integer, functionNode.ValueType);

      // Add new overload
      overload.Add(CustomFunction.Create("test", (float x) => x * 3.0f));

      // Execute should detect version change and re-resolve
      functionNode.Execute(null);
      Assert.Equal(10, functionNode.IntegerValue); // Still uses int overload (correct for int param)
    }

    #endregion

    #region Dynamic Type Resolution

    [Fact]
    public void DynamicTypes_VariableTypeChange_CorrectOverloadSelected()
    {
      // When a variable changes type between executions, the correct overload
      // should be selected each time based on the runtime type.

      var intFunc = CustomFunction.Create("test", (int x) => x * 2);
      var floatFunc = CustomFunction.Create("test", (float x) => x * 3.0f);

      var overload =
        new CustomFunctionOverload(intFunc, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      overload.Add(floatFunc);

      var variables = new DictionaryVariableContainer { ["x"] = new VariableValue(5) }; // int
      var varNode = new VariableNode();
      varNode.Build(new Stack<Node>(), new Element(new Token("x", TokenType.Identifier), ElementType.Variable),
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, variables);

      var functionNode = CreateFunctionNode("test", overload, varNode);

      // First execution with int
      functionNode.Execute(variables);
      Assert.Equal(ExtendedType.Integer, functionNode.ValueType);
      Assert.Equal(10, functionNode.IntegerValue); // 5 * 2

      // Change variable to float
      variables["x"] = new VariableValue(5.5f);
      functionNode.Execute(variables);
      Assert.Equal(ExtendedType.Float, functionNode.ValueType);
      Assert.True(System.Math.Abs(16.5f - functionNode.FloatValue) < 0.001f); // 5.5 * 3
    }

    [Fact]
    public void DynamicTypes_UndefinedAtCompileTime_ResolvedAtRuntime()
    {
      // Variables have Undefined type at compile time.
      // Resolution happens at runtime based on actual value type.

      var compiler = new Compiler();
      compiler.AddCustomFunction(CustomFunction.Create("double", (int x) => x * 2));
      compiler.AddCustomFunction(CustomFunction.Create("double", (float x) => x * 2.0f));

      var variables = new DictionaryVariableContainer { ["val"] = new VariableValue(10) };
      var script = compiler.Compile("double(val)", Compiler.Options.Immutable, variables);

      // Execute with int
      script.Execute();
      Assert.Equal(Type.Integer, script.ValueType);
      Assert.Equal(20, script.IntegerValue);

      // Execute with float
      variables["val"] = new VariableValue(10.5f);
      script.Execute();
      Assert.Equal(Type.Float, script.ValueType);
      Assert.True(System.Math.Abs(21.0f - script.FloatValue) < 0.001f);
    }

    [Fact]
    public void TypeFallback_IntegerToFloat_CorrectOverloadSelected()
    {
      // When only a float overload exists, integer arguments should fallback to it.
      // This tests the type compatibility system.

      var floatFunc = CustomFunction.Create("test", (float x) => x * 2.0f);
      var overload =
        new CustomFunctionOverload(floatFunc, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var paramNode = new FakeIntegerNode(5);
      var functionNode = CreateFunctionNode("test", overload, paramNode);

      functionNode.Execute(null);
      Assert.Equal(ExtendedType.Float, functionNode.ValueType);
      Assert.True(System.Math.Abs(10.0f - functionNode.FloatValue) < 0.001f);
    }

    [Fact]
    public void TypeFallback_WithExactMatch_ExactMatchPreferred()
    {
      // When both exact match and fallback are available, exact match should be preferred.

      var intFunc = CustomFunction.Create("test", (int x) => x * 2);
      var floatFunc = CustomFunction.Create("test", (float x) => x * 3.0f);

      var overload =
        new CustomFunctionOverload(intFunc, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      overload.Add(floatFunc);

      var paramNode = new FakeIntegerNode(5);
      var functionNode = CreateFunctionNode("test", overload, paramNode);

      functionNode.Execute(null);
      Assert.Equal(ExtendedType.Integer, functionNode.ValueType);
      Assert.Equal(10, functionNode.IntegerValue); // Uses int overload (x*2), not float (x*3)
    }

    #endregion

    #region Multiple Overload Selection

    [Fact]
    public void MultipleOverloads_DifferentTypes_CorrectSelection()
    {
      // With multiple overloads for different types, each should be selected correctly.

      var compiler = new Compiler();
      compiler.AddCustomFunction(CustomFunction.Create("process", (int x) => x * 2));
      compiler.AddCustomFunction(CustomFunction.Create("process", (float x) => x * 3.0f));
      compiler.AddCustomFunction(CustomFunction.Create("process", (string x) => x.Length));
      compiler.AddCustomFunction(CustomFunction.Create("process", (bool x) => x ? 1 : 0));

      // Test int
      var script1 = compiler.Compile("process(10)");
      script1.Execute();
      Assert.Equal(20, script1.IntegerValue);

      // Test float
      var script2 = compiler.Compile("process(10.5)");
      script2.Execute();
      Assert.True(System.Math.Abs(31.5f - script2.FloatValue) < 0.001f);

      // Test string
      var script3 = compiler.Compile("process('hello')");
      script3.Execute();
      Assert.Equal(5, script3.IntegerValue);

      // Test bool
      var script4 = compiler.Compile("process(true)");
      script4.Execute();
      Assert.Equal(1, script4.IntegerValue);
    }

    [Fact]
    public void MultipleOverloads_AddNewType_CorrectlySelected()
    {
      // Adding an overload for a new type should be selected for that type.

      var compiler = new Compiler();
      compiler.AddCustomFunction(CustomFunction.Create("calc", (int x) => x * 2));

      var script = compiler.Compile("calc(5)");
      script.Execute();
      Assert.Equal(10, script.IntegerValue);

      // Add float overload
      compiler.AddCustomFunction(CustomFunction.Create("calc", (float x) => x * 10.0f));
      script = compiler.Compile("calc(5.0)");

      script.Execute();
      Assert.True(System.Math.Abs(50.0f - script.FloatValue) < 0.001f); // 5.0 * 10 = 50
    }

    [Fact]
    public void MultipleOverloads_DifferentCallSites_IndependentSelection()
    {
      // Two call sites with different argument types should select different overloads.

      var intFunc = CustomFunction.Create("test", (int x) => x * 2);
      var floatFunc = CustomFunction.Create("test", (float x) => x * 3.0f);

      var overload =
        new CustomFunctionOverload(intFunc, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      overload.Add(floatFunc);

      var intParam = new FakeIntegerNode(5);
      var floatParam = new FakeFloatNode(5.0f);

      var intNode = CreateFunctionNode("test", overload, intParam);
      var floatNode = CreateFunctionNode("test", overload, floatParam);

      // Int call site uses int overload
      intNode.Execute(null);
      Assert.Equal(10, intNode.IntegerValue);

      // Float call site uses float overload
      floatNode.Execute(null);
      Assert.True(System.Math.Abs(15.0f - floatNode.FloatValue) < 0.001f);
    }

    [Fact]
    public void MultipleOverloads_AfterAddingThird_AllStillResolveCorrectly()
    {
      // After adding a third overload, existing call sites should still work correctly.

      var intFunc = CustomFunction.Create("test", (int x) => x * 2);
      var floatFunc = CustomFunction.Create("test", (float x) => x * 3.0f);

      var overload =
        new CustomFunctionOverload(intFunc, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      overload.Add(floatFunc);

      var intParam = new FakeIntegerNode(5);
      var floatParam = new FakeFloatNode(5.0f);

      var intNode = CreateFunctionNode("test", overload, intParam);
      var floatNode = CreateFunctionNode("test", overload, floatParam);

      // Initial execution
      intNode.Execute(null);
      floatNode.Execute(null);

      // Add third overload (string)
      overload.Add(CustomFunction.Create("test", (string x) => x.Length));

      // Both should still resolve correctly
      intNode.Execute(null);
      Assert.Equal(10, intNode.IntegerValue);

      floatNode.Execute(null);
      Assert.True(System.Math.Abs(15.0f - floatNode.FloatValue) < 0.001f);
    }

    #endregion

    #region Lookup Cache Behavior

    [Fact]
    public void LookupCache_AfterOverloadAdd_ResolutionStillCorrect()
    {
      // The lookup cache is cleared when an overload is added.
      // Verify that resolution still works correctly afterward.

      var testFunc = CustomFunction.Create("test", (int x) => x * 2);
      var overload =
        new CustomFunctionOverload(testFunc, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      var packedTypes = new PackedParameterTypes();
      packedTypes.AddType(ExtendedType.Integer);

      // First lookup populates cache
      var result1 = overload.Find(packedTypes);
      Assert.NotNull(result1);
      Assert.Equal(Type.Integer, result1.ReturnType);

      // Add overload (clears cache)
      overload.Add(CustomFunction.Create("test", (float x) => x * 3.0f));

      // Lookup should still work (cache repopulated)
      var result2 = overload.Find(packedTypes);
      Assert.NotNull(result2);
      Assert.Equal(Type.Integer, result2.ReturnType); // Still resolves to int overload
    }

    [Fact]
    public void LookupCache_DifferentTypes_IndependentResolution()
    {
      // Different parameter types should resolve independently in the lookup cache.

      var intFunc = CustomFunction.Create("test", (int x) => x * 2);
      var floatFunc = CustomFunction.Create("test", (float x) => x * 3.0f);

      var overload =
        new CustomFunctionOverload(intFunc, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      overload.Add(floatFunc);

      var intTypes = new PackedParameterTypes();
      intTypes.AddType(ExtendedType.Integer);

      var floatTypes = new PackedParameterTypes();
      floatTypes.AddType(ExtendedType.Float);

      // Both should resolve correctly
      var intResult = overload.Find(intTypes);
      var floatResult = overload.Find(floatTypes);

      Assert.NotNull(intResult);
      Assert.NotNull(floatResult);
      Assert.Equal(Type.Integer, intResult.ReturnType);
      Assert.Equal(Type.Float, floatResult.ReturnType);
    }

    #endregion
  }
}