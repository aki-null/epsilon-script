using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Helper;
using ScriptType = EpsilonScript.Type;

namespace EpsilonScript.Function
{
  public abstract class CustomFunction
  {
    private readonly ScriptType[] _parameterTypes;

    protected CustomFunction(string name, bool isConstant, ScriptType[] parameterTypes, ScriptType returnType)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentException("Function name cannot be null or empty", nameof(name));
      }

      Name = name.GetUniqueIdentifier();
      IsConstant = isConstant;
      _parameterTypes = parameterTypes ?? Array.Empty<ScriptType>();
      ReturnType = returnType;
    }

    public uint Name { get; }
    public bool IsConstant { get; }
    public ScriptType ReturnType { get; }
    public ScriptType[] ParameterTypes => _parameterTypes;

    protected void EnsureParameterCount(List<Node> parameters)
    {
      if ((parameters?.Count ?? 0) != _parameterTypes.Length)
      {
        throw new RuntimeException($"Invalid number of parameters for function: {Name}");
      }
    }

    protected void EnsureReturnType(ScriptType expected)
    {
      if (ReturnType != expected)
      {
        throw new InvalidOperationException("Function return type mismatch");
      }
    }

    public virtual int ExecuteInt(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return integer");
    }

    public virtual float ExecuteFloat(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return float");
    }

    public virtual string ExecuteString(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return string");
    }

    public virtual bool ExecuteBool(List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not return boolean");
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

  public sealed class CustomFunction<T1, TResult> : CustomFunction
  {
    private static readonly ScriptType[] ParameterCache = { TypeTraits<T1>.ScriptType };
    private readonly Func<T1, TResult> _func;

    public CustomFunction(string name, Func<T1, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ScriptType)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    private TResult Invoke(List<Node> parameters)
    {
      EnsureParameterCount(parameters);
      return _func(TypeTraits<T1>.Read(parameters[0]));
    }

    public override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    public override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    public override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    public override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }

  public sealed class CustomFunction<T1, T2, TResult> : CustomFunction
  {
    private static readonly ScriptType[] ParameterCache =
    {
      TypeTraits<T1>.ScriptType,
      TypeTraits<T2>.ScriptType
    };

    private readonly Func<T1, T2, TResult> _func;

    public CustomFunction(string name, Func<T1, T2, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ScriptType)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    private TResult Invoke(List<Node> parameters)
    {
      EnsureParameterCount(parameters);
      return _func(TypeTraits<T1>.Read(parameters[0]), TypeTraits<T2>.Read(parameters[1]));
    }

    public override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    public override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    public override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    public override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }

  public sealed class CustomFunction<T1, T2, T3, TResult> : CustomFunction
  {
    private static readonly ScriptType[] ParameterCache =
    {
      TypeTraits<T1>.ScriptType,
      TypeTraits<T2>.ScriptType,
      TypeTraits<T3>.ScriptType
    };

    private readonly Func<T1, T2, T3, TResult> _func;

    public CustomFunction(string name, Func<T1, T2, T3, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ScriptType)
    {
      _func = func ?? throw new ArgumentNullException(nameof(func));
    }

    private TResult Invoke(List<Node> parameters)
    {
      EnsureParameterCount(parameters);
      return _func(TypeTraits<T1>.Read(parameters[0]), TypeTraits<T2>.Read(parameters[1]),
        TypeTraits<T3>.Read(parameters[2]));
    }

    public override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    public override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    public override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    public override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }

  public sealed class CustomFunction<T1, T2, T3, T4, TResult> : CustomFunction
  {
    private static readonly ScriptType[] ParameterCache =
    {
      TypeTraits<T1>.ScriptType,
      TypeTraits<T2>.ScriptType,
      TypeTraits<T3>.ScriptType,
      TypeTraits<T4>.ScriptType
    };

    private readonly Func<T1, T2, T3, T4, TResult> _func;

    public CustomFunction(string name, Func<T1, T2, T3, T4, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ScriptType)
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

    public override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    public override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    public override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    public override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }

  public sealed class CustomFunction<T1, T2, T3, T4, T5, TResult> : CustomFunction
  {
    private static readonly ScriptType[] ParameterCache =
    {
      TypeTraits<T1>.ScriptType,
      TypeTraits<T2>.ScriptType,
      TypeTraits<T3>.ScriptType,
      TypeTraits<T4>.ScriptType,
      TypeTraits<T5>.ScriptType
    };

    private readonly Func<T1, T2, T3, T4, T5, TResult> _func;

    public CustomFunction(string name, Func<T1, T2, T3, T4, T5, TResult> func, bool isConstant = false)
      : base(name, isConstant, ParameterCache, TypeTraits<TResult>.ScriptType)
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

    public override int ExecuteInt(List<Node> parameters)
    {
      EnsureReturnType(Type.Integer);
      return TypeTraits<TResult>.ToInt(Invoke(parameters));
    }

    public override float ExecuteFloat(List<Node> parameters)
    {
      EnsureReturnType(Type.Float);
      return TypeTraits<TResult>.ToFloat(Invoke(parameters));
    }

    public override string ExecuteString(List<Node> parameters)
    {
      EnsureReturnType(Type.String);
      return TypeTraits<TResult>.ToStringValue(Invoke(parameters));
    }

    public override bool ExecuteBool(List<Node> parameters)
    {
      EnsureReturnType(Type.Boolean);
      return TypeTraits<TResult>.ToBool(Invoke(parameters));
    }
  }
}