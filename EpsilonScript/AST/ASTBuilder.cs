using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript.AST
{
  public static class ASTBuilder
  {
    private static Node CreateNode(ElementType type)
    {
      switch (type)
      {
        case ElementType.None:
          return new NullNode();
        case ElementType.Variable:
          return new VariableNode();
        case ElementType.Function:
          return new FunctionNode();
        case ElementType.Comma:
          return new TupleNode();
        case ElementType.Semicolon:
          return new SequenceNode();
        case ElementType.ComparisonEqual:
        case ElementType.ComparisonNotEqual:
        case ElementType.ComparisonLessThan:
        case ElementType.ComparisonGreaterThan:
        case ElementType.ComparisonLessThanOrEqualTo:
        case ElementType.ComparisonGreaterThanOrEqualTo:
          return new ComparisonNode();
        case ElementType.NegateOperator:
          return new NegateNode();
        case ElementType.PositiveOperator:
        case ElementType.NegativeOperator:
          return new SignOperator();
        case ElementType.BooleanOrOperator:
        case ElementType.BooleanAndOperator:
          return new BooleanOperationNode();
        case ElementType.BooleanLiteralTrue:
        case ElementType.BooleanLiteralFalse:
          return new BooleanNode();
        case ElementType.AssignmentOperator:
        case ElementType.AssignmentAddOperator:
        case ElementType.AssignmentSubtractOperator:
        case ElementType.AssignmentMultiplyOperator:
        case ElementType.AssignmentDivideOperator:
          return new AssignmentNode();
        case ElementType.AddOperator:
        case ElementType.SubtractOperator:
        case ElementType.MultiplyOperator:
        case ElementType.DivideOperator:
        case ElementType.ModuloOperator:
          return new ArithmeticNode();
        case ElementType.Integer:
          return new IntegerNode();
        case ElementType.Float:
          return new FloatNode();
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public static Node Build(List<Element> elements, Compiler.Options options,
      IDictionary<string, VariableValue> variables,
      IDictionary<string, CustomFunctionOverload> functions)
    {
      var rpnStack = new Stack<Node>();
      foreach (var element in elements)
      {
        var node = CreateNode(element.Type);
        node.Build(rpnStack, element, options, variables, functions);
        rpnStack.Push(node);
      }

      if (rpnStack.Count > 1)
      {
        throw new RuntimeException("Missing operator(s) to process all values");
      }

      return rpnStack.Pop();
    }
  }
}