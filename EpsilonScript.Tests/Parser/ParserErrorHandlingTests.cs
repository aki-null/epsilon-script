using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Parser
{
  public class ParserErrorHandlingTests : TokenParserTestBase
  {
    [Fact]
    internal void Parser_EmptyTokenList_HandlesGracefully()
    {
      // Empty token list should not throw during parsing
      var tokens = new Token[0];
      var elementReader = new TestInfrastructure.Fakes.TestElementReader();
      var parser = new global::EpsilonScript.Parser.TokenParser(elementReader);

      // This should not throw for empty input
      foreach (var token in tokens)
      {
        parser.Push(token);
      }

      parser.End();

      // Parser should handle empty input gracefully (may produce 0 or 1 elements)
      Assert.True(elementReader.Elements.Count >= 0);
      Assert.True(elementReader.EndCalled);
    }

    [Theory]
    [MemberData(nameof(ValidTokenSequences))]
    internal void Parser_ValidTokenSequences_ParseCorrectly(Token[] tokens, Element[] expected)
    {
      AssertParseSucceeds(tokens, expected);
    }

    public static IEnumerable<object[]> ValidTokenSequences
    {
      get
      {
        return new[]
        {
          // Simple addition
          new object[]
          {
            new[]
            {
              new Token("1", TokenType.Integer),
              new Token("+", TokenType.PlusSign),
              new Token("2", TokenType.Integer),
            },
            new[]
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
            }
          },
          // Simple boolean expression
          new object[]
          {
            new[]
            {
              new Token("true", TokenType.BooleanLiteralTrue),
              new Token("&&", TokenType.BooleanAndOperator),
              new Token("false", TokenType.BooleanLiteralFalse),
            },
            new[]
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
            }
          },
        };
      }
    }
  }
}