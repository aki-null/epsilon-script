using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Parser
{
  /// <summary>
  /// Type of parenthesis context - used to track whether parentheses are
  /// for grouping expressions or function calls.
  /// </summary>
  internal enum ParenthesisType
  {
    /// <summary>
    /// Regular grouping parenthesis for expressions like (1 + 2)
    /// </summary>
    Grouping,

    /// <summary>
    /// Function call parenthesis like func(args)
    /// </summary>
    Function
  }

  /// <summary>
  /// Parses tokens into elements while validating syntax rules.
  ///
  /// Architecture Note:
  /// The parser converts tokens to elements while preserving infix order.
  /// - Input: tokens in infix notation (e.g., "2 * 3" as [2, *, 3])
  /// - Output: elements in infix notation (same order, validated and categorized)
  /// - Precedence-based evaluation happens in a later compilation phase, not in the parser.
  ///
  /// Error Handling Policy:
  /// - ParserException: Thrown for invalid user input (syntax errors) - see ValidationEngine
  /// - ArgumentException: Thrown for programming errors (null tokens, invalid state)
  ///
  /// This distinction ensures that syntax errors are user-recoverable while
  /// programming errors fail fast during development.
  /// </summary>
  internal class TokenParser : ITokenReader
  {
    private enum State
    {
      Init = 0,
      Process,
      End
    }

    private State _state;

    private bool IsNextTokenAvailable => _state == State.Process;

    private Token _currentToken;
    private Token _nextToken;

    private ElementType? _previousElementType;
    private Token? _previousToken;
    private int _parenthesisDepth;

    // Stack to track the type of each opening parenthesis (grouping vs function)
    private readonly Stack<ParenthesisType> _parenthesisTypeStack;

    private readonly IElementReader _output;

    public TokenParser(IElementReader output)
    {
      _output = output;
      _parenthesisTypeStack = new Stack<ParenthesisType>();
    }

    public void Reset()
    {
      _state = State.Init;
      _currentToken = new Token();
      _nextToken = new Token();
      _previousElementType = null;
      _previousToken = null;
      _parenthesisDepth = 0;
      _parenthesisTypeStack.Clear();
    }

    private void PushElement(Token token, ElementType type)
    {
      if (token.Type == TokenType.None && type != ElementType.None)
      {
        throw new ArgumentException("Token type cannot be None when element type is not None", nameof(token));
      }

      // Order of operations ensures state consistency:
      // 1. Validate: Throws ParserException if syntax is invalid
      // 2. Push to output: Throws if output fails
      // 3. Update parser state: Only happens if validation and push succeed
      // This ordering ensures parser state is never inconsistent with validated output
      Validator.Validate(
        currentToken: token,
        currentElementType: type,
        previousToken: _previousToken,
        previousElementType: _previousElementType,
        parenthesisDepth: _parenthesisDepth,
        parenthesisTypeStack: _parenthesisTypeStack
      );

      _output.Push(new Element(token, type));

      UpdateState(token, type);
    }

    /// <summary>
    /// Updates parser state after validation passes
    /// </summary>
    private void UpdateState(Token token, ElementType type)
    {
      switch (type)
      {
        // Track parenthesis depth and type
        case ElementType.LeftParenthesis:
          _parenthesisDepth++;
          _parenthesisTypeStack.Push(ParenthesisType.Grouping);
          break;
        case ElementType.FunctionStartParenthesis:
          _parenthesisDepth++;
          _parenthesisTypeStack.Push(ParenthesisType.Function);
          break;
        case ElementType.RightParenthesis:
        {
          _parenthesisDepth--;
          // Safe to pop without checking count: ValidationEngine ensures parenthesisDepth > 0
          // before this method is called, guaranteeing the stack has a matching entry
          _parenthesisTypeStack.Pop();
          break;
        }
      }

      // Update previous element tracking
      _previousElementType = type;
      _previousToken = token;
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
        case TokenType.AssignmentModuloOperator:
          mappedType = ElementType.AssignmentModuloOperator;
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
        case TokenType.String:
          mappedType = ElementType.String;
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
        switch (_previousElementType)
        {
          case null:
          case ElementType.LeftParenthesis:
          case ElementType.FunctionStartParenthesis: // Function calls also need unary operators
          case ElementType.Comma: // After comma in function arguments, allow unary operators
            return true;
          case ElementType.RightParenthesis:
            return false;
          default:
            return _previousElementType.Value.IsOperator();
        }
      }
    }

    private bool IsCurrentTokenFunction => IsNextTokenAvailable && _nextToken.Type == TokenType.LeftParenthesis;

    private void Process()
    {
      if (_state == State.Init) return;

      switch (_currentToken.Type)
      {
        case TokenType.Identifier:
          // Detect whether the current token is a function or a variable
          var elementType = IsCurrentTokenFunction ? ElementType.Function : ElementType.Variable;
          PushElement(_currentToken, elementType);
          break;
        case TokenType.MinusSign:
          // Detect whether the current token is a negative operator or a subtract operator
          PushElement(_currentToken,
            IsCurrentTokenSignOperator ? ElementType.NegativeOperator : ElementType.SubtractOperator);
          break;
        case TokenType.PlusSign:
          // Detect whether the current token is a positive operator or a add operator
          var plusElementType = ElementType.AddOperator;
          if (IsCurrentTokenSignOperator)
          {
            plusElementType = ElementType.PositiveOperator;
          }

          PushElement(_currentToken, plusElementType);
          break;
        case TokenType.LeftParenthesis:
          if (_previousElementType == ElementType.Function)
          {
            PushElement(_currentToken, ElementType.FunctionStartParenthesis);
          }
          else
          {
            PushElementDirect(_currentToken);
          }

          break;
        case TokenType.RightParenthesis:
          if (_previousElementType == ElementType.FunctionStartParenthesis)
          {
            // Insert null element because parenthesis was opened but closed immediately for a function call.
            // This is needed so a function element has something to build its parameter list on even if no parameters
            // were specified.
            // Note: _currentToken is guaranteed valid here because FunctionStartParenthesis can only be set
            // after processing a valid function token, so _currentToken.Location is safe to use
            PushElement(new Token(ReadOnlyMemory<char>.Empty, TokenType.None, _currentToken.Location),
              ElementType.None);
          }

          PushElementDirect(_currentToken);
          break;
        default:
          // All other token types can be directly mapped to a corresponding element type
          PushElementDirect(_currentToken);
          break;
      }
    }

    public void Push(Token token)
    {
      if (token.Type == TokenType.None)
      {
        throw new ArgumentException("Cannot push empty token (TokenType.None)", nameof(token));
      }

      _currentToken = _nextToken;
      _nextToken = token;
      Process();
      _state = State.Process;
    }

    public void End()
    {
      _currentToken = _nextToken;
      _state = State.End;
      Process();

      Validator.ValidateExpressionEnd(_previousElementType, _currentToken, _parenthesisDepth);

      _output.End();
    }
  }
}