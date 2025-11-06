using EpsilonScript.AST;
using EpsilonScript.AST.Literal;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AstStringTests : AstTestBase
  {
    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("world")]
    [InlineData("Hello World")]
    [InlineData("123")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("with spaces")]
    [InlineData("with\ttabs")]
    [InlineData("with\nnewlines")]
    [InlineData("Special!@#$%^&*()Characters")]
    internal void String_Constructor_CreatesCorrectStringNode(string value)
    {
      var node = new StringNode(value);

      Assert.Equal(ExtendedType.String, node.ValueType);
      Assert.Equal(value, node.StringValue);
      Assert.True(node.IsPrecomputable);
    }

    [Fact]
    internal void String_DefaultConstructor_CreatesEmptyStringNode()
    {
      var node = new StringNode();

      // Default constructor doesn't initialize the value
      Assert.Equal(ExtendedType.Undefined, node.ValueType);
      Assert.Null(node.StringValue);
      Assert.True(node.IsPrecomputable);
    }

    [Theory]
    [InlineData("\"\"", "")]
    [InlineData("\"hello\"", "hello")]
    [InlineData("\"world\"", "world")]
    [InlineData("\"Hello World\"", "Hello World")]
    [InlineData("\"123\"", "123")]
    [InlineData("\"true\"", "true")]
    [InlineData("\"false\"", "false")]
    [InlineData("\"with spaces\"", "with spaces")]
    [InlineData("\"with\ttabs\"", "with\ttabs")]
    [InlineData("\"with\nnewlines\"", "with\nnewlines")]
    [InlineData("\"Special!@#$%^&*()Characters\"", "Special!@#$%^&*()Characters")]
    internal void String_BuildFromToken_RemovesQuotesAndCreatesCorrectValue(string tokenText, string expectedValue)
    {
      var node = new StringNode();
      var rpn = CreateStack();
      var token = new Token(tokenText, TokenType.String);
      var element = new Element(token, ElementType.String);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.Equal(ExtendedType.String, node.ValueType);
      Assert.Equal(expectedValue, node.StringValue);
    }

    [Theory]
    [InlineData("\"single character\"", "single character")]
    [InlineData("\"a\"", "a")]
    [InlineData("\" \"", " ")]
    internal void String_BuildFromToken_HandlesVariousLengths(string tokenText, string expectedValue)
    {
      var node = new StringNode();
      var rpn = CreateStack();
      var token = new Token(tokenText, TokenType.String);
      var element = new Element(token, ElementType.String);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.Equal(ExtendedType.String, node.ValueType);
      Assert.Equal(expectedValue, node.StringValue);
    }

    [Fact]
    internal void String_IsPrecomputable_ReturnsTrue()
    {
      var node = new StringNode("test");
      Assert.True(node.IsPrecomputable);
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    internal void String_BuildWithAllOptions_Succeeds(Compiler.Options options)
    {
      var node = new StringNode();
      var rpn = CreateStack();
      var token = new Token("\"test value\"", TokenType.String);
      var element = new Element(token, ElementType.String);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null), options, null);

      Assert.Equal(ExtendedType.String, node.ValueType);
      Assert.Equal("test value", node.StringValue);
    }

    [Fact]
    internal void String_Execute_DoesNothing()
    {
      var node = new StringNode("test");
      var originalValue = node.StringValue;

      // Execute should not change anything for string nodes
      node.Execute(null);

      Assert.Equal(ExtendedType.String, node.ValueType);
      Assert.Equal(originalValue, node.StringValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("very long string with many characters to test edge cases")]
    internal void String_HandlesDifferentStringLengths(string value)
    {
      var node = new StringNode(value);

      Assert.Equal(ExtendedType.String, node.ValueType);
      Assert.Equal(value, node.StringValue);
    }

    [Theory]
    [InlineData("\"Unicode: 你好世界\"", "Unicode: 你好世界")]
    [InlineData("\"Emoji: 😀🎉🚀\"", "Emoji: 😀🎉🚀")]
    [InlineData("\"Accents: café résumé naïve\"", "Accents: café résumé naïve")]
    internal void String_BuildFromToken_HandlesUnicodeCharacters(string tokenText, string expectedValue)
    {
      var node = new StringNode();
      var rpn = CreateStack();
      var token = new Token(tokenText, TokenType.String);
      var element = new Element(token, ElementType.String);

      node.Build(rpn, element,
        new CompilerContext(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, null),
        Compiler.Options.None, null);

      Assert.Equal(ExtendedType.String, node.ValueType);
      Assert.Equal(expectedValue, node.StringValue);
    }

    [Fact]
    internal void String_Optimize_ReturnsValueNode()
    {
      var node = new StringNode("test");

      var optimizedNode = node.Optimize();

      // StringNode is constant, so optimization should return a value node
      Assert.IsAssignableFrom<Node>(optimizedNode);
      Assert.Equal(ExtendedType.String, optimizedNode.ValueType);
      Assert.Equal("test", optimizedNode.StringValue);
    }
  }
}