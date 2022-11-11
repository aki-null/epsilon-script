using System.Runtime.InteropServices;

namespace EpsilonScript.Bytecode
{
  [StructLayout(LayoutKind.Explicit)]
  internal struct Instruction
  {
    [FieldOffset(0)] public int IntegerValue;
    [FieldOffset(0)] public float FloatValue;
    [FieldOffset(0)] public bool BooleanValue;

    [FieldOffset(sizeof(int))] public InstructionType Type;

    [FieldOffset(sizeof(int) + sizeof(InstructionType) + sizeof(byte) * 0)]
    public byte reg0;

    [FieldOffset(sizeof(int) + sizeof(InstructionType) + sizeof(byte) * 1)]
    public byte reg1;

    [FieldOffset(sizeof(int) + sizeof(InstructionType) + sizeof(byte) * 2)]
    public byte reg2;
  }
}