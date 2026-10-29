using System;
using EpsilonScript.Helper;

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

    internal CachedSourceText(string source, int hash)
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

      return new CachedSourceText(source, SpanText.Hash(source.AsSpan()));
    }
  }
}