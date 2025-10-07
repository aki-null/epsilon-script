using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace EpsilonScript.Benchmarks;

/// <summary>
/// Benchmarks simple arithmetic operations across different precision configurations.
/// Uses variables to prevent AST constant folding optimization.
/// </summary>
[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[RankColumn]
public class SimpleMathBenchmark
{
  private CompiledScript _scriptIntFloat = null!;
  private CompiledScript _scriptIntDouble = null!;
  private CompiledScript _scriptIntDecimal = null!;
  private CompiledScript _scriptLongFloat = null!;
  private CompiledScript _scriptLongDouble = null!;
  private CompiledScript _scriptLongDecimal = null!;

  private DictionaryVariableContainer _variables = null!;

  /// <summary>
  /// Simple arithmetic: (a + b) * c - d / e
  /// Uses 5 variables to prevent constant folding
  /// </summary>
  private const string SimpleExpression = "(a + b) * c - d / e";

  [GlobalSetup]
  public void Setup()
  {
    // Initialize variables with non-constant values
    _variables = new DictionaryVariableContainer
    {
      ["a"] = new VariableValue(10),
      ["b"] = new VariableValue(20),
      ["c"] = new VariableValue(3),
      ["d"] = new VariableValue(100),
      ["e"] = new VariableValue(4)
    };

    // Compile for each precision combination
    var compilerIntFloat = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
    _scriptIntFloat = compilerIntFloat.Compile(SimpleExpression, Compiler.Options.Immutable, _variables);

    var compilerIntDouble = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
    _scriptIntDouble = compilerIntDouble.Compile(SimpleExpression, Compiler.Options.Immutable, _variables);

    var compilerIntDecimal = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
    _scriptIntDecimal = compilerIntDecimal.Compile(SimpleExpression, Compiler.Options.Immutable, _variables);

    var compilerLongFloat = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
    _scriptLongFloat = compilerLongFloat.Compile(SimpleExpression, Compiler.Options.Immutable, _variables);

    var compilerLongDouble = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double);
    _scriptLongDouble = compilerLongDouble.Compile(SimpleExpression, Compiler.Options.Immutable, _variables);

    var compilerLongDecimal = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Decimal);
    _scriptLongDecimal = compilerLongDecimal.Compile(SimpleExpression, Compiler.Options.Immutable, _variables);
  }

  [Benchmark(Baseline = true)]
  public int Integer_Float()
  {
    _scriptIntFloat.Execute();
    return _scriptIntFloat.IntegerValue;
  }

  [Benchmark]
  public int Integer_Double()
  {
    _scriptIntDouble.Execute();
    return _scriptIntDouble.IntegerValue;
  }

  [Benchmark]
  public int Integer_Decimal()
  {
    _scriptIntDecimal.Execute();
    return _scriptIntDecimal.IntegerValue;
  }

  [Benchmark]
  public long Long_Float()
  {
    _scriptLongFloat.Execute();
    return _scriptLongFloat.LongValue;
  }

  [Benchmark]
  public long Long_Double()
  {
    _scriptLongDouble.Execute();
    return _scriptLongDouble.LongValue;
  }

  [Benchmark]
  public long Long_Decimal()
  {
    _scriptLongDecimal.Execute();
    return _scriptLongDecimal.LongValue;
  }
}
