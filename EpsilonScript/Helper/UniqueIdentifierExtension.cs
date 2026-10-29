using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EpsilonScript.Helper
{
  /// <summary>
  /// Extension methods for converting strings to unique integer identifiers.
  /// </summary>
  /// <remarks>
  /// Thread Safety: GetUniqueIdentifier() is thread-safe.
  /// ResetUniqueIdentifierCache() is NOT thread-safe and should only be called during test setup.
  /// </remarks>
  public static class UniqueIdentifierExtension
  {
    private static readonly ConcurrentDictionary<string, uint> StringIdCache = new ConcurrentDictionary<string, uint>();
    private static readonly ConcurrentDictionary<uint, string> IdStringCache = new ConcurrentDictionary<uint, string>();
    private static int _currentId = 0;

    // Per-thread memo of identifier text to id, so compiling an already-known identifier from a token span neither
    // allocates a string nor takes the ConcurrentDictionary path. Invalidated by ResetUniqueIdentifierCache.
    private const int SpanCacheSize = 256;
    [ThreadStatic] private static SpanCacheEntry[] _spanCache;
    [ThreadStatic] private static int _spanCacheGeneration;
    private static int _generation;

    private struct SpanCacheEntry
    {
      public string Text;
      public uint Id;
    }

    internal static uint GetUniqueIdentifier(ReadOnlySpan<char> s)
    {
      var generation = Volatile.Read(ref _generation);
      var cache = _spanCache;
      if (cache == null || _spanCacheGeneration != generation)
      {
        cache = _spanCache = new SpanCacheEntry[SpanCacheSize];
        _spanCacheGeneration = generation;
      }

      ref var entry = ref cache[SpanText.Hash(s) & (SpanCacheSize - 1)];
      if (entry.Text != null && SpanText.AreEqual(s, entry.Text.AsSpan()))
      {
        return entry.Id;
      }

      var text = new string(s);
      var id = text.GetUniqueIdentifier();
      entry.Text = text;
      entry.Id = id;
      return id;
    }

    public static uint GetUniqueIdentifier(this string s)
    {
      if (StringIdCache.TryGetValue(s, out var existingId))
      {
        return existingId;
      }

      var newId = (uint)Interlocked.Increment(ref _currentId);
      var actualId = StringIdCache.GetOrAdd(s, newId);

      if (actualId == newId)
      {
        // We won the race, add to reverse lookup
        IdStringCache.TryAdd(newId, s);
      }

      return actualId;
    }

    public static string GetStringFromUniqueIdentifier(this uint id)
    {
      return IdStringCache.GetValueOrDefault(id);
    }

    public static void ResetUniqueIdentifierCache()
    {
      StringIdCache.Clear();
      IdStringCache.Clear();
      _currentId = 0;
      Interlocked.Increment(ref _generation);
    }
  }
}