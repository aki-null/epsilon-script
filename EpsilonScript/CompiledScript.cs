using EpsilonScript.AST;

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
    private readonly Node _rootNode;
    private bool _isResultCached;
    public Type ValueType { get; private set; } = Type.Undefined;
    public bool IsPrecomputable { get; }

    /// <summary>
    /// Internal property for testing purposes to verify AST structure and optimizations
    /// </summary>
    internal Node RootNode => _rootNode;

    public int IntegerValue => _rootNode.IntegerValue;
    public long LongValue => _rootNode.LongValue;
    public float FloatValue => _rootNode.FloatValue;
    public double DoubleValue => _rootNode.DoubleValue;
    public decimal DecimalValue => _rootNode.DecimalValue;
    public bool BooleanValue => _rootNode.BooleanValue;
    public string StringValue => _rootNode.StringValue;

    public Compiler.IntegerPrecision IntegerPrecision { get; }

    public Compiler.FloatPrecision FloatPrecision { get; }

    internal CompiledScript(Node rootNode, Compiler.IntegerPrecision integerPrecision,
      Compiler.FloatPrecision floatPrecision)
    {
      _rootNode = rootNode;
      IntegerPrecision = integerPrecision;
      FloatPrecision = floatPrecision;
      // Precomputable status is cached, because this information is gathered from each AST node O(n)
      IsPrecomputable = rootNode.IsPrecomputable;
    }

    public void Execute(IVariableContainer variablesOverride = null)
    {
      // Do not execute this again if the script is precomputable and has been executed at least once.
      if (IsPrecomputable && _isResultCached)
      {
        return;
      }

      _rootNode.Execute(variablesOverride);

      // Direct cast from ExtendedType to Type is safe, because they have identical values.
      ValueType = (Type)_rootNode.ValueType;

      _isResultCached = true;
    }
  }
}