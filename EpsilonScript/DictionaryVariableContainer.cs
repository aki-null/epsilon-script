using System.Collections;
using System.Collections.Generic;

namespace EpsilonScript
{
  public class DictionaryVariableContainer : IVariableContainer, IDictionary<uint, VariableValue>
  {
    private readonly IDictionary<uint, VariableValue> _container = new Dictionary<uint, VariableValue>();

    public void Add(uint key, VariableValue value)
    {
      _container.Add(key, value);
    }

    public bool ContainsKey(uint key)
    {
      return _container.ContainsKey(key);
    }

    public bool Remove(uint key)
    {
      return _container.Remove(key);
    }

    bool IDictionary<uint, VariableValue>.TryGetValue(uint key, out VariableValue value)
    {
      return _container.TryGetValue(key, out value);
    }

    public VariableValue this[uint key]
    {
      get => _container[key];
      set => _container[key] = value;
    }

    public ICollection<uint> Keys => _container.Keys;
    public ICollection<VariableValue> Values => _container.Values;

    bool IVariableContainer.TryGetValue(uint variableKey, out VariableValue variableValue)
    {
      return _container.TryGetValue(variableKey, out variableValue);
    }

    public IEnumerator<KeyValuePair<uint, VariableValue>> GetEnumerator()
    {
      return _container.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add(KeyValuePair<uint, VariableValue> item)
    {
      _container.Add(item);
    }

    public void Clear()
    {
      _container.Clear();
    }

    public bool Contains(KeyValuePair<uint, VariableValue> item)
    {
      return _container.Contains(item);
    }

    public void CopyTo(KeyValuePair<uint, VariableValue>[] array, int arrayIndex)
    {
      _container.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<uint, VariableValue> item)
    {
      return _container.Remove(item);
    }

    public int Count => _container.Count;
    public bool IsReadOnly => _container.IsReadOnly;
  }
}