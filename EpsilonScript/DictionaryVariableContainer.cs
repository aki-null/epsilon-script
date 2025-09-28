using System.Collections;
using System.Collections.Generic;

namespace EpsilonScript
{
  public class DictionaryVariableContainer : IVariableContainer, IDictionary<VariableId, VariableValue>
  {
    private readonly IDictionary<VariableId, VariableValue> _container = new Dictionary<VariableId, VariableValue>();

    public void Add(VariableId key, VariableValue value)
    {
      _container.Add(key, value);
    }

    public bool ContainsKey(VariableId key)
    {
      return _container.ContainsKey(key);
    }

    public bool Remove(VariableId key)
    {
      return _container.Remove(key);
    }

    bool IDictionary<VariableId, VariableValue>.TryGetValue(VariableId key, out VariableValue value)
    {
      return _container.TryGetValue(key, out value);
    }

    public VariableValue this[VariableId key]
    {
      get => _container[key];
      set => _container[key] = value;
    }

    public ICollection<VariableId> Keys => _container.Keys;
    public ICollection<VariableValue> Values => _container.Values;

    bool IVariableContainer.TryGetValue(VariableId variableKey, out VariableValue variableValue)
    {
      return _container.TryGetValue(variableKey, out variableValue);
    }

    public IEnumerator<KeyValuePair<VariableId, VariableValue>> GetEnumerator()
    {
      return _container.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add(KeyValuePair<VariableId, VariableValue> item)
    {
      _container.Add(item);
    }

    public void Clear()
    {
      _container.Clear();
    }

    public bool Contains(KeyValuePair<VariableId, VariableValue> item)
    {
      return _container.Contains(item);
    }

    public void CopyTo(KeyValuePair<VariableId, VariableValue>[] array, int arrayIndex)
    {
      _container.CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<VariableId, VariableValue> item)
    {
      return _container.Remove(item);
    }

    public int Count => _container.Count;
    public bool IsReadOnly => _container.IsReadOnly;
  }
}