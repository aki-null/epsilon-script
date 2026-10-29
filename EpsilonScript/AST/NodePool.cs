using System;
using System.Collections.Generic;
using EpsilonScript.AST.Arithmetic;
using EpsilonScript.AST.Assignment;
using EpsilonScript.AST.Boolean;
using EpsilonScript.AST.Comparison;
using EpsilonScript.AST.Literal;
using EpsilonScript.AST.Sign;

namespace EpsilonScript.AST
{
  internal enum NodeKind : byte
  {
    Null,
    Variable,
    Function,
    Tuple,
    Sequence,
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    Negate,
    Positive,
    Negative,
    BooleanOr,
    BooleanAnd,
    Boolean,
    DirectAssignment,
    AddAssignment,
    SubtractAssignment,
    MultiplyAssignment,
    DivideAssignment,
    ModuloAssignment,
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Integer,
    Float,
    String,
    MultiplyAdd,
    AddMultiply,
    MultiplySubtract,
    SubtractMultiply,
    Count,
  }

  /// <summary>
  /// Reuses AST nodes across compilations. The tree only lives until it is lowered, so every node rented during
  /// a compilation is reset and returned once the program is built. Compilations nest when a function folded at
  /// compile time compiles another script, so each one returns only the nodes rented after its mark.
  /// </summary>
  internal sealed class NodePool
  {
    private readonly Stack<Node>[] _free = new Stack<Node>[(int)NodeKind.Count];
    private readonly List<Node> _rented = new List<Node>();

    public Node Rent(NodeKind kind)
    {
      var free = _free[(int)kind];
      Node node;
      if (free != null && free.Count > 0)
      {
        node = free.Pop();
      }
      else
      {
        node = Create(kind);
        node.Pool = this;
        node.PoolKind = kind;
      }

      _rented.Add(node);
      return node;
    }

    /// <summary>
    /// Marks the start of a compilation, for ReturnAll and Abandon.
    /// </summary>
    public int Mark => _rented.Count;

    /// <summary>
    /// Resets every node rented since the mark and makes it available again.
    /// </summary>
    public void ReturnAll(int mark)
    {
      for (var i = mark; i < _rented.Count; ++i)
      {
        var node = _rented[i];
        node.Reset();
        var kind = (int)node.PoolKind;
        (_free[kind] ?? (_free[kind] = new Stack<Node>())).Push(node);
      }

      _rented.RemoveRange(mark, _rented.Count - mark);
    }

    /// <summary>
    /// Hands every node rented since the mark to the caller; they are never reused. They stop renting from the pool,
    /// so nodes they create later are not tracked either.
    /// </summary>
    public void Abandon(int mark)
    {
      for (var i = mark; i < _rented.Count; ++i)
      {
        _rented[i].Pool = null;
      }

      _rented.RemoveRange(mark, _rented.Count - mark);
    }

    internal static Node Create(NodeKind kind)
    {
      switch (kind)
      {
        case NodeKind.Null: return new NullNode();
        case NodeKind.Variable: return new VariableNode();
        case NodeKind.Function: return new FunctionNode();
        case NodeKind.Tuple: return new TupleNode();
        case NodeKind.Sequence: return new SequenceNode();
        case NodeKind.Equal: return new EqualComparisonNode();
        case NodeKind.NotEqual: return new NotEqualComparisonNode();
        case NodeKind.LessThan: return new LessThanComparisonNode();
        case NodeKind.GreaterThan: return new GreaterThanComparisonNode();
        case NodeKind.LessThanOrEqual: return new LessThanOrEqualComparisonNode();
        case NodeKind.GreaterThanOrEqual: return new GreaterThanOrEqualComparisonNode();
        case NodeKind.Negate: return new NegateNode();
        case NodeKind.Positive: return new PositiveSignNode();
        case NodeKind.Negative: return new NegativeSignNode();
        case NodeKind.BooleanOr: return new BooleanOrNode();
        case NodeKind.BooleanAnd: return new BooleanAndNode();
        case NodeKind.Boolean: return new BooleanNode();
        case NodeKind.DirectAssignment: return new DirectAssignmentNode();
        case NodeKind.AddAssignment: return new AddAssignmentNode();
        case NodeKind.SubtractAssignment: return new SubtractAssignmentNode();
        case NodeKind.MultiplyAssignment: return new MultiplyAssignmentNode();
        case NodeKind.DivideAssignment: return new DivideAssignmentNode();
        case NodeKind.ModuloAssignment: return new ModuloAssignmentNode();
        case NodeKind.Add: return new AddNode();
        case NodeKind.Subtract: return new SubtractNode();
        case NodeKind.Multiply: return new MultiplyNode();
        case NodeKind.Divide: return new DivideNode();
        case NodeKind.Modulo: return new ModuloNode();
        case NodeKind.Integer: return new IntegerNode();
        case NodeKind.Float: return new FloatNode();
        case NodeKind.String: return new StringNode();
        case NodeKind.MultiplyAdd: return new MultiplyAddNode();
        case NodeKind.AddMultiply: return new AddMultiplyNode();
        case NodeKind.MultiplySubtract: return new MultiplySubtractNode();
        case NodeKind.SubtractMultiply: return new SubtractMultiplyNode();
        default:
          throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
      }
    }
  }
}
