using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Parser;

namespace EpsilonScript.Function
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

    // List of all possible function signatures.
    // Anything that doesn't fit into one of these signatures can't be used.
    // This is done to avoid using dynamic function invocation using reflection for performance optimization.
    private readonly Func<int, int> _funcIntInt;
    private readonly Func<float, int> _funcFloatInt;
    private readonly Func<int, float> _funcIntFloat;
    private readonly Func<float, float> _funcFloatFloat;
    private readonly Func<int, int, int> _funcIntIntInt;
    private readonly Func<float, float, float> _funcFloatFloatFloat;
    private readonly Func<bool, int, int, int> _funcBoolIntIntInt;
    private readonly Func<bool, float, float, float> _funcBoolFloatFloatFloat;

    // Cached list of all function parameters
    private static readonly Type[] IntParamType = {EpsilonScript.Type.Integer};
    private static readonly Type[] FloatParamType = {EpsilonScript.Type.Float};
    private static readonly Type[] IntIntParamTYpe = {EpsilonScript.Type.Integer, EpsilonScript.Type.Integer};
    private static readonly Type[] FloatFloatParamType = {EpsilonScript.Type.Float, EpsilonScript.Type.Float};

    private static readonly Type[] BoolIntIntParamType =
      {EpsilonScript.Type.Boolean, EpsilonScript.Type.Integer, EpsilonScript.Type.Integer};

    private static readonly Type[] BoolFloatFloatParamType =
      {EpsilonScript.Type.Boolean, EpsilonScript.Type.Float, EpsilonScript.Type.Float};

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

    private int NumberOfParameters
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

    public Type[] ParameterTypes
    {
      get
      {
        return Type switch
        {
          FunctionType.IntInt => IntParamType,
          FunctionType.FloatInt => FloatParamType,
          FunctionType.IntFloat => IntParamType,
          FunctionType.FloatFloat => FloatParamType,
          FunctionType.IntIntInt => IntIntParamTYpe,
          FunctionType.FloatFloatFloat => FloatFloatParamType,
          FunctionType.BoolIntIntInt => BoolIntIntParamType,
          FunctionType.BoolFloatFloatFloat => BoolFloatFloatParamType,
          _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported function type")
        };
      }
    }

    public bool IsConstant { get; }

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

    private void CheckParameterCount(List<Node> parameters)
    {
      if ((parameters?.Count ?? 0) != NumberOfParameters)
      {
        throw new RuntimeException($"Invalid number of parameters for function: {Name}");
      }
    }

    public int ExecuteInt(List<Node> parameters)
    {
      CheckParameterCount(parameters);
      return Type switch
      {
        FunctionType.IntInt => _funcIntInt(parameters[0].IntegerValue),
        FunctionType.FloatInt => _funcFloatInt(parameters[0].FloatValue),
        FunctionType.IntIntInt => _funcIntIntInt(parameters[0].IntegerValue, parameters[1].IntegerValue),
        FunctionType.BoolIntIntInt => _funcBoolIntIntInt(parameters[0].BooleanValue, parameters[1].IntegerValue,
          parameters[2].IntegerValue),
        _ => throw new ArgumentOutOfRangeException(nameof(Type), Type,
          "Unsupported function type (does not return integer?)")
      };
    }

    public float ExecuteFloat(List<Node> parameters)
    {
      CheckParameterCount(parameters);
      return Type switch
      {
        FunctionType.IntFloat => _funcIntFloat(parameters[0].IntegerValue),
        FunctionType.FloatFloat => _funcFloatFloat(parameters[0].FloatValue),
        FunctionType.FloatFloatFloat => _funcFloatFloatFloat(parameters[0].FloatValue, parameters[1].FloatValue),
        FunctionType.BoolFloatFloatFloat => _funcBoolFloatFloatFloat(parameters[0].BooleanValue,
          parameters[1].FloatValue,
          parameters[2].FloatValue),
        _ => throw new ArgumentOutOfRangeException(nameof(Type), Type,
          "Unsupported function type (does not return float?)")
      };
    }
  }
}