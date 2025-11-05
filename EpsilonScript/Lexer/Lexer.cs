using System;
using System.Runtime.CompilerServices;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Lexer
{
  /// <summary>
  /// Tokenizes input strings into tokens for the parser.
  ///
  /// Error Handling Policy:
  /// - LexerException: Thrown for invalid user input (unexpected characters, malformed literals)
  /// - ArgumentException: Reserved for programming errors (null input, invalid state)
  ///
  /// This distinction ensures that lexical errors are user-recoverable while
  /// programming errors fail fast during development.
  /// </summary>
  internal ref struct Lexer
  {
    private const char Eof = (char)0;

    private const string BooleanTrueToken = "true";
    private const string BooleanFalseToken = "false";

    private ReadOnlyMemory<char> _buffer;
    private LexerCursor _cursor;

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
      return IsIdentifierStart(c) || IsNumber(c) || c == '.';
    }

    private char AcceptStringStart()
    {
      var c = _cursor.Peek();
      if (c == '"' || c == '\'')
      {
        _cursor.Advance();
        return c;
      }

      return Eof;
    }

    private bool AcceptIdentifierStart()
    {
      if (IsIdentifierStart(_cursor.Peek()))
      {
        _cursor.Advance();
        return true;
      }

      return false;
    }

    private bool AcceptExponent()
    {
      if (IsExponent(_cursor.Peek()))
      {
        _cursor.Advance();
        return true;
      }

      return false;
    }

    private bool Accept(char c)
    {
      return _cursor.Accept(c);
    }

    private bool AcceptRunStringBody(char closingQuote)
    {
      var success = false;
      while (true)
      {
        var current = _cursor.Advance();
        if (current == closingQuote)
        {
          // Found closing quotation mark
          success = true;
          break;
        }

        if (current == Eof)
        {
          // Unclosed string
          break;
        }
      }

      return success;
    }

    private bool AcceptRunIdentifierBody()
    {
      var matched = false;
      while (IsIdentifierBody(_cursor.Peek()))
      {
        _cursor.Advance();
        matched = true;
      }

      return matched;
    }

    private bool AcceptRunNumbers()
    {
      var matched = false;
      while (IsNumber(_cursor.Peek()))
      {
        _cursor.Advance();
        matched = true;
      }

      return matched;
    }

    private void SkipWhiteSpaces()
    {
      while (char.IsWhiteSpace(_cursor.Peek()))
      {
        _cursor.Advance();
      }

      _cursor.MarkTokenStart();
    }


    /// <summary>
    /// Gets a human-readable description of a character for error messages
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static string GetCharacterDescription(char c)
    {
      // Special handling for common control characters
      return c switch
      {
        '\t' => "tab character (Unicode U+0009)",
        '\n' => "newline character (Unicode U+000A)",
        '\r' => "carriage return (Unicode U+000D)",
        '\0' => "null character (Unicode U+0000)",
        '\b' => "backspace (Unicode U+0008)",
        '\f' => "form feed (Unicode U+000C)",
        '\v' => "vertical tab (Unicode U+000B)",
        _ when char.IsControl(c) => $"control character (Unicode U+{(int)c:X4})",
        _ => $"'{c}'"
      };
    }

    private Token Emit(TokenType tokenType)
    {
      // Get location from cursor
      var location = _cursor.GetTokenLocation();

      // Get raw token text from cursor
      var rawToken = _cursor.GetTokenText(_buffer);
      var tokenSpan = rawToken.Span;

      // Trim leading whitespace
      var startIdx = 0;
      while (startIdx < tokenSpan.Length && char.IsWhiteSpace(tokenSpan[startIdx]))
      {
        ++startIdx;
      }

      // Trim trailing whitespace
      var endIdx = tokenSpan.Length - 1;
      while (endIdx >= startIdx && char.IsWhiteSpace(tokenSpan[endIdx]))
      {
        --endIdx;
      }

      // Create trimmed memory slice
      var trimmedToken = rawToken.Slice(startIdx, endIdx - startIdx + 1);

      var token = new Token(trimmedToken, tokenType, location);

      // Mark start of next token
      _cursor.MarkTokenStart();

      return token;
    }

    public void Execute(ReadOnlyMemory<char> content, ITokenReader output)
    {
      _buffer = content;
      _cursor = new LexerCursor(content);

      while (true)
      {
        SkipWhiteSpaces();

        if (AcceptIdentifierStart())
        {
          AcceptRunIdentifierBody();
          var token = _cursor.GetTokenSpan().Trim();
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
                throw new LexerException(_cursor.TokenStart.Line,
                  "Float exponent requires an integer value (e.g., '1.0e10', not '1.0e')");
              }
            }

            output.Push(Emit(TokenType.Float));
          }
          else
          {
            // Check if integer is immediately followed by exponent character
            // This is invalid - float literals with exponent notation require a decimal point (e.g., 2.0e10, not 2e10)
            if (AcceptExponent())
            {
              throw new LexerException(_cursor.TokenStart.Line,
                "Float exponent notation requires decimal point (e.g., use '2.0e10' instead of '2e10')");
            }

            output.Push(Emit(TokenType.Integer));
          }

          continue;
        }

        var quoteChar = AcceptStringStart();
        if (quoteChar != Eof)
        {
          if (!AcceptRunStringBody(quoteChar))
          {
            var quoteType = quoteChar == '"' ? "double" : "single";
            throw new LexerException(_cursor.TokenStart.Line,
              $"String literal does not have a closing {quoteType} quotation mark");
          }

          output.Push(Emit(TokenType.String));
          continue;
        }

        var nextChar = _cursor.Advance();
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
            if (_cursor.Accept('='))
            {
              output.Push(Emit(TokenType.ComparisonEqual));
            }
            else
            {
              output.Push(Emit(TokenType.AssignmentOperator));
            }

            break;
          case '<':
            if (_cursor.Accept('='))
            {
              output.Push(Emit(TokenType.ComparisonLessThanOrEqualTo));
            }
            else
            {
              output.Push(Emit(TokenType.ComparisonLessThan));
            }

            break;
          case '>':
            if (_cursor.Accept('='))
            {
              output.Push(Emit(TokenType.ComparisonGreaterThanOrEqualTo));
            }
            else
            {
              output.Push(Emit(TokenType.ComparisonGreaterThan));
            }

            break;
          case '!':
            if (_cursor.Accept('='))
            {
              output.Push(Emit(TokenType.ComparisonNotEqual));
            }
            else
            {
              output.Push(Emit(TokenType.NegateOperator));
            }

            break;
          case '|':
            if (_cursor.Accept('|'))
            {
              output.Push(Emit(TokenType.BooleanOrOperator));
            }
            else
            {
              throw new LexerException(_cursor.TokenStart.Line,
                "OR boolean operator requires two vertical bar characters '||'");
            }

            break;
          case '&':
            if (_cursor.Accept('&'))
            {
              output.Push(Emit(TokenType.BooleanAndOperator));
            }
            else
            {
              throw new LexerException(_cursor.TokenStart.Line,
                "AND boolean operator requires two ampersand characters '&&'");
            }

            break;
          case '+':
            if (_cursor.Accept('='))
            {
              output.Push(Emit(TokenType.AssignmentAddOperator));
            }
            else
            {
              output.Push(Emit(TokenType.PlusSign));
            }

            break;
          case '-':
            if (_cursor.Accept('='))
            {
              output.Push(Emit(TokenType.AssignmentSubtractOperator));
            }
            else
            {
              output.Push(Emit(TokenType.MinusSign));
            }

            break;
          case '*':
            if (_cursor.Accept('='))
            {
              output.Push(Emit(TokenType.AssignmentMultiplyOperator));
            }
            else
            {
              output.Push(Emit(TokenType.MultiplyOperator));
            }

            break;
          case '/':
            if (_cursor.Accept('='))
            {
              output.Push(Emit(TokenType.AssignmentDivideOperator));
            }
            else
            {
              output.Push(Emit(TokenType.DivideOperator));
            }

            break;
          case '%':
            if (_cursor.Accept('='))
            {
              output.Push(Emit(TokenType.AssignmentModuloOperator));
            }
            else
            {
              output.Push(Emit(TokenType.ModuloOperator));
            }

            break;
          case ',':
            output.Push(Emit(TokenType.Comma));
            break;
          case ';':
            output.Push(Emit(TokenType.Semicolon));
            break;
          default:
            var charDescription = GetCharacterDescription(nextChar);
            throw new LexerException(_cursor.TokenStart.Line,
              $"Unexpected character {charDescription}. Expected identifier, number, string, operator, or parenthesis.");
        }
      }
    }
  }
}