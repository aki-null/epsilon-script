using System.Collections.Generic;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST
{
  internal sealed class SequenceNode : Node
  {
    internal override int Lower(RegisterCompiler compiler) => compiler.LowerSequence(_leftNode, _rightNode);

    private Node _leftNode; // Null when the sequence has a single element
    private Node _rightNode;
    private bool _isSingleNode;

    public override bool IsPrecomputable =>
      _isSingleNode ? _rightNode.IsPrecomputable : (_leftNode.IsPrecomputable && _rightNode.IsPrecomputable);

    internal override void Reset()
    {
      base.Reset();
      _leftNode = null;
      _rightNode = null;
      _isSingleNode = false;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      if (!rpnStack.TryPop(out _rightNode))
      {
        throw new ParserException(element.Token, "Cannot find tokens to sequence");
      }

      // Handle trailing semicolon - if there's only one operand, treat it as a no-op
      if (!rpnStack.TryPop(out _leftNode))
      {
        _isSingleNode = true;
        _leftNode = null;
      }
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      if (_isSingleNode)
      {
        return _rightNode.Optimize();
      }

      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();
      return this;
    }

    public override void Validate()
    {
      _leftNode?.Validate();
      _rightNode?.Validate();
    }
  }
}