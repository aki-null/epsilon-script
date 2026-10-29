using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using EpsilonScript.Function;
using Xunit;

namespace EpsilonScript.Tests.VirtualMachine
{
  /// <summary>
  /// Register program behaviour that the baseline corpus does not pin down: node pooling across compilations,
  /// result caching, short-circuiting and the decimal conversion cache.
  /// </summary>
  [Trait("Category", "Unit")]
  [Trait("Component", "VirtualMachine")]
  public class RegisterProgramTests
  {
    private static readonly string[] Names = { "i", "j", "f", "g", "b", "s", "z" };

    private static readonly string[] Binary =
      { "+", "-", "*", "/", "%", "==", "!=", "<", "<=", ">", ">=", "&&", "||" };

    private static readonly string[] Functions1 = { "abs", "sin", "sqrt", "floor", "len", "lower", "ctxlen" };
    private static readonly string[] Functions2 = { "min", "max", "pow", "pick" };
    private static readonly string[] Returns = { "rint()", "rlong()", "rdouble()", "rdecimal()", "rnear()" };

    private sealed class Generator
    {
      private readonly Random _random;

      public Generator(int seed) => _random = new Random(seed);

      public string Expression(int depth)
      {
        if (depth <= 0 || _random.Next(4) == 0)
        {
          return Leaf();
        }

        switch (_random.Next(10))
        {
          case 0:
            return "-" + Paren(depth - 1);
          case 1:
            return "!" + Paren(depth - 1);
          case 2:
            return Functions1[_random.Next(Functions1.Length)] + "(" + Expression(depth - 1) + ")";
          case 3:
            return Functions2[_random.Next(Functions2.Length)] + "(" + Expression(depth - 1) + ", " +
                   Expression(depth - 1) + ")";
          case 4:
            return "ifelse(" + Expression(depth - 1) + ", " + Expression(depth - 1) + ", " + Expression(depth - 1) +
                   ")";
          default:
            return Paren(depth - 1) + " " + Binary[_random.Next(Binary.Length)] + " " + Paren(depth - 1);
        }
      }

      private string Paren(int depth) => "(" + Expression(depth) + ")";

      private string Leaf()
      {
        switch (_random.Next(10))
        {
          // Non-deterministic zero-argument functions returning types other than the configured ones
          case 8: return Returns[_random.Next(Returns.Length)];
          case 9: return _random.Next(2) == 0 ? "rnear()" : "0.3";
          case 0: return _random.Next(-3, 4).ToString();
          case 1: return (_random.Next(-30, 30) / 4.0).ToString("0.0#", CultureInfo.InvariantCulture);
          case 2: return _random.Next(2) == 0 ? "true" : "false";
          case 3: return "\"s" + _random.Next(3) + "\"";
          default: return Names[_random.Next(Names.Length)];
        }
      }

      public VariableValue Value()
      {
        switch (_random.Next(7))
        {
          case 6: return new VariableValue((decimal)(_random.Next(-20, 21) / 4.0));
          case 0: return new VariableValue(_random.Next(-5, 6));
          case 1: return new VariableValue((long)_random.Next(-5, 6));
          case 2: return new VariableValue((float)(_random.Next(-20, 21) / 4.0));
          case 3: return new VariableValue(_random.Next(-20, 21) / 8.0);
          case 4: return new VariableValue(_random.Next(2) == 0);
          default: return new VariableValue("v" + _random.Next(3));
        }
      }
    }

    private static string Outcome(Func<string> read)
    {
      try
      {
        return read();
      }
      catch (Exception e)
      {
        return "throws " + e.GetType().Name + ": " + e.Message;
      }
    }

    private static string Describe(CompiledScript script)
    {
      switch (script.ValueType)
      {
        case Type.Integer: return "int " + script.IntegerValue;
        case Type.Long: return "long " + script.LongValue;
        case Type.Float: return "float " + BitConverter.SingleToInt32Bits(script.FloatValue);
        case Type.Double: return "double " + BitConverter.DoubleToInt64Bits(script.DoubleValue);
        case Type.Decimal: return "decimal " + script.DecimalValue.ToString(CultureInfo.InvariantCulture);
        case Type.Boolean: return "bool " + script.BooleanValue;
        case Type.String: return "string " + script.StringValue;
        default: return script.ValueType + " " + Outcome(() => script.IntegerValue.ToString());
      }
    }

    private static DictionaryVariableContainer Variables(int seed)
    {
      var generator = new Generator(seed);
      var variables = new DictionaryVariableContainer();
      foreach (var name in Names)
      {
        variables[name] = generator.Value();
      }

      return variables;
    }

    private static Compiler CreateCompiler(Compiler.IntegerPrecision integerPrecision,
      Compiler.FloatPrecision floatPrecision)
    {
      var compiler = new Compiler(integerPrecision, floatPrecision);
      // Precision-independent signatures covering bool/string parameters, an int return in long precision, and
      // context variables
      compiler.AddCustomFunction(CustomFunction.Create("pick", (bool c, string a) => c ? a : "n",
        isDeterministic: true));
      compiler.AddCustomFunction(CustomFunction.CreateContextual("ctxlen", "s", (string s, string t) =>
        s.Length * 10 + t.Length));
      compiler.AddCustomFunction(CustomFunction.Create("rint", () => 3));
      compiler.AddCustomFunction(CustomFunction.Create("rlong", () => -7L));
      compiler.AddCustomFunction(CustomFunction.Create("rdouble", () => 2.5));
      compiler.AddCustomFunction(CustomFunction.Create("rdecimal", () => 1.25m));
      // Nearly but not exactly 0.3, for the tolerant float equality
      compiler.AddCustomFunction(CustomFunction.Create("rnear", () => 0.1 + 0.2));
      // One float ulp above 0.3f as a double, and 0.3f as a float: unequal but nearly equal to 0.3
      compiler.AddCustomFunction(CustomFunction.Create("rulp",
        () => (double)BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(0.3f) + 1)));
      compiler.AddCustomFunction(CustomFunction.Create("rfloat", () => 0.3f));
      compiler.AddCustomFunction(CustomFunction.Create("rdec3", () => 0.3m));
      return compiler;
    }

    /// <summary>
    /// Programs must not refer to the pooled tree they were lowered from: scripts compiled earlier keep working
    /// after later compilations reuse every node.
    /// </summary>
    [Fact]
    public void EarlierScripts_SurviveNodeReuse()
    {
      var compiler = CreateCompiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      var generator = new Generator(99);
      var scripts = new List<(string Source, CompiledScript Script, int Seed)>();
      for (var scriptIndex = 0; scriptIndex < 1000; ++scriptIndex)
      {
        var source = generator.Expression(4);
        var variables = Variables(scriptIndex);
        try
        {
          scripts.Add((source, compiler.Compile(source, Compiler.Options.Immutable, variables), scriptIndex));
        }
        catch (Exception)
        {
          // Compile errors are covered by the differential tests
        }
      }

      var failures = new StringBuilder();
      for (var index = 0; index < scripts.Count; ++index)
      {
        var (source, script, seed) = scripts[index];
        var freshVariables = Variables(seed);
        var fresh = CreateCompiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float)
          .Compile(source, Compiler.Options.Immutable, freshVariables);
        var expected = Outcome(() =>
        {
          fresh.Execute();
          return Describe(fresh);
        });
        var actual = Outcome(() =>
        {
          script.Execute();
          return Describe(script);
        });
        if (expected != actual && failures.Length < 4000)
        {
          failures.AppendLine($"{source}:\n  expected [{expected}]\n  actual   [{actual}]");
        }
      }

      Assert.True(failures.Length == 0, failures.ToString());
      Assert.True(scripts.Count > 200, $"only {scripts.Count} scripts compiled");
    }

    /// <summary>
    /// A pooled node must not keep state, such as tuple elements, from the compilation that last used it, and NoAlloc
    /// must apply only to the compilation that asked for it.
    /// </summary>
    [Theory]
    [InlineData("s + i * j", "string x6")]
    [InlineData("s = i", "string 2")]
    [InlineData("max(i, j)", "int 3")]
    public void NodeReuse_DoesNotLeakState(string source, string expected)
    {
      var compiler = CreateCompiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float);
      DictionaryVariableContainer Fixed() => new DictionaryVariableContainer
        { ["i"] = new VariableValue(2), ["j"] = new VariableValue(3), ["s"] = new VariableValue("x") };

      foreach (var options in new[] { Compiler.Options.NoAlloc, Compiler.Options.None, Compiler.Options.None })
      {
        var script = compiler.Compile(source, options, Fixed());
        var outcome = Outcome(() =>
        {
          script.Execute();
          return Describe(script);
        });
        if (options == Compiler.Options.None)
        {
          Assert.Equal(expected, outcome);
        }
      }
    }

    [Fact]
    public void OperatorScript_UsesRegisterProgram()
    {
      var variables = new DictionaryVariableContainer { ["a"] = new VariableValue(1), ["b"] = new VariableValue(2) };
      var script = new Compiler().Compile("(a + b) * a - b / a", Compiler.Options.Immutable, variables);
      Assert.NotNull(script.Program);
      script.Execute();
      Assert.Equal(1, script.IntegerValue);
      script.Execute();
      Assert.Equal(1, script.IntegerValue);
    }

    [Fact]
    public void ConstantScripts_CacheResult()
    {
      var script = new Compiler().Compile("1 + 2", Compiler.Options.Immutable);
      Assert.True(script.IsPrecomputable);
      Assert.Equal(Type.Undefined, script.ValueType);
      script.Execute();
      Assert.Equal(Type.Integer, script.ValueType);
      Assert.Equal(3, script.IntegerValue);
      script.Execute();
      Assert.Equal(3, script.IntegerValue);
    }

    /// <summary>
    /// A deterministic function folded at compile time may compile another script with the same compiler. The
    /// nested compilation must not disturb the fold or the tree of the one that is still in progress.
    /// </summary>
    [Fact]
    public void CompileFromFoldedFunction_DoesNotDisturbOuterCompile()
    {
      Compiler compiler = null;
      compiler = new Compiler();
      compiler.AddCustomFunction(CustomFunction.Create("g", (int x) =>
      {
        var inner = compiler.Compile("100 + 200");
        inner.Execute();
        return x + inner.IntegerValue;
      }, isDeterministic: true));
      var variables = new DictionaryVariableContainer { ["x"] = new VariableValue(4) };

      // g(1) folds while the rest of the tree is not constant
      var script = compiler.Compile("g(1) + 10 * x", Compiler.Options.Immutable, variables);
      script.Execute();
      Assert.Equal(341, script.IntegerValue);

      var folded = compiler.Compile("g(1) + 10");
      folded.Execute();
      Assert.Equal(311, folded.IntegerValue);

      // The pool and the evaluation program are still usable afterwards
      var next = compiler.Compile("g(2) - 1");
      next.Execute();
      Assert.Equal(301, next.IntegerValue);
    }

    // Falls back to a parent container, by re-implementing the interface the script lookup goes through
    private sealed class LayeredContainer : DictionaryVariableContainer, IVariableContainer
    {
      private readonly IVariableContainer _parent;

      public LayeredContainer(IVariableContainer parent) => _parent = parent;

      bool IVariableContainer.TryGetValue(VariableId variableKey, out VariableValue variableValue) =>
        ((IDictionary<VariableId, VariableValue>)this).TryGetValue(variableKey, out variableValue) ||
        _parent.TryGetValue(variableKey, out variableValue);
    }

    [Fact]
    public void ContainerSubclass_LookupGoesThroughItsInterface()
    {
      var parent = new DictionaryVariableContainer { ["g"] = new VariableValue(2) };
      var layered = new LayeredContainer(parent) { ["l"] = new VariableValue(4) };
      var compiler = new Compiler();
      compiler.AddCustomFunction(CustomFunction.CreateContextual("ctxg", "g", (int g) => g * 10));

      var script = compiler.Compile("g + l", Compiler.Options.Immutable, layered);
      script.Execute();
      Assert.Equal(6, script.IntegerValue);

      // As the override container, and through a contextual function
      var plain = compiler.Compile("g + ctxg()", Compiler.Options.Immutable,
        new DictionaryVariableContainer { ["g"] = new VariableValue(1) });
      plain.Execute(new LayeredContainer(parent));
      Assert.Equal(22, plain.IntegerValue);
    }

    [Fact]
    public void AssignmentToUnsupportedVariable_FailsBeforeTheValueRuns()
    {
      var counter = new VariableValue(0);
      var variables = new DictionaryVariableContainer
        { ["u"] = new VariableValue(Type.Undefined), ["i"] = counter };
      var script = new Compiler().Compile("u = (i = 5)", Compiler.Options.None, variables);

      var error = Assert.Throws<ArgumentOutOfRangeException>(() => script.Execute());
      Assert.StartsWith("Unsupported variable type", error.Message);
      Assert.Equal(0, counter.IntegerValue);
    }

    /// <summary>
    /// An entry that exists is used even when its value is null, as IVariableContainer.TryGetValue reports it:
    /// reading it throws rather than falling back to the compile-time container or reporting it undefined.
    /// </summary>
    [Theory]
    [InlineData("x + 1", true)]
    [InlineData("x + 1", false)]
    [InlineData("ctx()", true)]
    [InlineData("ctx()", false)]
    public void NullEntry_IsUsedAndThrowsOnRead(string source, bool inOverride)
    {
      var compiler = new Compiler();
      compiler.AddCustomFunction(CustomFunction.CreateContextual("ctx", "x", (int x) => x * 10));
      var compileTime = new DictionaryVariableContainer { ["x"] = inOverride ? new VariableValue(5) : null };
      var script = compiler.Compile(source, Compiler.Options.Immutable, compileTime);
      var overrides = inOverride ? new DictionaryVariableContainer { ["x"] = null } : null;

      Assert.Throws<NullReferenceException>(() => script.Execute(overrides));
    }

    [Fact]
    public void RemovedEntry_IsUndefined()
    {
      var variables = new DictionaryVariableContainer { ["x"] = null, ["y"] = new VariableValue(1) };
      var script = new Compiler().Compile("x", Compiler.Options.Immutable, variables);
      Assert.Throws<NullReferenceException>(() => script.Execute());

      variables.Remove("x");
      var error = Assert.Throws<RuntimeException>(() => script.Execute());
      Assert.Contains("Undefined variable: x", error.Message);
    }

    /// <summary>
    /// A result that is a literal's register must not be overwritten when the result register is seeded with the
    /// root's value before the first Execute.
    /// </summary>
    [Theory]
    [InlineData("x = 1; 2", 2)]
    [InlineData("(x = 1; 2) + 3", 5)]
    [InlineData("y = (x = 1; 2)", 2)]
    public void ConstantResult_KeepsItsValue(string source, int expected)
    {
      var variables = new DictionaryVariableContainer { ["x"] = new VariableValue(0), ["y"] = new VariableValue(0) };
      var script = new Compiler().Compile(source, Compiler.Options.None, variables);
      script.Execute();
      Assert.Equal(expected, script.IntegerValue);
      script.Execute();
      Assert.Equal(expected, script.IntegerValue);
      Assert.Equal(1, variables["x"].IntegerValue);
    }

    /// <summary>
    /// A NoAlloc script allocates nothing when it runs, including its first run.
    /// </summary>
    [Theory]
    [InlineData("x = x + 1")]
    [InlineData("s == t")]
    [InlineData("s = t")]
    [InlineData("len(s) + x")]
    [InlineData("rdec() * 2 + x")]
    [InlineData("d = 2")]
    [InlineData("d += x")]
    public void NoAllocScript_FirstExecute_DoesNotAllocate(string source)
    {
      var compiler = new Compiler();
      // Decimal results in a float-precision script
      compiler.AddCustomFunction(CustomFunction.Create("rdec", () => 1.5m));
      DictionaryVariableContainer Variables() => new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(1), ["s"] = new VariableValue("a"), ["t"] = new VariableValue("b"),
        ["d"] = new VariableValue(1.5m),
      };

      // Warm up type initialization and JIT on another script
      compiler.Compile(source, Compiler.Options.NoAlloc, Variables()).Execute();
      var script = compiler.Compile(source, Compiler.Options.NoAlloc, Variables());
      var before = System.GC.GetAllocatedBytesForCurrentThread();
      script.Execute();
      Assert.Equal(0, System.GC.GetAllocatedBytesForCurrentThread() - before);
    }

    [Fact]
    public void ShortCircuit_DoesNotEvaluateRight()
    {
      var variables = new DictionaryVariableContainer { ["t"] = new VariableValue(true) };
      var script = new Compiler().Compile("t || missing > 0", Compiler.Options.Immutable, variables);
      Assert.NotNull(script.Program);
      script.Execute();
      Assert.True(script.BooleanValue);
    }

    [Fact]
    public void DecimalConversionCache_InvalidatedByOtherVariableTypes()
    {
      var variables = new DictionaryVariableContainer { ["x"] = new VariableValue(1.5f) };
      var script = new Compiler(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal)
        .Compile("x * 2.0", Compiler.Options.Immutable, variables);
      Assert.NotNull(script.Program);

      script.Execute();
      Assert.Equal(3m, script.DecimalValue);
      variables["x"] = new VariableValue(4m);
      script.Execute();
      Assert.Equal(8m, script.DecimalValue);
      // Same float bits as the first run: the register must not keep the decimal variable's value
      variables["x"] = new VariableValue(1.5f);
      script.Execute();
      Assert.Equal(3m, script.DecimalValue);
    }
  }
}
