using System;
using System.Collections.Generic;
using EpsilonScript.Function;

namespace EpsilonScript
{
  /// <summary>
  /// Compiler wrapper that caches compiled scripts when the same
  /// source/options/variable container are used.
  /// </summary>
  /// <remarks>
  /// Thread Safety: Not thread-safe. Use from a single thread or add your own synchronization if needed.
  /// </remarks>
  public sealed class CachingCompiler
  {
    private readonly Compiler _compiler;
    private readonly Dictionary<CachingCompilerKey, CompiledScript> _cache;
    private readonly CachingCompilerKeyComparer _keyComparer = CachingCompilerKeyComparer.Instance;

    public CachingCompiler()
    {
      _compiler = new Compiler();
      _cache = new Dictionary<CachingCompilerKey, CompiledScript>(_keyComparer);
    }

    public CachingCompiler(Compiler.IntegerPrecision integerPrecision, Compiler.FloatPrecision floatPrecision)
    {
      _compiler = new Compiler(integerPrecision, floatPrecision);
      _cache = new Dictionary<CachingCompilerKey, CompiledScript>(_keyComparer);
    }

    public CompiledScript Compile(string source, Compiler.Options options = Compiler.Options.None,
      IVariableContainer variables = null)
    {
      return Compile(CachedSourceText.From(source), options, variables);
    }

    public CompiledScript Compile(CachedSourceText cachedSource, Compiler.Options options = Compiler.Options.None,
      IVariableContainer variables = null)
    {
      var key = new CachingCompilerKey(
        cachedSource.Source,
        cachedSource.Hash,
        options,
        variables);

      if (_cache.TryGetValue(key, out var cached))
      {
        return cached;
      }

      var compiled = _compiler.Compile(key.Source, key.Options, key.Variables);
      _cache[key] = compiled;
      return compiled;
    }

    /// <summary>
    /// Clears cached scripts. Call this if the caller mutates shared state that affects compilation.
    /// </summary>
    public void Clear()
    {
      _cache.Clear();
    }

    /// <summary>
    /// Adds a custom function and invalidates cached scripts.
    /// </summary>
    public void AddCustomFunction(CustomFunction func)
    {
      _compiler.AddCustomFunction(func);
      InvalidateForCustomFunctionChange();
    }

    /// <summary>
    /// Adds multiple custom functions and invalidates cached scripts.
    /// </summary>
    public void AddCustomFunctionRange(IEnumerable<CustomFunction> functions)
    {
      _compiler.AddCustomFunctionRange(functions);
      InvalidateForCustomFunctionChange();
    }

    private void InvalidateForCustomFunctionChange()
    {
      // Needed because constant folding picks an overload at compile-time.
      // Example: only float overload exists, int call folds via float. Later an int overload is added;
      // without cache clear, recompiling through the cache would keep the float-folded version.
      _cache.Clear();
    }
  }
}