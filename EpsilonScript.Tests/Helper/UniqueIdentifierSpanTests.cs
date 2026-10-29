using System;
using EpsilonScript.Helper;
using Xunit;

namespace EpsilonScript.Tests.Helper
{
  [Trait("Category", "Unit")]
  [Trait("Component", "Helper")]
  public class UniqueIdentifierSpanTests
  {
    [Fact]
    public void SpanLookup_MatchesStringLookup()
    {
      var name = "span_lookup_" + Guid.NewGuid().ToString("N");
      var fromSpan = UniqueIdentifierExtension.GetUniqueIdentifier(name.AsSpan());
      Assert.Equal(name.GetUniqueIdentifier(), fromSpan);
      Assert.Equal(fromSpan, UniqueIdentifierExtension.GetUniqueIdentifier(name.AsSpan()));
    }

    [Fact]
    public void SpanLookup_FromSliceOfLargerString_MatchesStringLookup()
    {
      var name = "slice_" + Guid.NewGuid().ToString("N");
      var source = "(" + name + " + 1)";
      var fromSpan = UniqueIdentifierExtension.GetUniqueIdentifier(source.AsSpan(1, name.Length));
      Assert.Equal(name.GetUniqueIdentifier(), fromSpan);
    }

    [Fact]
    public void SpanLookup_ManyDistinctNames_KeepDistinctIds()
    {
      // More names than memo slots, so slot collisions and evictions are exercised.
      var prefix = "many_" + Guid.NewGuid().ToString("N") + "_";
      var ids = new uint[2000];
      for (var round = 0; round < 2; ++round)
      {
        for (var i = 0; i < ids.Length; ++i)
        {
          var name = prefix + i;
          var id = UniqueIdentifierExtension.GetUniqueIdentifier(name.AsSpan());
          Assert.Equal(name.GetUniqueIdentifier(), id);
          if (round == 0) ids[i] = id;
          else Assert.Equal(ids[i], id);
        }
      }

      Assert.Equal(ids.Length, new System.Collections.Generic.HashSet<uint>(ids).Count);
    }

    [Fact]
    public void SpanLookup_SameLengthDifferentText_DistinctIds()
    {
      var a = UniqueIdentifierExtension.GetUniqueIdentifier("ab_same_len_x".AsSpan());
      var b = UniqueIdentifierExtension.GetUniqueIdentifier("ab_same_len_y".AsSpan());
      Assert.NotEqual(a, b);
      Assert.Equal("ab_same_len_x", a.GetStringFromUniqueIdentifier());
      Assert.Equal("ab_same_len_y", b.GetStringFromUniqueIdentifier());
    }
  }
}
