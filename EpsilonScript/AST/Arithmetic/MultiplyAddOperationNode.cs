using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Arithmetic
{
  /// <summary>
  /// Abstract base class for multiply-add/subtract operations.
  /// Lowers (a * b) ± c to a single VM instruction.
  /// Concrete subclasses implement specific operation patterns.
  /// </summary>
  internal abstract class MultiplyAddOperationNode : Node
  {
    protected Node Multiplier1; // a in (a * b) ± c
    protected Node Multiplier2; // b in (a * b) ± c
    protected Node Addend; // c in (a * b) ± c

    // Secondary location - for the multiply operation
    // Primary location (base.Location) stores the add/subtract operation
    private SourceLocation _multiplyLocation;

    public override bool IsPrecomputable =>
      Multiplier1.IsPrecomputable && Multiplier2.IsPrecomputable && Addend.IsPrecomputable;

    internal abstract OpCode LoweredOp { get; }

    internal override SourceLocation MultiplyLocation => _multiplyLocation;

    internal override int Lower(RegisterCompiler compiler) =>
      compiler.LowerOperation(this, LoweredOp, Multiplier1, Multiplier2, Addend);

    /// <summary>
    /// Constructor used during optimization phase to create multiply-add node.
    /// </summary>
    internal MultiplyAddOperationNode Initialize(Node addend, MultiplyNode multiplyNode,
      ArithmeticOperationNode addSubtractNode)
    {
      Multiplier1 = multiplyNode.LeftChildNode;
      Multiplier2 = multiplyNode.RightChildNode;
      Addend = addend;

      // Primary location: the add/subtract operation (top-level operation)
      Location = addSubtractNode.Location;

      // Secondary location: the multiply operation (nested operation)
      _multiplyLocation = multiplyNode.Location;
      return this;
    }

    internal override void Reset()
    {
      base.Reset();
      Multiplier1 = null;
      Multiplier2 = null;
      Addend = null;
      _multiplyLocation = default;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      // This node is only created during optimization, never during initial AST build
      throw new InvalidOperationException($"{GetType().Name} cannot be built from RPN stack");
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      Multiplier1 = Multiplier1.Optimize();
      Multiplier2 = Multiplier2.Optimize();
      Addend = Addend.Optimize();

      return this;
    }

    public override void Validate()
    {
      Multiplier1?.Validate();
      Multiplier2?.Validate();
      Addend?.Validate();
    }
  }
}