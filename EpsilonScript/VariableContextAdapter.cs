using System.Runtime.CompilerServices;
using EpsilonScript.Function;

namespace EpsilonScript
{
  /// <summary>
  /// Adapter that wraps IVariableContainer instances for contextual function variable access.
  /// </summary>
  internal readonly struct VariableContextAdapter
  {
    private readonly IVariableContainer _overrideContainer;
    private readonly IVariableContainer _compileTimeContainer;

    public VariableContextAdapter(IVariableContainer overrideContainer, IVariableContainer compileTimeContainer)
    {
      _overrideContainer = overrideContainer;
      _compileTimeContainer = compileTimeContainer;
    }

    /// <summary>
    /// The variable from the override container, else the compile-time one. Returned rather than through an out
    /// parameter, which under IL2CPP costs a GC write barrier. An entry is used even when its value is null, which
    /// then fails in the conversion.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public VariableValue GetVariable(VariableId variableId, out bool found)
    {
      found = false;
      VariableValue value = null;
      if (_overrideContainer != null)
      {
        value = DictionaryVariableContainer.Lookup(_overrideContainer, variableId, out found);
      }

      if (!found && _compileTimeContainer != null)
      {
        value = DictionaryVariableContainer.Lookup(_compileTimeContainer, variableId, out found);
      }

      return value;
    }
  }
}