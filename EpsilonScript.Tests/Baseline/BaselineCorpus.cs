using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using EpsilonScript.Function;

namespace EpsilonScript.Tests.Baseline
{
  /// <summary>
  /// Writes a trace of public-API outcomes for a fixed corpus of scripts: compile results and errors, values read
  /// through every getter before and after each execution, runtime errors, and variable state after assignments.
  /// Uses only the public API, so the same file builds against earlier versions of the library (see trace.sh).
  /// </summary>
  internal static class BaselineCorpus
  {
    private static readonly string[] Names = { "i", "j", "f", "g", "b", "s", "z" };

    private static readonly string[] Binary =
      { "+", "-", "*", "/", "%", "==", "!=", "<", "<=", ">", ">=", "&&", "||" };

    private static readonly string[] Functions1 = { "abs", "sin", "sqrt", "floor", "len", "lower", "ctxlen", "dneg" };
    private static readonly string[] Functions2 = { "min", "max", "pow", "pick" };

    private static readonly string[] Returns =
      { "rint()", "rlong()", "rdouble()", "rdecimal()", "rnear()", "rbool()", "rstr()" };

    private static readonly string[] AssignmentOperators = { "=", "+=", "-=", "*=", "/=", "%=" };

    private static readonly string[] Fragments =
    {
      "i", "f", "s", "b", "z", "1", "2.5", "\"t\"", "true", "false", "+", "-", "*", "/", "%", "(", ")", ",", ";", "=",
      "+=", "==", "<", "&&", "||", "!", "sin", "max", "ifelse", "rint", "()", "1 / 0", "i * j", "s + i", "dneg",
      "2147483648", "1e3", "0.1",
    };

    // Operand combinations, folding orders and errors that random scripts reach too seldom
    private static readonly string[] Targeted =
    {
      "s + i * j", "s + f * g", "s - i * j", "i * j - s", "i * j + s", "b + i * j", "i * b + j", "s + i * b",
      "s + rdouble() * f", "s + rdecimal()", "s + b", "s + s", "s * i", "s / s", "b + i", "i - b",
      "f == rdouble()", "rulp() == 0.3", "rfloat() == 0.3", "rdec3() == rnear()", "rnear() == 0.3", "0.3 == rnear()",
      "rlong() + rint()", "rdecimal() * f", "rdouble() / 0", "rlong() % 0", "rdecimal() % 0", "rint() / 0.0",
      "-s", "-b", "!i", "!s", "-rlong()", "-rdecimal()", "s < i", "b == i", "s == b", "z == z", "z < f",
      "b && i", "i || b", "b && s", "!b || s", "(i > j) && rint()", "false && (1 / 0 > 0)", "(1 / 0 > 0) && false",
      "1 + (false && (1 / 0 > 0))", "true && b", "b && true", "false || b", "b || false", "1 && true", "true || 1",
      "(\"a\" > 1) && (1 / 0 > 0)", "(1 / 0 > 0) && (\"a\" > 1)", "x && false", "false && x", "(i = 3) && false",
      "i = rdouble()", "f = rdouble()", "z = rdouble()", "f = s", "b = s", "b = 1", "s = i", "s = b", "i += s",
      "i /= 0", "i %= 0", "f /= 0", "z %= 0", "z *= rint()", "s = rnear()", "1 = 2", "i + 1 = 2",
      "1 / 0", "1 % 0", "1.0 / 0", "\"a\" + 1", "\"a\" - 1", "\"a\" + 1 * 2", "1 * 2 + \"a\"", "dneg(false)",
      "dneg(true)", "dneg(\"\")", "dneg(\"a\")", "dneg(i)", "dneg(1 > 2)", "dneg(s)", "pick(true, \"a\")",
      "pick(1, 2)", "max(1, 2, 3)", "sin()", "ifelse(1, 2, 3)", "(1, 2)", "(1, 2);", "1;", ";", "1; 2", "i; j",
      "(1, 2) == 1", "f(1)", "undefined + 1", "i = 1; i + 1", "2147483648", "-2147483648",
      "9223372036854775807 + 1", "1e40 * 1e40", "\"\"", "\"a\" == \"a\"", "-(-1)", "!(!true)", "!true", "-true",
      "+\"a\"", "+i", "1 + 2 * 3 - sqrt(16.0) * 2.5", "(1 + 2) * i", "i * (1 + 2)",
    };

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

        switch (_random.Next(11))
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
          case 5:
            return "+" + Paren(depth - 1);
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
          // Nearly but not exactly 0.3, for the tolerant float equality
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

    private static string Describe(VariableValue value)
    {
      switch (value.Type)
      {
        case Type.Float: return "Float:" + BitConverter.SingleToInt32Bits(value.FloatValue);
        case Type.Double: return "Double:" + BitConverter.DoubleToInt64Bits(value.DoubleValue);
        default: return value.Type + ":" + value.StringValue;
      }
    }

    private static string DescribeVariables(DictionaryVariableContainer variables)
    {
      var sb = new StringBuilder();
      foreach (var name in Names)
      {
        sb.Append(name).Append('=').Append(Describe(variables[name])).Append(' ');
      }

      return sb.ToString();
    }

    // Every getter, so conversions between result types are covered as well
    private static string Describe(CompiledScript script)
    {
      return "type=" + (int)script.ValueType +
             " int=" + Outcome(() => script.IntegerValue.ToString()) +
             " long=" + Outcome(() => script.LongValue.ToString()) +
             " float=" + Outcome(() => BitConverter.SingleToInt32Bits(script.FloatValue).ToString()) +
             " double=" + Outcome(() => BitConverter.DoubleToInt64Bits(script.DoubleValue).ToString()) +
             " decimal=" + Outcome(() => script.DecimalValue.ToString(CultureInfo.InvariantCulture)) +
             " bool=" + Outcome(() => script.BooleanValue.ToString()) +
             " string=" + Outcome(() => script.StringValue ?? "null");
    }

    private static Compiler CreateCompiler(Compiler.IntegerPrecision integerPrecision,
      Compiler.FloatPrecision floatPrecision)
    {
      var compiler = new Compiler(integerPrecision, floatPrecision);
      // Precision-independent signatures covering bool/string parameters, return types other than the configured
      // ones, and context variables
      compiler.AddCustomFunction(CustomFunction.Create("pick", (bool c, string a) => c ? a : "n",
        isDeterministic: true));
      compiler.AddCustomFunction(CustomFunction.CreateContextual("ctxlen", "s", (string s, string t) =>
        s.Length * 10 + t.Length));
      compiler.AddCustomFunction(CustomFunction.Create("rint", () => 3));
      compiler.AddCustomFunction(CustomFunction.Create("rlong", () => -7L));
      compiler.AddCustomFunction(CustomFunction.Create("rdouble", () => 2.5));
      compiler.AddCustomFunction(CustomFunction.Create("rdecimal", () => 1.25m));
      compiler.AddCustomFunction(CustomFunction.Create("rnear", () => 0.1 + 0.2));
      compiler.AddCustomFunction(CustomFunction.Create("rbool", () => true));
      compiler.AddCustomFunction(CustomFunction.Create("rstr", () => "r"));
      // One float ulp above 0.3f as a double, and 0.3f as a float: unequal but nearly equal to 0.3
      compiler.AddCustomFunction(CustomFunction.Create("rulp",
        () => (double)BitConverter.Int32BitsToSingle(BitConverter.SingleToInt32Bits(0.3f) + 1)));
      compiler.AddCustomFunction(CustomFunction.Create("rfloat", () => 0.3f));
      compiler.AddCustomFunction(CustomFunction.Create("rdec3", () => 0.3m));
      // Deterministic, so calls with constant arguments fold at compile time; throws for false or an empty string
      compiler.AddCustomFunction(CustomFunction.Create("dneg", (bool v) =>
      {
        if (!v)
        {
          throw new InvalidOperationException("false");
        }

        return "yes";
      }, isDeterministic: true));
      compiler.AddCustomFunction(CustomFunction.Create("dneg", (string v) =>
      {
        if (v.Length == 0)
        {
          throw new InvalidOperationException("empty");
        }

        return v + v;
      }, isDeterministic: true));
      return compiler;
    }

    private static List<(string Source, Compiler.Options Options, int Seed)> Sources(
      Compiler.IntegerPrecision integerPrecision, Compiler.FloatPrecision floatPrecision)
    {
      var sources = new List<(string, Compiler.Options, int)>();
      var generator = new Generator(1000 + (int)integerPrecision * 10 + (int)floatPrecision);
      var random = new Random(2000 + (int)integerPrecision * 10 + (int)floatPrecision);
      foreach (var source in Targeted)
      {
        sources.Add((source, Compiler.Options.None, 5));
        sources.Add((source, Compiler.Options.NoAlloc, 5));
        sources.Add((source, Compiler.Options.Immutable, 5));
      }

      for (var index = 0; index < 4000; ++index)
      {
        var options = index % 4 == 0
          ? Compiler.Options.NoAlloc | Compiler.Options.Immutable
          : Compiler.Options.Immutable;
        sources.Add((generator.Expression(4), options, index));
      }

      for (var index = 0; index < 2000; ++index)
      {
        var sb = new StringBuilder();
        var count = random.Next(1, 4);
        for (var i = 0; i < count; ++i)
        {
          sb.Append(Names[random.Next(Names.Length)]).Append(' ')
            .Append(AssignmentOperators[random.Next(AssignmentOperators.Length)]).Append(' ')
            .Append(generator.Expression(3)).Append("; ");
        }

        sb.Append(generator.Expression(3));
        sources.Add((sb.ToString(), index % 4 == 0 ? Compiler.Options.NoAlloc : Compiler.Options.None,
          index + 50000));
      }

      // Malformed scripts fail part way through parsing, building, optimizing or validating
      for (var index = 0; index < 4000; ++index)
      {
        var sb = new StringBuilder();
        var count = random.Next(1, 8);
        for (var i = 0; i < count; ++i)
        {
          sb.Append(Fragments[random.Next(Fragments.Length)]).Append(' ');
        }

        sources.Add((sb.ToString(), (Compiler.Options)random.Next(4), index + 90000));
      }

      return sources;
    }

    public static void Write(TextWriter writer, Compiler.IntegerPrecision integerPrecision,
      Compiler.FloatPrecision floatPrecision)
    {
      // Scripts and trace lines format numbers with the current culture; some cultures change the minus sign
      var culture = CultureInfo.CurrentCulture;
      CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
      try
      {
        WriteCorpus(writer, integerPrecision, floatPrecision);
      }
      finally
      {
        CultureInfo.CurrentCulture = culture;
      }
    }

    private static void WriteCorpus(TextWriter writer, Compiler.IntegerPrecision integerPrecision,
      Compiler.FloatPrecision floatPrecision)
    {
      // One compiler for the whole corpus, as an application would use it
      var compiler = CreateCompiler(integerPrecision, floatPrecision);
      var kept = new List<(string Source, CompiledScript Script)>();
      foreach (var (source, options, seed) in Sources(integerPrecision, floatPrecision))
      {
        var variables = Variables(seed);
        CompiledScript script = null;
        var compiled = Outcome(() =>
        {
          script = compiler.Compile(source, options, variables);
          return "ok precomputable=" + script.IsPrecomputable;
        });
        writer.WriteLine($"## {source} [{options}] compile: {compiled}");
        if (script == null)
        {
          continue;
        }

        writer.WriteLine("  before: " + Describe(script));
        var mutations = new Generator(seed + 1);
        for (var run = 0; run < 4; ++run)
        {
          // Change variable types between runs, including through an override container
          DictionaryVariableContainer overrides = null;
          if (run == 3)
          {
            overrides = new DictionaryVariableContainer { ["i"] = mutations.Value() };
          }
          else if (run > 0)
          {
            variables[Names[(seed + run) % Names.Length]] = mutations.Value();
          }

          var result = Outcome(() =>
          {
            script.Execute(overrides);
            return Describe(script);
          });
          writer.WriteLine($"  run {run}: {result} | {DescribeVariables(variables)}");
        }

        if (kept.Count < 300)
        {
          kept.Add((source, script));
        }
      }

      // Earlier programs must not depend on later compilations
      foreach (var (source, script) in kept)
      {
        writer.WriteLine($"## again {source}: " + Outcome(() =>
        {
          script.Execute();
          return Describe(script);
        }));
      }

      WriteScenarios(writer, integerPrecision, floatPrecision);
    }

    // Host-side sequences the generated scripts do not reach: container mutations between runs, containers other
    // than DictionaryVariableContainer, values changed in place, functions added after compiling, contextual and
    // many-parameter functions, the caching compiler, and results after a failed run
    private static void WriteScenarios(TextWriter writer, Compiler.IntegerPrecision integerPrecision,
      Compiler.FloatPrecision floatPrecision)
    {
      void Step(string label, Func<string> action) => writer.WriteLine($"## scenario {label}: {Outcome(action)}");

      string Run(CompiledScript script, IVariableContainer overrides = null)
      {
        script.Execute(overrides);
        return Describe(script);
      }

      var compiler = new Compiler(integerPrecision, floatPrecision);

      // Container mutations and in-place value changes between runs of the same scripts
      var variables = new DictionaryVariableContainer();
      var x = new VariableValue(1);
      variables["x"] = x;
      variables["y"] = new VariableValue(2.5);
      CompiledScript read = null, write = null;
      Step("compile read", () =>
      {
        read = compiler.Compile("x + y * 2", Compiler.Options.Immutable, variables);
        return "ok";
      });
      Step("compile write", () =>
      {
        write = compiler.Compile("x = x * 2 + 1; y += x", Compiler.Options.None, variables);
        return "ok";
      });
      var sequence = new (string Label, Action Mutate)[]
      {
        ("initial", () => { }),
        ("x in place float", () => x.CopyFrom(new VariableValue(2.5f))),
        ("x in place string", () => x.CopyFrom(new VariableValue("q"))),
        ("x in place long", () => x.CopyFrom(new VariableValue(3L))),
        ("x replaced undefined", () => variables["x"] = new VariableValue(Type.Undefined)),
        ("x undefined set", () => variables["x"].IntegerValue = 4),
        ("x removed", () => variables.Remove("x")),
        ("x re-added", () => variables.Add("x", new VariableValue(10L))),
        ("x removed by pair", () => variables.Remove(new KeyValuePair<VariableId, VariableValue>("x", variables["x"]))),
        ("x added by pair", () => variables.Add(new KeyValuePair<VariableId, VariableValue>("x", new VariableValue(3)))),
        ("x null", () => variables["x"] = null),
        ("x decimal", () => variables["x"] = new VariableValue(1.5m)),
        ("x boolean", () => variables["x"] = new VariableValue(true)),
        ("y nan", () => { variables["x"] = new VariableValue(1); variables["y"] = new VariableValue(double.NaN); }),
        ("y huge", () => variables["y"] = new VariableValue(1e30)),
        ("y huge float", () => variables["y"] = new VariableValue(float.PositiveInfinity)),
        ("cleared", () => variables.Clear()),
        ("refilled", () =>
        {
          variables["y"] = new VariableValue(0.5f);
          variables["x"] = new VariableValue(-2);
        }),
      };
      foreach (var (label, mutate) in sequence)
      {
        mutate();
        Step(label + " read", () => Run(read));
        Step(label + " write", () => Run(write) + " | x=" + Outcome(() => Describe(variables["x"])) + " y=" +
                                     Outcome(() => Describe(variables["y"])));
      }

      // Container types: subclasses, an interface re-implementation, and a plain IVariableContainer
      var subclass = new SubclassContainer { ["x"] = new VariableValue(5) };
      var reimplemented = new ReimplementedContainer { ["x"] = new VariableValue(5) };
      var plain = new PlainContainer();
      plain.Values["x"] = new VariableValue(7);
      foreach (var (label, container) in new (string, IVariableContainer)[]
                 { ("subclass", subclass), ("reimplemented", reimplemented), ("plain", plain) })
      {
        CompiledScript script = null;
        Step(label + " compile", () =>
        {
          script = compiler.Compile("x * 3", Compiler.Options.None, container);
          return "ok";
        });
        if (script == null)
        {
          continue;
        }

        Step(label + " run", () => Run(script));
        Step(label + " as override", () => Run(script, plain));
        Step(label + " reimplemented override", () => Run(script, new ReimplementedContainer()));
        Step(label + " override", () => Run(script, new DictionaryVariableContainer { ["x"] = new VariableValue(2) }));
      }

      subclass["x"] = new VariableValue(6);
      Step("subclass after set", () => Run(compiler.Compile("x * 3", Compiler.Options.None, subclass)));
      plain.Values.Remove("x");
      Step("plain after remove", () => Run(compiler.Compile("x * 3", Compiler.Options.None, plain)));

      // Contextual functions with two and three context variables, overridden, missing and null
      var context = new DictionaryVariableContainer
      {
        ["x"] = new VariableValue(1), ["y"] = new VariableValue(2.5), ["s"] = new VariableValue("ab"),
      };
      Step("add cx2", () =>
      {
        compiler.AddCustomFunction(CustomFunction.CreateContextual("cx2", "x", "y",
          (int a, double b, string c) => a * 100 + (int)(b * 10) + c.Length));
        return "ok";
      });
      Step("add cx3", () =>
      {
        compiler.AddCustomFunction(CustomFunction.CreateContextual("cx3", "x", "y", "s",
          (int a, float b, string c, bool d) => d ? c + a + b : c));
        return "ok";
      });
      CompiledScript cx2 = null, cx3 = null;
      Step("compile cx2", () =>
      {
        cx2 = compiler.Compile("cx2(\"abc\")", Compiler.Options.Immutable, context);
        return "ok";
      });
      Step("compile cx3", () =>
      {
        cx3 = compiler.Compile("cx3(x > 0)", Compiler.Options.Immutable, context);
        return "ok";
      });
      var overrides = new (string Label, IVariableContainer Container)[]
      {
        ("none", null),
        ("y string", new DictionaryVariableContainer { ["y"] = new VariableValue("4.5") }),
        ("y null", new DictionaryVariableContainer { ["y"] = null }),
        ("s override", new DictionaryVariableContainer { ["s"] = new VariableValue("zzz"), ["x"] = new VariableValue(-1) }),
        ("plain", plain),
        ("reimplemented", new ReimplementedContainer()),
      };
      foreach (var (label, container) in overrides)
      {
        Step("cx2 " + label, () => Run(cx2, container));
        Step("cx3 " + label, () => Run(cx3, container));
      }

      context.Remove("x");
      Step("cx2 x removed", () => Run(cx2));
      Step("cx3 x removed", () => Run(cx3));

      // A context variable of an unsupported type fails only once the variable is found
      var unsupported = new Compiler(integerPrecision, floatPrecision);
      Step("add cxu", () =>
      {
        unsupported.AddCustomFunction(CustomFunction.CreateContextual<DateTime, int>("cxu", "u", a => 1));
        unsupported.AddCustomFunction(CustomFunction.CreateContextual<int, DateTime, int, int>("cxv", "x", "u",
          (a, b, c) => a + c));
        return "ok";
      });
      Step("cxu missing", () => Run(unsupported.Compile("cxu()")));
      Step("cxu present", () => Run(unsupported.Compile("cxu()", Compiler.Options.None,
        new DictionaryVariableContainer { ["u"] = new VariableValue(1) })));
      Step("cxv first missing", () => Run(unsupported.Compile("cxv(2)")));
      Step("cxv second missing", () => Run(unsupported.Compile("cxv(2)", Compiler.Options.None,
        new DictionaryVariableContainer { ["x"] = new VariableValue(1) })));
      Step("cxv present", () => Run(unsupported.Compile("cxv(2)", Compiler.Options.None,
        new DictionaryVariableContainer { ["x"] = new VariableValue(1), ["u"] = new VariableValue(1) })));

      // Overloads added after compiling; numeric parameters in every precision, mismatched ones refused
      var functions = new Compiler(integerPrecision, floatPrecision);
      var numeric = new DictionaryVariableContainer
      {
        ["i"] = new VariableValue(3), ["f"] = new VariableValue(2.5f), ["d"] = new VariableValue(1.25m),
      };
      var additions = new (string Label, Func<CustomFunction> Create)[]
      {
        ("ff int", () => CustomFunction.Create("ff", (int a) => a + 1)),
        ("ff long", () => CustomFunction.Create("ff", (long a) => a + 2)),
        ("n3 int", () => CustomFunction.Create("n3", (int a, int b, int c) => a * b - c)),
        ("n3 long", () => CustomFunction.Create("n3", (long a, long b, long c) => a * b - c)),
        ("n4 float", () => CustomFunction.Create("n4", (float a, float b, float c, float e) => a * b - c / e)),
        ("n4 double", () => CustomFunction.Create("n4", (double a, double b, double c, double e) => a * b - c / e)),
        ("n4 decimal", () => CustomFunction.Create("n4", (decimal a, decimal b, decimal c, decimal e) => a * b - c / e)),
        ("n5 int float", () => CustomFunction.Create("n5", (int a, float b, string c, bool e, int g) =>
          e ? c + (a + g) : c + b)),
        ("n5 long double", () => CustomFunction.Create("n5", (long a, double b, string c, bool e, long g) =>
          e ? c + (a + g) : c + b)),
        ("n5 int decimal", () => CustomFunction.Create("n5", (int a, decimal b, string c, bool e, int g) =>
          e ? c + (a + g) : c + b)),
        ("n5 long decimal", () => CustomFunction.Create("n5", (long a, decimal b, string c, bool e, long g) =>
          e ? c + (a + g) : c + b)),
      };
      CompiledScript ff = null;
      Step("ff unknown compile", () =>
      {
        ff = functions.Compile("ff(f)", Compiler.Options.Immutable, numeric);
        return "ok";
      });
      foreach (var (label, create) in additions)
      {
        Step("add " + label, () =>
        {
          functions.AddCustomFunction(create());
          return "ok";
        });
        if (ff == null)
        {
          Step("ff compile after " + label, () =>
          {
            ff = functions.Compile("ff(f)", Compiler.Options.Immutable, numeric);
            return "ok";
          });
        }
      }

      Step("add ff float", () =>
      {
        functions.AddCustomFunction(CustomFunction.Create("ff", (float a) => a * 10));
        functions.AddCustomFunction(CustomFunction.Create("ff", (double a) => a * 20));
        functions.AddCustomFunction(CustomFunction.Create("ff", (decimal a) => a * 30));
        return "ok";
      });
      if (ff != null)
      {
        Step("ff after float overload", () => Run(ff));
      }

      foreach (var source in new[]
               {
                 "ff(i)", "ff(f)", "ff(d)", "n3(i, 2, 1)", "n3(i, f, 1)", "n4(f, 2, i, 4)", "n4(d, 2, 3, 0)",
                 "n5(i, f, \"p\", true, 2)", "n5(i, d, \"p\", false, 2)", "n5(1, 2, 3, 4, 5)",
               })
      {
        Step("call " + source, () => Run(functions.Compile(source, Compiler.Options.None, numeric)));
      }

      // A CustomFunction subclass from outside the library cannot implement execution
      Step("external function", () =>
      {
        var external = new Compiler(integerPrecision, floatPrecision);
        external.AddCustomFunction(new ExternalFunction("ext", Type.Integer));
        external.AddCustomFunction(new ExternalFunction("extc", Type.String, true));
        var plainCall = Outcome(() => Run(external.Compile("ext()")));
        var contextualCall = Outcome(() => Run(external.Compile("extc() + 1")));
        return plainCall + " / " + contextualCall;
      });

      // Results read after a failed run, and after a later successful one
      var failing = new DictionaryVariableContainer
        { ["a"] = new VariableValue(7), ["b"] = new VariableValue(0), ["f"] = new VariableValue(1e30) };
      foreach (var source in new[]
               {
                 "a / b", "a % b", "f / 0.0", "a * b + 1 / b", "1 / b + a * b", "a + true", "a * a * a * a * a * f",
                 "f * f * 1.0e30 - a", "a = f",
               })
      {
        CompiledScript script = null;
        Step("failing compile " + source, () =>
        {
          script = compiler.Compile(source, Compiler.Options.None, failing);
          return "ok";
        });
        if (script == null)
        {
          continue;
        }

        Step("failing run " + source, () => Run(script));
        Step("failing after " + source, () => Describe(script));
        Step("failing recovered " + source, () => Run(script,
          new DictionaryVariableContainer { ["b"] = new VariableValue(2), ["f"] = new VariableValue(3) }));
      }

      // Results read after a run that fails following a successful one
      foreach (var (source, before, after) in new (string, Action<DictionaryVariableContainer>,
                 Action<DictionaryVariableContainer>)[]
               {
                 ("p; q + r", v => v["r"] = new VariableValue(1.5f), v => v["r"] = new VariableValue(true)),
                 ("p; 0 - r", v => v["r"] = new VariableValue(2.5), v => v["r"] = new VariableValue("s")),
                 ("p; q / r", v => v["r"] = new VariableValue(4.0), v => v["r"] = new VariableValue(0)),
                 ("q * r", v => v["r"] = new VariableValue(3),
                   v => { v["q"] = new VariableValue(decimal.MaxValue); v["r"] = new VariableValue(2m); }),
                 ("q * r", v => v["r"] = new VariableValue(1.5m),
                   v => { v["q"] = new VariableValue(decimal.MaxValue); v["r"] = new VariableValue(2m); }),
                 ("q * r + q", v => v["r"] = new VariableValue(1.5m),
                   v => { v["q"] = new VariableValue(decimal.MaxValue); v["r"] = new VariableValue(2m); }),
                 ("q / r", v => v["r"] = new VariableValue(4m), v => v["r"] = new VariableValue(0.1m)),
                 ("abs(r)", v => v["r"] = new VariableValue(1e30), v => v["r"] = new VariableValue(long.MinValue)),
                 ("abs(r)", v => v["r"] = new VariableValue(-3), v => v["r"] = new VariableValue(int.MinValue)),
               })
      {
        var values = new DictionaryVariableContainer { ["p"] = new VariableValue(1), ["q"] = new VariableValue(2) };
        before(values);
        CompiledScript script = null;
        Step("refailing compile " + source, () =>
        {
          script = compiler.Compile(source, Compiler.Options.None, values);
          return "ok";
        });
        if (script == null)
        {
          continue;
        }

        Step("refailing first " + source, () => Run(script));
        after(values);
        Step("refailing run " + source, () => Run(script));
        Step("refailing after " + source, () => Describe(script));
      }

      // Folding a short-circuited subtree, whose skipped operand would fail if it ran
      var folding = new Compiler(integerPrecision, floatPrecision);
      var folds = 0;
      folding.AddCustomFunction(CustomFunction.Create("nb", (bool a) =>
      {
        ++folds;
        return !a;
      }, true));
      var flag = new DictionaryVariableContainer { ["b"] = new VariableValue(false) };
      foreach (var source in new[]
               {
                 "b || (nb(true) && nb(1))", "nb(true) && true - 1 || b", "nb(true) && -true || b",
                 "nb(true) && ifelse(1, 2, 3) || b", "(nb(false) || nb(1)) && b", "b || nb(false) && false",
                 "nb(nb(true)) && true && b", "nb(nb(true)) && true && true && b", "b && (nb(false) || nb(1))",
                 "b || (nb(true) && (nb(true) || nb(1)))",
               })
      {
        folds = 0;
        Step("fold " + source, () => Run(folding.Compile(source, Compiler.Options.None, flag)) + " calls=" + folds);
      }

      // The caching compiler returns the same script for the same source, options and variables
      var caching = new CachingCompiler(integerPrecision, floatPrecision);

      // The lookup was renamed from Compile to GetOrCompile after 03593b8; the trace runs against both
      CompiledScript CacheGet(CachingCompiler cache, object source, Compiler.Options options,
        IVariableContainer variables)
      {
        foreach (var method in typeof(CachingCompiler).GetMethods())
        {
          var parameters = method.GetParameters();
          if ((method.Name == "GetOrCompile" || method.Name == "Compile") && parameters.Length == 3 &&
              parameters[0].ParameterType == source.GetType())
          {
            try
            {
              return (CompiledScript)method.Invoke(cache, new[] { source, options, variables });
            }
            catch (System.Reflection.TargetInvocationException e)
            {
              System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(e.InnerException).Throw();
            }
          }
        }

        throw new MissingMethodException(nameof(CachingCompiler), "GetOrCompile");
      }
      var cacheVariables = new DictionaryVariableContainer { ["x"] = new VariableValue(4) };
      Step("cache", () =>
      {
        var first = CacheGet(caching, "x + 1", Compiler.Options.Immutable, cacheVariables);
        var same = CacheGet(caching, "x + " + "1", Compiler.Options.Immutable, cacheVariables);
        var cached = CacheGet(caching, CachedSourceText.From("x + 1"), Compiler.Options.Immutable, cacheVariables);
        var otherOptions = CacheGet(caching, "x + 1", Compiler.Options.None, cacheVariables);
        var otherVariables = CacheGet(caching, "x + 1", Compiler.Options.Immutable,
          new DictionaryVariableContainer { ["x"] = new VariableValue(5) });
        var otherSource = CacheGet(caching, "x + 2", Compiler.Options.Immutable, cacheVariables);
        caching.Clear();
        var afterClear = CacheGet(caching, "x + 1", Compiler.Options.Immutable, cacheVariables);
        return ReferenceEquals(first, same) + " " + ReferenceEquals(first, cached) + " " +
               ReferenceEquals(first, otherOptions) + " " + ReferenceEquals(first, otherVariables) + " " +
               ReferenceEquals(first, otherSource) + " " + ReferenceEquals(first, afterClear) + " " +
               Run(first) + " " + Run(otherVariables) + " " + Run(otherSource);
      });
    }

    private sealed class SubclassContainer : DictionaryVariableContainer
    {
    }

    // Answers x itself, whatever the dictionary holds
    private sealed class ReimplementedContainer : DictionaryVariableContainer, IVariableContainer
    {
      bool IVariableContainer.TryGetValue(VariableId key, out VariableValue value)
      {
        if (key == (VariableId)"x")
        {
          value = new VariableValue(100);
          return true;
        }

        value = null;
        return false;
      }
    }

    private sealed class PlainContainer : IVariableContainer
    {
      public readonly Dictionary<VariableId, VariableValue> Values = new Dictionary<VariableId, VariableValue>();

      public bool TryGetValue(VariableId key, out VariableValue value) => Values.TryGetValue(key, out value);
    }

    private sealed class ExternalFunction : CustomFunction
    {
      public ExternalFunction(string name, Type returnType, bool contextual = false)
        : base(name, false, Array.Empty<Type>(), returnType, contextual)
      {
      }
    }
  }
}
