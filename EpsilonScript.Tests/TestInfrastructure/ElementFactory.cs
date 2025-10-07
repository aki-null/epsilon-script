using System;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Tests.TestInfrastructure
{
  public static class ElementFactory
  {
    internal static Element Create(string text, TokenType tokenType, ElementType elementType)
    {
      // For None tokens, use ReadOnlyMemory<char>.Empty to match what the parser produces
      if (tokenType == TokenType.None && string.IsNullOrEmpty(text))
      {
        return new Element(new Token(ReadOnlyMemory<char>.Empty, tokenType), elementType);
      }

      return new Element(new Token(text, tokenType), elementType);
    }

    internal static Element[] Create(params (string text, TokenType tokenType, ElementType elementType)[] elements)
    {
      var result = new Element[elements.Length];
      for (var i = 0; i < elements.Length; ++i)
      {
        var entry = elements[i];
        result[i] = Create(entry.text, entry.tokenType, entry.elementType);
      }

      return result;
    }
  }
}