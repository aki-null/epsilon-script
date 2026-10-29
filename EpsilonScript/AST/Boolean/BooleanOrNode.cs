using EpsilonScript.AST.Literal;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Boolean
{
  /// <summary>
  /// Boolean OR operation: left || right
  /// Short-circuits evaluation: if left is true, right is not evaluated.
  /// </summary>
  internal sealed class BooleanOrNode : BooleanOperationNode
  {
    internal override int Lower(RegisterCompiler compiler) =>
      compiler.LowerBoolean(this, OpCode.OrLeft, LeftNode, RightNode);

    public override Node Optimize()
    {
      // Check for early short-circuit opportunities before optimizing children
      // This prevents unnecessary work when we can already determine the result
      EnsureExecuted(RightNode);
      EnsureExecuted(LeftNode);

      // true || anything => true (don't need to optimize right side)
      if (LeftNode.IsPrecomputable && LeftNode.ValueType == ExtendedType.Boolean && LeftNode.BooleanValue)
      {
        return CreateBooleanNode(true);
      }

      // anything || true => true (don't need to optimize left side)
      if (RightNode.IsPrecomputable && RightNode.ValueType == ExtendedType.Boolean && RightNode.BooleanValue)
      {
        return CreateBooleanNode(true);
      }

      // Optimize child nodes
      LeftNode = LeftNode.Optimize();
      RightNode = RightNode.Optimize();

      // Handle type errors for constant expressions
      if (IsPrecomputable &&
          (LeftNode.ValueType != ExtendedType.Boolean || RightNode.ValueType != ExtendedType.Boolean))
      {
        Execute(null); // This will throw RuntimeException for type mismatches
        return CreateValueNode();
      }

      // Ensure optimized constant nodes are executed
      EnsureExecuted(LeftNode);
      EnsureExecuted(RightNode);

      // Short-circuit optimizations after child optimization
      // false || expression => expression
      if (LeftNode.IsPrecomputable && LeftNode.ValueType == ExtendedType.Boolean && !LeftNode.BooleanValue)
      {
        return RightNode;
      }

      // expression || false => expression
      if (RightNode.IsPrecomputable && RightNode.ValueType == ExtendedType.Boolean && !RightNode.BooleanValue)
      {
        return LeftNode;
      }

      // Constant folding: if both operands are constant boolean, evaluate at compile time
      if (IsPrecomputable && LeftNode.ValueType == ExtendedType.Boolean &&
          RightNode.ValueType == ExtendedType.Boolean)
      {
        Execute(null);
        return CreateValueNode();
      }

      return this;
    }
  }
}