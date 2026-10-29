using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EpsilonScript.Helper;

namespace EpsilonScript
{
  internal readonly struct CachingCompilerKey
  {
    public ReadOnlyMemory<char> Source { get; }
    public int SourceHash { get; }
    public Compiler.Options Options { get; }
    public IVariableContainer Variables { get; }

    public CachingCompilerKey(ReadOnlyMemory<char> source, int sourceHash, Compiler.Options options,
      IVariableContainer variables)
    {
      Source = source;
      SourceHash = sourceHash;
      Options = options;
      Variables = variables;
    }
  }

  internal sealed class CachingCompilerKeyComparer : IEqualityComparer<CachingCompilerKey>
  {
    public static CachingCompilerKeyComparer Instance { get; } = new CachingCompilerKeyComparer();

    private CachingCompilerKeyComparer()
    {
    }

    public bool Equals(CachingCompilerKey x, CachingCompilerKey y)
    {
      if (x.SourceHash != y.SourceHash ||
          x.Options != y.Options ||
          !ReferenceEquals(x.Variables, y.Variables))
      {
        return false;
      }

      if (x.Source.Length != y.Source.Length)
      {
        return false;
      }

      if (MemoryMarshal.TryGetString(x.Source, out var xString, out var xStart, out var xLength) &&
          MemoryMarshal.TryGetString(y.Source, out var yString, out var yStart, out var yLength))
      {
        if (ReferenceEquals(xString, yString) && xStart == yStart && xLength == yLength)
        {
          return true;
        }
      }

      return SpanText.AreEqual(x.Source.FastSpan(), y.Source.FastSpan());
    }

    public int GetHashCode(CachingCompilerKey obj)
    {
      // Plain integer mixing: System.HashCode.Add<T> goes through EqualityComparer<T>.Default per field, which is
      // slow under IL2CPP. The value only feeds the cache dictionary.
      unchecked
      {
        var hash = obj.SourceHash;
        hash = hash * 31 + (int)obj.Options;
        hash = hash * 31 + obj.Source.Length;
        if (obj.Variables != null)
        {
          hash = hash * 31 + RuntimeHelpers.GetHashCode(obj.Variables);
        }

        return hash;
      }
    }
  }
}
