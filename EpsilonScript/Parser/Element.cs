﻿using EpsilonScript.Lexer;

namespace EpsilonScript.Parser
{
  public struct Element
  {
    public Token Token { get; }
    public ElementType Type { get; }

    public Element(Token token, ElementType type)
    {
      Token = token;
      Type = type;
    }

    public override string ToString()
    {
      return $"{Token}: {Type}";
    }
  }
}