using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.Lexer;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.TestInfrastructure
{
  public abstract class LexerTestBase
  {
    protected static void AssertLexSucceeds(string source, IReadOnlyList<Token> expected)
    {
      var lexer = new EpsilonScript.Lexer.Lexer();
      var tokenReader = new TestTokenReader();
      using var enumerator = expected.GetEnumerator();
      lexer.Execute(source.AsMemory(), tokenReader);
      foreach (var token in tokenReader.Tokens)
      {
        Assert.True(enumerator.MoveNext(), "Too many tokens produced by lexer");
        Assert.Equal(enumerator.Current.Type, token.Type);
        Assert.Equal(enumerator.Current.Text.ToString(), token.Text.ToString());
      }

      Assert.False(enumerator.MoveNext(), "Not enough tokens produced by lexer");
      Assert.True(tokenReader.EndCalled, "Token reader not closed");
    }

    protected static void AssertLexFails(string source)
    {
      Assert.Throws<LexerException>(() =>
      {
        var lexer = new global::EpsilonScript.Lexer.Lexer();
        var tokenReader = new TestTokenReader();
        lexer.Execute(source.AsMemory(), tokenReader);
      });
    }
  }
}
