using System;
using System.Runtime.CompilerServices;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Lexer
{
  public ref struct Lexer
  {
    private const char Eof = (char)0;

    private const string BooleanTrueToken = "true";
    private const string BooleanFalseToken = "false";

    private ReadOnlyMemory<char> _buffer;
    private ReadOnlySpan<char> _spanBuffer;
    private int _start;
    private int _current;
    private int _startLineNumber;
    private int _currentLineNumber;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIdentifierStart(char c)
    {
      return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsExponent(char c)
    {
      return c == 'e' || c == 'E';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsNumber(char c)
    {
      return c >= '0' && c <= '9';
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIdentifierBody(char c)
    {
      return IsIdentifierStart(c) || IsNumber(c);
    }

    private bool Backup()
    {
      if (_current <= _start) return false;
      --_current;
      if (_current < _spanBuffer.Length && _spanBuffer[_current] == '\n') --_currentLineNumber;
      return true;
    }

    private char Next()
    {
      var res = _current >= _spanBuffer.Length ? Eof : _spanBuffer[_current];
      if (res == '\n') ++_currentLineNumber;
      ++_current;
      return res;
    }

    private bool AcceptStringStart()
    {
      if (Next() == '"')
      {
        return true;
      }

      Backup();
      return false;
    }

    private bool AcceptIdentifierStart()
    {
      if (IsIdentifierStart(Next()))
      {
        return true;
      }

      Backup();
      return false;
    }

    private bool AcceptExponent()
    {
      if (IsExponent(Next()))
      {
        return true;
      }

      Backup();
      return false;
    }

    private bool Accept(char c)
    {
      if (c == Next())
      {
        return true;
      }

      Backup();
      return false;
    }

    private bool AcceptRunStringBody()
    {
      var success = false;
      while (true)
      {
	      var next = Next();
	      if (next == '"')
	      {
		      success = true;
		      break;
	      }

	      if (next == Eof)
	      {
		      break;
	      }
      }
      Next();

      Backup();
      return success;
    }

    private bool AcceptRunIdentifierBody()
    {
      var success = false;
      while (IsIdentifierBody(Next()))
      {
        success = true;
      }

      Backup();
      return success;
    }

    private bool AcceptRunNumbers()
    {
      var success = false;
      while (IsNumber(Next()))
      {
        success = true;
      }

      Backup();
      return success;
    }

    private void Ignore()
    {
      _start = _current;
    }

    private void SkipWhiteSpaces()
    {
      while (char.IsWhiteSpace(Next()))
      {
      }

      Backup();
      Ignore();
    }

    private ReadOnlySpan<char> CurrentToken => _spanBuffer.Slice(_start, _current - _start).Trim();

    private ReadOnlyMemory<char> OutputToken
    {
      get
      {
        // We use ReadOnlySpan to read and trim the token
        var tokenSpan = _spanBuffer.Slice(_start, _current - _start);
        var idx = 0;
        while (idx < tokenSpan.Length && char.IsWhiteSpace(tokenSpan[idx]))
        {
          ++idx;
        }

        tokenSpan = tokenSpan[idx..];

        // ReadOnlyMemory of equivalent range from tokenSpan above
        var result = _buffer.Slice(_start + idx, tokenSpan.Length);

        idx = tokenSpan.Length - 1;
        while (idx >= 0 && char.IsWhiteSpace(tokenSpan[idx]))
        {
          --idx;
        }

        return result[..(idx + 1)];
      }
    }

    private Token Emit(TokenType tokenType)
    {
      var token = new Token(OutputToken, tokenType, _startLineNumber);
      _start = _current;
      _startLineNumber = _currentLineNumber;
      return token;
    }

    public void Execute(ReadOnlyMemory<char> content, ITokenReader output)
    {
      _buffer = content;
      _spanBuffer = content.Span;
      _start = 0;
      _current = 0;
      _startLineNumber = 1;
      _currentLineNumber = 1;

      while (true)
      {
        SkipWhiteSpaces();

        if (AcceptStringStart())
        {
	        if (!AcceptRunStringBody())
	        {
		        throw new LexerException(_startLineNumber, "String literal does not have a closing double quotation mark");
	        }
	        output.Push(Emit(TokenType.String));
          continue;
        }

        if (AcceptIdentifierStart())
        {
          AcceptRunIdentifierBody();
          var token = CurrentToken;
          if (token.Equals(BooleanTrueToken.AsSpan(), StringComparison.Ordinal))
          {
            output.Push(Emit(TokenType.BooleanLiteralTrue));
          }
          else if (token.Equals(BooleanFalseToken.AsSpan(), StringComparison.Ordinal))
          {
            output.Push(Emit(TokenType.BooleanLiteralFalse));
          }
          else
          {
            output.Push(Emit(TokenType.Identifier));
          }

          continue;
        }

        if (AcceptRunNumbers())
        {
          if (Accept('.'))
          {
            // float
            AcceptRunNumbers();
            if (AcceptExponent())
            {
              // Accept + or -
              if (!Accept('+'))
              {
                Accept('-');
              }

              // Integers lead by + or - sign
              if (!AcceptRunNumbers())
              {
                throw new LexerException(_startLineNumber, "Integer value is required for float exponent value");
              }
            }

            output.Push(Emit(TokenType.Float));
          }
          else
          {
            output.Push(Emit(TokenType.Integer));
          }

          continue;
        }

        var nextChar = Next();
        switch (nextChar)
        {
          case Eof:
            output.End();
            return;
          case '(':
            output.Push(Emit(TokenType.LeftParenthesis));
            break;
          case ')':
            output.Push(Emit(TokenType.RightParenthesis));
            break;
          case '=':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                output.Push(Emit(TokenType.ComparisonEqual));
                break;
              default:
                Backup();
                output.Push(Emit(TokenType.AssignmentOperator));
                break;
            }

            break;
          case '<':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                output.Push(Emit(TokenType.ComparisonLessThanOrEqualTo));
                break;
              default:
                Backup();
                output.Push(Emit(TokenType.ComparisonLessThan));
                break;
            }

            break;
          case '>':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                output.Push(Emit(TokenType.ComparisonGreaterThanOrEqualTo));
                break;
              default:
                Backup();
                output.Push(Emit(TokenType.ComparisonGreaterThan));
                break;
            }

            break;
          case '!':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                output.Push(Emit(TokenType.ComparisonNotEqual));
                break;
              default:
                Backup();
                output.Push(Emit(TokenType.NegateOperator));
                break;
            }

            break;
          case '|':
            nextChar = Next();
            if (nextChar == '|')
            {
              output.Push(Emit(TokenType.BooleanOrOperator));
            }
            else
            {
              throw new LexerException(_startLineNumber,
                "OR boolean operand requires two vertical bar characters (||)");
            }

            break;
          case '&':
            nextChar = Next();
            if (nextChar == '&')
            {
              output.Push(Emit(TokenType.BooleanAndOperator));
            }
            else
            {
              throw new LexerException(_startLineNumber, "AND boolean operand requires two ampersand characters (&&)");
            }

            break;
          case '+':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                output.Push(Emit(TokenType.AssignmentAddOperator));
                break;
              default:
                Backup();
                output.Push(Emit(TokenType.PlusSign));
                break;
            }

            break;
          case '-':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                output.Push(Emit(TokenType.AssignmentSubtractOperator));
                break;
              default:
                Backup();
                output.Push(Emit(TokenType.MinusSign));
                break;
            }

            break;
          case '*':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                output.Push(Emit(TokenType.AssignmentMultiplyOperator));
                break;
              default:
                Backup();
                output.Push(Emit(TokenType.MultiplyOperator));
                break;
            }

            break;
          case '/':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                output.Push(Emit(TokenType.AssignmentDivideOperator));
                break;
              default:
                Backup();
                output.Push(Emit(TokenType.DivideOperator));
                break;
            }

            break;
          case '%':
            output.Push(Emit(TokenType.ModuloOperator));
            break;
          case ',':
            output.Push(Emit(TokenType.Comma));
            break;
          case ';':
            output.Push(Emit(TokenType.Semicolon));
            break;
          default:
            throw new LexerException(_startLineNumber, $"Unexpected character found: {nextChar}");
        }
      }
    }
  }
}