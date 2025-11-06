using System.Collections.Generic;
using System.Text;
using EpsilonScript.Function;

namespace EpsilonScript
{
  /// <summary>
  /// Compiler context containing immutable compiler configuration and reusable resources.
  /// </summary>
  /// <remarks>
  /// Thread Safety: NOT thread-safe. Contains mutable buffers that are reused during compilation and execution.
  /// </remarks>
  internal sealed class CompilerContext
  {
    public Compiler.IntegerPrecision IntegerPrecision { get; }

    public Compiler.FloatPrecision FloatPrecision { get; }

    public IDictionary<VariableId, CustomFunctionOverload> Functions { get; }

    /// <summary>
    /// Reusable StringBuilder for efficient string concatenation without intermediate allocations.
    /// Nodes should clear this before use and read the result immediately after building.
    /// </summary>
    public StringBuilder StringBuilder { get; }

    /// <summary>
    /// Reusable char buffer for culture-invariant number formatting without allocations.
    /// Used with TryFormat to convert numbers to strings with InvariantCulture.
    /// Size of 64 chars handles worst-case formatting (decimal ~31 chars max with default format).
    /// </summary>
    public char[] FormatBuffer { get; }

    /// <summary>
    /// Gets the configured integer type based on precision setting.
    /// </summary>
    public ExtendedType ConfiguredIntegerType { get; }

    /// <summary>
    /// Gets the configured float type based on precision setting.
    /// </summary>
    public ExtendedType ConfiguredFloatType { get; }

    public CompilerContext(Compiler.IntegerPrecision integerPrecision, Compiler.FloatPrecision floatPrecision,
      IDictionary<VariableId, CustomFunctionOverload> functions)
    {
      IntegerPrecision = integerPrecision;
      FloatPrecision = floatPrecision;
      Functions = functions ?? new Dictionary<VariableId, CustomFunctionOverload>();
      StringBuilder = new StringBuilder(256);
      FormatBuffer = new char[64];
      ConfiguredIntegerType = integerPrecision == Compiler.IntegerPrecision.Integer
        ? ExtendedType.Integer
        : ExtendedType.Long;
      ConfiguredFloatType = floatPrecision switch
      {
        Compiler.FloatPrecision.Float => ExtendedType.Float,
        Compiler.FloatPrecision.Double => ExtendedType.Double,
        Compiler.FloatPrecision.Decimal => ExtendedType.Decimal,
        _ => ExtendedType.Float
      };
    }
  }
}