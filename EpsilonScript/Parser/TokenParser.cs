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

    private ElementType? PreviousElementType =>
      Elements.Count > 0 ? Elements[Elements.Count - 1].Type : (ElementType?) null;

    private void PushElement(Token token, ElementType type)
    {
      Elements.Add(new Element(token, type));
    }

    private void PushElementDirect(Token token)
    {
      ElementType mappedType;
      switch (token.Type)
      {
        case TokenType.None:
          mappedType = ElementType.None;
          break;
        case TokenType.LeftParenthesis:
          mappedType = ElementType.LeftParenthesis;
          break;
        case TokenType.RightParenthesis:
          mappedType = ElementType.RightParenthesis;
          break;
        case TokenType.Comma:
          mappedType = ElementType.Comma;
          break;
        case TokenType.Semicolon:
          mappedType = ElementType.Semicolon;
          break;
        case TokenType.ComparisonEqual:
          mappedType = ElementType.ComparisonEqual;
          break;
        case TokenType.ComparisonNotEqual:
          mappedType = ElementType.ComparisonNotEqual;
          break;
        case TokenType.ComparisonLessThan:
          mappedType = ElementType.ComparisonLessThan;
          break;
        case TokenType.ComparisonGreaterThan:
          mappedType = ElementType.ComparisonGreaterThan;
          break;
        case TokenType.ComparisonLessThanOrEqualTo:
          mappedType = ElementType.ComparisonLessThanOrEqualTo;
          break;
        case TokenType.ComparisonGreaterThanOrEqualTo:
          mappedType = ElementType.ComparisonGreaterThanOrEqualTo;
          break;
        case TokenType.NegateOperator:
          mappedType = ElementType.NegateOperator;
          break;
        case TokenType.BooleanOrOperator:
          mappedType = ElementType.BooleanOrOperator;
          break;
        case TokenType.BooleanAndOperator:
          mappedType = ElementType.BooleanAndOperator;
          break;
        case TokenType.BooleanLiteralTrue:
          mappedType = ElementType.BooleanLiteralTrue;
          break;
        case TokenType.BooleanLiteralFalse:
          mappedType = ElementType.BooleanLiteralFalse;
          break;
        case TokenType.AssignmentOperator:
          mappedType = ElementType.AssignmentOperator;
          break;
        case TokenType.AssignmentAddOperator:
          mappedType = ElementType.AssignmentAddOperator;
          break;
        case TokenType.AssignmentSubtractOperator:
          mappedType = ElementType.AssignmentSubtractOperator;
          break;
        case TokenType.AssignmentMultiplyOperator:
          mappedType = ElementType.AssignmentMultiplyOperator;
          break;
        case TokenType.AssignmentDivideOperator:
          mappedType = ElementType.AssignmentDivideOperator;
          break;
        case TokenType.PlusSign:
          mappedType = ElementType.AddOperator;
          break;
        case TokenType.MultiplyOperator:
          mappedType = ElementType.MultiplyOperator;
          break;
        case TokenType.DivideOperator:
          mappedType = ElementType.DivideOperator;
          break;
        case TokenType.ModuloOperator:
          mappedType = ElementType.ModuloOperator;
          break;
        case TokenType.Integer:
          mappedType = ElementType.Integer;
          break;
        case TokenType.Float:
          mappedType = ElementType.Float;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(token.Type), token.Type,
            "No direct token to element type map available");
      }

      PushElement(token, mappedType);
    }

    private bool IsCurrentTokenSignOperator
    {
      get
      {
        // Next token must exist for a sign operator to work on
        if (!IsNextTokenAvailable) return false;
        switch (PreviousElementType)
        {
          case null:
          case ElementType.LeftParenthesis:
            return true;
          case ElementType.RightParenthesis:
            return false;
          default:
            return PreviousElementType.Value.IsOperator();
        }
      }
    }

    private bool IsCurrentTokenFunction => IsNextTokenAvailable && NextToken.Type == TokenType.LeftParenthesis;

    public void Parse(IList<Token> tokens)
    {
      _tokens = tokens;
      _index = 0;
      Elements.Clear();

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
              PushElement(new Token("", TokenType.None, currentToken.LineNumber), ElementType.None);
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