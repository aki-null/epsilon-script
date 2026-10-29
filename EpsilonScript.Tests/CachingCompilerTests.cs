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

      var first = cache.GetOrCompile(source, Compiler.Options.Immutable);
      var second = cache.GetOrCompile(source, Compiler.Options.Immutable);

      Assert.Same(first, second);
    }

    [Fact]
    public void Clear_RemovesCachedEntries()
    {
      var cache = new CachingCompiler();
      var source = CachedSourceText.From("3 * 3");

      var first = cache.GetOrCompile(source, Compiler.Options.Immutable);
      cache.Clear();
      var second = cache.GetOrCompile(source, Compiler.Options.Immutable);

      Assert.NotSame(first, second);
    }

    [Fact]
    public void Count_TracksCachedEntries()
    {
      var cache = new CachingCompiler(capacity: 4);
      var sourceA = CachedSourceText.From("1 + 1");
      var sourceB = CachedSourceText.From("2 + 2");

      Assert.Equal(0, cache.Count);

      cache.GetOrCompile(sourceA, Compiler.Options.Immutable);
      Assert.Equal(1, cache.Count);

      cache.GetOrCompile(sourceA, Compiler.Options.Immutable);
      Assert.Equal(1, cache.Count);

      cache.GetOrCompile(sourceB, Compiler.Options.Immutable);
      Assert.Equal(2, cache.Count);

      cache.Clear();
      Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Constructor_WithCapacity_CompilesAndCaches()
    {
      var cache = new CachingCompiler(capacity: 8);
      var source = CachedSourceText.From("3 + 4");

      var first = cache.GetOrCompile(source, Compiler.Options.Immutable);
      var second = cache.GetOrCompile(source, Compiler.Options.Immutable);

      Assert.Same(first, second);
      Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void PrecisionConstructor_WithCapacity_CompilesAndCaches()
    {
      var cache = new CachingCompiler(
        Compiler.IntegerPrecision.Long,
        Compiler.FloatPrecision.Double,
        capacity: 8);
      var source = CachedSourceText.From("3 + 4");

      var first = cache.GetOrCompile(source, Compiler.Options.Immutable);
      var second = cache.GetOrCompile(source, Compiler.Options.Immutable);

      Assert.Same(first, second);
      Assert.Equal(1, cache.Count);
      Assert.Equal(Compiler.IntegerPrecision.Long, first.IntegerPrecision);
      Assert.Equal(Compiler.FloatPrecision.Double, first.FloatPrecision);
    }

    [Fact]
    public void Constructor_WithNegativeCapacity_Throws()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => new CachingCompiler(capacity: -1));
    }

    [Fact]
    public void PrecisionConstructor_WithNegativeCapacity_Throws()
    {
      Assert.Throws<ArgumentOutOfRangeException>(() => new CachingCompiler(
        Compiler.IntegerPrecision.Integer,
        Compiler.FloatPrecision.Float,
        capacity: -1));
    }

    [Fact]
    public void GetOrCompile_ManySourceStrings_EachKeepsItsScript()
    {
      var cache = new CachingCompiler();
      var sources = new string[200];
      var scripts = new CompiledScript[sources.Length];
      for (var i = 0; i < sources.Length; ++i)
      {
        sources[i] = i + " + 1";
        scripts[i] = cache.GetOrCompile(sources[i], Compiler.Options.Immutable);
      }

      // Same instances, then equal strings built anew, after the source slots have been overwritten
      for (var i = 0; i < sources.Length; ++i)
      {
        Assert.Same(scripts[i], cache.GetOrCompile(sources[i], Compiler.Options.Immutable));
        Assert.Same(scripts[i], cache.GetOrCompile(i + " + 1", Compiler.Options.Immutable));
        scripts[i].Execute();
        Assert.Equal(i + 1, scripts[i].IntegerValue);
      }

      Assert.Equal(sources.Length, cache.Count);
    }

    [Fact]
    public void Constructor_NullCompilerThrows()
    {
      Assert.Throws<ArgumentNullException>(() => new CachingCompiler((Compiler)null));
    }

    [Fact]
    public void WrappedCompiler_FunctionAddedDirectly_DropsCachedScripts()
    {
      var compiler = new Compiler();
      var cache = new CachingCompiler(compiler);
      Assert.Same(compiler, cache.Compiler);
      compiler.AddCustomFunction(CustomFunction.Create("g", (float v) => v + 0.5f, isDeterministic: true));

      var source = CachedSourceText.From("g(1)");
      var first = cache.GetOrCompile(source, Compiler.Options.Immutable);
      Assert.Same(first, cache.GetOrCompile(source, Compiler.Options.Immutable));
      first.Execute();
      Assert.Equal(1.5f, first.FloatValue);

      // The int overload now wins the fold, so the float-folded script must not be returned
      compiler.AddCustomFunction(CustomFunction.Create("g", (int v) => v + 10, isDeterministic: true));
      var second = cache.GetOrCompile(source, Compiler.Options.Immutable);
      second.Execute();

      Assert.NotSame(first, second);
      Assert.Equal(11, second.IntegerValue);
      Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void TrimExcess_AfterClear_KeepsCacheUsable()
    {
      var cache = new CachingCompiler(capacity: 16);
      var source = CachedSourceText.From("5 * 5");

      var first = cache.GetOrCompile(source, Compiler.Options.Immutable);
      Assert.Equal(1, cache.Count);

      cache.Clear();
      cache.TrimExcess();

      var second = cache.GetOrCompile(source, Compiler.Options.Immutable);

      Assert.NotSame(first, second);
      Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void Compile_DifferentOptionsCreatesDifferentEntries()
    {
      var cache = new CachingCompiler();
      var source = CachedSourceText.From("value + 1");

      var mutable = cache.GetOrCompile(source, Compiler.Options.None);
      var immutable = cache.GetOrCompile(source, Compiler.Options.Immutable);

      Assert.NotSame(mutable, immutable);
    }

    [Fact]
    public void Compile_DifferentVariableContainersAreNotShared()
    {
      var cache = new CachingCompiler();
      var source = CachedSourceText.From("value");

      var containerA = new DictionaryVariableContainer { ["value"] = new VariableValue(1) };
      var containerB = new DictionaryVariableContainer { ["value"] = new VariableValue(2) };

      var scriptA = cache.GetOrCompile(source, Compiler.Options.None, containerA);
      var scriptB = cache.GetOrCompile(source, Compiler.Options.None, containerB);

      Assert.NotSame(scriptA, scriptB);
    }

    [Fact]
    public void AddCustomFunction_InvalidatesCacheEntries()
    {
      var cache = new CachingCompiler();
      cache.AddCustomFunction(CustomFunction.Create("foo", () => 1));
      var source = CachedSourceText.From("foo()");

      var first = cache.GetOrCompile(source, Compiler.Options.Immutable);

      cache.AddCustomFunction(CustomFunction.Create("bar", () => 2));

      var second = cache.GetOrCompile(source, Compiler.Options.Immutable);

      Assert.NotSame(first, second);
    }

    [Fact]
    public void Compile_AllowsPrehashedSourceText()
    {
      var cache = new CachingCompiler();
      var text = "10 + 20";

      var prehashed = new CachedSourceText(text, 123);

      var first = cache.GetOrCompile(prehashed, Compiler.Options.Immutable);
      var second = cache.GetOrCompile(new CachedSourceText(text, 123), Compiler.Options.Immutable);

      Assert.Same(first, second);
    }

    [Fact]
    public void Compile_DifferentSourceSameHash_NotShared()
    {
      var cache = new CachingCompiler();
      var sourceA = new CachedSourceText("alpha", 42);
      var sourceB = new CachedSourceText("beta", 42);

      var scriptA = cache.GetOrCompile(sourceA, Compiler.Options.Immutable);
      var scriptB = cache.GetOrCompile(sourceB, Compiler.Options.Immutable);

      Assert.NotSame(scriptA, scriptB);
    }

    [Fact]
    public void Compile_SameTextDifferentStringInstances_ReusesCachedScript()
    {
      var cache = new CachingCompiler();
      var textA = new string(new[] { 'x', ' ', '+', ' ', '1' });
      var textB = new string(new[] { 'x', ' ', '+', ' ', '1' });
      var variables = new DictionaryVariableContainer { ["x"] = new VariableValue(1) };

      var first = cache.GetOrCompile(CachedSourceText.From(textA), Compiler.Options.Immutable, variables);
      var second = cache.GetOrCompile(CachedSourceText.From(textB), Compiler.Options.Immutable, variables);

      Assert.Same(first, second);
      Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void AddCustomFunction_InvalidationAffectsConstantFoldedOverloads()
    {
      var cache = new CachingCompiler();
      cache.AddCustomFunction(CustomFunction.Create("g", (float v) => v + 0.5f, isDeterministic: true));

      var source = CachedSourceText.From("g(1)");
      var first = cache.GetOrCompile(source, Compiler.Options.Immutable);
      first.Execute();
      var firstValue = first.FloatValue;

      cache.AddCustomFunction(CustomFunction.Create("g", (int v) => v + 10, isDeterministic: true));

      var second = cache.GetOrCompile(source, Compiler.Options.Immutable);
      second.Execute();
      var secondValue = second.IntegerValue;

      Assert.NotSame(first, second);
      Assert.Equal(1.5f, firstValue);
      Assert.Equal(11, secondValue);
    }

    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("sqrt(pow(x * cos(angle), 2.0)) * scale + offset")]
    [InlineData("\u65e5\u672c\u8a9e + \"\ud83d\ude00\"")]
    public void CachedSourceText_From_HashMatchesFnv1a(string source)
    {
      unchecked
      {
        var expected = (int)2166136261 ^ source.Length;
        foreach (var c in source)
        {
          expected ^= c;
          expected *= 16777619;
        }

        Assert.Equal(expected, CachedSourceText.From(source).Hash);
      }
    }

    [Fact]
    public void AddCustomFunctionRange_WhenLaterFunctionThrows_InvalidatesCache()
    {
      var cache = new CachingCompiler();
      cache.AddCustomFunction(CustomFunction.Create("g", (float v) => v + 0.5f, isDeterministic: true));

      var source = CachedSourceText.From("g(1)");
      var first = cache.GetOrCompile(source, Compiler.Options.Immutable);
      first.Execute();
      var firstValue = first.FloatValue;

      var functions = new[]
      {
        CustomFunction.Create("g", (int v) => v + 10, isDeterministic: true),
        CustomFunction.Create("g", (int v) => v + 20, isDeterministic: false),
      };

      Assert.Throws<ArgumentException>(() => cache.AddCustomFunctionRange(functions));

      var second = cache.GetOrCompile(source, Compiler.Options.Immutable);
      second.Execute();

      Assert.NotSame(first, second);
      Assert.Equal(1.5f, firstValue);
      Assert.Equal(11, second.IntegerValue);
    }
  }
}
