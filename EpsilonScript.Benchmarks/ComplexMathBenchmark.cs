using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace EpsilonScript.Benchmarks;

/// <summary>
/// Benchmarks complex expressions with nested operations and function calls across different precision configurations.
/// Uses variables to prevent AST constant folding optimization.
/// </summary>
[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[RankColumn]
public class ComplexMathBenchmark
{
  private CompiledScript _scriptIntFloat = null!;
  private CompiledScript _scriptIntDouble = null!;
  private CompiledScript _scriptIntDecimal = null!;
  private CompiledScript _scriptLongFloat = null!;
  private CompiledScript _scriptLongDouble = null!;
  private CompiledScript _scriptLongDecimal = null!;

  private DictionaryVariableContainer _variables = null!;

  /// <summary>
  /// Complex expression with:
  /// - Trigonometric functions (sin, cos)
  /// - Power operations (pow)
  /// - Square root (sqrt)
  /// - Nested arithmetic operations
  /// - Multiple variable references
  ///
  /// Example calculation: distance formula with rotation and scaling
  /// sqrt(pow(x * cos(angle) - y * sin(angle), 2) + pow(x * sin(angle) + y * cos(angle), 2)) * scale + offset
  /// </summary>
  private const string ComplexExpression =
    "sqrt(pow(x * cos(angle) - y * sin(angle), 2.0) + pow(x * sin(angle) + y * cos(angle), 2.0)) * scale + offset";

  [GlobalSetup]
  public void Setup()
  {
    // Initialize variables with non-constant values
    _variables = new DictionaryVariableContainer
    {
      ["x"] = new VariableValue(3.0f),
      ["y"] = new VariableValue(4.0f),
      ["angle"] = new VariableValue(0.785398f), // ~45 degrees in radians
      ["scale"] = new VariableValue(2.0f),
      ["offset"] = new VariableValue(10.0f)
    };

    // Compile for each precision combination
    var compilerIntFloat = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
    _scriptIntFloat = compilerIntFloat.Compile(ComplexExpression, Compiler.Options.Immutable, _variables);

    var compilerIntDouble = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
    _scriptIntDouble = compilerIntDouble.Compile(ComplexExpression, Compiler.Options.Immutable, _variables);

    var compilerIntDecimal = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
    _scriptIntDecimal = compilerIntDecimal.Compile(ComplexExpression, Compiler.Options.Immutable, _variables);

    var compilerLongFloat = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
    _scriptLongFloat = compilerLongFloat.Compile(ComplexExpression, Compiler.Options.Immutable, _variables);

    var compilerLongDouble = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double);
    _scriptLongDouble = compilerLongDouble.Compile(ComplexExpression, Compiler.Options.Immutable, _variables);

    var compilerLongDecimal = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Decimal);
    _scriptLongDecimal = compilerLongDecimal.Compile(ComplexExpression, Compiler.Options.Immutable, _variables);
  }

  [Benchmark(Baseline = true)]
  public float Integer_Float()
  {
    _scriptIntFloat.Execute();
    return _scriptIntFloat.FloatValue;
  }

  [Benchmark]
  public double Integer_Double()
  {
    _scriptIntDouble.Execute();
    return _scriptIntDouble.DoubleValue;
  }

  [Benchmark]
  public decimal Integer_Decimal()
  {
    _scriptIntDecimal.Execute();
    return _scriptIntDecimal.DecimalValue;
  }

  [Benchmark]
  public float Long_Float()
  {
    _scriptLongFloat.Execute();
    return _scriptLongFloat.FloatValue;
  }

  [Benchmark]
  public double Long_Double()
  {
    _scriptLongDouble.Execute();
    return _scriptLongDouble.DoubleValue;
  }

  [Benchmark]
  public decimal Long_Decimal()
  {
    _scriptLongDecimal.Execute();
    return _scriptLongDecimal.DecimalValue;
  }
}
