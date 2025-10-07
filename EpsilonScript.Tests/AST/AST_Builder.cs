using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Intermediate;
using EpsilonScript.Tests.TestInfrastructure;
using Xunit;

namespace EpsilonScript.Tests.AST
{
  public class AST_Builder
  {
    [Theory]
    [MemberData(nameof(SuccessData))]
    internal void AST_Build_Succeeds(List<Element> elements, ExtendedType expectedType)
    {
      var builder = new AstBuilder(null);
      builder.Configure(Compiler.Options.Immutable, null, Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float);
      foreach (var element in elements)
      {
        builder.Push(element);
      }

      builder.End();
      var result = builder.Result;
      Assert.NotNull(result);

      // Execute the node to determine its value type
      result.Execute(null);
      Assert.Equal(expectedType, result.ValueType);
    }

    [Theory]
    [MemberData(nameof(FailData))]
    internal void AST_Build_Fails(List<Element> elements)
    {
      Assert.Throws<RuntimeException>(() =>
      {
        var builder = new AstBuilder(null);
        builder.Configure(Compiler.Options.Immutable, null, Compiler.IntegerPrecision.Integer,
          Compiler.FloatPrecision.Float);
        foreach (var element in elements)
        {
          builder.Push(element);
        }

        builder.End();
      });
    }

    public static IEnumerable<object[]> SuccessData
    {
      get
      {
        return new[]
        {
          // Simple integer literal
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("42", TokenType.Integer), ElementType.Integer)
            },
            ExtendedType.Integer
          },
          // Simple float literal
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("3.14", TokenType.Float), ElementType.Float)
            },
            ExtendedType.Float
          },
          // Simple boolean literal
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue)
            },
            ExtendedType.Boolean
          },
          // Simple string literal
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("\"hello\"", TokenType.String), ElementType.String)
            },
            ExtendedType.String
          },
          // Simple arithmetic: 2 + 3 (in RPN: 2 3 +)
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("2", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator)
            },
            ExtendedType.Integer
          },
          // Mixed arithmetic: 2.5 * 4 (in RPN: 2.5 4 *)
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("2.5", TokenType.Float), ElementType.Float),
              new Element(new Token("4", TokenType.Integer), ElementType.Integer),
              new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator)
            },
            ExtendedType.Float
          },
          // Comparison: 5 > 3 (in RPN: 5 3 >)
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("5", TokenType.Integer), ElementType.Integer),
              new Element(new Token("3", TokenType.Integer), ElementType.Integer),
              new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan)
            },
            ExtendedType.Boolean
          },
          // Boolean operation: true && false (in RPN: true false &&)
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),
              new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator)
            },
            ExtendedType.Boolean
          },
          // Unary negation: !true (in RPN: true !)
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
              new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator)
            },
            ExtendedType.Boolean
          },
          // String concatenation: "hello" + "world" (in RPN: "hello" "world" +)
          new object[]
          {
            new List<Element>
            {
              new Element(new Token("\"hello\"", TokenType.String), ElementType.String),
              new Element(new Token("\"world\"", TokenType.String), ElementType.String),
              new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator)
            },
            ExtendedType.String
          }
        };
      }
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