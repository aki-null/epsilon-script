using System.Runtime.CompilerServices;

namespace EpsilonScript.VirtualMachine
{
  /// <summary>
  /// A variable name bound to the compile-time container. VariableNode embeds one, and lowering copies it into the
  /// program for each LoadVariable and BindVariable instruction, so resolution is shared without a separate allocation.
  /// Calls must go through the field or array element so the cache updates in place.
  /// </summary>
  internal struct VariableSlot
  {
    private VariableId _name;
    private IVariableContainer _variables;
    private SourceLocation _location;

    // Fast path for the common case of a compile-time DictionaryVariableContainer: the resolved entry is reused
    // until the container is mutated. Avoids a hash lookup and the GC write barriers IL2CPP emits for out params.
    private DictionaryVariableContainer _dictionaryVariables;
    private VariableValue _cachedVariable;
    private int _cachedVersion;

    public VariableSlot(VariableId name, IVariableContainer variables, SourceLocation location)
    {
      _name = name;
      _variables = variables;
      _dictionaryVariables = DictionaryVariableContainer.AsExact(variables);
      _location = location;
      _cachedVariable = null;
      _cachedVersion = 0;
    }

    // The compile-time variable when it is cached and the container has not changed since, otherwise null
    private VariableValue CachedVariable
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _cachedVariable != null && _cachedVersion == _dictionaryVariables.Version ? _cachedVariable : null;
    }

    /// <summary>
    /// Returns the variable from the override container, or else from the compile-time container. Throws for an
    /// undefined variable. An entry is used even when its value is null; reading it then throws.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VariableValue Resolve(IVariableContainer variablesOverride)
    {
      return (variablesOverride == null ? CachedVariable : null) ?? ResolveUncached(variablesOverride);
    }

    private VariableValue ResolveUncached(IVariableContainer variablesOverride)
    {
      if (variablesOverride != null)
      {
        var overridden = DictionaryVariableContainer.Lookup(variablesOverride, _name, out var overrideFound);
        if (overrideFound)
        {
          return overridden;
        }

        // Not overridden: the compile-time entry may still be cached
        var cached = CachedVariable;
        if (cached != null)
        {
          return cached;
        }
      }

      var dictionary = _dictionaryVariables;
      if (dictionary != null)
      {
        var resolved = dictionary.Find(_name, out var found);
        if (!found)
        {
          throw new RuntimeException($"Undefined variable: {_name}", _location);
        }

        // A null entry is not cached: CachedVariable treats null as a miss
        _cachedVariable = resolved;
        _cachedVersion = dictionary.Version;
        return resolved;
      }

      if (_variables == null || !_variables.TryGetValue(_name, out var variable))
      {
        throw new RuntimeException($"Undefined variable: {_name}", _location);
      }

      return variable;
    }
  }
}
