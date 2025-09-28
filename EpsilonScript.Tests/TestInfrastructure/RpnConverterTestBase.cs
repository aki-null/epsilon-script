using System.Collections.Generic;
using EpsilonScript.Intermediate;
using Xunit;
using EpsilonScript.Tests.TestInfrastructure.Fakes;

namespace EpsilonScript.Tests.TestInfrastructure
{
  public abstract class RpnConverterTestBase
  {
    protected static void AssertRpnSucceeds(IList<Element> input, IList<Element> expected)
    {
      var elementReader = new TestElementReader();
      var converter = new EpsilonScript.Parser.RpnConverter(elementReader);
      foreach (var element in input)
      {
        converter.Push(element);
      }

      converter.End();
      Assert.True(elementReader.EndCalled, "Element reader not closed");

      var output = elementReader.Elements;
      Assert.Equal(expected.Count, output.Count);
      for (var i = 0; i < output.Count; ++i)
      {
        // Element doesn't override Equals, so we need to compare fields manually
        Assert.Equal(expected[i].Type, output[i].Type);
        Assert.Equal(expected[i].Token.Type, output[i].Token.Type);
        Assert.Equal(expected[i].Token.Text.ToString(), output[i].Token.Text.ToString());
      }
    }

    protected static void AssertRpnFails(IList<Element> input)
    {
      var elementReader = new TestElementReader();
      var converter = new global::EpsilonScript.Parser.RpnConverter(elementReader);
      Assert.Throws<ParserException>(() =>
      {
        foreach (var element in input)
        {
          converter.Push(element);
        }

        converter.End();
      });
    }

    // Helper method to create test cases from string representations
    protected static object[] CreateTestCase(string infix, string rpn)
    {
      var inputTokens = ParseExpression(infix);
      var expectedTokens = ParseExpression(rpn);

      var input = new List<Element>();
      var expected = new List<Element>();

      foreach (var token in inputTokens)
      {
        input.Add(CreateElement(token));
      }

      foreach (var token in expectedTokens)
      {
        expected.Add(CreateElement(token));
      }

      return new object[] { input.ToArray(), expected.ToArray() };
    }

    // Parse expression into tokens
    private static List<string> ParseExpression(string expr)
    {
      var tokens = new List<string>();
      var i = 0;

      while (i < expr.Length)
      {
        if (char.IsWhiteSpace(expr[i]))
        {
          i++;
          continue;
        }

        if (expr[i] == '"')
        {
          // String literal
          var j = i + 1;
          while (j < expr.Length && expr[j] != '"')
            j++;
          tokens.Add(expr.Substring(i, j - i + 1));
          i = j + 1;
        }
        else if (char.IsLetter(expr[i]) || expr[i] == '_')
        {
          // Identifier or keyword
          var j = i;
          while (j < expr.Length && (char.IsLetterOrDigit(expr[j]) || expr[j] == '_'))
            j++;
          tokens.Add(expr.Substring(i, j - i));
          i = j;
        }
        else if (char.IsDigit(expr[i]) || (i + 1 < expr.Length && expr[i] == '-' && char.IsDigit(expr[i + 1])))
        {
          // Number (integer or float)
          var j = i;
          if (expr[i] == '-') j++;
          while (j < expr.Length && (char.IsDigit(expr[j]) || expr[j] == '.'))
            j++;
          tokens.Add(expr.Substring(i, j - i));
          i = j;
        }
        else if (i + 1 < expr.Length)
        {
          // Check for two-character operators
          var twoChar = expr.Substring(i, 2);
          if (twoChar == "+=" || twoChar == "-=" || twoChar == "*=" || twoChar == "/=" ||
              twoChar == "==" || twoChar == "!=" || twoChar == "<=" || twoChar == ">=" ||
              twoChar == "&&" || twoChar == "||")
          {
            tokens.Add(twoChar);
            i += 2;
          }
          else
          {
            // Single character operator
            tokens.Add(expr[i].ToString());
            i++;
          }
        }
        else
        {
          // Last character
          tokens.Add(expr[i].ToString());
          i++;
        }
      }

      return tokens;
    }

    // Create Element from token string
    private static Element CreateElement(string token)
    {
      return token switch
      {
        // Arithmetic operators
        "+" => new Element(new Token("+", TokenType.PlusSign), ElementType.AddOperator),
        "-" => new Element(new Token("-", TokenType.MinusSign), ElementType.SubtractOperator),
        "*" => new Element(new Token("*", TokenType.MultiplyOperator), ElementType.MultiplyOperator),
        "/" => new Element(new Token("/", TokenType.DivideOperator), ElementType.DivideOperator),
        "%" => new Element(new Token("%", TokenType.ModuloOperator), ElementType.ModuloOperator),

        // Assignment operators
        "=" => new Element(new Token("=", TokenType.AssignmentOperator), ElementType.AssignmentOperator),
        "+=" => new Element(new Token("+=", TokenType.AssignmentAddOperator), ElementType.AssignmentAddOperator),
        "-=" => new Element(new Token("-=", TokenType.AssignmentSubtractOperator),
          ElementType.AssignmentSubtractOperator),
        "*=" => new Element(new Token("*=", TokenType.AssignmentMultiplyOperator),
          ElementType.AssignmentMultiplyOperator),
        "/=" => new Element(new Token("/=", TokenType.AssignmentDivideOperator), ElementType.AssignmentDivideOperator),

        // Comparison operators
        "==" => new Element(new Token("==", TokenType.ComparisonEqual), ElementType.ComparisonEqual),
        "!=" => new Element(new Token("!=", TokenType.ComparisonNotEqual), ElementType.ComparisonNotEqual),
        "<" => new Element(new Token("<", TokenType.ComparisonLessThan), ElementType.ComparisonLessThan),
        ">" => new Element(new Token(">", TokenType.ComparisonGreaterThan), ElementType.ComparisonGreaterThan),
        "<=" => new Element(new Token("<=", TokenType.ComparisonLessThanOrEqualTo),
          ElementType.ComparisonLessThanOrEqualTo),
        ">=" => new Element(new Token(">=", TokenType.ComparisonGreaterThanOrEqualTo),
          ElementType.ComparisonGreaterThanOrEqualTo),

        // Boolean operators
        "&&" => new Element(new Token("&&", TokenType.BooleanAndOperator), ElementType.BooleanAndOperator),
        "||" => new Element(new Token("||", TokenType.BooleanOrOperator), ElementType.BooleanOrOperator),
        "!" => new Element(new Token("!", TokenType.NegateOperator), ElementType.NegateOperator),

        // Boolean literals
        "true" => new Element(new Token("true", TokenType.BooleanLiteralTrue), ElementType.BooleanLiteralTrue),
        "false" => new Element(new Token("false", TokenType.BooleanLiteralFalse), ElementType.BooleanLiteralFalse),

        // Delimiters
        "(" => new Element(new Token("(", TokenType.LeftParenthesis), ElementType.LeftParenthesis),
        ")" => new Element(new Token(")", TokenType.RightParenthesis), ElementType.RightParenthesis),
        "," => new Element(new Token(",", TokenType.Comma), ElementType.Comma),
        ";" => new Element(new Token(";", TokenType.Semicolon), ElementType.Semicolon),

        // Literals and identifiers
        _ when token.StartsWith("\"") && token.EndsWith("\"") =>
          new Element(new Token(token, TokenType.String), ElementType.String),
        _ when token.Contains(".") =>
          new Element(new Token(token, TokenType.Float), ElementType.Float),
        _ when int.TryParse(token, out _) =>
          new Element(new Token(token, TokenType.Integer), ElementType.Integer),
        _ => // Identifier or variable
          new Element(new Token(token, TokenType.Identifier), ElementType.Variable)
      };
    }
  }
}