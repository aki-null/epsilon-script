using EpsilonScript.Function;

namespace EpsilonScript.Bytecode
{
  internal class Program
  {
    public Program(MutableProgram input)
    {
      Instructions = input.Instructions.ToArray();
      StringTable = input.StringTable.ToArray();
      Functions = input.Functions;
    }

    public readonly Instruction[] Instructions;
    public readonly string[] StringTable;
    public readonly CustomFunctionContainer Functions;
  }
}