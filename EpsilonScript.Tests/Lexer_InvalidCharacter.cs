using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests
{
  public class Lexer_InvalidCharacter : Lexer_Base
  {
    [Theory]
    [MemberData(nameof(IncorrectData))]
    public void Lexer_InvalidCharacter_Fails(string input)
    {
      Fails(input);
    }

    public static IEnumerable<object[]> IncorrectData
    {
      get
      {
        return new[]
        {
          new object[] {"{"},
          new object[] {"}"},
          new object[] {"["},
          new object[] {"]"},
          new object[] {"~"},
          new object[] {"@"},
          new object[] {"#"},
          new object[] {"$"},
          new object[] {"^"},
          new object[] {":"},
          new object[] {"'"},
          new object[] {"\""},
          new object[] {"\\"},
          new object[] {"."},
          new object[] {"?"},
          new object[] {"突然の日本語"},
        };
      }
    }
  }
}