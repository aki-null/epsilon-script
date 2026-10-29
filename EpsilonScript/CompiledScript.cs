using EpsilonScript.VirtualMachine;

namespace EpsilonScript
{
  /// <summary>
  /// Represents a compiled EpsilonScript that can be executed.
  /// </summary>
  /// <remarks>
  /// Thread Safety: Execute() is NOT thread-safe due to mutable state.
  /// </remarks>
  public class CompiledScript
  {
    private readonly RegisterProgram _program;
    private bool _isResultCached;
    public Type ValueType { get; private set; } = Type.Undefined;
    public bool IsPrecomputable { get; }

    internal RegisterProgram Program => _program;

    public int IntegerValue => _program.ReadInteger(_program.ResultRegister);
    public long LongValue => _program.ReadLong(_program.ResultRegister);
    public float FloatValue => _program.ReadFloat(_program.ResultRegister);
    public double DoubleValue => _program.ReadDouble(_program.ResultRegister);
    public decimal DecimalValue => _program.ReadDecimal(_program.ResultRegister);
    public bool BooleanValue => _program.ReadBoolean(_program.ResultRegister);
    public string StringValue => _program.ReadString(_program.ResultRegister);

    public Compiler.IntegerPrecision IntegerPrecision { get; }

    public Compiler.FloatPrecision FloatPrecision { get; }

    internal CompiledScript(RegisterProgram program, bool isPrecomputable,
      Compiler.IntegerPrecision integerPrecision, Compiler.FloatPrecision floatPrecision)
    {
      IntegerPrecision = integerPrecision;
      FloatPrecision = floatPrecision;
      IsPrecomputable = isPrecomputable;
      _program = program;
    }

    public void Execute(IVariableContainer variablesOverride = null)
    {
      // Do not execute this again if the script is precomputable and has been executed at least once.
      if (IsPrecomputable && _isResultCached)
      {
        return;
      }

      var program = _program;
      program.Execute(variablesOverride);
      // Direct cast from ExtendedType to Type is safe, because they have identical values.
      ValueType = (Type)program.ResultType;
      _isResultCached = true;
    }
  }
}