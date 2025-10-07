using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.AST
{
  [Trait("Category", "Unit")]
  [Trait("Component", "AST")]
  public class AST_String : AstTestBase
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
    public void AST_String_Constructor_CreatesCorrectStringNode(string value)
    {
      var node = new StringNode(value);

      Assert.Equal(Type.String, node.ValueType);
      Assert.Equal(value, node.StringValue);
      Assert.True(node.IsConstant);
    }

    [Fact]
    public void AST_String_DefaultConstructor_CreatesEmptyStringNode()
    {
      var node = new StringNode();

      // Default constructor doesn't initialize the value
      Assert.Equal(Type.Undefined, node.ValueType);
      Assert.Null(node.StringValue);
      Assert.True(node.IsConstant);
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
    public void AST_String_BuildFromToken_RemovesQuotesAndCreatesCorrectValue(string tokenText, string expectedValue)
    {
      var node = new StringNode();
      var rpn = CreateStack();
      var token = new Token(tokenText, TokenType.String);
      var element = new Element(token, ElementType.String);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Equal(Type.String, node.ValueType);
      Assert.Equal(expectedValue, node.StringValue);
    }

    [Theory]
    [InlineData("\"single character\"", "single character")]
    [InlineData("\"a\"", "a")]
    [InlineData("\" \"", " ")]
    public void AST_String_BuildFromToken_HandlesVariousLengths(string tokenText, string expectedValue)
    {
      var node = new StringNode();
      var rpn = CreateStack();
      var token = new Token(tokenText, TokenType.String);
      var element = new Element(token, ElementType.String);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Equal(Type.String, node.ValueType);
      Assert.Equal(expectedValue, node.StringValue);
    }

    [Fact]
    public void AST_String_IsConstant_ReturnsTrue()
    {
      var node = new StringNode("test");
      Assert.True(node.IsConstant);
    }

    [Theory]
    [InlineData(Compiler.Options.None)]
    [InlineData(Compiler.Options.Immutable)]
    public void AST_String_BuildWithAllOptions_Succeeds(Compiler.Options options)
    {
      var node = new StringNode();
      var rpn = CreateStack();
      var token = new Token("\"test value\"", TokenType.String);
      var element = new Element(token, ElementType.String);

      node.Build(rpn, element, options, null, null, Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);

      Assert.Equal(Type.String, node.ValueType);
      Assert.Equal("test value", node.StringValue);
    }

    [Fact]
    public void AST_String_Execute_DoesNothing()
    {
      var node = new StringNode("test");
      var originalValue = node.StringValue;

      // Execute should not change anything for string nodes
      node.Execute(null);

      Assert.Equal(Type.String, node.ValueType);
      Assert.Equal(originalValue, node.StringValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("abc")]
    [InlineData("very long string with many characters to test edge cases")]
    public void AST_String_HandlesDifferentStringLengths(string value)
    {
      var node = new StringNode(value);

      Assert.Equal(Type.String, node.ValueType);
      Assert.Equal(value, node.StringValue);
    }

    [Theory]
    [InlineData("\"Unicode: 你好世界\"", "Unicode: 你好世界")]
    [InlineData("\"Emoji: 😀🎉🚀\"", "Emoji: 😀🎉🚀")]
    [InlineData("\"Accents: café résumé naïve\"", "Accents: café résumé naïve")]
    public void AST_String_BuildFromToken_HandlesUnicodeCharacters(string tokenText, string expectedValue)
    {
      var node = new StringNode();
      var rpn = CreateStack();
      var token = new Token(tokenText, TokenType.String);
      var element = new Element(token, ElementType.String);

      node.Build(rpn, element, Compiler.Options.None, null, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);

      Assert.Equal(Type.String, node.ValueType);
      Assert.Equal(expectedValue, node.StringValue);
    }

    [Fact]
    public void AST_String_Optimize_ReturnsValueNode()
    {
      var node = new StringNode("test");

      var optimizedNode = node.Optimize();

      // StringNode is constant, so optimization should return a value node
      Assert.IsAssignableFrom<Node>(optimizedNode);
      Assert.Equal(Type.String, optimizedNode.ValueType);
      Assert.Equal("test", optimizedNode.StringValue);
    }
  }
}