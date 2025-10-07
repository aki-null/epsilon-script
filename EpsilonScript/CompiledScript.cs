using EpsilonScript.AST;

namespace EpsilonScript
{
  public class CompiledScript
  {
    private readonly Node _rootNode;
    private bool _isResultCached;
    public Type ValueType { get; private set; } = Type.Undefined;
    public bool IsConstant { get; }

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
      // Constness is cached, because this information is gathered from each AST node O(n)
      IsConstant = rootNode.IsConstant;
    }

    public void Execute(IVariableContainer variablesOverride = null)
    {
      // Do not execute this again if the script is completely constant, and it has been executed at least once.
      if (IsConstant && _isResultCached)
      {
        return;
      }

      _rootNode.Execute(variablesOverride);

      // Direct cast is safe because AST.ValueType and Type enums have identical values
      ValueType = (Type)_rootNode.ValueType;

      _isResultCached = true;
    }
  }
}