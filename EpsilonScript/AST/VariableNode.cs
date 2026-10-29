using System.Collections.Generic;
using EpsilonScript.Helper;
using EpsilonScript.AST.Assignment;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST
{
  internal sealed class VariableNode : Node
  {
    private VariableSlot _slot;

    internal override int Lower(RegisterCompiler compiler) => compiler.LowerVariable(ref _slot);

    internal override int LowerAssignmentTarget(RegisterCompiler compiler, AssignmentOperationNode assignment,
      Node value) => compiler.LowerAssignment(assignment, ref _slot, value);

    public override bool IsPrecomputable => false;

    internal override void Reset()
    {
      base.Reset();
      _slot = default;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      var name = UniqueIdentifierExtension.GetUniqueIdentifier(element.Token.Text.FastSpan());
      _slot = new VariableSlot(name, variables, Location);
    }
  }
}