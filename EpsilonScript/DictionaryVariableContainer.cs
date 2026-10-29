using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EpsilonScript
{
  /// <summary>
  /// Dictionary-based implementation of IVariableContainer for storing script variables.
  /// </summary>
  /// <remarks>
  /// Thread Safety: NOT thread-safe. Each thread should use its own instance,
  /// or use external synchronization for shared access.
  /// </remarks>
  public class DictionaryVariableContainer : IVariableContainer, IDictionary<VariableId, VariableValue>
  {
    // Source of truth for the IDictionary API (enumeration order, exceptions, key/value views).
    private readonly Dictionary<VariableId, VariableValue> _container =
      new Dictionary<VariableId, VariableValue>(VariableIdComparer.Instance);

    // Script lookup index mirroring _container. Under IL2CPP, Dictionary lookups go through comparer interface calls
    // and return through an out param that costs a GC write barrier; this index uses inline uint compares and
    // returns the value directly. Built on the first write and updated in place on add, set, remove and clear; a
    // removed entry keeps its key with the Removed marker until the index fills up and is rebuilt. Only writes touch
    // it, so concurrent readers are as safe as with Dictionary.
    private int[] _buckets; // 1-based entry index, 0 = empty; null until the first write
    private uint[] _keys;
    private VariableValue[] _values;
    private int[] _next; // 1-based entry index, 0 = end of chain
    private int _indexCount;
    private int _bucketShift;

    /// <summary>
    /// Incremented on every mutation so variable slots can cache resolved entries.
    /// </summary>
    internal int Version
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)]
      get => _version;
    }

    private int _version;

    // A subclass can re-implement IVariableContainer.TryGetValue, so Find only stands in for it on the exact type.
    // Checked once here: GetType and typeof go through the IL2CPP reflection runtime.
    private readonly bool _isExactType;

    public DictionaryVariableContainer()
    {
      _isExactType = GetType() == typeof(DictionaryVariableContainer);
      // Initializes Removed now: left to the first Find, it would allocate in the first run of a NoAlloc script
      System.GC.KeepAlive(Removed);
    }

    // Index value of a removed entry. Distinct from null, which is a valid stored value.
    private static readonly VariableValue Removed = new VariableValue(Type.Undefined);

    /// <summary>
    /// The container as a DictionaryVariableContainer when Find may stand in for its TryGetValue, otherwise null.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static DictionaryVariableContainer AsExact(IVariableContainer container) =>
      container is DictionaryVariableContainer dictionary && dictionary._isExactType ? dictionary : null;

    /// <summary>
    /// Looks up a variable the way IVariableContainer.TryGetValue does: <paramref name="found"/> is true for an
    /// entry even when its value is null. Uses the index for a DictionaryVariableContainer.
    /// </summary>
    /// <remarks>
    /// Returning the entry instead of using an out parameter avoids the GC write barrier IL2CPP emits for
    /// reference-typed out params.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static VariableValue Lookup(IVariableContainer container, VariableId key, out bool found)
    {
      var dictionary = AsExact(container);
      if (dictionary != null)
      {
        return dictionary.Find(key, out found);
      }

      found = container.TryGetValue(key, out var value);
      return value;
    }

    /// <summary>
    /// Returns the entry for <paramref name="key"/>; <paramref name="found"/> is false when it is absent.
    /// </summary>
    internal VariableValue Find(VariableId key, out bool found)
    {
      var buckets = _buckets;
      if (buckets == null)
      {
        found = false;
        return null;
      }

      var k = (uint)key;
      var keys = _keys;
      var next = _next;
      for (var i = buckets[Bucket(k)]; i != 0; i = next[i - 1])
      {
        if (keys[i - 1] == k)
        {
          var value = _values[i - 1];
          found = !ReferenceEquals(value, Removed);
          return found ? value : null;
        }
      }

      found = false;
      return null;
    }

    private int Bucket(uint key) => (int)((key * 2654435769u) >> _bucketShift);

    private void RebuildIndex()
    {
      var bits = 2;
      while (1 << bits < _container.Count * 2)
      {
        ++bits;
      }

      var size = 1 << bits;
      _bucketShift = 32 - bits;
      if (_buckets != null && _buckets.Length == size)
      {
        // Keys and chain links are overwritten as entries are appended
        System.Array.Clear(_buckets, 0, size);
        System.Array.Clear(_values, 0, _indexCount);
      }
      else
      {
        _buckets = new int[size];
        _keys = new uint[size];
        _values = new VariableValue[size];
        _next = new int[size];
      }

      _indexCount = 0;
      foreach (var pair in _container)
      {
        Append(pair.Key, pair.Value);
      }
    }

    private void Append(uint key, VariableValue value)
    {
      var bucket = Bucket(key);
      _keys[_indexCount] = key;
      _values[_indexCount] = value;
      _next[_indexCount] = _buckets[bucket];
      _buckets[bucket] = ++_indexCount;
    }

    private void IndexSet(VariableId key, VariableValue value)
    {
      if (_buckets == null)
      {
        RebuildIndex();
        return;
      }

      var k = (uint)key;
      for (var i = _buckets[Bucket(k)]; i != 0; i = _next[i - 1])
      {
        if (_keys[i - 1] == k)
        {
          _values[i - 1] = value;
          return;
        }
      }

      // Keep the load factor at or below 1/2. Rebuilding drops removed entries and grows when needed.
      if (_indexCount * 2 >= _keys.Length)
      {
        RebuildIndex();
        return;
      }

      Append(k, value);
    }

    private void IndexRemove(VariableId key)
    {
      if (_buckets == null)
      {
        return;
      }

      var k = (uint)key;
      for (var i = _buckets[Bucket(k)]; i != 0; i = _next[i - 1])
      {
        if (_keys[i - 1] == k)
        {
          _values[i - 1] = Removed;
          return;
        }
      }
    }

    public void Add(VariableId key, VariableValue value)
    {
      _container.Add(key, value);
      IndexSet(key, value);
      ++_version;
    }

    public bool ContainsKey(VariableId key)
    {
      return _container.ContainsKey(key);
    }

    public bool Remove(VariableId key)
    {
      ++_version;
      if (!_container.Remove(key))
      {
        return false;
      }

      IndexRemove(key);
      return true;
    }

    bool IDictionary<VariableId, VariableValue>.TryGetValue(VariableId key, out VariableValue value)
    {
      return _container.TryGetValue(key, out value);
    }

    public VariableValue this[VariableId key]
    {
      get => _container[key];
      set
      {
        _container[key] = value;
        IndexSet(key, value);
        ++_version;
      }
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
      ((ICollection<KeyValuePair<VariableId, VariableValue>>)_container).Add(item);
      IndexSet(item.Key, item.Value);
      ++_version;
    }

    public void Clear()
    {
      _container.Clear();
      if (_buckets != null)
      {
        System.Array.Clear(_buckets, 0, _buckets.Length);
        System.Array.Clear(_values, 0, _indexCount);
        _indexCount = 0;
      }

      ++_version;
    }

    public bool Contains(KeyValuePair<VariableId, VariableValue> item)
    {
      return ((ICollection<KeyValuePair<VariableId, VariableValue>>)_container).Contains(item);
    }

    public void CopyTo(KeyValuePair<VariableId, VariableValue>[] array, int arrayIndex)
    {
      ((ICollection<KeyValuePair<VariableId, VariableValue>>)_container).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<VariableId, VariableValue> item)
    {
      ++_version;
      if (!((ICollection<KeyValuePair<VariableId, VariableValue>>)_container).Remove(item))
      {
        return false;
      }

      IndexRemove(item.Key);
      return true;
    }

    public int Count => _container.Count;
    public bool IsReadOnly => false;

    private sealed class VariableIdComparer : IEqualityComparer<VariableId>
    {
      public static readonly VariableIdComparer Instance = new VariableIdComparer();

      public bool Equals(VariableId x, VariableId y) => (uint)x == (uint)y;
      public int GetHashCode(VariableId obj) => (int)(uint)obj;
    }
  }
}
