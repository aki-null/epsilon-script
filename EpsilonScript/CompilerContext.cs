using System.Collections.Generic;
using System.Text;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript
{
  /// <summary>
  /// Compiler configuration, reusable buffers, and the evaluator constant folding runs subtrees on.
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

    // Lowers subtrees for Evaluate; separate from the compiler that lowers whole scripts
    private readonly RegisterCompiler _evaluator = new RegisterCompiler();

    // Set while the reused evaluation program runs. A folded function may compile another script with the same
    // compiler, whose folds must then get their own evaluator.
    private bool _evaluating;

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

    /// <summary>
    /// Runs a subtree on the register VM and keeps the result as the node's value, and with inPlace each executed
    /// descendant's as its value. Optimization folds constants with it, so compile-time and run-time evaluation
    /// share one implementation. NoAlloc checks do not apply, so constant concatenation folds in NoAlloc scripts as
    /// well.
    /// </summary>
    internal void Evaluate(Node node, IVariableContainer variablesOverride, bool inPlace)
    {
      var nested = _evaluating;
      var evaluator = nested ? new RegisterCompiler() : _evaluator;
      var program = evaluator.CompileForEvaluation(node, this, inPlace);
      _evaluating = true;
      try
      {
        program.Execute(variablesOverride);
        if (inPlace)
        {
          evaluator.WriteBack();
        }
        else
        {
          node.SetResult(program, program.ResultRegister);
        }
      }
      finally
      {
        _evaluating = nested;
        evaluator.ReleaseEvaluation();
      }
    }
  }
}
