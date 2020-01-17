using System;
using System.Collections.Generic;

namespace EpsilonScript
{
  public class Lexer
  {
    public enum TokenType
    {
      Identifier,
      LeftParenthesis,
      RightParenthesis,
      Comma,
      Semicolon,
      ComparisonEqual,
      ComparisonNotEqual,
      ComparisonLessThan,
      ComparisonGreaterThan,
      ComparisonLessThanOrEqualTo,
      ComparisonGreaterThanOrEqualTo,
      NegateOperator,
      BooleanOrOperator,
      BooleanAndOperator,
      BooleanLiteralTrue,
      BooleanLiteralFalse,
      AssignmentOperator,
      AssignmentAddOperator,
      AssignmentSubtractOperator,
      AssignmentMultiplyOperator,
      AssignmentDivideOperator,
      PlusSign,
      MinusSign,
      MultiplyOperator,
      DivideOperator,
      Integer,
      Float,
    }

    private const char EOF = (char)0;
    private const string Alphabets = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Numbers = "0123456789";
    private const string IdentifierStart = Alphabets + "_";
    private const string IdentifierBody = IdentifierStart + Numbers;

    private const string BooleanTrueToken = "true";
    private const string BooleanFalseToken = "false";

    public struct Token
    {
      public string text;
      public TokenType type;

      public Token(string text, TokenType type)
      {
        this.text = text;
        this.type = type;
      }

      public override string ToString()
      {
        return $"{type}: {text}";
      }
    }

    private string _content;

    private int _start;
    private int _current;
    private int _startLineNumber;
    private int _currentLineNumber;

    private bool IsFinished => _current >= _content.Length;

    public Lexer(string content)
    {
      _content = content;
    }

    private bool Backup()
    {
      if (_current <= _start) return false;
      --_current;
      if (_current < _content.Length && _content[_current] == '\n') --_currentLineNumber;
      return true;
    }

    private char Next()
    {
      var res = _current >= _content.Length ? EOF : _content[_current];
      if (res == '\n') ++_currentLineNumber;
      ++_current;
      return res;
    }

    private bool Accept(string chars)
    {
      if (chars.Contains(Next()))
      {
        return true;
      }
      Backup();
      return false;
    }

    private bool AcceptRun(string chars)
    {
      var success = false;
      while (chars.Contains(Next()))
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
      while (Char.IsWhiteSpace(Next()))
      {
      }
      Backup();
      Ignore();
    }

    private string CurrentToken => _content.Substring(_start, _current - _start).Trim();

    private Token Emit(TokenType tokenType)
    {
      var token = new Token(CurrentToken, tokenType);
      _start = _current;
      _startLineNumber = _currentLineNumber;
      return token;
    }

    public IEnumerable<Token> Process()
    {
      _start = 0;
      _current = 0;
      _startLineNumber = 1;
      _currentLineNumber = 1;
      while (true)
      {
        SkipWhiteSpaces();
        if (Accept(IdentifierStart))
        {
          AcceptRun(IdentifierBody);
          var token = CurrentToken;
          switch (token)
          {
            case BooleanTrueToken:
              yield return Emit(TokenType.BooleanLiteralTrue);
              break;
            case BooleanFalseToken:
              yield return Emit(TokenType.BooleanLiteralFalse);
              break;
            default:
              yield return Emit(TokenType.Identifier);
              break;
          }
          continue;
        }
        if (AcceptRun(Numbers))
        {
          if (Accept("."))
          {
            // float
            AcceptRun(Numbers);
            if (Accept("e"))
            {
              // Accept + or -
              if (!Accept("+"))
              {
                Accept("-");
              }
              // Integers lead by + or - sign
              if (!AcceptRun(Numbers))
              {
                throw new LexerException(_startLineNumber, "Integer value is required for float exponent value");
              }
            }
            yield return Emit(TokenType.Float);
          }
          else
          {
            yield return Emit(TokenType.Integer);
          }
          continue;
        }
        var nextChar = Next();
        switch (nextChar)
        {
          case EOF:
            yield break;
          case '(':
            yield return Emit(TokenType.LeftParenthesis);
            break;
          case ')':
            yield return Emit(TokenType.RightParenthesis);
            break;
          case '=':
            {
              nextChar = Next();
              switch (nextChar)
              {
                case '=':
                  yield return Emit(TokenType.ComparisonEqual);
                  break;
                default:
                  Backup();
                  yield return Emit(TokenType.AssignmentOperator);
                  break;
              }
              break;
            }
          case '<':
            {
              nextChar = Next();
              switch (nextChar)
              {
                case '=':
                  yield return Emit(TokenType.ComparisonLessThanOrEqualTo);
                  break;
                default:
                  Backup();
                  yield return Emit(TokenType.ComparisonLessThan);
                  break;
              }
              break;
            }
          case '>':
            {
              nextChar = Next();
              switch (nextChar)
              {
                case '=':
                  yield return Emit(TokenType.ComparisonGreaterThanOrEqualTo);
                  break;
                default:
                  Backup();
                  yield return Emit(TokenType.ComparisonGreaterThan);
                  break;
              }
              break;
            }
          case '!':
            {
              nextChar = Next();
              switch (nextChar)
              {
                case '=':
                  yield return Emit(TokenType.ComparisonNotEqual);
                  break;
                default:
                  Backup();
                  yield return Emit(TokenType.NegateOperator);
                  break;
              }
              break;
            }
          case '|':
            {
              nextChar = Next();
              if (nextChar == '|')
              {
                yield return Emit(TokenType.BooleanOrOperator);
              }
              else
              {
                throw new LexerException(_startLineNumber, $"OR boolean operand requires two vertical bar characters (||)");
              }
              break;
            }
          case '&':
            {
              nextChar = Next();
              if (nextChar == '&')
              {
                yield return Emit(TokenType.BooleanAndOperator);
              }
              else
              {
                throw new LexerException(_startLineNumber, $"AND boolean operand requires two ampersand characters (&&)");
              }
              break;
            }
          case '+':
            {
              nextChar = Next();
              switch (nextChar)
              {
                case '=':
                  yield return Emit(TokenType.AssignmentAddOperator);
                  break;
                default:
                  Backup();
                  yield return Emit(TokenType.PlusSign);
                  break;
              }
              break;
            }
          case '-':
            {
              nextChar = Next();
              switch (nextChar)
              {
                case '=':
                  yield return Emit(TokenType.AssignmentSubtractOperator);
                  break;
                default:
                  Backup();
                  yield return Emit(TokenType.MinusSign);
                  break;
              }
              break;
            }
          case '*':
            {
              nextChar = Next();
              switch (nextChar)
              {
                case '=':
                  yield return Emit(TokenType.AssignmentMultiplyOperator);
                  break;
                default:
                  Backup();
                  yield return Emit(TokenType.MultiplyOperator);
                  break;
              }
              break;
            }
          case '/':
            {
              nextChar = Next();
              switch (nextChar)
              {
                case '=':
                  yield return Emit(TokenType.AssignmentDivideOperator);
                  break;
                default:
                  Backup();
                  yield return Emit(TokenType.DivideOperator);
                  break;
              }
              break;
            }
          case ',':
            {
              yield return Emit(TokenType.Comma);
              break;
            }
          case ';':
            {
              yield return Emit(TokenType.Semicolon);
              break;
            }
          default:
            throw new LexerException(_startLineNumber, $"Unexpected character found: {nextChar}");
        }
      }
    }
  }

  [Serializable]
  public class LexerException : Exception
  {
    public int LineNumber { get; private set; }

    public LexerException(int lineNumber)
    {
      LineNumber = lineNumber;
    }

    public LexerException(int lineNumber, string message)
      : base(message)
    {
      LineNumber = lineNumber;
    }

    public LexerException(int lineNumber, string message, Exception inner)
      : base(message, inner)
    {
      LineNumber = lineNumber;
    }

    public override string ToString()
    {
      return $"Line {LineNumber}: {Message}";
    }
  }
}
