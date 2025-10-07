using System;
using System.Collections.Generic;
using EpsilonScript.AST;

namespace EpsilonScript.Function
{
  public abstract class CustomFunction
  {
    internal Type[] ParameterTypes { get; }

    protected CustomFunction(string name, bool isConstant, Type[] parameterTypes, Type returnType)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentException("Function name cannot be null or empty", nameof(name));
      }

      Name = name;
      IsConstant = isConstant;
      ParameterTypes = parameterTypes ?? Array.Empty<Type>();
      ReturnType = returnType;
    }

    public VariableId Name { get; }
    public bool IsConstant { get; }
    public Type ReturnType { get; }

    private protected void EnsureParameterCount(List<Node> parameters)
    {
      if ((parameters?.Count ?? 0) != ParameterTypes.Length)
      {
        throw new RuntimeException($"Invalid number of parameters for function: {Name}");
      }
    }

    protected void EnsureReturnType(Type expected)
    {
      if (ReturnType != expected)
      {
        throw new InvalidOperationException("Function return type mismatch");
      }
    }

    internal virtual int ExecuteInt(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return integer");
    }

    internal virtual long ExecuteLong(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return long");
    }

    internal virtual float ExecuteFloat(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return float");
    }

    internal virtual double ExecuteDouble(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return double");
    }

    internal virtual decimal ExecuteDecimal(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return decimal");
    }

    internal virtual string ExecuteString(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return string");
    }

    internal virtual bool ExecuteBool(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return boolean");
    }

    public static CustomFunction Create<TResult>(string name, Func<TResult> func, bool isConstant = false)
    {
      return new CustomFunction<TResult>(name, func, isConstant);
    }

    public static CustomFunction Create<T1, TResult>(string name, Func<T1, TResult> func, bool isConstant = false)
    {
      return new CustomFunction<T1, TResult>(name, func, isConstant);
    }

    public static CustomFunction Create<T1, T2, TResult>(string name, Func<T1, T2, TResult> func,
      bool isConstant = false)
    {
      return new CustomFunction<T1, T2, TResult>(name, func, isConstant);
    }

    public static CustomFunction Create<T1, T2, T3, TResult>(string name, Func<T1, T2, T3, TResult> func,
      bool isConstant = false)
    {
      return new CustomFunction<T1, T2, T3, TResult>(name, func, isConstant);
    }

    public static CustomFunction Create<T1, T2, T3, T4, TResult>(string name,
      Func<T1, T2, T3, T4, TResult> func, bool isConstant = false)
    {
      return new CustomFunction<T1, T2, T3, T4, TResult>(name, func, isConstant);
    }

    public static CustomFunction Create<T1, T2, T3, T4, T5, TResult>(string name,
      Func<T1, T2, T3, T4, T5, TResult> func, bool isConstant = false)
    {
      return new CustomFunction<T1, T2, T3, T4, T5, TResult>(name, func, isConstant);
    }
  }

  public sealed class CustomFunction<TResult> : CustomFunction
  {
    private readonly Func<TResult> _func;

    public CustomFunction(string name, Func<TResult> func, bool isConstant = false)
      : base(name, isConstant, Array.Empty<Type>(), TypeTraits<TResult>.ValueType)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    private TResult Invoke(List<Node> parameters)
    {
      EnsureParameterCount(parameters);
      return _func();
    }

    internal override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    internal override long ExecuteLong(List<Node> parameters)
    {
      EnsureReturnType(Type.Long);
      return TypeTraits<TResult>.ToLong(Invoke(parameters));
    }

    internal override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    internal override double ExecuteDouble(List<Node> parameters)
    {
      EnsureReturnType(Type.Double);
      return TypeTraits<TResult>.ToDouble(Invoke(parameters));
    }

    internal override decimal ExecuteDecimal(List<Node> parameters)
    {
      EnsureReturnType(Type.Decimal);
      return TypeTraits<TResult>.ToDecimal(Invoke(parameters));
    }

    internal override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    internal override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }

  public sealed class CustomFunction<T1, TResult> : CustomFunction
  {
    private static readonly Type[] ParameterCache = { TypeTraits<T1>.ValueType };
    private readonly Func<T1, TResult> _func;

    public CustomFunction(string name, Func<T1, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ValueType)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    private TResult Invoke(List<Node> parameters)
    {
      EnsureParameterCount(parameters);
      return _func(TypeTraits<T1>.Read(parameters[0]));
    }

    internal override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    internal override long ExecuteLong(List<Node> parameters)
    {
      EnsureReturnType(Type.Long);
      return TypeTraits<TResult>.ToLong(Invoke(parameters));
    }

    internal override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    internal override double ExecuteDouble(List<Node> parameters)
    {
      EnsureReturnType(Type.Double);
      return TypeTraits<TResult>.ToDouble(Invoke(parameters));
    }

    internal override decimal ExecuteDecimal(List<Node> parameters)
    {
      EnsureReturnType(Type.Decimal);
      return TypeTraits<TResult>.ToDecimal(Invoke(parameters));
    }

    internal override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    internal override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }

  public sealed class CustomFunction<T1, T2, TResult> : CustomFunction
  {
    private static readonly Type[] ParameterCache =
    {
      TypeTraits<T1>.ValueType,
      TypeTraits<T2>.ValueType
    };

    private readonly Func<T1, T2, TResult> _func;

    public CustomFunction(string name, Func<T1, T2, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ValueType)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    private TResult Invoke(List<Node> parameters)
    {
      EnsureParameterCount(parameters);
      return _func(TypeTraits<T1>.Read(parameters[0]), TypeTraits<T2>.Read(parameters[1]));
    }

    internal override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    internal override long ExecuteLong(List<Node> parameters)
    {
      EnsureReturnType(Type.Long);
      return TypeTraits<TResult>.ToLong(Invoke(parameters));
    }

    internal override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    internal override double ExecuteDouble(List<Node> parameters)
    {
      EnsureReturnType(Type.Double);
      return TypeTraits<TResult>.ToDouble(Invoke(parameters));
    }

    internal override decimal ExecuteDecimal(List<Node> parameters)
    {
      EnsureReturnType(Type.Decimal);
      return TypeTraits<TResult>.ToDecimal(Invoke(parameters));
    }

    internal override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    internal override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }

  public sealed class CustomFunction<T1, T2, T3, TResult> : CustomFunction
  {
    private static readonly Type[] ParameterCache =
    {
      TypeTraits<T1>.ValueType,
      TypeTraits<T2>.ValueType,
      TypeTraits<T3>.ValueType
    };

    private readonly Func<T1, T2, T3, TResult> _func;

    public CustomFunction(string name, Func<T1, T2, T3, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ValueType)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    private TResult Invoke(List<Node> parameters)
    {
      EnsureParameterCount(parameters);
      return _func(TypeTraits<T1>.Read(parameters[0]), TypeTraits<T2>.Read(parameters[1]),
        TypeTraits<T3>.Read(parameters[2]));
    }

    internal override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    internal override long ExecuteLong(List<Node> parameters)
    {
      EnsureReturnType(Type.Long);
      return TypeTraits<TResult>.ToLong(Invoke(parameters));
    }

    internal override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    internal override double ExecuteDouble(List<Node> parameters)
    {
      EnsureReturnType(Type.Double);
      return TypeTraits<TResult>.ToDouble(Invoke(parameters));
    }

    internal override decimal ExecuteDecimal(List<Node> parameters)
    {
      EnsureReturnType(Type.Decimal);
      return TypeTraits<TResult>.ToDecimal(Invoke(parameters));
    }

    internal override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    internal override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }

  public sealed class CustomFunction<T1, T2, T3, T4, TResult> : CustomFunction
  {
    private static readonly Type[] ParameterCache =
    {
      TypeTraits<T1>.ValueType,
      TypeTraits<T2>.ValueType,
      TypeTraits<T3>.ValueType,
      TypeTraits<T4>.ValueType
    };

    private readonly Func<T1, T2, T3, T4, TResult> _func;

    public CustomFunction(string name, Func<T1, T2, T3, T4, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ValueType)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    private TResult Invoke(List<Node> parameters)
    {
      EnsureParameterCount(parameters);
      return _func(
        TypeTraits<T1>.Read(parameters[0]),
        TypeTraits<T2>.Read(parameters[1]),
        TypeTraits<T3>.Read(parameters[2]),
        TypeTraits<T4>.Read(parameters[3]));
    }

    internal override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    internal override long ExecuteLong(List<Node> parameters)
    {
      EnsureReturnType(Type.Long);
      return TypeTraits<TResult>.ToLong(Invoke(parameters));
    }

    internal override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    internal override double ExecuteDouble(List<Node> parameters)
    {
      EnsureReturnType(Type.Double);
      return TypeTraits<TResult>.ToDouble(Invoke(parameters));
    }

    internal override decimal ExecuteDecimal(List<Node> parameters)
    {
      EnsureReturnType(Type.Decimal);
      return TypeTraits<TResult>.ToDecimal(Invoke(parameters));
    }

    internal override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    internal override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }

  public sealed class CustomFunction<T1, T2, T3, T4, T5, TResult> : CustomFunction
  {
    private static readonly Type[] ParameterCache =
    {
      TypeTraits<T1>.ValueType,
      TypeTraits<T2>.ValueType,
      TypeTraits<T3>.ValueType,
      TypeTraits<T4>.ValueType,
      TypeTraits<T5>.ValueType
    };

    private readonly Func<T1, T2, T3, T4, T5, TResult> _func;

    public CustomFunction(string name, Func<T1, T2, T3, T4, T5, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ValueType)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    private TResult Invoke(List<Node> parameters)
    {
      EnsureParameterCount(parameters);
      return _func(
        TypeTraits<T1>.Read(parameters[0]),
        TypeTraits<T2>.Read(parameters[1]),
        TypeTraits<T3>.Read(parameters[2]),
        TypeTraits<T4>.Read(parameters[3]),
        TypeTraits<T5>.Read(parameters[4]));
    }

    internal override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    internal override long ExecuteLong(List<Node> parameters)
    {
      EnsureReturnType(Type.Long);
      return TypeTraits<TResult>.ToLong(Invoke(parameters));
    }

    internal override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    internal override double ExecuteDouble(List<Node> parameters)
    {
      EnsureReturnType(Type.Double);
      return TypeTraits<TResult>.ToDouble(Invoke(parameters));
    }

    internal override decimal ExecuteDecimal(List<Node> parameters)
    {
      EnsureReturnType(Type.Decimal);
      return TypeTraits<TResult>.ToDecimal(Invoke(parameters));
    }

    internal override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    internal override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }
}