using System;
using EpsilonScript.AST;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Parser;
using EpsilonScript.VirtualMachine;

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

    private readonly CustomFunctionContainer _functions = new CustomFunctionContainer();

    // This virtual machine instance is used to execute constant part of a program to calculate a concrete value
    private readonly VirtualMachine.VirtualMachine _constantVm = new VirtualMachine.VirtualMachine();
    private readonly MutableProgram _programCache;

    public Compiler()
    {
      // Built-in functions
      _functions.Add(new CustomFunction("sin", (float v) => (float)System.Math.Sin(v), true));
      _functions.Add(new CustomFunction("cos", (float v) => (float)System.Math.Cos(v), true));
      _functions.Add(new CustomFunction("tan", (float v) => (float)System.Math.Tan(v), true));
      _functions.Add(new CustomFunction("asin", (float v) => (float)System.Math.Asin(v), true));
      _functions.Add(new CustomFunction("acos", (float v) => (float)System.Math.Acos(v), true));
      _functions.Add(new CustomFunction("atan", (float v) => (float)System.Math.Atan(v), true));
      _functions.Add(new CustomFunction("sinh", (float v) => (float)System.Math.Sinh(v), true));
      _functions.Add(new CustomFunction("cosh", (float v) => (float)System.Math.Cosh(v), true));
      _functions.Add(new CustomFunction("tanh", (float v) => (float)System.Math.Tanh(v), true));
      _functions.Add(new CustomFunction("atan2", (v1, v2) => (float)System.Math.Atan2(v1, v2), true));
      _functions.Add(new CustomFunction("sqrt", (float v) => (float)System.Math.Sqrt(v), true));
      _functions.Add(new CustomFunction("abs", System.Math.Abs, true));
      _functions.Add(new CustomFunction("abs", (float v) => System.Math.Abs(v), true));
      _functions.Add(new CustomFunction("floor", v => (float)System.Math.Floor(v), true));
      _functions.Add(new CustomFunction("ceil", v => (float)System.Math.Ceiling(v), true));
      _functions.Add(new CustomFunction("trunc", v => (float)System.Math.Truncate(v), true));
      _functions.Add(new CustomFunction("min", System.Math.Min, true));
      _functions.Add(new CustomFunction("min", (float v1, float v2) => System.Math.Min(v1, v2), true));
      _functions.Add(new CustomFunction("max", System.Math.Max, true));
      _functions.Add(new CustomFunction("max", (float v1, float v2) => System.Math.Max(v1, v2), true));
      _functions.Add(new CustomFunction("ifelse", (bool cond, int v1, int v2) => cond ? v1 : v2, true));
      _functions.Add(new CustomFunction("ifelse", (bool cond, float v1, float v2) => cond ? v1 : v2, true));
      _functions.Add(new CustomFunction("ifelse", (bool cond, string v1, string v2) => cond ? v1 : v2, true));
      _functions.Add(new CustomFunction("pow", (v1, v2) => (float)System.Math.Pow(v1, v2), true));
      _functions.Add(new CustomFunction("pow", (v1, v2) => (int)System.Math.Pow((int)v1, (int)v2), true));
      _functions.Add(new CustomFunction("lower", s => s.ToLowerInvariant(), true));
      _functions.Add(new CustomFunction("upper", s => s.ToUpperInvariant(), true));
      _functions.Add(new CustomFunction("len", s => s.Length, true));

      _astBuilder = new AstBuilder(_functions);
      _rpnConverter = new RpnConverter(_astBuilder);
      _tokenParser = new TokenParser(_rpnConverter);

      _programCache = new MutableProgram(_functions);
    }

    public CompiledScript Compile(string source, Options options = Options.None)
    {
      return Compile(source.AsMemory(), options);
    }

    public CompiledScript Compile(ReadOnlyMemory<char> source, Options options = Options.None)
    {
      try
      {
        _astBuilder.Configure(options);
        new Lexer.Lexer().Execute(source, _tokenParser);
        var rootNode = _astBuilder.Result;

        byte nextRegisterIdx = 0;
        rootNode.Encode(_programCache, ref nextRegisterIdx, _constantVm);
        if (nextRegisterIdx > 0)
        {
          _programCache.Instructions.Add(new Instruction
          {
            Type = InstructionType.Return,
            reg0 = (byte)(nextRegisterIdx - 1)
          });
        }

        _programCache.OptimizeVariableLoad();

        return new CompiledScript(new Program(_programCache));
      }
      finally
      {
        _astBuilder.Reset();
        _rpnConverter.Reset();
        _tokenParser.Reset();
        _programCache.Reset();
      }
    }

    public void AddCustomFunction(CustomFunction func)
    {
      _functions.Add(func);
    }
  }
}