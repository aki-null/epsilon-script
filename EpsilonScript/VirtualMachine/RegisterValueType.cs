using System.Runtime.CompilerServices;

namespace EpsilonScript.VirtualMachine
{
  internal enum RegisterValueType : byte
  {
    Integer = 0,
    Float = 1 << 0,

    // Warning: 1 << 1 is intentionally made vacant for certain branching performance improvement in dynamic arithmetic
    Boolean = 1 << 2,
    String = 1 << 3,
    StringStack = 1 << 4,
    Variable = 1 << 5
  }

  internal static class StackValueTypeExtension
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNumber(this RegisterValueType t)
    {
      return t <= RegisterValueType.Float;
    }
  }
}