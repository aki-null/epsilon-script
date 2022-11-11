using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public abstract class Lexer_Base
  {
    internal static void Succeeds(string testStr, Token[] expected)
    {
      var lexer = new Lexer.Lexer();
      var tokenReader = new TestTokenReader();
      using var expectedNext = ((IEnumerable<Token>)expected).GetEnumerator();
      lexer.Execute(testStr.AsMemory(), tokenReader);
      foreach (var token in tokenReader.Tokens)
      {
        Assert.True(expectedNext.MoveNext(), "Too many tokens");
        Assert.Equal(expectedNext.Current.Type, token.Type);
        Assert.Equal(expectedNext.Current.Text.ToString(), token.Text.ToString());
      }

      Assert.False(expectedNext.MoveNext(), "Not enough tokens");

      Assert.True(tokenReader.EndCalled, "Token reader not closed");
    }

    protected static void Fails(string testStr)
    {
      Assert.Throws<LexerException>(() =>
      {
        var lexer = new Lexer.Lexer();
        var tokenReader = new TestTokenReader();
        lexer.Execute(testStr.AsMemory(), tokenReader);
      });
    }
  }
}