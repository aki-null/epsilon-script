using System.Collections.Generic;
using EpsilonScript.Lexer;
using Xunit;

namespace EpsilonScript.Tests
{
  public abstract class Lexer_Base
  {
    protected static void Succeeds(string testStr, Token[] expected)
    {
      var lexer = new Lexer.Lexer();
      using var expectedNext = ((IEnumerable<Token>) expected).GetEnumerator();
      foreach (var token in lexer.Analyze(testStr))
      {
        Assert.True(expectedNext.MoveNext(), "Too many tokens");
        Assert.Equal(expectedNext.Current.Type, token.Type);
        Assert.Equal(expectedNext.Current.Text, token.Text);
      }

      Assert.False(expectedNext.MoveNext(), "Not enough tokens");
    }

    protected static void Fails(string testStr)
    {
      var lexer = new Lexer.Lexer();
      Assert.Throws<LexerException>(() =>
      {
        foreach (var token in lexer.Analyze(testStr))
        {
        }
      });
    }
  }
}