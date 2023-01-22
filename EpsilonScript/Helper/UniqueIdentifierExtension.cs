using System.Collections.Generic;
using System.Threading;

namespace EpsilonScript.Helper
{
  public static class UniqueIdentifierExtension
  {
    private static readonly Dictionary<string, int> StringIdCache = new Dictionary<string, int>();
    private static readonly List<string> StringIdReverseLookup = new List<string>();
    private static int _currentId = 1;

    private static readonly ReaderWriterLockSlim CacheLock = new ReaderWriterLockSlim();

    public static int GetUniqueIdentifier(this string s)
    {
      CacheLock.EnterUpgradeableReadLock();
      try
      {
        if (StringIdCache.TryGetValue(s, out var id))
        {
          return id;
        }

        CacheLock.EnterWriteLock();
        try
        {
          id = _currentId;
          StringIdCache[s] = id;
          StringIdReverseLookup.Add(s);
          ++_currentId;
          return id;
        }
        finally
        {
          CacheLock.ExitWriteLock();
        }
      }
      finally
      {
        CacheLock.ExitUpgradeableReadLock();
      }
    }

    public static string GetStringFromUniqueIdentifier(this int s)
    {
      CacheLock.EnterReadLock();
      try
      {
        var lookupPos = s - 1;
        return StringIdReverseLookup[(int)lookupPos];
      }
      finally
      {
        CacheLock.ExitReadLock();
      }
    }

    public static void ResetUniqueIdentifierCache()
    {
      CacheLock.EnterWriteLock();
      try
      {
        StringIdCache.Clear();
        StringIdReverseLookup.Clear();
        _currentId = 1;
      }
      finally
      {
        CacheLock.ExitWriteLock();
      }
    }
  }
}