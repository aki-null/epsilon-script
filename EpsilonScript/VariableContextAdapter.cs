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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetVariable<T>(VariableId variableId, out T value)
    {
      // Check override container first
      if (_overrideContainer != null && _overrideContainer.TryGetValue(variableId, out var variableValue))
      {
        value = TypeTraits<T>.FromVariableValue(variableValue);
        return true;
      }

      // Fall back to compile-time container
      if (_compileTimeContainer != null && _compileTimeContainer.TryGetValue(variableId, out variableValue))
      {
        value = TypeTraits<T>.FromVariableValue(variableValue);
        return true;
      }

      value = default;
      return false;
    }
  }
}