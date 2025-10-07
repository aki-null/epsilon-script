using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class AstBuilder : IElementReader
  {
    public Node Result { get; private set; }

    private readonly Stack<Node> _rpnStack = new Stack<Node>();

    private Compiler.Options _options;
    private IVariableContainer _variables;
    private Compiler.IntegerPrecision _intPrecision;
    private Compiler.FloatPrecision _floatPrecision;
    private readonly IDictionary<VariableId, CustomFunctionOverload> _functions;

    public AstBuilder(IDictionary<VariableId, CustomFunctionOverload> functions)
    {
      _functions = functions;
    }

    public void Reset()
    {
      _rpnStack.Clear();
      _options = Compiler.Options.None;
      _variables = null;
      _intPrecision = Compiler.IntegerPrecision.Integer;
      _floatPrecision = Compiler.FloatPrecision.Float;
      Result = null;
    }

    public void Configure(Compiler.Options options, IVariableContainer variables,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      _options = options;
      _variables = variables;
      _intPrecision = intPrecision;
      _floatPrecision = floatPrecision;
    }

    public void Push(Element element)
    {
      var node = CreateNode(element.Type);
      node.Build(_rpnStack, element, _options, _variables, _functions, _intPrecision, _floatPrecision);
      _rpnStack.Push(node);
    }

    public void End()
    {
      if (_rpnStack.Count > 1)
      {
        throw new RuntimeException("Missing operator(s) to process all values");
      }

      Result = _rpnStack.Pop();
    }

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
        case ElementType.String:
          return new StringNode();
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}