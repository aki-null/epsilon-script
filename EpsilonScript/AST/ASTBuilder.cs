using System;
using System.Collections.Generic;
using EpsilonScript.AST.Arithmetic;
using EpsilonScript.AST.Assignment;
using EpsilonScript.AST.Boolean;
using EpsilonScript.AST.Comparison;
using EpsilonScript.AST.Literal;
using EpsilonScript.AST.Sign;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class AstBuilder : IElementReader
  {
    public Node Result { get; private set; }

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
      var node = CreateNode(element.Type);
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
          return new EqualComparisonNode();
        case ElementType.ComparisonNotEqual:
          return new NotEqualComparisonNode();
        case ElementType.ComparisonLessThan:
          return new LessThanComparisonNode();
        case ElementType.ComparisonGreaterThan:
          return new GreaterThanComparisonNode();
        case ElementType.ComparisonLessThanOrEqualTo:
          return new LessThanOrEqualComparisonNode();
        case ElementType.ComparisonGreaterThanOrEqualTo:
          return new GreaterThanOrEqualComparisonNode();
        case ElementType.NegateOperator:
          return new NegateNode();
        case ElementType.PositiveOperator:
          return new PositiveSignNode();
        case ElementType.NegativeOperator:
          return new NegativeSignNode();
        case ElementType.BooleanOrOperator:
          return new BooleanOrNode();
        case ElementType.BooleanAndOperator:
          return new BooleanAndNode();
        case ElementType.BooleanLiteralTrue:
        case ElementType.BooleanLiteralFalse:
          return new BooleanNode();
        case ElementType.AssignmentOperator:
          return new DirectAssignmentNode();
        case ElementType.AssignmentAddOperator:
          return new AddAssignmentNode();
        case ElementType.AssignmentSubtractOperator:
          return new SubtractAssignmentNode();
        case ElementType.AssignmentMultiplyOperator:
          return new MultiplyAssignmentNode();
        case ElementType.AssignmentDivideOperator:
          return new DivideAssignmentNode();
        case ElementType.AssignmentModuloOperator:
          return new ModuloAssignmentNode();
        case ElementType.AddOperator:
          return new AddNode();
        case ElementType.SubtractOperator:
          return new SubtractNode();
        case ElementType.MultiplyOperator:
          return new MultiplyNode();
        case ElementType.DivideOperator:
          return new DivideNode();
        case ElementType.ModuloOperator:
          return new ModuloNode();
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