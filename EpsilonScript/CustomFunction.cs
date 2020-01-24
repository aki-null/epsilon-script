using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Parser;

namespace EpsilonScript
{
  public class CustomFunction
  {
    private enum FunctionType
    {
      IntInt,
      FloatInt,
      IntFloat,
      FloatFloat,
      IntIntInt,
      FloatFloatFloat,
      BoolIntIntInt,
      BoolFloatFloatFloat,
    }

    private readonly Func<int, int> _funcIntInt;
    private readonly Func<float, int> _funcFloatInt;
    private readonly Func<int, float> _funcIntFloat;
    private readonly Func<float, float> _funcFloatFloat;
    private readonly Func<int, int, int> _funcIntIntInt;
    private readonly Func<float, float, float> _funcFloatFloatFloat;
    private readonly Func<bool, int, int, int> _funcBoolIntIntInt;
    private readonly Func<bool, float, float, float> _funcBoolFloatFloatFloat;

    private FunctionType Type { get; }

    public string Name { get; }

    public Type ReturnType
    {
      get
      {
        return Type switch
        {
          FunctionType.IntInt => EpsilonScript.Type.Integer,
          FunctionType.FloatInt => EpsilonScript.Type.Integer,
          FunctionType.IntFloat => EpsilonScript.Type.Float,
          FunctionType.FloatFloat => EpsilonScript.Type.Float,
          FunctionType.IntIntInt => EpsilonScript.Type.Integer,
          FunctionType.FloatFloatFloat => EpsilonScript.Type.Float,
          FunctionType.BoolIntIntInt => EpsilonScript.Type.Integer,
          FunctionType.BoolFloatFloatFloat => EpsilonScript.Type.Float,
          _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported function type")
        };
      }
    }

    private int NumberOfArguments
    {
      get
      {
        return Type switch
        {
          FunctionType.IntInt => 1,
          FunctionType.FloatInt => 1,
          FunctionType.IntFloat => 1,
          FunctionType.FloatFloat => 1,
          FunctionType.IntIntInt => 2,
          FunctionType.FloatFloatFloat => 2,
          FunctionType.BoolIntIntInt => 3,
          FunctionType.BoolFloatFloatFloat => 3,
          _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported function type")
        };
      }
    }

    public bool IsConstant { get; private set; }

    public CustomFunction(string name, Func<int, int> func, bool isConstant = false)
    {
      Name = name;
      _funcIntInt = func;
      Type = FunctionType.IntInt;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<float, int> func, bool isConstant = false)
    {
      Name = name;
      _funcFloatInt = func;
      Type = FunctionType.FloatInt;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<int, float> func, bool isConstant = false)
    {
      Name = name;
      _funcIntFloat = func;
      Type = FunctionType.IntFloat;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<float, float> func, bool isConstant = false)
    {
      Name = name;
      _funcFloatFloat = func;
      Type = FunctionType.FloatFloat;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<int, int, int> func, bool isConstant = false)
    {
      Name = name;
      _funcIntIntInt = func;
      Type = FunctionType.IntIntInt;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<float, float, float> func, bool isConstant = false)
    {
      Name = name;
      _funcFloatFloatFloat = func;
      Type = FunctionType.FloatFloatFloat;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<bool, float, float, float> func, bool isConstant = false)
    {
      Name = name;
      _funcBoolFloatFloatFloat = func;
      Type = FunctionType.BoolFloatFloatFloat;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<bool, int, int, int> func, bool isConstant = false)
    {
      Name = name;
      _funcBoolIntIntInt = func;
      Type = FunctionType.BoolIntIntInt;
      IsConstant = isConstant;
    }

    private void CheckArgumentCount(List<Node> arguments)
    {
      if ((arguments?.Count ?? 0) != NumberOfArguments)
      {
        throw new RuntimeException($"Invalid number of arguments for function: {Name}");
      }
    }

    public int ExecuteInt(List<Node> arguments)
    {
      CheckArgumentCount(arguments);
      return Type switch
      {
        FunctionType.IntInt => _funcIntInt(arguments[0].IntegerValue),
        FunctionType.FloatInt => _funcFloatInt(arguments[0].FloatValue),
        FunctionType.IntIntInt => _funcIntIntInt(arguments[0].IntegerValue, arguments[1].IntegerValue),
        FunctionType.BoolIntIntInt => _funcBoolIntIntInt(arguments[0].BooleanValue, arguments[1].IntegerValue,
          arguments[2].IntegerValue),
        _ => throw new ArgumentOutOfRangeException(nameof(Type), Type,
          "Unsupported function type (does not return integer?)")
      };
    }

    public float ExecuteFloat(List<Node> arguments)
    {
      CheckArgumentCount(arguments);
      return Type switch
      {
        FunctionType.IntFloat => _funcIntFloat(arguments[0].IntegerValue),
        FunctionType.FloatFloat => _funcFloatFloat(arguments[0].FloatValue),
        FunctionType.FloatFloatFloat => _funcFloatFloatFloat(arguments[0].FloatValue, arguments[1].FloatValue),
        FunctionType.BoolFloatFloatFloat => _funcBoolFloatFloatFloat(arguments[0].BooleanValue, arguments[1].FloatValue,
          arguments[2].FloatValue),
        _ => throw new ArgumentOutOfRangeException(nameof(Type), Type,
          "Unsupported function type (does not return float?)")
      };
    }
  }
}