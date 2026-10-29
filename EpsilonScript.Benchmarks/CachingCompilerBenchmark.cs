using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace EpsilonScript.Benchmarks;

[SimpleJob(RuntimeMoniker.Net90)]
[MemoryDiagnoser]
[RankColumn]
public class CachingCompilerBenchmark
{
  private const string ShortExpression = "damage * 2 + armor";
  private static readonly string LongExpression = BuildLongExpression();

  private Compiler _compiler = null!;
  private CachingCompiler _cache = null!;
  private CachingCompiler _sizedCache = null!;
  private DictionaryVariableContainer _variables = null!;
  private CachedSourceText[] _missSources = null!;
  private CachedSourceText _shortCachedSource;
  private CachedSourceText _longCachedSource;

  [GlobalSetup]
  public void Setup()
  {
    _compiler = new Compiler();
    _cache = new CachingCompiler();
    _sizedCache = new CachingCompiler(capacity: 512);
    _variables = new DictionaryVariableContainer
    {
      ["damage"] = new VariableValue(10),
      ["armor"] = new VariableValue(3),
    };
    _shortCachedSource = CachedSourceText.From(ShortExpression);
    _longCachedSource = CachedSourceText.From(LongExpression);
    _missSources = BuildMissSources();

    _cache.GetOrCompile(ShortExpression, Compiler.Options.Immutable, _variables);
    _cache.GetOrCompile(_shortCachedSource, Compiler.Options.Immutable, _variables);
    _cache.GetOrCompile(_longCachedSource, Compiler.Options.Immutable, _variables);
  }

  [Benchmark(Baseline = true)]
  public CompiledScript Compiler_CompileShort()
  {
    return _compiler.Compile(ShortExpression, Compiler.Options.Immutable, _variables);
  }

  [Benchmark]
  public CompiledScript CachingCompiler_StringHitShort()
  {
    return _cache.GetOrCompile(ShortExpression, Compiler.Options.Immutable, _variables);
  }

  [Benchmark]
  public CompiledScript CachingCompiler_CachedSourceHitShort()
  {
    return _cache.GetOrCompile(_shortCachedSource, Compiler.Options.Immutable, _variables);
  }

  [Benchmark]
  public CompiledScript CachingCompiler_CachedSourceHitLong()
  {
    return _cache.GetOrCompile(_longCachedSource, Compiler.Options.Immutable, _variables);
  }

  [Benchmark(OperationsPerInvoke = 512)]
  public CompiledScript CachingCompiler_MissSized()
  {
    _sizedCache.Clear();
    CompiledScript compiled = null!;
    for (var i = 0; i < _missSources.Length; i++)
    {
      compiled = _sizedCache.GetOrCompile(_missSources[i], Compiler.Options.Immutable, _variables);
    }

    return compiled;
  }

  [Benchmark]
  public CachedSourceText CachedSourceText_FromLong()
  {
    return CachedSourceText.From(LongExpression);
  }

  private static string BuildLongExpression()
  {
    var builder = new System.Text.StringBuilder("damage");
    for (var i = 0; i < 64; i++)
    {
      builder.Append(" + armor * ");
      builder.Append(i + 1);
    }

    return builder.ToString();
  }

  private static CachedSourceText[] BuildMissSources()
  {
    var sources = new CachedSourceText[512];
    for (var i = 0; i < sources.Length; i++)
    {
      sources[i] = CachedSourceText.From("damage + " + i.ToString(System.Globalization.CultureInfo.InvariantCulture));
    }

    return sources;
  }
}
