using System;
using System.Collections.Generic;
using EpsilonScript.Lexer;

namespace EpsilonScript.Parser
{
  public class TokenParser
  {
    private int _index;
    private IList<Token> _tokens;

    public List<Element> Elements { get; } = new List<Element>();

    private Token NextToken => _tokens[_index + 1];
    private bool IsNextTokenAvailable => _index + 1 < _tokens.Count;
    private bool IsEnd => _index >= _tokens.Count;
    private ElementType? PreviousElementType => Elements.Count > 0 ? Elements[^1].Type : (ElementType?) null;

    private void PushElement(Token token, ElementType type)
    {
      Elements.Add(new Element(token, type));
    }

    private void PushElementDirect(Token token)
    {
      var mappedType = token.Type switch
      {
        TokenType.None => ElementType.None,
        TokenType.LeftParenthesis => ElementType.LeftParenthesis,
        TokenType.RightParenthesis => ElementType.RightParenthesis,
        TokenType.Comma => ElementType.Comma,
        TokenType.Semicolon => ElementType.Semicolon,
        TokenType.ComparisonEqual => ElementType.ComparisonEqual,
        TokenType.ComparisonNotEqual => ElementType.ComparisonNotEqual,
        TokenType.ComparisonLessThan => ElementType.ComparisonLessThan,
        TokenType.ComparisonGreaterThan => ElementType.ComparisonGreaterThan,
        TokenType.ComparisonLessThanOrEqualTo => ElementType.ComparisonLessThanOrEqualTo,
        TokenType.ComparisonGreaterThanOrEqualTo => ElementType.ComparisonGreaterThanOrEqualTo,
        TokenType.NegateOperator => ElementType.NegateOperator,
        TokenType.BooleanOrOperator => ElementType.BooleanOrOperator,
        TokenType.BooleanAndOperator => ElementType.BooleanAndOperator,
        TokenType.BooleanLiteralTrue => ElementType.BooleanLiteralTrue,
        TokenType.BooleanLiteralFalse => ElementType.BooleanLiteralFalse,
        TokenType.AssignmentOperator => ElementType.AssignmentOperator,
        TokenType.AssignmentAddOperator => ElementType.AssignmentAddOperator,
        TokenType.AssignmentSubtractOperator => ElementType.AssignmentSubtractOperator,
        TokenType.AssignmentMultiplyOperator => ElementType.AssignmentMultiplyOperator,
        TokenType.AssignmentDivideOperator => ElementType.AssignmentDivideOperator,
        TokenType.PlusSign => ElementType.AddOperator,
        TokenType.MultiplyOperator => ElementType.MultiplyOperator,
        TokenType.DivideOperator => ElementType.DivideOperator,
        TokenType.Integer => ElementType.Integer,
        TokenType.Float => ElementType.Float,
        _ => throw new ArgumentOutOfRangeException(nameof(token.Type), token.Type,
          "No direct token to element type map available")
      };
      PushElement(token, mappedType);
    }

    private bool IsCurrentTokenSignOperator
    {
      get
      {
        // Next token must exist for a sign operator to work on
        if (!IsNextTokenAvailable) return false;
        return PreviousElementType switch
        {
          null => true, // Start of an expression
          ElementType.LeftParenthesis => true, // Inside of parenthesis start
          ElementType.RightParenthesis => false, // Right after an end of parenthesis
          _ => PreviousElementType.Value.IsOperator() // Cannot subtract after an operator
        };
      }
    }

    private bool IsCurrentTokenFunction => IsNextTokenAvailable && NextToken.Type == TokenType.LeftParenthesis;

    public void Parse(IList<Token> tokens)
    {
      _tokens = tokens;

      while (!IsEnd)
      {
        var currentToken = _tokens[_index];
        switch (currentToken.Type)
        {
          case TokenType.Identifier:
            // Detect whether the current token is a function or a variable
            PushElement(currentToken, IsCurrentTokenFunction ? ElementType.Function : ElementType.Variable);
            break;
          case TokenType.MinusSign:
            // Detect whether the current token is a negative operator or a subtract operator
            PushElement(currentToken,
              IsCurrentTokenSignOperator ? ElementType.NegativeOperator : ElementType.SubtractOperator);
            break;
          case TokenType.PlusSign:
            // Detect whether the current token is a positive operator or a add operator
            PushElement(currentToken,
              IsCurrentTokenSignOperator ? ElementType.PositiveOperator : ElementType.AddOperator);
            break;
          case TokenType.LeftParenthesis:
            if (PreviousElementType == ElementType.Function)
            {
              PushElement(currentToken, ElementType.FunctionStartParenthesis);
            }
            else
            {
              PushElementDirect(currentToken);
            }

            break;
          case TokenType.RightParenthesis:
            if (PreviousElementType == ElementType.FunctionStartParenthesis)
            {
              // Insert null element because parenthesis was opened but closed immediately for a function call.
              // This is needed so a function element has something to build its parameter list on even if no parameters
              // were specified.
              PushElement(new Token("", TokenType.None, currentToken.LineNumber, currentToken.Position),
                ElementType.None);
            }

            PushElementDirect(currentToken);
            break;
          default:
            // All other token types can be directly mapped to a corresponding element type
            PushElementDirect(currentToken);
            break;
        }

        ++_index;
      }
    }
  }
}