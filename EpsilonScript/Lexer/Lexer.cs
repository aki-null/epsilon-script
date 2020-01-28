using System.Collections.Generic;

namespace EpsilonScript.Lexer
{
  public class Lexer
  {
    private const char Eof = (char) 0;
    private const string Alphabets = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const string Numbers = "0123456789";
    private const string IdentifierStart = Alphabets + "_";
    private const string IdentifierBody = IdentifierStart + Numbers;

    private const string BooleanTrueToken = "true";
    private const string BooleanFalseToken = "false";

    private string _content;

    private int _start;
    private int _current;
    private int _startLineNumber;
    private int _currentLineNumber;

    private bool Backup()
    {
      if (_current <= _start) return false;
      --_current;
      if (_current < _content.Length && _content[_current] == '\n') --_currentLineNumber;
      return true;
    }

    private char Next()
    {
      var res = _current >= _content.Length ? Eof : _content[_current];
      if (res == '\n') ++_currentLineNumber;
      ++_current;
      return res;
    }

    private bool Accept(string chars)
    {
      if (chars.IndexOf(Next()) >= 0)
      {
        return true;
      }

      Backup();
      return false;
    }

    private bool AcceptRun(string chars)
    {
      var success = false;
      while (chars.IndexOf(Next()) >= 0)
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

    private string CurrentToken => _content.Substring(_start, _current - _start).Trim();

    private Token Emit(TokenType tokenType)
    {
      var token = new Token(CurrentToken, tokenType, _startLineNumber);
      _start = _current;
      _startLineNumber = _currentLineNumber;
      return token;
    }

    public IList<Token> Analyze(string content)
    {
      _content = content;
      _start = 0;
      _current = 0;
      _startLineNumber = 1;
      _currentLineNumber = 1;

      var tokens = new List<Token>();

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
              tokens.Add(Emit(TokenType.BooleanLiteralTrue));
              break;
            case BooleanFalseToken:
              tokens.Add(Emit(TokenType.BooleanLiteralFalse));
              break;
            default:
              tokens.Add(Emit(TokenType.Identifier));
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
            if (Accept("eE"))
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

            tokens.Add(Emit(TokenType.Float));
          }
          else
          {
            tokens.Add(Emit(TokenType.Integer));
          }

          continue;
        }

        var nextChar = Next();
        switch (nextChar)
        {
          case Eof:
            return tokens;
          case '(':
            tokens.Add(Emit(TokenType.LeftParenthesis));
            break;
          case ')':
            tokens.Add(Emit(TokenType.RightParenthesis));
            break;
          case '=':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                tokens.Add(Emit(TokenType.ComparisonEqual));
                break;
              default:
                Backup();
                tokens.Add(Emit(TokenType.AssignmentOperator));
                break;
            }

            break;
          case '<':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                tokens.Add(Emit(TokenType.ComparisonLessThanOrEqualTo));
                break;
              default:
                Backup();
                tokens.Add(Emit(TokenType.ComparisonLessThan));
                break;
            }

            break;
          case '>':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                tokens.Add(Emit(TokenType.ComparisonGreaterThanOrEqualTo));
                break;
              default:
                Backup();
                tokens.Add(Emit(TokenType.ComparisonGreaterThan));
                break;
            }

            break;
          case '!':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                tokens.Add(Emit(TokenType.ComparisonNotEqual));
                break;
              default:
                Backup();
                tokens.Add(Emit(TokenType.NegateOperator));
                break;
            }

            break;
          case '|':
            nextChar = Next();
            if (nextChar == '|')
            {
              tokens.Add(Emit(TokenType.BooleanOrOperator));
            }
            else
            {
              throw new LexerException(_startLineNumber,
                $"OR boolean operand requires two vertical bar characters (||)");
            }

            break;
          case '&':
            nextChar = Next();
            if (nextChar == '&')
            {
              tokens.Add(Emit(TokenType.BooleanAndOperator));
            }
            else
            {
              throw new LexerException(_startLineNumber, $"AND boolean operand requires two ampersand characters (&&)");
            }

            break;
          case '+':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                tokens.Add(Emit(TokenType.AssignmentAddOperator));
                break;
              default:
                Backup();
                tokens.Add(Emit(TokenType.PlusSign));
                break;
            }

            break;
          case '-':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                tokens.Add(Emit(TokenType.AssignmentSubtractOperator));
                break;
              default:
                Backup();
                tokens.Add(Emit(TokenType.MinusSign));
                break;
            }

            break;
          case '*':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                tokens.Add(Emit(TokenType.AssignmentMultiplyOperator));
                break;
              default:
                Backup();
                tokens.Add(Emit(TokenType.MultiplyOperator));
                break;
            }

            break;
          case '/':
            nextChar = Next();
            switch (nextChar)
            {
              case '=':
                tokens.Add(Emit(TokenType.AssignmentDivideOperator));
                break;
              default:
                Backup();
                tokens.Add(Emit(TokenType.DivideOperator));
                break;
            }

            break;
          case ',':
            tokens.Add(Emit(TokenType.Comma));
            break;
          case ';':
            tokens.Add(Emit(TokenType.Semicolon));
            break;
          default:
            throw new LexerException(_startLineNumber, $"Unexpected character found: {nextChar}");
        }
      }
    }
  }
}