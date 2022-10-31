using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using EpsilonScript.AST;
using EpsilonScript.Helper;

namespace EpsilonScript.Function
{
  public class CustomFunction
  {
    private enum Signature
    {
      IntInt,
      FloatInt,
      IntFloat,
      FloatFloat,
      IntIntInt,
      FloatFloatFloat,
      BoolIntIntInt,
      BoolFloatFloatFloat,
      StringInt,
      StringBool
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct CustomFunctionUnion
    {
      // List of all possible function signatures.
      // Anything that doesn't fit into one of these signatures can't be used.
      // This is done to avoid using dynamic function invocation using reflection for performance optimization.
      [FieldOffset(0)] public Func<int, int> intInt;
      [FieldOffset(0)] public Func<float, int> floatInt;
      [FieldOffset(0)] public Func<int, float> intFloat;
      [FieldOffset(0)] public Func<float, float> floatFloat;
      [FieldOffset(0)] public Func<int, int, int> intIntInt;
      [FieldOffset(0)] public Func<float, float, float> floatFloatFloat;
      [FieldOffset(0)] public Func<bool, int, int, int> boolIntIntInt;
      [FieldOffset(0)] public Func<bool, float, float, float> boolFloatFloatFloat;
      [FieldOffset(0)] public Func<string, int> stingInt;
      [FieldOffset(0)] public Func<string, bool> stringBool;
    }

    private Signature Type { get; }
    private readonly CustomFunctionUnion _func;
    public uint Name { get; }

    // Cached list of all function parameters
    private static readonly Type[] IntParamType = { EpsilonScript.Type.Integer };
    private static readonly Type[] FloatParamType = { EpsilonScript.Type.Float };
    private static readonly Type[] IntIntParamTYpe = { EpsilonScript.Type.Integer, EpsilonScript.Type.Integer };
    private static readonly Type[] FloatFloatParamType = { EpsilonScript.Type.Float, EpsilonScript.Type.Float };

    private static readonly Type[] BoolIntIntParamType =
      { EpsilonScript.Type.Boolean, EpsilonScript.Type.Integer, EpsilonScript.Type.Integer };

    private static readonly Type[] BoolFloatFloatParamType =
      { EpsilonScript.Type.Boolean, EpsilonScript.Type.Float, EpsilonScript.Type.Float };

    private static readonly Type[] StringParamType =
      { EpsilonScript.Type.String };

    public Type ReturnType
    {
      get
      {
        switch (Type)
        {
          case Signature.IntInt:
          case Signature.FloatInt:
          case Signature.IntIntInt:
          case Signature.BoolIntIntInt:
          case Signature.StringInt:
            return EpsilonScript.Type.Integer;
          case Signature.IntFloat:
          case Signature.FloatFloat:
          case Signature.FloatFloatFloat:
          case Signature.BoolFloatFloatFloat:
            return EpsilonScript.Type.Float;
          case Signature.StringBool:
            return EpsilonScript.Type.Boolean;
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
          case Signature.IntInt:
          case Signature.FloatInt:
          case Signature.IntFloat:
          case Signature.FloatFloat:
          case Signature.StringInt:
          case Signature.StringBool:
            return 1;
          case Signature.IntIntInt:
          case Signature.FloatFloatFloat:
            return 2;
          case Signature.BoolIntIntInt:
          case Signature.BoolFloatFloatFloat:
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
          case Signature.IntInt:
          case Signature.IntFloat:
            return IntParamType;
          case Signature.FloatInt:
          case Signature.FloatFloat:
            return FloatParamType;
          case Signature.IntIntInt:
            return IntIntParamTYpe;
          case Signature.FloatFloatFloat:
            return FloatFloatParamType;
          case Signature.BoolIntIntInt:
            return BoolIntIntParamType;
          case Signature.BoolFloatFloatFloat:
            return BoolFloatFloatParamType;
          case Signature.StringInt:
          case Signature.StringBool:
            return StringParamType;
          default:
            throw new ArgumentOutOfRangeException(nameof(Type), Type, "Unsupported function type");
        }
      }
    }

    public bool IsConstant { get; }

    public CustomFunction(string name, Func<int, int> func, bool isConstant = false)
    {
      Name = name.GetUniqueIdentifier();
      Type = Signature.IntInt;
      _func.intInt = func;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<float, int> func, bool isConstant = false)
    {
      Name = name.GetUniqueIdentifier();
      Type = Signature.FloatInt;
      _func.floatInt = func;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<int, float> func, bool isConstant = false)
    {
      Name = name.GetUniqueIdentifier();
      Type = Signature.IntFloat;
      _func.intFloat = func;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<float, float> func, bool isConstant = false)
    {
      Name = name.GetUniqueIdentifier();
      Type = Signature.FloatFloat;
      _func.floatFloat = func;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<int, int, int> func, bool isConstant = false)
    {
      Name = name.GetUniqueIdentifier();
      Type = Signature.IntIntInt;
      _func.intIntInt = func;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<float, float, float> func, bool isConstant = false)
    {
      Name = name.GetUniqueIdentifier();
      Type = Signature.FloatFloatFloat;
      _func.floatFloatFloat = func;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<bool, float, float, float> func, bool isConstant = false)
    {
      Name = name.GetUniqueIdentifier();
      Type = Signature.BoolFloatFloatFloat;
      _func.boolFloatFloatFloat = func;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<bool, int, int, int> func, bool isConstant = false)
    {
      Name = name.GetUniqueIdentifier();
      Type = Signature.BoolIntIntInt;
      _func.boolIntIntInt = func;
      IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<string, int> func, bool isConstant = false)
    {
	    Name = name.GetUniqueIdentifier();
	    Type = Signature.StringInt;
	    _func.stingInt = func;
	    IsConstant = isConstant;
    }

    public CustomFunction(string name, Func<string, bool> func, bool isConstant = false)
    {
	    Name = name.GetUniqueIdentifier();
	    Type = Signature.StringBool;
	    _func.stringBool = func;
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
        case Signature.StringInt:
          return _func.stingInt(parameters[0].StringValue);
        case Signature.IntInt:
          return _func.intInt(parameters[0].IntegerValue);
        case Signature.FloatInt:
          return _func.floatInt(parameters[0].FloatValue);
        case Signature.IntIntInt:
          return _func.intIntInt(parameters[0].IntegerValue, parameters[1].IntegerValue);
        case Signature.BoolIntIntInt:
          return _func.boolIntIntInt(parameters[0].BooleanValue, parameters[1].IntegerValue,
            parameters[2].IntegerValue);
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
        case Signature.IntFloat:
          return _func.intFloat(parameters[0].IntegerValue);
        case Signature.FloatFloat:
          return _func.floatFloat(parameters[0].FloatValue);
        case Signature.FloatFloatFloat:
          return _func.floatFloatFloat(parameters[0].FloatValue, parameters[1].FloatValue);
        case Signature.BoolFloatFloatFloat:
          return _func.boolFloatFloatFloat(parameters[0].BooleanValue, parameters[1].FloatValue,
            parameters[2].FloatValue);
        default:
          throw new ArgumentOutOfRangeException(nameof(Type), Type,
            "Unsupported function type (does not return float?)");
      }
    }
  }
}