using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Helper;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal class FunctionNode : Node
  {
    private Node _childNode;
    private int _functionNameId;
    private bool _isFunctionDefinedAndConstant;

    public override bool IsConstant => _isFunctionDefinedAndConstant && AreParametersConstant;

    private bool AreParametersConstant => _childNode.IsConstant;

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions)
    {
      // Unfortunately function name string needs to be allocated here to generate a name ID
      var funcName = element.Token.Text.ToString();
      _functionNameId = funcName.GetUniqueIdentifier();

      // Functions might already be defined and constant, which will allow for a compile time optimization to
      // happen.
      _isFunctionDefinedAndConstant = functions.Query(_functionNameId, out var func) && func.IsConstant;

      if (!rpnStack.TryPop(out _childNode))
      {
        throw new ParserException(element.Token, $"Cannot find parameters for calling function: {funcName}");
      }
    }

    public override void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      if (TryEncodeConstant(program, ref nextRegisterIdx, constantVm))
      {
        return;
      }

      var paramCount = _childNode switch
      {
        NullNode _ => 0,
        TupleNode tupleNode => tupleNode.Count,
        _ => 1
      };

      _childNode.Encode(program, ref nextRegisterIdx, constantVm);

      // reg0 = store address
      // reg1 = function parameter count
      // reg2 = first function parameter register index
      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.CallFunction,
        IntegerValue = _functionNameId,
        reg0 = (byte)(nextRegisterIdx - paramCount),
        reg1 = (byte)paramCount,
        reg2 = (byte)(nextRegisterIdx - paramCount)
      });

      nextRegisterIdx = (byte)(nextRegisterIdx - paramCount + 1);
    }
  }
}