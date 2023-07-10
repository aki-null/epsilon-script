using EpsilonScript.AST;
using EpsilonScript.Bytecode;
using Xunit;

namespace EpsilonScript.Tests
{
  public static class ASTTestHelper
  {
    internal static void AssertSingleInstructionProgram(Instruction expected, MutableProgram actualProgram)
    {
      Assert.Equal(1, actualProgram.Instructions.Count);
      AssertEqualInstruction(expected, actualProgram.Instructions[0]);
    }
    
    internal static void AssertEqualInstruction(Instruction expected, Instruction actual)
    {
      Assert.Equal(expected.Type, actual.Type);
      // The instruction may not contain an integer data, but comparison should work fine
      Assert.Equal(expected.IntegerValue, actual.IntegerValue);
      Assert.Equal(expected.reg0, actual.reg0);
      Assert.Equal(expected.reg1, actual.reg1);
      Assert.Equal(expected.reg2, actual.reg2);
    }

    internal static void ValidateDependencyProgramEncode(ref byte nextRegisterIdx, out int instructionsCount,
        EpsilonScript.Bytecode.MutableProgram actualProgram, params Node[] nodes)
    {
        instructionsCount = 0;
        var depProgram = PrepareDependencyProgram(ref nextRegisterIdx, nodes);
        for (var i = 0; i < depProgram.Instructions.Count; ++i)
        {
            var expected = depProgram.Instructions[i];
            var actual = actualProgram.Instructions[i];
            AssertEqualInstruction(expected, actual);
        }
        instructionsCount = depProgram.Instructions.Count;
    }

    private static EpsilonScript.Bytecode.MutableProgram PrepareDependencyProgram(ref byte nextRegisterIdx,
      params Node[] nodes)
    {
      var constantVm = new VirtualMachine.VirtualMachine();
      var prog = new EpsilonScript.Bytecode.MutableProgram(null);
      foreach (var node in nodes)
      {
        node.Encode(prog, ref nextRegisterIdx, constantVm);
      }
      return prog;
    }
  }
}