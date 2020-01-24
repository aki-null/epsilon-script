using System;
using System.Collections.Generic;

namespace EpsilonScript.Parser
{
  public class RPNConverter
  {
    private readonly Stack<Element> _operatorStack = new Stack<Element>();
    public List<Element> Rpn { get; } = new List<Element>();

    public void Reset()
    {
      _operatorStack.Clear();
      Rpn.Clear();
    }

    private void PopParenthesis(in Element element)
    {
      while (_operatorStack.TryPop(out var topOperator))
      {
        if (topOperator.Type == ElementType.FunctionStartParenthesis || topOperator.Type == ElementType.LeftParenthesis)
        {
          return;
        }

        Rpn.Add(topOperator);
      }

      throw new ParserException(element.Token, "Unopened parenthesis found");
    }

    private void PushStandardElement(in Element element)
    {
      if (element.Type.IsValue())
      {
        Rpn.Add(element);
        return;
      }

      if (!element.Type.IsOperator())
      {
        throw new ArgumentOutOfRangeException(nameof(element.Type), "Element type not configured correctly");
      }

      while (_operatorStack.TryPeek(out var topOperator))
      {
        var submitOperator = false;
        var topPrecedence = topOperator.Type.Precedence();
        var incomingPrecedence = element.Type.Precedence();
        if (incomingPrecedence < topPrecedence)
        {
          // Incoming operator has lower precedence, hence submit operator to RPN list
          submitOperator = true;
        }
        else if (incomingPrecedence == topPrecedence && topOperator.Type.Associativity() == Associativity.Left)
        {
          // The precedence is the same, hence submit operator to RPN list if the operator on top of the stack
          // is LEFT associative
          submitOperator = true;
        }

        if (!submitOperator)
        {
          break;
        }

        Rpn.Add(topOperator);
        _operatorStack.Pop();
      }

      _operatorStack.Push(element);
    }

    private void Flush()
    {
      while (_operatorStack.TryPop(out var topOperator))
      {
        if (topOperator.Type == ElementType.LeftParenthesis)
        {
          throw new ParserException(topOperator.Token, "Unclosed parenthesis found");
        }

        Rpn.Add(topOperator);
      }
    }

    private void PushElement(in Element element)
    {
      switch (element.Type)
      {
        // Shunting-yard algorithm
        case ElementType.FunctionStartParenthesis:
        case ElementType.LeftParenthesis:
          _operatorStack.Push(element);
          break;
        case ElementType.RightParenthesis:
          PopParenthesis(element);
          break;
        default:
          PushStandardElement(element);
          break;
      }
    }

    public void Convert(List<Element> elements)
    {
      foreach (var element in elements)
      {
        PushElement(in element);
      }

      Flush();
    }
  }
}