using System.Collections.Generic;
using EpsilonScript.Helper;

namespace EpsilonScript
{
  public class DictionaryVariableContainer : IVariableContainer
  {
    private readonly IDictionary<int, VariableValue> _container = new Dictionary<int, VariableValue>();

    public void Add(int key, VariableValue value)
    {
      _container.Add(key, value);
    }

    public void Add(string key, VariableValue value)
    {
      _container.Add(key.GetUniqueIdentifier(), value);
    }

    public bool ContainsKey(int key)
    {
      return _container.ContainsKey(key);
    }

    public bool ContainsKey(string key)
    {
      return _container.ContainsKey(key.GetUniqueIdentifier());
    }

    public bool Remove(int key)
    {
      return _container.Remove(key);
    }

    public bool Remove(string key)
    {
      return _container.Remove(key.GetUniqueIdentifier());
    }

    public bool TryGetValue(int key, out VariableValue value)
    {
      return _container.TryGetValue(key, out value);
    }

    public bool TryGetValue(string key, out VariableValue value)
    {
      return _container.TryGetValue(key.GetUniqueIdentifier(), out value);
    }

    public VariableValue this[int key]
    {
      get => _container[key];
      set => _container[key] = value;
    }

    public VariableValue this[string key]
    {
      get => _container[key.GetUniqueIdentifier()];
      set => _container[key.GetUniqueIdentifier()] = value;
    }

    public void Clear()
    {
      _container.Clear();
    }

    public int Count => _container.Count;
  }
}