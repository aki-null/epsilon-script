using System;
using System.Collections.Generic;
using EpsilonScript;
using EpsilonScript.Function;
using Xunit;

namespace EpsilonScript.Tests
{
  [Trait("Category", "Unit")]
  [Trait("Component", "CachingCompiler")]
  public class CachingCompilerTests
  {
    [Fact]
    public void Compile_ReusesCachedScriptWhenKeyMatches()
    {
      var cache = new CachingCompiler();
      var source = CachedSourceText.From("1 + 2");

      var first = cache.Compile(source, Compiler.Options.Immutable);
      var second = cache.Compile(source, Compiler.Options.Immutable);

      Assert.Same(first, second);
    }

    [Fact]
    public void Clear_RemovesCachedEntries()
    {
      var cache = new CachingCompiler();
      var source = CachedSourceText.From("3 * 3");

      var first = cache.Compile(source, Compiler.Options.Immutable);
      cache.Clear();
      var second = cache.Compile(source, Compiler.Options.Immutable);

      Assert.NotSame(first, second);
    }

    [Fact]
    public void Compile_DifferentOptionsCreatesDifferentEntries()
    {
      var cache = new CachingCompiler();
      var source = CachedSourceText.From("value + 1");

      var mutable = cache.Compile(source, Compiler.Options.None);
      var immutable = cache.Compile(source, Compiler.Options.Immutable);

      Assert.NotSame(mutable, immutable);
    }

    [Fact]
    public void Compile_DifferentVariableContainersAreNotShared()
    {
      var cache = new CachingCompiler();
      var source = CachedSourceText.From("value");

      var containerA = new DictionaryVariableContainer { ["value"] = new VariableValue(1) };
      var containerB = new DictionaryVariableContainer { ["value"] = new VariableValue(2) };

      var scriptA = cache.Compile(source, Compiler.Options.None, containerA);
      var scriptB = cache.Compile(source, Compiler.Options.None, containerB);

      Assert.NotSame(scriptA, scriptB);
    }

    [Fact]
    public void AddCustomFunction_InvalidatesCacheEntries()
    {
      var cache = new CachingCompiler();
      cache.AddCustomFunction(CustomFunction.Create("foo", () => 1));
      var source = CachedSourceText.From("foo()");

      var first = cache.Compile(source, Compiler.Options.Immutable);

      cache.AddCustomFunction(CustomFunction.Create("bar", () => 2));

      var second = cache.Compile(source, Compiler.Options.Immutable);

      Assert.NotSame(first, second);
    }

    [Fact]
    public void Compile_AllowsPrehashedSourceText()
    {
      var cache = new CachingCompiler();
      var text = "10 + 20";

      var prehashed = new CachedSourceText(text, 123);

      var first = cache.Compile(prehashed, Compiler.Options.Immutable);
      var second = cache.Compile(new CachedSourceText(text, 123), Compiler.Options.Immutable);

      Assert.Same(first, second);
    }

    [Fact]
    public void Compile_DifferentSourceSameHash_NotShared()
    {
      var cache = new CachingCompiler();
      var sourceA = new CachedSourceText("alpha", 42);
      var sourceB = new CachedSourceText("beta", 42);

      var scriptA = cache.Compile(sourceA, Compiler.Options.Immutable);
      var scriptB = cache.Compile(sourceB, Compiler.Options.Immutable);

      Assert.NotSame(scriptA, scriptB);
    }

    [Fact]
    public void AddCustomFunction_InvalidationAffectsConstantFoldedOverloads()
    {
      var cache = new CachingCompiler();
      cache.AddCustomFunction(CustomFunction.Create("g", (float v) => v + 0.5f, isDeterministic: true));

      var source = CachedSourceText.From("g(1)");
      var first = cache.Compile(source, Compiler.Options.Immutable);
      first.Execute();
      var firstValue = first.FloatValue;

      cache.AddCustomFunction(CustomFunction.Create("g", (int v) => v + 10, isDeterministic: true));

      var second = cache.Compile(source, Compiler.Options.Immutable);
      second.Execute();
      var secondValue = second.IntegerValue;

      Assert.NotSame(first, second);
      Assert.Equal(1.5f, firstValue);
      Assert.Equal(11, secondValue);
    }
  }
}