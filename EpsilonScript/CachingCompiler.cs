using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EpsilonScript.Function;

namespace EpsilonScript
{
  /// <summary>
  /// Compiler wrapper that compiles each source once per options and compile-time variable container, and returns
  /// the same script for later requests.
  /// </summary>
  /// <remarks>
  /// Every caller with the same key shares one CompiledScript, including its result values: executing it through
  /// one reference changes what the others read. Thread Safety: Not thread-safe. Use from a single thread or add
  /// your own synchronization, and do not execute a shared script concurrently.
  /// </remarks>
  public sealed class CachingCompiler
  {
    private readonly Compiler _compiler;
    private readonly Dictionary<CachingCompilerKey, CompiledScript> _cache;
    private readonly CachingCompilerKeyComparer _keyComparer = CachingCompilerKeyComparer.Instance;

    // The compiler's function set the cached scripts were compiled against
    private int _functionsVersion;

    // Content hashes of recently seen source strings, by reference. Sources loaded once are passed as the same
    // string instance on every call, so a hit skips hashing the whole source. Direct-mapped and fixed in size, so
    // callers building a new string per call only overwrite slots.
    private const int SourceSlotCount = 64;
    private readonly string[] _sourceStrings = new string[SourceSlotCount];
    private readonly int[] _sourceHashes = new int[SourceSlotCount];

    public int Count => _cache.Count;

    public CachingCompiler() : this(new Compiler(), 0)
    {
    }

    public CachingCompiler(int capacity) : this(new Compiler(), capacity)
    {
    }

    public CachingCompiler(Compiler.IntegerPrecision integerPrecision, Compiler.FloatPrecision floatPrecision)
      : this(new Compiler(integerPrecision, floatPrecision), 0)
    {
    }

    public CachingCompiler(Compiler.IntegerPrecision integerPrecision, Compiler.FloatPrecision floatPrecision,
      int capacity) : this(new Compiler(integerPrecision, floatPrecision), capacity)
    {
    }

    /// <summary>
    /// Caches scripts compiled by an existing compiler. Functions may be added to it directly; the cache notices
    /// and drops scripts compiled before.
    /// </summary>
    public CachingCompiler(Compiler compiler) : this(compiler, 0)
    {
    }

    public CachingCompiler(Compiler compiler, int capacity)
    {
      if (capacity < 0)
      {
        throw new ArgumentOutOfRangeException(nameof(capacity));
      }

      _compiler = compiler ?? throw new ArgumentNullException(nameof(compiler));
      _functionsVersion = compiler.FunctionsVersion;
      _cache = capacity > 0
        ? new Dictionary<CachingCompilerKey, CompiledScript>(capacity, _keyComparer)
        : new Dictionary<CachingCompilerKey, CompiledScript>(_keyComparer);
    }

    /// <summary>
    /// The compiler that compiles cache misses.
    /// </summary>
    public Compiler Compiler => _compiler;

    /// <summary>
    /// Returns the script cached for this source, options and compile-time variable container, compiling it on
    /// first use. The script is shared with every other caller of the same key.
    /// </summary>
    public CompiledScript GetOrCompile(string source, Compiler.Options options = Compiler.Options.None,
      IVariableContainer variables = null)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      var slot = RuntimeHelpers.GetHashCode(source) & (SourceSlotCount - 1);
      if (!ReferenceEquals(_sourceStrings[slot], source))
      {
        _sourceStrings[slot] = source;
        _sourceHashes[slot] = CachedSourceText.From(source).Hash;
      }

      return GetOrCompile(new CachedSourceText(source, _sourceHashes[slot]), options, variables);
    }

    /// <summary>
    /// GetOrCompile with a source hashed in advance, which skips hashing it on every call.
    /// </summary>
    public CompiledScript GetOrCompile(CachedSourceText cachedSource, Compiler.Options options = Compiler.Options.None,
      IVariableContainer variables = null)
    {
      // Constant folding picks overloads at compile time: a script compiled before a function was added may
      // fold differently now, so it must not be returned
      if (_functionsVersion != _compiler.FunctionsVersion)
      {
        _cache.Clear();
        _functionsVersion = _compiler.FunctionsVersion;
      }

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
      Array.Clear(_sourceStrings, 0, SourceSlotCount);
    }

    /// <summary>
    /// Releases excess cache storage. Useful after Clear() when the caller wants to return memory.
    /// </summary>
    public void TrimExcess()
    {
      _cache.TrimExcess();
    }

    /// <summary>
    /// Adds a custom function to the compiler. Scripts cached before are dropped on the next lookup.
    /// </summary>
    public void AddCustomFunction(CustomFunction func)
    {
      _compiler.AddCustomFunction(func);
    }

    /// <summary>
    /// Adds custom functions to the compiler. Scripts cached before are dropped on the next lookup, even when
    /// adding one throws.
    /// </summary>
    public void AddCustomFunctionRange(IEnumerable<CustomFunction> functions)
    {
      _compiler.AddCustomFunctionRange(functions);
    }
  }
}
