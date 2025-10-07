using System.Collections.Generic;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure;

namespace EpsilonScript.Tests.Lexer
{
  public class Lexer_InvalidCharacter : LexerTestBase
  {
    [Theory]
    [MemberData(nameof(IncorrectData))]
    internal void Lexer_InvalidCharacter_AssertLexFails(string input)
    {
      AssertLexFails(input);
    }

    public static IEnumerable<object[]> IncorrectData
    {
      get
      {
        return new[]
        {
          new object[] { "{" },
          new object[] { "}" },
          new object[] { "[" },
          new object[] { "]" },
          new object[] { "~" },
          new object[] { "@" },
          new object[] { "#" },
          new object[] { "$" },
          new object[] { "^" },
          new object[] { ":" },
          new object[] { "'" },
          new object[] { "\"" },
          new object[] { "\\" },
          new object[] { "." },
          new object[] { "?" },
          new object[] { "突然の日本語" },
        };
      }
    }
  }
}