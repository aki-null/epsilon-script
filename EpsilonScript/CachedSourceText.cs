using System;

namespace EpsilonScript
{
  /// <summary>
  /// Represents source text with a precomputed hash so callers can avoid re-hashing
  /// the same script string when repeatedly compiling.
  /// </summary>
  /// <remarks>
  /// Intentionally string-only to ensure the cached key is immutable (no spans or pooled buffers).
  /// </remarks>
  public readonly struct CachedSourceText
  {
    public ReadOnlyMemory<char> Source { get; }
    public int Hash { get; }

    public CachedSourceText(string source, int hash)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      Source = source.AsMemory();
      Hash = hash;
    }

    public static CachedSourceText From(string source)
    {
      if (source == null)
      {
        throw new ArgumentNullException(nameof(source));
      }

      return new CachedSourceText(source, ComputeHash(source.AsSpan()));
    }

    private static int ComputeHash(ReadOnlySpan<char> source)
    {
      // FNV-1a 32-bit over UTF-16 chars (fast, deterministic, no extra deps).
      unchecked
      {
        const int offset = unchecked((int)2166136261);
        const int prime = 16777619;

        var hash = offset ^ source.Length;
        for (var i = 0; i < source.Length; i++)
        {
          hash ^= source[i];
          hash *= prime;
        }

        return hash;
      }
    }
  }
}