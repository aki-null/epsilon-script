using System;

namespace EpsilonScript.Helper
{
  /// <summary>
  /// Hashing and comparison of UTF-16 text. Pointer loops avoid the span bounds check per char, which IL2CPP does
  /// not eliminate, and MemoryExtensions.SequenceEqual, which looks up type objects on every call under IL2CPP.
  /// </summary>
  internal static class SpanText
  {
    /// <summary>
    /// FNV-1a 32-bit over UTF-16 chars, seeded with the length. Fast, deterministic, and dependency-free.
    /// </summary>
    internal static unsafe int Hash(ReadOnlySpan<char> text)
    {
      unchecked
      {
        const int offset = unchecked((int)2166136261);
        const int prime = 16777619;

        var hash = offset ^ text.Length;
        fixed (char* start = text)
        {
          var p = start;
          var end = start + text.Length;
          while (p < end)
          {
            hash ^= *p++;
            hash *= prime;
          }
        }

        return hash;
      }
    }

    /// <summary>
    /// Ordinal equality.
    /// </summary>
    internal static unsafe bool AreEqual(ReadOnlySpan<char> left, ReadOnlySpan<char> right)
    {
      if (left.Length != right.Length)
      {
        return false;
      }

      fixed (char* leftStart = left, rightStart = right)
      {
        for (var i = 0; i < left.Length; ++i)
        {
          if (leftStart[i] != rightStart[i])
          {
            return false;
          }
        }
      }

      return true;
    }
  }
}
