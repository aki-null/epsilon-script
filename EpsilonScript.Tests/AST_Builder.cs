using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using EpsilonScript.Parser;
using Xunit;

namespace EpsilonScript.Tests
{
  public class AST_Builder
  {
    [Theory]
    [MemberData(nameof(FailData))]
    public void AST_Build_Fails(List<Element> elements)
    {
      Assert.Throws<RuntimeException>(() =>
      {
        var builder = new AstBuilder(null);
        builder.Configure(Compiler.Options.Immutable, null);
        foreach (var element in elements)
        {
          builder.Push(element);
        }

        builder.End();
      });
    }

    public static IEnumerable<object[]> FailData
    {
      get
      {
        return new[]
        {
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("1", TokenType.Integer), ElementType.Integer),
              new Element(new Token("1", TokenType.Integer), ElementType.Integer)
            }
          },
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("ifelse", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("count", TokenType.Identifier), ElementType.Variable),
              new Element(new Token("0", TokenType.Integer), ElementType.Integer),
              new Element(new Token("100", TokenType.Integer), ElementType.Integer),
              new Element(new Token("200", TokenType.Integer), ElementType.Integer),
              new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
            }
          },
        };
      }
    }
  }
}