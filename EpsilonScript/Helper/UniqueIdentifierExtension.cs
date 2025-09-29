using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace EpsilonScript.Helper
{
  public static class UniqueIdentifierExtension
  {
    private static readonly ConcurrentDictionary<string, uint> StringIdCache = new ConcurrentDictionary<string, uint>();
    private static readonly ConcurrentDictionary<uint, string> IdStringCache = new ConcurrentDictionary<uint, string>();
    private static int _currentId = 0;

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
    }
  }
}