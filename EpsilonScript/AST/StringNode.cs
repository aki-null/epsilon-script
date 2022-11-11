using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class StringNode : Node
  {
    private string _stringValue;

    private void Initialize(string value)
    {
      _stringValue = value;
    }

    public StringNode()
    {
    }

    public StringNode(string value)
    {
      Initialize(value);
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      var span = element.Token.Text;
      // Slicing accounts for quotation marks
      Initialize(span.Slice(1, span.Length - 2).ToString());
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      PushValue(program, ref nextRegisterIdx, _stringValue);
    }
  }
}