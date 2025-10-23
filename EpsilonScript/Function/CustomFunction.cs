using System;
using System.Collections.Generic;
using EpsilonScript.AST;

namespace EpsilonScript.Function
{
  public abstract partial class CustomFunction
  {
    public Type[] ParameterTypes { get; }

    protected CustomFunction(string name, bool isConstant, Type[] parameterTypes, Type returnType,
      bool hasContext = false)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentException("Function name cannot be null or empty", nameof(name));
      }

      Name = name;
      IsConstant = isConstant;
      ParameterTypes = parameterTypes ?? Array.Empty<Type>();
      ReturnType = returnType;
      HasContext = hasContext;
    }

    public VariableId Name { get; }
    public bool IsConstant { get; }
    public Type ReturnType { get; }
    public bool HasContext { get; }

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

    // Contextual execution methods (with VariableContextAdapter)
    internal virtual int ExecuteInt(VariableContextAdapter context, List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not support contextual execution with integer return");
    }

    internal virtual long ExecuteLong(VariableContextAdapter context, List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not support contextual execution with long return");
    }

    internal virtual float ExecuteFloat(VariableContextAdapter context, List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not support contextual execution with float return");
    }

    internal virtual double ExecuteDouble(VariableContextAdapter context, List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not support contextual execution with double return");
    }

    internal virtual decimal ExecuteDecimal(VariableContextAdapter context, List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not support contextual execution with decimal return");
    }

    internal virtual string ExecuteString(VariableContextAdapter context, List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not support contextual execution with string return");
    }

    internal virtual bool ExecuteBool(VariableContextAdapter context, List<Node> parameters)
    {
      throw new InvalidOperationException("Function does not support contextual execution with boolean return");
    }
  }
}