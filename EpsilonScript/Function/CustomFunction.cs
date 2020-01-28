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
        switch (Type)
        {
          case FunctionType.IntInt:
          case FunctionType.FloatInt:
            return EpsilonScript.Type.Integer;
          case FunctionType.IntFloat:
          case FunctionType.FloatFloat:
            return EpsilonScript.Type.Float;
          case FunctionType.IntIntInt:
            return EpsilonScript.Type.Integer;
          case FunctionType.FloatFloatFloat:
            return EpsilonScript.Type.Float;
          case FunctionType.BoolIntIntInt:
            return EpsilonScript.Type.Integer;
          case FunctionType.BoolFloatFloatFloat:
            return EpsilonScript.Type.Float;
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported function type");
        }
      }
    }

    private int NumberOfParameters
    {
      get
      {
        switch (Type)
        {
          case FunctionType.IntInt:
          case FunctionType.FloatInt:
          case FunctionType.IntFloat:
          case FunctionType.FloatFloat:
            return 1;
          case FunctionType.IntIntInt:
          case FunctionType.FloatFloatFloat:
            return 2;
          case FunctionType.BoolIntIntInt:
          case FunctionType.BoolFloatFloatFloat:
            return 3;
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported function type");
        }
      }
    }

    public Type[] ParameterTypes
    {
      get
      {
        switch (Type)
        {
          case FunctionType.IntInt:
          case FunctionType.IntFloat:
            return IntParamType;
          case FunctionType.FloatInt:
          case FunctionType.FloatFloat:
            return FloatParamType;
          case FunctionType.IntIntInt:
            return IntIntParamTYpe;
          case FunctionType.FloatFloatFloat:
            return FloatFloatParamType;
          case FunctionType.BoolIntIntInt:
            return BoolIntIntParamType;
          case FunctionType.BoolFloatFloatFloat:
            return BoolFloatFloatParamType;
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported function type");
        }
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
      switch (Type)
      {
        case FunctionType.IntInt:
          return _funcIntInt(parameters[0].IntegerValue);
        case FunctionType.FloatInt:
          return _funcFloatInt(parameters[0].FloatValue);
        case FunctionType.IntIntInt:
          return _funcIntIntInt(parameters[0].IntegerValue, parameters[1].IntegerValue);
        case FunctionType.BoolIntIntInt:
          return _funcBoolIntIntInt(parameters[0].BooleanValue, parameters[1].IntegerValue, parameters[2].IntegerValue);
        default:
          throw new ArgumentOutOfRangeException(nameof(Type), Type,
            "Unsupported function type (does not return integer?)");
      }
    }

    public float ExecuteFloat(List<Node> parameters)
    {
      CheckParameterCount(parameters);
      switch (Type)
      {
        case FunctionType.IntFloat:
          return _funcIntFloat(parameters[0].IntegerValue);
        case FunctionType.FloatFloat:
          return _funcFloatFloat(parameters[0].FloatValue);
        case FunctionType.FloatFloatFloat:
          return _funcFloatFloatFloat(parameters[0].FloatValue, parameters[1].FloatValue);
        case FunctionType.BoolFloatFloatFloat:
          return _funcBoolFloatFloatFloat(parameters[0].BooleanValue, parameters[1].FloatValue,
            parameters[2].FloatValue);
        default:
          throw new ArgumentOutOfRangeException(nameof(Type), Type,
            "Unsupported function type (does not return float?)");
      }
    }
  }
}