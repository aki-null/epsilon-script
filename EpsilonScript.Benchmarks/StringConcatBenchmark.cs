using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace EpsilonScript.Benchmarks;

/// <summary>
/// Benchmarks string concatenation with various numeric types across different precision configurations.
/// Tests the performance of culture-invariant number formatting with reusable buffers.
/// </summary>
[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[RankColumn]
public class StringConcatBenchmark
{
  private CompiledScript _scriptIntFloat = null!;
  private CompiledScript _scriptIntDouble = null!;
  private CompiledScript _scriptIntDecimal = null!;
  private CompiledScript _scriptLongFloat = null!;
  private CompiledScript _scriptLongDouble = null!;
  private CompiledScript _scriptLongDecimal = null!;

  private DictionaryVariableContainer _variables = null!;

  /// <summary>
  /// String concatenation with various numeric types:
  /// "Value: " + intVar + ", Float: " + floatVar + ", Result: " + (intVar * floatVar)
  /// </summary>
  private const string ConcatExpression = "'Value: ' + intVar + ', Float: ' + floatVar + ', Result: ' + (intVar * floatVar)";

  [GlobalSetup]
  public void Setup()
  {
    // Initialize variables with non-constant values
    _variables = new DictionaryVariableContainer
    {
      ["intVar"] = new VariableValue(42),
      ["floatVar"] = new VariableValue(3.14159f)
    };

    // Compile for each precision combination
    var compilerIntFloat = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
    _scriptIntFloat = compilerIntFloat.Compile(ConcatExpression, Compiler.Options.Immutable, _variables);

    var compilerIntDouble = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double);
    _scriptIntDouble = compilerIntDouble.Compile(ConcatExpression, Compiler.Options.Immutable, _variables);

    var compilerIntDecimal = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal);
    _scriptIntDecimal = compilerIntDecimal.Compile(ConcatExpression, Compiler.Options.Immutable, _variables);

    var compilerLongFloat = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float);
    _scriptLongFloat = compilerLongFloat.Compile(ConcatExpression, Compiler.Options.Immutable, _variables);

    var compilerLongDouble = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double);
    _scriptLongDouble = compilerLongDouble.Compile(ConcatExpression, Compiler.Options.Immutable, _variables);

    var compilerLongDecimal = new Compiler(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Decimal);
    _scriptLongDecimal = compilerLongDecimal.Compile(ConcatExpression, Compiler.Options.Immutable, _variables);
  }

  [Benchmark(Baseline = true)]
  public string Integer_Float()
  {
    _scriptIntFloat.Execute();
    return _scriptIntFloat.StringValue;
  }

  [Benchmark]
  public string Integer_Double()
  {
    _scriptIntDouble.Execute();
    return _scriptIntDouble.StringValue;
  }

  [Benchmark]
  public string Integer_Decimal()
  {
    _scriptIntDecimal.Execute();
    return _scriptIntDecimal.StringValue;
  }

  [Benchmark]
  public string Long_Float()
  {
    _scriptLongFloat.Execute();
    return _scriptLongFloat.StringValue;
  }

  [Benchmark]
  public string Long_Double()
  {
    _scriptLongDouble.Execute();
    return _scriptLongDouble.StringValue;
  }

  [Benchmark]
  public string Long_Decimal()
  {
    _scriptLongDecimal.Execute();
    return _scriptLongDecimal.StringValue;
  }
}
