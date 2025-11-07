using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Boolean
{
  internal sealed class NegateNode : Node
  {
    private Node _childNode;

    public override bool IsPrecomputable => _childNode.IsPrecomputable;

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      ValueType = ExtendedType.Boolean;

      if (!rpnStack.TryPop(out _childNode))
      {
        throw new ParserException(element.Token, "Cannot find value to perform negate operation on");
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _childNode.Execute(variablesOverride);
      if (_childNode.ValueType != ExtendedType.Boolean)
      {
        throw CreateRuntimeException("Cannot negate a non-boolean value");
      }

      BooleanValue = !_childNode.BooleanValue;
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

    public override void ConfigureNoAlloc()
    {
      _childNode?.ConfigureNoAlloc();
    }
  }
}