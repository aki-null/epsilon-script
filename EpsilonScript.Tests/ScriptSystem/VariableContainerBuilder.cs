using System.Collections.Generic;

namespace EpsilonScript.Tests.ScriptSystem
{
  public class VariableContainerBuilder
  {
    private readonly Dictionary<VariableId, VariableValue> _variables = new Dictionary<VariableId, VariableValue>();

    public VariableContainerBuilder WithInteger(string name, int value)
    {
      _variables[name] = new VariableValue(value);
      return this;
    }

    public VariableContainerBuilder WithFloat(string name, float value)
    {
      _variables[name] = new VariableValue(value);
      return this;
    }

    public VariableContainerBuilder WithBoolean(string name, bool value)
    {
      _variables[name] = new VariableValue(value);
      return this;
    }

    public VariableContainerBuilder WithString(string name, string value)
    {
      _variables[name] = new VariableValue(value);
      return this;
    }

    public DictionaryVariableContainer Build()
    {
      var container = new DictionaryVariableContainer();
      foreach (var pair in _variables)
      {
        container[pair.Key] = pair.Value;
      }

      return container;
    }
  }
}