using EpsilonScript.Intermediate;
using EpsilonScript.Tests.TestInfrastructure;
using System;
using Xunit;

namespace EpsilonScript.Tests.Parser
{
  /// <summary>
  /// Tests for malformed input and error recovery in the parser
  /// Ensures the parser fails gracefully with informative errors
  /// </summary>
  [Trait("Category", "Unit")]
  [Trait("Component", "Parser")]
  [Trait("Priority", "Medium")]
  public class ParserMalformedInputTests : TokenParserTestBase
  {
    #region Trailing Operators

    [Theory]
    [InlineData("1 +")]
    [InlineData("1 -")]
    [InlineData("1 *")]
    [InlineData("1 /")]
    [InlineData("1 %")]
    public void Parser_TrailingArithmeticOperator_Fails(string input)
    {
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("true &&")]
    [InlineData("false ||")]
    public void Parser_TrailingLogicalOperator_Fails(string input)
    {
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("x =")]
    [InlineData("y +=")]
    [InlineData("z -=")]
    [InlineData("a *=")]
    [InlineData("b /=")]
    public void Parser_TrailingAssignmentOperator_Fails(string input)
    {
      // These should fail because assignment has no right-hand side
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("1 <")]
    [InlineData("2 >")]
    [InlineData("3 ==")]
    [InlineData("4 !=")]
    [InlineData("5 <=")]
    [InlineData("6 >=")]
    public void Parser_TrailingComparisonOperator_Fails(string input)
    {
      AssertParseFails(input);
    }

    #endregion

    #region Leading Operators

    [Theory]
    [InlineData("* 1")]
    [InlineData("/ 2")]
    [InlineData("% 3")]
    public void Parser_LeadingMultiplicativeOperator_Fails(string input)
    {
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("&& true")]
    [InlineData("|| false")]
    public void Parser_LeadingLogicalOperator_Fails(string input)
    {
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("== 5")]
    [InlineData("!= 10")]
    [InlineData("< 20")]
    [InlineData("> 30")]
    [InlineData("<= 40")]
    [InlineData(">= 50")]
    public void Parser_LeadingComparisonOperator_Fails(string input)
    {
      AssertParseFails(input);
    }

    #endregion

    #region Consecutive Operators

    [Theory]
    [InlineData("5 ** 6")]
    [InlineData("7 // 8")]
    public void Parser_DoubleMultiplicativeOperators_Fail(string input)
    {
      // These should fail - "**" and "//" are invalid because * and / cannot be unary operators
      // Note: "1 ++ 2" and "3 -- 4" are actually valid - they parse as "1 + (+2)" and "3 - (-4)"
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("5 *** 6")]
    public void Parser_TripleOperators_Fail(string input)
    {
      // This should fail - "***" is invalid because * cannot be unary
      // Note: "1 +++ 2" and "3 --- 4" are actually valid - they parse as "1 + (+(+2))" and "3 - (-(-4))"
      AssertParseFails(input);
    }

    [Fact]
    public void Parser_MixedOperators_ParseAsUnary()
    {
      // These parse successfully because the second operator is unary:
      // "1 +- 2" parses as "1 + (-2)" - binary add followed by unary minus
      // "5 *+ 6" parses as "5 * (+6)" - binary multiply followed by unary plus
      // This is valid per spec (unary + and - operators)
      //
      // Note: "5 +* 6" would fail because * cannot be unary (tested in DoubleMultiplicativeOperators_Fail)

      // Test "1 +- 2" -> should parse as: 1 (Integer) + (AddOperator) - (NegativeOperator) 2 (Integer)
      // The key verification: the minus should be classified as NegativeOperator (unary), not SubtractOperator (binary)
      var elements = ParseString("1 +- 2");
      Assert.Equal(4, elements.Count);
      Assert.Equal(ElementType.Integer, elements[0].Type);
      Assert.Equal(ElementType.AddOperator, elements[1].Type); // Binary add
      Assert.Equal(ElementType.NegativeOperator, elements[2].Type); // Unary minus - this is the critical assertion
      Assert.Equal(ElementType.Integer, elements[3].Type);

      // Similarly verify other cases parse successfully
      AssertParseSucceeds("3 -+ 4");
      AssertParseSucceeds("5 *+ 6");
      AssertParseSucceeds("7 /- 8");
    }

    [Theory]
    [InlineData("1 &&& true")]
    [InlineData("false ||| true")]
    public void Parser_TripleLogicalOperators_Fail(string input)
    {
      // Will fail at lexer level - single & or | is invalid
      Assert.ThrowsAny<Exception>(() => ParseString(input));
    }

    #endregion

    #region Missing Operands

    [Theory]
    [InlineData("+ + 1")]
    [InlineData("- - - 2")]
    [InlineData("! ! ! true")]
    public void Parser_MultipleUnaryOperators_Work(string input)
    {
      // Multiple unary operators are valid
      AssertParseSucceeds(input);
    }

    [Fact]
    public void Parser_EmptyParentheses_Fail()
    {
      // Empty parentheses should fail - no expression inside
      AssertParseFails("()");
      AssertParseFails("(())");
      AssertParseFails("((()))");
      AssertParseFails("1 + ()");
      AssertParseFails("() + 1");
    }

    [Fact]
    public void Parser_JustOperator_Fails()
    {
      AssertParseFails("+");
      AssertParseFails("-");
      AssertParseFails("*");
      AssertParseFails("/");
      AssertParseFails("!");
    }

    #endregion

    #region Mismatched Parentheses

    [Theory]
    [InlineData("(1 + 2")]
    [InlineData("((1 + 2)")]
    [InlineData("(((1 + 2")]
    [InlineData("(1 + (2 * 3)")]
    public void Parser_UnclosedParenthesis_Fails(string input)
    {
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("1 + 2)")]
    [InlineData("1 + 2))")]
    [InlineData("((1 + 2)))")]
    public void Parser_ExtraClosingParenthesis_Fails(string input)
    {
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("(1 + 2)(3 + 4)")]
    [InlineData("(5)(6)")]
    public void Parser_AdjacentParentheses_NoOperator_Fails(string input)
    {
      // Two expressions side by side without operator should fail
      AssertParseFails(input);
    }

    [Fact]
    public void Parser_FunctionChaining_Fails()
    {
      // func()() is invalid - EpsilonScript doesn't support function returning function
      AssertParseFails("func()()");
    }

    #endregion

    #region Invalid Function Syntax

    [Fact]
    public void Parser_FunctionWithTrailingComma_Fails()
    {
      AssertParseFails("func(1, 2, 3,)");
    }

    [Fact]
    public void Parser_FunctionWithLeadingComma_Fails()
    {
      AssertParseFails("func(, 1, 2)");
    }

    [Fact]
    public void Parser_FunctionWithDoubleComma_Fails()
    {
      AssertParseFails("func(1,, 2)");
    }

    [Fact]
    public void Parser_FunctionWithOnlyCommas_Fails()
    {
      AssertParseFails("func(,,,)");
    }

    [Fact]
    public void Parser_FunctionUnclosed_Fails()
    {
      AssertParseFails("func(1, 2, 3");
    }

    [Fact]
    public void Parser_FunctionExtraClosing_Fails()
    {
      AssertParseFails("func(1, 2))");
    }

    #endregion

    #region Invalid Sequences

    [Theory]
    [InlineData("1 2")]
    [InlineData("1 2 3")]
    [InlineData("true false")]
    [InlineData("42 99")]
    public void Parser_AdjacentLiterals_NoOperator_Fails(string input)
    {
      // Two literals side by side without operator should fail
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("x y")]
    [InlineData("var1 var2 var3")]
    public void Parser_AdjacentVariables_NoOperator_Fails(string input)
    {
      // Adjacent identifiers without operator should fail
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("42 func()")]
    [InlineData("x func()")]
    [InlineData("true func()")]
    public void Parser_ValueFollowedByFunction_NoOperator_Fails(string input)
    {
      // Value followed by function call without operator should fail
      AssertParseFails(input);
    }

    [Theory]
    [InlineData("func() 42")]
    [InlineData("func() x")]
    [InlineData("func() true")]
    [InlineData("func() \"test\"")]
    public void Parser_FunctionFollowedByValue_NoOperator_Fails(string input)
    {
      // Function call followed by value without operator should fail
      AssertParseFails(input);
    }

    #endregion

    #region Comma Edge Cases

    [Fact]
    public void Parser_LeadingComma_Fails()
    {
      AssertParseFails(", 1");
    }

    [Fact]
    public void Parser_TrailingComma_InExpression_Fails()
    {
      AssertParseFails("1 ,");
    }

    [Fact]
    public void Parser_DoubleComma_OutsideFunction_Fails()
    {
      AssertParseFails("1 ,, 2");
    }

    #endregion

    #region Assignment Edge Cases

    [Fact]
    public void Parser_DoubleAssignment_Invalid()
    {
      // "x = = 5" should fail
      AssertParseFails("x = = 5");
    }

    [Fact]
    public void Parser_Assignment_ToLiteral_Fails()
    {
      // Assignment to literals should fail at parse time
      AssertParseFails("1 = 2");
    }

    [Fact]
    public void Parser_Assignment_ToExpression_Fails()
    {
      // Assignment to expressions should fail at parse time
      AssertParseFails("(x + 1) = 2");
    }

    [Fact]
    public void Parser_Assignment_ToFunctionResult_Fails()
    {
      // Assignment to function call results should fail at parse time
      // The previous element is RightParenthesis, not Variable
      AssertParseFails("func() = 5");
    }

    #endregion

    #region Stress Tests

    [Fact]
    public void Parser_DeeplyNestedParentheses_Succeeds()
    {
      // Test 50 levels of nesting to ensure parser handles deep recursion
      var depth = 50;
      var input = new string('(', depth) + "1" + new string(')', depth);

      // Should parse successfully without stack overflow
      AssertParseSucceeds(input);
    }

    [Fact]
    public void Parser_DeeplyNestedParentheses_UnclosedAtEnd_Fails()
    {
      // Test 30 levels with one missing closing paren
      var depth = 30;
      var input = new string('(', depth) + "1" + new string(')', depth - 1);

      // Should fail with unclosed parenthesis error
      AssertParseFails(input);
    }

    [Fact]
    public void Parser_DeeplyNestedFunctionCalls_Succeeds()
    {
      // Test deeply nested function calls: func(func(func(...)))
      var depth = 30;
      var openings = string.Concat(System.Linq.Enumerable.Repeat("func(", depth));
      var closings = new string(')', depth);
      var input = openings + "1" + closings;

      // Should parse successfully
      AssertParseSucceeds(input);
    }

    [Fact]
    public void Parser_VeryLongExpression_Succeeds()
    {
      // Test expression with many operators: 1 + 1 + 1 + ... (100 terms)
      var terms = 100;
      var input = string.Join(" + ", System.Linq.Enumerable.Repeat("1", terms));

      // Should parse successfully
      AssertParseSucceeds(input);
    }

    #endregion

    #region Helper Methods

    private void AssertParseFails(string input)
    {
      Assert.ThrowsAny<Exception>(() => ParseString(input));
    }

    private void AssertParseSucceeds(string input)
    {
      // Parse should complete without exceptions
      var elements = ParseString(input);

      // Basic sanity checks
      Assert.NotNull(elements);
      Assert.NotEmpty(elements);

      // Verify elements have valid types (not None, except for empty function args)
      foreach (var element in elements)
      {
        // ElementType.None is allowed to have TokenType.None (for empty function args)
        // All other element types must have a valid token type
        if (element.Type == ElementType.None)
        {
          Assert.Equal(TokenType.None, element.Token.Type);
        }
        else
        {
          Assert.NotEqual(TokenType.None, element.Token.Type);
        }
      }
    }

    #endregion
  }
}