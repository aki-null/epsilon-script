using System.Runtime.InteropServices;

namespace EpsilonScript.VirtualMachine
{
  internal enum OpCode : byte
  {
    LoadVariable,
    // Resolves an assignment target and records its VariableValue for the assignment, without loading its value.
    // Runs before the value, so an undefined or unsupported target fails first.
    BindVariable,
    Move,
    // Assignments: B is the value register, C the target's variable index (bound by BindVariable)
    AssignDirect,
    AssignAdd,
    AssignSubtract,
    AssignMultiply,
    AssignDivide,
    AssignModulo,
    AssignNonVariable, // Assignment whose target is not a variable: always an error, after both sides ran
    MakeTuple, // Marks the destination as a tuple; its elements were evaluated by the preceding instructions
    Call, // Custom function call; arguments are the site's operand registers

    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    MultiplyAdd, // a * b + c
    AddMultiply, // c + a * b
    MultiplySubtract, // a * b - c
    SubtractMultiply, // c - a * b
    Negate,
    Positive,
    Not,

    Equal,
    NotEqual,
    LessThan,
    LessThanOrEqual,
    GreaterThan,
    GreaterThanOrEqual,

    AndLeft, // Short-circuits to C when A is false
    OrLeft, // Short-circuits to C when A is true
    BooleanRight, // Result of && or ||: the right operand B, once the left side did not short-circuit
    Halt, // Ends a reused program whose code array is longer than its code
  }

  /// <summary>
  /// A register value. Holds no references so register writes need no GC write barrier; strings and decimals
  /// live in side arrays indexed by the same register number.
  /// </summary>
  [StructLayout(LayoutKind.Explicit)]
  internal struct Register
  {
    [FieldOffset(0)]
    public long Integer;

    [FieldOffset(0)]
    public double Float;

    [FieldOffset(8)]
    public ExtendedType Type;
  }

  internal struct Instruction
  {
    public OpCode Op;

    public int Destination;
    public int A;
    public int B;
    public int C;

    // LoadVariable and BindVariable: index into the program's variables. Call: index into its functions. Otherwise:
    // index into its locations for errors; multiply-add forms report multiply errors at Site + 1. Assignments keep
    // their variable index in C.
    public int Site;
  }

  /// <summary>
  /// Call site state: the overload resolved for the last argument types, as FunctionNode caches it.
  /// </summary>
  internal sealed class FunctionSite
  {
    private static readonly int[] NoArguments = new int[0];

    public Function.CustomFunctionOverload Overload;
    public int[] Arguments = NoArguments;
    public IVariableContainer CompileTimeVariables;
    public SourceLocation Location;
    public Function.CustomFunction Function;
    public Function.PackedParameterTypes PackedTypes;
    public int Version;
    public bool IsContextual;

    /// <summary>
    /// Sets the site up for a call with the given argument count, keeping the argument array when it fits. A site
    /// is reset only by the lowering of the program that owns it, or of the next evaluation that reuses it.
    /// </summary>
    public void Reset(Function.CustomFunctionOverload overload, int argumentCount,
      IVariableContainer compileTimeVariables, SourceLocation location)
    {
      Overload = overload;
      if (Arguments.Length != argumentCount)
      {
        Arguments = argumentCount == 0 ? NoArguments : new int[argumentCount];
      }

      CompileTimeVariables = compileTimeVariables;
      Location = location;
      Function = null;
      PackedTypes = default;
      Version = 0;
      IsContextual = false;
    }

    // Drops the references of a reused evaluation site
    public void Release()
    {
      Overload = null;
      CompileTimeVariables = null;
      Function = null;
    }

    public Function.CustomFunction Resolve(Function.PackedParameterTypes packedTypes)
    {
      var function = Overload.Find(packedTypes);
      if (function == null)
      {
        throw new RuntimeException(Overload.CreateMismatchMessage(packedTypes), Location);
      }

      Function = function;
      PackedTypes = packedTypes;
      Version = Overload.Version;
      IsContextual = function.IsContextual;
      return function;
    }
  }
}
