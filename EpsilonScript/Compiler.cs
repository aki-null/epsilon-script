using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Parser;

namespace EpsilonScript
{
  public class Compiler
  {
    private readonly Lexer.Lexer _lexer = new Lexer.Lexer();
    private readonly TokenParser _tokenParser = new TokenParser();
    private readonly RPNConverter _rpnConverter = new RPNConverter();

    private readonly Dictionary<string, CustomFunction> _functions = new Dictionary<string, CustomFunction>();

    public Compiler()
    {
      AddCustomFunction(new CustomFunction("sin", (float v) => (float) System.Math.Sin(v), true));
      AddCustomFunction(new CustomFunction("cos", (float v) => (float) System.Math.Cos(v), true));
      AddCustomFunction(new CustomFunction("tan", (float v) => (float) System.Math.Tan(v), true));
      AddCustomFunction(new CustomFunction("asin", (float v) => (float) System.Math.Asin(v), true));
      AddCustomFunction(new CustomFunction("acos", (float v) => (float) System.Math.Acos(v), true));
      AddCustomFunction(new CustomFunction("atan", (float v) => (float) System.Math.Atan(v), true));
      AddCustomFunction(new CustomFunction("sinh", (float v) => (float) System.Math.Sinh(v), true));
      AddCustomFunction(new CustomFunction("cosh", (float v) => (float) System.Math.Cosh(v), true));
      AddCustomFunction(new CustomFunction("tang", (float v) => (float) System.Math.Tanh(v), true));
      AddCustomFunction(new CustomFunction("atan2", (v1, v2) => (float) System.Math.Atan2(v1, v2), true));
      AddCustomFunction(new CustomFunction("sqrt", (float v) => (float) System.Math.Sqrt(v), true));
      AddCustomFunction(new CustomFunction("abs", System.Math.Abs, true));
      AddCustomFunction(new CustomFunction("absf", (float v) => System.Math.Abs(v), true));
      AddCustomFunction(new CustomFunction("floor", v => (float) System.Math.Floor(v), true));
      AddCustomFunction(new CustomFunction("ceil", v => (float) System.Math.Ceiling(v), true));
      AddCustomFunction(new CustomFunction("trunc", v => (float) System.Math.Truncate(v), true));
      AddCustomFunction(new CustomFunction("min", System.Math.Min, true));
      AddCustomFunction(new CustomFunction("minf", (float v1, float v2) => System.Math.Min(v1, v2), true));
      AddCustomFunction(new CustomFunction("max", System.Math.Max, true));
      AddCustomFunction(new CustomFunction("maxf", (float v1, float v2) => System.Math.Max(v1, v2), true));
      AddCustomFunction(new CustomFunction("ifelse", (cond, v1, v2) => cond ? v1 : v2, true));
      AddCustomFunction(new CustomFunction("ifelsef", (bool cond, float v1, float v2) => cond ? v1 : v2, true));
      AddCustomFunction(new CustomFunction("pow", (v1, v2) => (float) System.Math.Pow(v1, v2), true));
    }

    public Node Compile(string source, IDictionary<string, VariableValue> variables = null)
    {
      _rpnConverter.Reset();
      // Tokenization
      var tokens = _lexer.Analyze(source);
      // Parse
      _tokenParser.Parse(tokens);
      // Convert to RPN
      _rpnConverter.Convert(_tokenParser.Elements);
      // Build AST
      return ASTBuilder.Build(_rpnConverter.Rpn, variables, _functions);
    }

    public void AddCustomFunction(CustomFunction func)
    {
      if (_functions.ContainsKey(func.Name))
      {
        throw new RuntimeException($"Function named {func.Name} is already defined");
      }

      _functions[func.Name] = func;
    }
  }
}