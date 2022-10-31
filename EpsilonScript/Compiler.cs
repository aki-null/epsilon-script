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

    private readonly TokenParser _tokenParser;
    private readonly RpnConverter _rpnConverter;
    private readonly AstBuilder _astBuilder;

    private readonly Dictionary<uint, CustomFunctionOverload> _functions =
      new Dictionary<uint, CustomFunctionOverload>();

    public Compiler()
    {
      // Built-in functions
      AddCustomFunction(new CustomFunction("sin", (float v) => (float)System.Math.Sin(v), true));
      AddCustomFunction(new CustomFunction("cos", (float v) => (float)System.Math.Cos(v), true));
      AddCustomFunction(new CustomFunction("tan", (float v) => (float)System.Math.Tan(v), true));
      AddCustomFunction(new CustomFunction("asin", (float v) => (float)System.Math.Asin(v), true));
      AddCustomFunction(new CustomFunction("acos", (float v) => (float)System.Math.Acos(v), true));
      AddCustomFunction(new CustomFunction("atan", (float v) => (float)System.Math.Atan(v), true));
      AddCustomFunction(new CustomFunction("sinh", (float v) => (float)System.Math.Sinh(v), true));
      AddCustomFunction(new CustomFunction("cosh", (float v) => (float)System.Math.Cosh(v), true));
      AddCustomFunction(new CustomFunction("tanh", (float v) => (float)System.Math.Tanh(v), true));
      AddCustomFunction(new CustomFunction("atan2", (v1, v2) => (float)System.Math.Atan2(v1, v2), true));
      AddCustomFunction(new CustomFunction("sqrt", (float v) => (float)System.Math.Sqrt(v), true));
      AddCustomFunction(new CustomFunction("abs", System.Math.Abs, true));
      AddCustomFunction(new CustomFunction("abs", (float v) => System.Math.Abs(v), true));
      AddCustomFunction(new CustomFunction("floor", v => (float)System.Math.Floor(v), true));
      AddCustomFunction(new CustomFunction("ceil", v => (float)System.Math.Ceiling(v), true));
      AddCustomFunction(new CustomFunction("trunc", v => (float)System.Math.Truncate(v), true));
      AddCustomFunction(new CustomFunction("min", System.Math.Min, true));
      AddCustomFunction(new CustomFunction("min", (float v1, float v2) => System.Math.Min(v1, v2), true));
      AddCustomFunction(new CustomFunction("max", System.Math.Max, true));
      AddCustomFunction(new CustomFunction("max", (float v1, float v2) => System.Math.Max(v1, v2), true));
      AddCustomFunction(new CustomFunction("ifelse", (cond, v1, v2) => cond ? v1 : v2, true));
      AddCustomFunction(new CustomFunction("ifelse", (bool cond, float v1, float v2) => cond ? v1 : v2, true));
      AddCustomFunction(new CustomFunction("pow", (v1, v2) => (float)System.Math.Pow(v1, v2), true));
      AddCustomFunction(new CustomFunction("lower", s => s.ToLowerInvariant(), true));
      AddCustomFunction(new CustomFunction("upper", s => s.ToUpperInvariant(), true));
      AddCustomFunction(new CustomFunction("len", s => s.Length, true));

      _astBuilder = new AstBuilder(_functions);
      _rpnConverter = new RpnConverter(_astBuilder);
      _tokenParser = new TokenParser(_rpnConverter);
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

      _astBuilder.Configure(options, variables);

      new Lexer.Lexer().Execute(source, _tokenParser);
      var rootNode = _astBuilder.Result;
      rootNode.Optimize();

      _astBuilder.Reset();
      _rpnConverter.Reset();

      return new CompiledScript(rootNode);
    }

    public void AddCustomFunction(CustomFunction func)
    {
      if (_functions.TryGetValue(func.Name, out var overload))
      {
        overload.Add(func);
      }
      else
      {
        _functions[func.Name] = new CustomFunctionOverload(func);
      }
    }
  }
}