using System.Runtime.InteropServices;

namespace EpsilonScript
{
  /// <summary>
  /// A struct to hold concrete values
  /// </summary>
  [StructLayout(LayoutKind.Explicit)]
  public struct ConcreteValue
  {
    [FieldOffset(0)] public int IntegerValue;
    [FieldOffset(0)] public float FloatValue;
    [FieldOffset(0)] public bool BooleanValue;

    [FieldOffset(sizeof(int))] public Type Type;

    [FieldOffset(sizeof(int) + sizeof(Type))]
    public string StringValue;
  }
}