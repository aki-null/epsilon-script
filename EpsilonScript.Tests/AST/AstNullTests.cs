using EpsilonScript.AST;
using EpsilonScript.AST.Literal;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.AST
{
  public class AstNullTests : AstTestBase
  {
    [Fact]
    internal void Null_Build_SetsCorrectValueType()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.Null(node.TupleValue);
      Assert.Null(node.StringValue);
      Assert.Null(node.Variable);
    }

    [Fact]
    internal void Null_IsPrecomputable_ReturnsTrue()
    {
      var node = new NullNode();

      Assert.True(node.IsPrecomputable); // Null is always constant
    }

    [Fact]
    internal void Null_Execute_DoesNothing()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      // Execute should not throw or change anything
      node.Execute(null);

      Assert.Null(node.TupleValue);
      Assert.Null(node.StringValue);
      Assert.Null(node.Variable);
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    internal void Null_WorksWithAllCompilerOptions(Compiler.Options options)
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null), options, null);
      node.Execute(null);

      Assert.Null(node.TupleValue);
      Assert.Null(node.StringValue);
      Assert.Null(node.Variable);
    }

    [Fact]
    internal void Null_Optimize_ReturnsValueNode()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      var optimizedNode = node.Optimize();

      // Since NullNode is constant, optimization should return a value node
      Assert.IsAssignableFrom<Node>(optimizedNode);
      Assert.Null(optimizedNode.TupleValue);
      Assert.Null(optimizedNode.StringValue);
      Assert.Null(optimizedNode.Variable);
    }

    [Fact]
    internal void Null_BuildWithEmptyStack_Succeeds()
    {
      var node = new NullNode();
      var rpn = CreateStack(); // Empty stack is fine for null
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      // Should not throw an exception
      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.Null(node.TupleValue);
      Assert.Null(node.StringValue);
      Assert.Null(node.Variable);
    }

    [Fact]
    internal void Null_BuildWithNonEmptyStack_Succeeds()
    {
      var node = new NullNode();
      var rpn = CreateStack(new FakeIntegerNode(42)); // Non-empty stack should also work
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      // Should not consume from stack or throw an exception
      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.Null(node.TupleValue);
      Assert.Null(node.StringValue);
      Assert.Null(node.Variable);
      // Stack should still contain the integer node since null doesn't consume it
      Assert.True(rpn.TryPop(out var remainingNode));
      Assert.IsAssignableFrom<Node>(remainingNode);
    }

    [Fact]
    internal void Null_DefaultValues_AreCorrect()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.Equal(0, node.IntegerValue);
      Assert.Equal(0.0f, node.FloatValue);
      Assert.False(node.BooleanValue);
      Assert.Null(node.StringValue);
      Assert.Null(node.TupleValue);
      Assert.Null(node.Variable);
    }

    [Fact]
    internal void Null_MultipleExecutions_RemainConsistent()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      // Execute multiple times
      node.Execute(null);
      node.Execute(null);
      node.Execute(null);

      // Should remain null
      Assert.Null(node.TupleValue);
      Assert.Null(node.StringValue);
      Assert.Null(node.Variable);
    }

    [Fact]
    internal void Null_WithNullVariableContainer_WorksCorrectly()
    {
      var node = new NullNode();
      var rpn = CreateStack();
      var element = new Element(new Token("null", TokenType.Identifier), ElementType.None);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);
      node.Execute(null); // null variable container should be fine

      Assert.Null(node.TupleValue);
      Assert.Null(node.StringValue);
      Assert.Null(node.Variable);
    }
  }
}