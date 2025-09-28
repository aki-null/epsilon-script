using System.Collections.Generic;
using EpsilonScript.Helper;

namespace EpsilonScript.Tests.ScriptSystem
{
  public class VariableContainerBuilder
  {
    private readonly Dictionary<uint, VariableValue> _variables = new Dictionary<uint, VariableValue>();

    public VariableContainerBuilder WithInteger(string name, int value)
    {
      _variables[name.GetUniqueIdentifier()] = new VariableValue(value);
      return this;
    }

    public VariableContainerBuilder WithFloat(string name, float value)
    {
      _variables[name.GetUniqueIdentifier()] = new VariableValue(value);
      return this;
    }

    public VariableContainerBuilder WithBoolean(string name, bool value)
    {
      _variables[name.GetUniqueIdentifier()] = new VariableValue(value);
      return this;
    }

    public VariableContainerBuilder WithString(string name, string value)
    {
      _variables[name.GetUniqueIdentifier()] = new VariableValue(value);
      return this;
    }

    public VariableContainerBuilder WithValue(string name, VariableValue value)
    {
      _variables[name.GetUniqueIdentifier()] = value;
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