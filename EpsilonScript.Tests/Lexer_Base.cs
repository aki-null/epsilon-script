using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public abstract class Lexer_Base
  {
    protected void Succeeds(string testStr, Lexer.Token[] expected)
    {
      var lexer = new Lexer(testStr);
      var expectedNext = ((IEnumerable<Lexer.Token>)expected).GetEnumerator();
      foreach (var token in lexer.Process())
      {
        Assert.True(expectedNext.MoveNext(), "Too many tokens");
        Assert.Equal(expectedNext.Current.type, token.type);
        Assert.Equal(expectedNext.Current.text, token.text);
      }
      Assert.False(expectedNext.MoveNext(), "Not enough tokens");
    }

    protected void Fails(string testStr)
    {
      var lexer = new Lexer(testStr);
      Assert.Throws(typeof(LexerException), () =>
          {
            foreach (var token in lexer.Process())
            {
            }
          });
    }
  }
}
