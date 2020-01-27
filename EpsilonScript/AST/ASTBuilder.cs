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
      return type switch
      {
        ElementType.None => (Node) new NullNode(),
        ElementType.Variable => new VariableNode(),
        ElementType.Function => new FunctionNode(),
        ElementType.Comma => new TupleNode(),
        ElementType.Semicolon => new SequenceNode(),
        ElementType.ComparisonEqual => new ComparisonNode(),
        ElementType.ComparisonNotEqual => new ComparisonNode(),
        ElementType.ComparisonLessThan => new ComparisonNode(),
        ElementType.ComparisonGreaterThan => new ComparisonNode(),
        ElementType.ComparisonLessThanOrEqualTo => new ComparisonNode(),
        ElementType.ComparisonGreaterThanOrEqualTo => new ComparisonNode(),
        ElementType.NegateOperator => new NegateNode(),
        ElementType.PositiveOperator => new SignOperator(),
        ElementType.NegativeOperator => new SignOperator(),
        ElementType.BooleanOrOperator => new BooleanOperationNode(),
        ElementType.BooleanAndOperator => new BooleanOperationNode(),
        ElementType.BooleanLiteralTrue => new BooleanNode(),
        ElementType.BooleanLiteralFalse => new BooleanNode(),
        ElementType.AssignmentOperator => new AssignmentNode(),
        ElementType.AssignmentAddOperator => new AssignmentNode(),
        ElementType.AssignmentSubtractOperator => new AssignmentNode(),
        ElementType.AssignmentMultiplyOperator => new AssignmentNode(),
        ElementType.AssignmentDivideOperator => new AssignmentNode(),
        ElementType.AddOperator => new ArithmeticNode(),
        ElementType.SubtractOperator => new ArithmeticNode(),
        ElementType.MultiplyOperator => new ArithmeticNode(),
        ElementType.DivideOperator => new ArithmeticNode(),
        ElementType.Integer => new IntegerNode(),
        ElementType.Float => new FloatNode(),
        _ => throw new ArgumentOutOfRangeException()
      };
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

      return rpnStack.Pop();
    }
  }
}