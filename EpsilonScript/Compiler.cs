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

    private readonly Dictionary<VariableId, CustomFunctionOverload> _functions =
      new Dictionary<VariableId, CustomFunctionOverload>();

    public Compiler()
    {
      // Built-in functions
      AddCustomFunction(CustomFunction.Create("sin", (float v) => (float)System.Math.Sin(v), true));
      AddCustomFunction(CustomFunction.Create("cos", (float v) => (float)System.Math.Cos(v), true));
      AddCustomFunction(CustomFunction.Create("tan", (float v) => (float)System.Math.Tan(v), true));
      AddCustomFunction(CustomFunction.Create("asin", (float v) => (float)System.Math.Asin(v), true));
      AddCustomFunction(CustomFunction.Create("acos", (float v) => (float)System.Math.Acos(v), true));
      AddCustomFunction(CustomFunction.Create("atan", (float v) => (float)System.Math.Atan(v), true));
      AddCustomFunction(CustomFunction.Create("sinh", (float v) => (float)System.Math.Sinh(v), true));
      AddCustomFunction(CustomFunction.Create("cosh", (float v) => (float)System.Math.Cosh(v), true));
      AddCustomFunction(CustomFunction.Create("tanh", (float v) => (float)System.Math.Tanh(v), true));
      AddCustomFunction(CustomFunction.Create("atan2", (float v1, float v2) => (float)System.Math.Atan2(v1, v2), true));
      AddCustomFunction(CustomFunction.Create("sqrt", (float v) => (float)System.Math.Sqrt(v), true));
      AddCustomFunction(CustomFunction.Create("abs", (int v) => System.Math.Abs(v), true));
      AddCustomFunction(CustomFunction.Create("abs", (float v) => System.Math.Abs(v), true));
      AddCustomFunction(CustomFunction.Create("floor", (float v) => (float)System.Math.Floor(v), true));
      AddCustomFunction(CustomFunction.Create("ceil", (float v) => (float)System.Math.Ceiling(v), true));
      AddCustomFunction(CustomFunction.Create("trunc", (float v) => (float)System.Math.Truncate(v), true));
      AddCustomFunction(CustomFunction.Create("min", (int v1, int v2) => System.Math.Min(v1, v2), true));
      AddCustomFunction(CustomFunction.Create("min", (float v1, float v2) => System.Math.Min(v1, v2), true));
      AddCustomFunction(CustomFunction.Create("max", (int v1, int v2) => System.Math.Max(v1, v2), true));
      AddCustomFunction(CustomFunction.Create("max", (float v1, float v2) => System.Math.Max(v1, v2), true));
      AddCustomFunction(CustomFunction.Create("ifelse", (bool cond, int v1, int v2) => cond ? v1 : v2, true));
      AddCustomFunction(CustomFunction.Create("ifelse", (bool cond, float v1, float v2) => cond ? v1 : v2, true));
      AddCustomFunction(CustomFunction.Create("ifelse", (bool cond, string v1, string v2) => cond ? v1 : v2, true));
      AddCustomFunction(CustomFunction.Create("pow", (float v1, float v2) => (float)System.Math.Pow(v1, v2), true));
      AddCustomFunction(CustomFunction.Create("lower", (string s) => s.ToLowerInvariant(), true));
      AddCustomFunction(CustomFunction.Create("upper", (string s) => s.ToUpperInvariant(), true));
      AddCustomFunction(CustomFunction.Create("len", (string s) => s.Length, true));

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