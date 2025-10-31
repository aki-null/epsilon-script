using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Parser;

namespace EpsilonScript
{
  public class Compiler
  {
    [Flags]
    public enum Options : short
    {
      None = 0,
      Immutable = 1,
    }

    public enum IntegerPrecision
    {
      Integer, // 32-bit int
      Long, // 64-bit long
    }

    public enum FloatPrecision
    {
      Float, // 32-bit float
      Double, // 64-bit double
      Decimal, // 128-bit decimal
    }

    private readonly TokenParser _tokenParser;
    private readonly RpnConverter _rpnConverter;
    private readonly AstBuilder _astBuilder;

    private readonly Dictionary<VariableId, CustomFunctionOverload> _functions =
      new Dictionary<VariableId, CustomFunctionOverload>();

    public IntegerPrecision DefaultIntegerType { get; }
    public FloatPrecision DefaultFloatType { get; }

    public Compiler() : this(IntegerPrecision.Integer, FloatPrecision.Float)
    {
    }

    public Compiler(IntegerPrecision integerPrecision, FloatPrecision floatPrecision)
    {
      DefaultIntegerType = integerPrecision;
      DefaultFloatType = floatPrecision;

      // Register built-in functions with appropriate precision
      RegisterBuiltInFunctions();

      _astBuilder = new AstBuilder(_functions);
      _rpnConverter = new RpnConverter(_astBuilder);
      _tokenParser = new TokenParser(_rpnConverter);
    }

    private void RegisterBuiltInFunctions()
    {
      // Register integer precision functions
      if (DefaultIntegerType == IntegerPrecision.Integer)
      {
        AddCustomFunction(CustomFunction.Create("abs", (int v) => System.Math.Abs(v), isDeterministic: true));
        AddCustomFunction(CustomFunction.Create("min", (int v1, int v2) => System.Math.Min(v1, v2),
          isDeterministic: true));
        AddCustomFunction(CustomFunction.Create("max", (int v1, int v2) => System.Math.Max(v1, v2),
          isDeterministic: true));
        AddCustomFunction(CustomFunction.Create("ifelse", (bool cond, int v1, int v2) => cond ? v1 : v2,
          isDeterministic: true));
      }
      else // Long
      {
        AddCustomFunction(CustomFunction.Create("abs", (long v) => System.Math.Abs(v), isDeterministic: true));
        AddCustomFunction(CustomFunction.Create("min", (long v1, long v2) => System.Math.Min(v1, v2),
          isDeterministic: true));
        AddCustomFunction(CustomFunction.Create("max", (long v1, long v2) => System.Math.Max(v1, v2),
          isDeterministic: true));
        AddCustomFunction(CustomFunction.Create("ifelse", (bool cond, long v1, long v2) => cond ? v1 : v2,
          isDeterministic: true));
      }

      // Register float precision functions
      switch (DefaultFloatType)
      {
        case FloatPrecision.Float:
          RegisterFloatFunctions();
          break;
        case FloatPrecision.Double:
          RegisterDoubleFunctions();
          break;
        case FloatPrecision.Decimal:
          RegisterDecimalFunctions();
          break;
      }

      // String functions (precision-independent)
      AddCustomFunction(CustomFunction.Create("lower", (string s) => s.ToLowerInvariant(), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("upper", (string s) => s.ToUpperInvariant(), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("len", (string s) => s.Length, isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("ifelse", (bool cond, string v1, string v2) => cond ? v1 : v2,
        isDeterministic: true));
    }

    private void RegisterFloatFunctions()
    {
      AddCustomFunction(CustomFunction.Create("sin", (float v) => MathF.Sin(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("cos", (float v) => MathF.Cos(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("tan", (float v) => MathF.Tan(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("asin", (float v) => MathF.Asin(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("acos", (float v) => MathF.Acos(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("atan", (float v) => MathF.Atan(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("sinh", (float v) => MathF.Sinh(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("cosh", (float v) => MathF.Cosh(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("tanh", (float v) => MathF.Tanh(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("atan2", (float v1, float v2) => MathF.Atan2(v1, v2),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("sqrt", (float v) => MathF.Sqrt(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("abs", (float v) => MathF.Abs(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("floor", (float v) => MathF.Floor(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("ceil", (float v) => MathF.Ceiling(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("trunc", (float v) => MathF.Truncate(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("min", (float v1, float v2) => MathF.Min(v1, v2), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("max", (float v1, float v2) => MathF.Max(v1, v2), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("pow", (float v1, float v2) => MathF.Pow(v1, v2), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("ifelse", (bool cond, float v1, float v2) => cond ? v1 : v2,
        isDeterministic: true));
    }

    private void RegisterDoubleFunctions()
    {
      AddCustomFunction(CustomFunction.Create("sin", (double v) => System.Math.Sin(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("cos", (double v) => System.Math.Cos(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("tan", (double v) => System.Math.Tan(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("asin", (double v) => System.Math.Asin(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("acos", (double v) => System.Math.Acos(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("atan", (double v) => System.Math.Atan(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("sinh", (double v) => System.Math.Sinh(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("cosh", (double v) => System.Math.Cosh(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("tanh", (double v) => System.Math.Tanh(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("atan2", (double v1, double v2) => System.Math.Atan2(v1, v2),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("sqrt", (double v) => System.Math.Sqrt(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("abs", (double v) => System.Math.Abs(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("floor", (double v) => System.Math.Floor(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("ceil", (double v) => System.Math.Ceiling(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("trunc", (double v) => System.Math.Truncate(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("min", (double v1, double v2) => System.Math.Min(v1, v2),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("max", (double v1, double v2) => System.Math.Max(v1, v2),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("pow", (double v1, double v2) => System.Math.Pow(v1, v2),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("ifelse", (bool cond, double v1, double v2) => cond ? v1 : v2,
        isDeterministic: true));
    }

    private void RegisterDecimalFunctions()
    {
      AddCustomFunction(CustomFunction.Create("sqrt", (decimal v) => (decimal)System.Math.Sqrt((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("abs", (decimal v) => System.Math.Abs(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("floor", (decimal v) => System.Math.Floor(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("ceil", (decimal v) => System.Math.Ceiling(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("trunc", (decimal v) => System.Math.Truncate(v), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("min", (decimal v1, decimal v2) => System.Math.Min(v1, v2),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("max", (decimal v1, decimal v2) => System.Math.Max(v1, v2),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("ifelse", (bool cond, decimal v1, decimal v2) => cond ? v1 : v2,
        isDeterministic: true));

      // Trigonometric functions with decimal (convert to double for computation)
      AddCustomFunction(CustomFunction.Create("sin", (decimal v) => (decimal)System.Math.Sin((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("cos", (decimal v) => (decimal)System.Math.Cos((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("tan", (decimal v) => (decimal)System.Math.Tan((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("asin", (decimal v) => (decimal)System.Math.Asin((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("acos", (decimal v) => (decimal)System.Math.Acos((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("atan", (decimal v) => (decimal)System.Math.Atan((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("sinh", (decimal v) => (decimal)System.Math.Sinh((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("cosh", (decimal v) => (decimal)System.Math.Cosh((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("tanh", (decimal v) => (decimal)System.Math.Tanh((double)v),
        isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("atan2",
        (decimal v1, decimal v2) => (decimal)System.Math.Atan2((double)v1, (double)v2), isDeterministic: true));
      AddCustomFunction(CustomFunction.Create("pow",
        (decimal v1, decimal v2) => (decimal)System.Math.Pow((double)v1, (double)v2), isDeterministic: true));
    }

    public CompiledScript Compile(string source, Options options = Options.None, IVariableContainer variables = null)
    {
      return Compile(source.AsMemory(), options, variables);
    }

    public CompiledScript Compile(ReadOnlyMemory<char> source, Options options = Options.None,
      IVariableContainer variables = null)
    {
      _astBuilder.Reset();
      _rpnConverter.Reset();
      _tokenParser.Reset();

      _astBuilder.Configure(options, variables, DefaultIntegerType, DefaultFloatType);

      new Lexer.Lexer().Execute(source, _tokenParser);
      var rootNode = _astBuilder.Result;
      rootNode = rootNode.Optimize();

      _astBuilder.Reset();
      _rpnConverter.Reset();

      return new CompiledScript(rootNode, DefaultIntegerType, DefaultFloatType);
    }

    public void AddCustomFunction(CustomFunction func)
    {
      if (_functions.TryGetValue(func.Name, out var overload))
      {
        overload.Add(func);
      }
      else
      {
        _functions[func.Name] = new CustomFunctionOverload(func, DefaultFloatType);
      }
    }

    public void AddCustomFunctionRange(IEnumerable<CustomFunction> functions)
    {
      if (functions == null)
      {
        throw new ArgumentNullException(nameof(functions));
      }

      foreach (var func in functions)
      {
        AddCustomFunction(func);
      }
    }
  }
}