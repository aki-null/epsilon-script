using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.TestInfrastructure
{
  public abstract class TokenParserTestBase
  {
    /// <summary>
    /// Lex a string into tokens
    /// </summary>
    internal static IList<Token> LexString(string input)
    {
      var lexer = new EpsilonScript.Lexer.Lexer();
      var tokenReader = new TestTokenReader();
      lexer.Execute(input.AsMemory(), tokenReader);
      return tokenReader.Tokens;
    }

    /// <summary>
    /// Parse tokens into elements
    /// </summary>
    internal static IList<Element> ParseTokens(IList<Token> tokens)
    {
      var elementReader = new TestElementReader();
      var parser = new EpsilonScript.Parser.TokenParser(elementReader);
      foreach (var token in tokens)
      {
        parser.Push(token);
      }

      parser.End();
      return elementReader.Elements;
    }

    /// <summary>
    /// Parse a string expression into elements
    /// </summary>
    internal static IList<Element> ParseString(string input)
    {
      var tokens = LexString(input);
      return ParseTokens(tokens);
    }

    internal static void AssertParseSucceeds(IList<Token> tokens, IList<Element> expected)
    {
      var elementReader = new TestElementReader();
      var parser = new EpsilonScript.Parser.TokenParser(elementReader);
      foreach (var token in tokens)
      {
        parser.Push(token);
      }

      parser.End();
      Assert.True(elementReader.EndCalled, "Element reader not closed");

      var output = elementReader.Elements;
      Assert.Equal(expected.Count, output.Count);
      for (var i = 0; i < output.Count; ++i)
      {
        Assert.Equal(expected[i], output[i]);
      }
    }

    internal static object[] CreateTestCase(params Element[] elements)
    {
      var tokens = new Token[elements.Length];
      for (var i = 0; i < elements.Length; ++i)
      {
        tokens[i] = elements[i].Token;
      }

      return new object[] { tokens, elements };
    }
  }
}