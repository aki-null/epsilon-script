using EpsilonScript.AST.Literal;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Boolean
{
  /// <summary>
  /// Boolean AND operation: left && right
  /// Short-circuits evaluation: if left is false, right is not evaluated.
  /// </summary>
  internal sealed class BooleanAndNode : BooleanOperationNode
  {
    internal override int Lower(RegisterCompiler compiler) =>
      compiler.LowerBoolean(this, OpCode.AndLeft, LeftNode, RightNode);

    public override Node Optimize()
    {
      // Check for early short-circuit opportunities before optimizing children
      // This prevents unnecessary work when we can already determine the result
      EnsureExecuted(RightNode);
      EnsureExecuted(LeftNode);

      // false && anything => false (don't need to optimize right side)
      if (LeftNode.IsPrecomputable && LeftNode.ValueType == ExtendedType.Boolean && !LeftNode.BooleanValue)
      {
        return CreateBooleanNode(false);
      }

      // anything && false => false (don't need to optimize left side)
      if (RightNode.IsPrecomputable && RightNode.ValueType == ExtendedType.Boolean && !RightNode.BooleanValue)
      {
        return CreateBooleanNode(false);
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
      // true && expression => expression
      if (LeftNode.IsPrecomputable && LeftNode.ValueType == ExtendedType.Boolean && LeftNode.BooleanValue)
      {
        return RightNode;
      }

      // expression && true => expression
      if (RightNode.IsPrecomputable && RightNode.ValueType == ExtendedType.Boolean && RightNode.BooleanValue)
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