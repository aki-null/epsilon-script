using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class AstBuilder : IElementReader
  {
    public Node Result { get; private set; }

    // Nodes are rented per compilation; Compiler returns them once the tree is lowered
    public NodePool Pool { get; } = new NodePool();

    private readonly Stack<Node> _rpnStack = new Stack<Node>();
    private readonly CompilerContext _context;
    private Compiler.Options _options;
    private IVariableContainer _variables;

    public AstBuilder(CompilerContext context)
    {
      _context = context;
    }

    public void Reset()
    {
      _rpnStack.Clear();
      _options = Compiler.Options.None;
      _variables = null;
      Result = null;
    }

    public void Configure(Compiler.Options options, IVariableContainer variables)
    {
      _options = options;
      _variables = variables;
    }

    public void Push(Element element)
    {
      var node = Pool.Rent(KindOf(element.Type));
      node.Build(_rpnStack, element, _context, _options, _variables);
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

    private static NodeKind KindOf(ElementType type)
    {
      switch (type)
      {
        case ElementType.None:
          return NodeKind.Null;
        case ElementType.Variable:
          return NodeKind.Variable;
        case ElementType.Function:
          return NodeKind.Function;
        case ElementType.Comma:
          return NodeKind.Tuple;
        case ElementType.Semicolon:
          return NodeKind.Sequence;
        case ElementType.ComparisonEqual:
          return NodeKind.Equal;
        case ElementType.ComparisonNotEqual:
          return NodeKind.NotEqual;
        case ElementType.ComparisonLessThan:
          return NodeKind.LessThan;
        case ElementType.ComparisonGreaterThan:
          return NodeKind.GreaterThan;
        case ElementType.ComparisonLessThanOrEqualTo:
          return NodeKind.LessThanOrEqual;
        case ElementType.ComparisonGreaterThanOrEqualTo:
          return NodeKind.GreaterThanOrEqual;
        case ElementType.NegateOperator:
          return NodeKind.Negate;
        case ElementType.PositiveOperator:
          return NodeKind.Positive;
        case ElementType.NegativeOperator:
          return NodeKind.Negative;
        case ElementType.BooleanOrOperator:
          return NodeKind.BooleanOr;
        case ElementType.BooleanAndOperator:
          return NodeKind.BooleanAnd;
        case ElementType.BooleanLiteralTrue:
        case ElementType.BooleanLiteralFalse:
          return NodeKind.Boolean;
        case ElementType.AssignmentOperator:
          return NodeKind.DirectAssignment;
        case ElementType.AssignmentAddOperator:
          return NodeKind.AddAssignment;
        case ElementType.AssignmentSubtractOperator:
          return NodeKind.SubtractAssignment;
        case ElementType.AssignmentMultiplyOperator:
          return NodeKind.MultiplyAssignment;
        case ElementType.AssignmentDivideOperator:
          return NodeKind.DivideAssignment;
        case ElementType.AssignmentModuloOperator:
          return NodeKind.ModuloAssignment;
        case ElementType.AddOperator:
          return NodeKind.Add;
        case ElementType.SubtractOperator:
          return NodeKind.Subtract;
        case ElementType.MultiplyOperator:
          return NodeKind.Multiply;
        case ElementType.DivideOperator:
          return NodeKind.Divide;
        case ElementType.ModuloOperator:
          return NodeKind.Modulo;
        case ElementType.Integer:
          return NodeKind.Integer;
        case ElementType.Float:
          return NodeKind.Float;
        case ElementType.String:
          return NodeKind.String;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }
  }
}