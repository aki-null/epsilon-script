using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.AST
{
  public class AST_Null : AstTestBase
  {
    [Fact]
    public void AST_Null_Build_SetsCorrectValueType()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Equal(ValueType.Null, node.ValueType);
    }

    [Fact]
    public void AST_Null_IsConstant_ReturnsTrue()
    {
      var node = new NullNode();

      Assert.True(node.IsConstant); // Null is always constant
    }

    [Fact]
    public void AST_Null_Execute_DoesNothing()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      // Execute should not throw or change anything
      node.Execute(null);

      Assert.Equal(ValueType.Null, node.ValueType);
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    public void AST_Null_WorksWithAllCompilerOptions(Compiler.Options options)
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element, options, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      node.Execute(null);

      Assert.Equal(ValueType.Null, node.ValueType);
    }

    [Fact]
    public void AST_Null_Optimize_ReturnsValueNode()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      var optimizedNode = node.Optimize();

      // Since NullNode is constant, optimization should return a value node
      Assert.IsAssignableFrom<Node>(optimizedNode);
      Assert.Equal(ValueType.Null, optimizedNode.ValueType);
    }

    [Fact]
    public void AST_Null_BuildWithEmptyStack_Succeeds()
    {
      var node = new NullNode();
      var rpn = CreateStack(); // Empty stack is fine for null
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      // Should not throw an exception
      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Equal(ValueType.Null, node.ValueType);
    }

    [Fact]
    public void AST_Null_BuildWithNonEmptyStack_Succeeds()
    {
      var node = new NullNode();
      var rpn = CreateStack(new FakeIntegerNode(42)); // Non-empty stack should also work
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      // Should not consume from stack or throw an exception
      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Equal(ValueType.Null, node.ValueType);
      // Stack should still contain the integer node since null doesn't consume it
      Assert.True(rpn.TryPop(out var remainingNode));
      Assert.IsAssignableFrom<Node>(remainingNode);
    }

    [Fact]
    public void AST_Null_DefaultValues_AreCorrect()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Equal(ValueType.Null, node.ValueType);
      Assert.Equal(0, node.IntegerValue);
      Assert.Equal(0.0f, node.FloatValue);
      Assert.False(node.BooleanValue);
      Assert.Null(node.StringValue);
      Assert.Null(node.TupleValue);
      Assert.Null(node.Variable);
    }

    [Fact]
    public void AST_Null_MultipleExecutions_RemainConsistent()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      // Execute multiple times
      node.Execute(null);
      node.Execute(null);
      node.Execute(null);

      // Should remain null
      Assert.Equal(ValueType.Null, node.ValueType);
    }

    [Fact]
    public void AST_Null_WithNullVariableContainer_WorksCorrectly()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      node.Execute(null); // null variable container should be fine

      Assert.Equal(ValueType.Null, node.ValueType);
    }
  }
}