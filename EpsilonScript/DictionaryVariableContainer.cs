using System.Collections;
using System.Collections.Generic;

namespace EpsilonScript
{
  public class DictionaryVariableContainer : IVariableContainer, IDictionary<int, VariableValue>
  {
    private readonly IDictionary<int, VariableValue> _container = new Dictionary<int, VariableValue>();

    public void Add(int key, VariableValue value)
    {
      _container.Add(key, value);
    }

    public bool ContainsKey(int key)
    {
      return _container.ContainsKey(key);
    }

    public bool Remove(int key)
    {
      return _container.Remove(key);
    }

    bool IDictionary<int, VariableValue>.TryGetValue(int key, out VariableValue value)
    {
      return _container.TryGetValue(key, out value);
    }

    public VariableValue this[int key]
    {
      get => _container[key];
      set => _container[key] = value;
    }

    public ICollection<int> Keys => _container.Keys;
    public ICollection<VariableValue> Values => _container.Values;

    public bool TryGetValue(int variableKey, out VariableValue variableValue)
    {
      return _container.TryGetValue(variableKey, out variableValue);
    }

    public IEnumerator<KeyValuePair<int, VariableValue>> GetEnumerator()
    {
      return _container.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add(KeyValuePair<int, VariableValue> item)
    {
      _container.Add(item);
    }

    public void Clear()
    {
      _container.Clear();
    }

    public bool Contains(KeyValuePair<int, VariableValue> item)
    {
      return _container.Contains(item);
    }

    public void CopyTo(KeyValuePair<int, VariableValue>[] array, int arrayIndex)
    {
      _container.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<int, VariableValue> item)
    {
      return _container.Remove(item);
    }

    public int Count => _container.Count;
    public bool IsReadOnly => _container.IsReadOnly;
  }
}