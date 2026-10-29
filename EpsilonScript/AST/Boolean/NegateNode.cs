using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST.Boolean
{
  internal sealed class NegateNode : Node
  {
    private Node _childNode;

    public override bool IsPrecomputable => _childNode.IsPrecomputable;

    internal override int Lower(RegisterCompiler compiler) => compiler.LowerOperation(this, OpCode.Not, _childNode);

    internal override void Reset()
    {
      base.Reset();
      _childNode = null;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      ValueType = ExtendedType.Boolean;

      if (!rpnStack.TryPop(out _childNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform negate operation on");
      }
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      _childNode = _childNode.Optimize();
      return this;
    }

    public override void Validate()
    {
      _childNode?.Validate();
    }
  }
}