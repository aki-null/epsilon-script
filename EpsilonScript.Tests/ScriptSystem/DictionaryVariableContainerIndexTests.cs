using System.Collections.Generic;
using Xunit;

namespace EpsilonScript.Tests.ScriptSystem
{
  /// <summary>
  /// Verifies the internal lookup index of DictionaryVariableContainer stays consistent with the dictionary contents.
  /// </summary>
  [Trait("Category", "Unit")]
  [Trait("Component", "ScriptSystem")]
  public class DictionaryVariableContainerIndexTests
  {
    // The entry's value, or null when absent
    private static VariableValue Find(DictionaryVariableContainer container, VariableId key) =>
      container.Find(key, out _);

    [Fact]
    public void Find_Empty_ReturnsNull()
    {
      var container = new DictionaryVariableContainer();
      Assert.Null(Find(container, 1u));
    }

    [Fact]
    public void Find_AfterManyAdds_ReturnsEachEntry()
    {
      var container = new DictionaryVariableContainer();
      var values = new List<VariableValue>();
      for (var i = 0; i < 1000; ++i)
      {
        var value = new VariableValue(i);
        values.Add(value);
        container.Add((uint)i, value);
        // Interleave lookups so the index is grown incrementally rather than built once
        Assert.Same(value, Find(container, (uint)i));
      }

      for (var i = 0; i < 1000; ++i)
      {
        Assert.Same(values[i], Find(container, (uint)i));
      }

      Assert.Null(Find(container, 1000u));
    }

    [Fact]
    public void Find_KeysWithIdenticalLowBits_ReturnsEachEntry()
    {
      var container = new DictionaryVariableContainer();
      for (uint i = 0; i < 64; ++i)
      {
        container[i << 20] = new VariableValue((int)i);
      }

      for (uint i = 0; i < 64; ++i)
      {
        Assert.Equal((int)i, Find(container, i << 20).IntegerValue);
      }

      Assert.Null(Find(container, 65u << 20));
    }

    [Fact]
    public void Find_MaxAndZeroIds_Work()
    {
      var container = new DictionaryVariableContainer
      {
        [0u] = new VariableValue(1),
        [uint.MaxValue] = new VariableValue(2),
      };

      Assert.Equal(1, Find(container, 0u).IntegerValue);
      Assert.Equal(2, Find(container, uint.MaxValue).IntegerValue);
    }

    [Fact]
    public void Find_AfterIndexerReplace_ReturnsNewValue()
    {
      var container = new DictionaryVariableContainer { ["a"] = new VariableValue(1) };
      Assert.Equal(1, Find(container, "a").IntegerValue);

      var replacement = new VariableValue(2);
      container["a"] = replacement;
      Assert.Same(replacement, Find(container, "a"));
    }

    [Fact]
    public void Find_AfterRemove_ReturnsNullAndKeepsOthers()
    {
      var container = new DictionaryVariableContainer
      {
        ["a"] = new VariableValue(1),
        ["b"] = new VariableValue(2),
      };
      Assert.NotNull(Find(container, "a"));

      Assert.True(container.Remove("a"));
      Assert.Null(Find(container, "a"));
      Assert.Equal(2, Find(container, "b").IntegerValue);

      container.Add("a", new VariableValue(3));
      Assert.Equal(3, Find(container, "a").IntegerValue);
    }

    [Fact]
    public void Find_AfterRemoveKeyValuePair_ReturnsNull()
    {
      var value = new VariableValue(1);
      var container = new DictionaryVariableContainer { ["a"] = value };
      Assert.NotNull(Find(container, "a"));

      ICollection<KeyValuePair<VariableId, VariableValue>> collection = container;
      Assert.True(collection.Remove(new KeyValuePair<VariableId, VariableValue>("a", value)));
      Assert.Null(Find(container, "a"));
    }

    [Fact]
    public void Find_AfterClear_ReturnsNull()
    {
      var container = new DictionaryVariableContainer { ["a"] = new VariableValue(1) };
      Assert.NotNull(Find(container, "a"));

      container.Clear();
      Assert.Null(Find(container, "a"));

      container["a"] = new VariableValue(5);
      Assert.Equal(5, Find(container, "a").IntegerValue);
    }

    [Fact]
    public void Find_AgreesWithTryGetValue_AfterMixedMutations()
    {
      var container = new DictionaryVariableContainer();
      IVariableContainer lookup = container;
      for (uint i = 0; i < 200; ++i)
      {
        container[i] = new VariableValue((int)i);
        if (i % 3 == 0) container.Remove(i / 2);
        if (i % 7 == 0) container[i / 3] = new VariableValue(-(int)i);
        if (i % 11 == 0) Find(container, i);
      }

      for (uint i = 0; i < 220; ++i)
      {
        var found = lookup.TryGetValue(i, out var expected);
        var actual = container.Find(i, out var actualFound);
        Assert.Equal(found, actualFound);
        Assert.Same(found ? expected : null, actual);
      }
    }

    [Fact]
    public void Find_AgreesWithTryGetValue_AfterRemoveAndReaddChurn()
    {
      // Removed entries stay in the index until it fills up and rebuilds
      var container = new DictionaryVariableContainer();
      IVariableContainer lookup = container;
      for (uint i = 0; i < 8; ++i)
      {
        container[i] = new VariableValue((int)i);
      }

      for (uint round = 0; round < 100; ++round)
      {
        container.Remove(round % 8);
        Assert.Null(Find(container, round % 8));
        container[100 + round] = new VariableValue((int)round);
        container.Remove(100 + round);
        container[round % 8] = new VariableValue((int)round);
        if (round % 10 == 0)
        {
          container.Clear();
          Assert.Null(Find(container, round % 8));
          for (uint i = 0; i < 8; ++i)
          {
            container[i] = new VariableValue((int)i);
          }
        }

        for (uint i = 0; i < 210; ++i)
        {
          var found = lookup.TryGetValue(i, out var expected);
          var actual = container.Find(i, out var actualFound);
          Assert.Equal(found, actualFound);
          Assert.Same(found ? expected : null, actual);
        }
      }
    }

    [Fact]
    public void RemoveAndReadd_DoesNotAllocateOnLookup()
    {
      var value = new VariableValue(1);
      var container = new DictionaryVariableContainer { ["a"] = value, ["b"] = new VariableValue(2) };
      for (var i = 0; i < 4; ++i)
      {
        container.Remove("a");
        container["a"] = value;
        Find(container, "a");
      }

      var before = System.GC.GetAllocatedBytesForCurrentThread();
      for (var i = 0; i < 100; ++i)
      {
        container.Remove("a");
        container["a"] = value;
        Assert.Same(value, Find(container, "a"));
      }

      Assert.Equal(0, System.GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void Find_NullEntry_IsFound_RemovedEntryIsNot()
    {
      var container = new DictionaryVariableContainer { ["a"] = null, ["b"] = new VariableValue(1) };
      Assert.Null(container.Find("a", out var found));
      Assert.True(found);

      container.Remove("a");
      container.Find("a", out found);
      Assert.False(found);

      container["a"] = null;
      container.Find("a", out found);
      Assert.True(found);
    }

    /// <summary>
    /// Reads must not touch the index, so a container that is no longer written can be read from many threads, as
    /// with Dictionary. The last add grows the index.
    /// </summary>
    [Fact]
    public void Find_ConcurrentReaders_SeeEveryEntry()
    {
      var container = new DictionaryVariableContainer();
      var values = new VariableValue[257];
      for (var i = 0; i < values.Length; ++i)
      {
        values[i] = new VariableValue(i);
        container.Add((uint)i, values[i]);
      }

      System.Threading.Tasks.Parallel.For(0, 64, _ =>
      {
        for (var i = 0; i < values.Length; ++i)
        {
          Assert.Same(values[i], Find(container, (uint)i));
        }
      });
    }

    [Fact]
    public void DuplicateAdd_StillThrows_AndIndexUnchanged()
    {
      var original = new VariableValue(1);
      var container = new DictionaryVariableContainer { ["a"] = original };
      Assert.Throws<System.ArgumentException>(() => container.Add("a", new VariableValue(2)));
      Assert.Same(original, Find(container, "a"));
    }
  }
}
