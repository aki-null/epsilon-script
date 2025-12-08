using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

      return x.Source.Span.SequenceEqual(y.Source.Span);
    }

    public int GetHashCode(CachingCompilerKey obj)
    {
      var hash = new HashCode();
      hash.Add(obj.SourceHash);
      hash.Add(obj.Options);
      hash.Add(obj.Source.Length);

      if (obj.Variables != null)
      {
        hash.Add(RuntimeHelpers.GetHashCode(obj.Variables));
      }

      return hash.ToHashCode();
    }
  }
}