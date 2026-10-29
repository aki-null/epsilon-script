using System;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.Function
{
  public abstract partial class CustomFunction
  {
    public Type[] ParameterTypes { get; }

    protected CustomFunction(string name, bool isDeterministic, Type[] parameterTypes, Type returnType,
      bool hasContext = false)
    {
      if (string.IsNullOrEmpty(name))
      {
        throw new ArgumentException("Function name cannot be null or empty", nameof(name));
      }

      Name = name;
      IsDeterministic = isDeterministic;
      ParameterTypes = parameterTypes ?? Array.Empty<Type>();
      ReturnType = returnType;
      IsContextual = hasContext;
    }

    public VariableId Name { get; }
    public bool IsDeterministic { get; }
    public Type ReturnType { get; }
    public bool IsContextual { get; }

    protected void EnsureReturnType(Type expected)
    {
      if (ReturnType != expected)
      {
        throw new InvalidOperationException("Function return type mismatch");
      }
    }

    /// <summary>
    /// Calls the function with arguments read from VM registers and writes the result to a register.
    /// </summary>
    internal virtual void Invoke(RegisterProgram program, int[] arguments, int destination)
    {
      throw new InvalidOperationException($"Function '{Name}' does not return {ReturnTypeName()}");
    }

    internal virtual void Invoke(RegisterProgram program, VariableContextAdapter context, int[] arguments,
      int destination)
    {
      throw new InvalidOperationException(
        $"Function '{Name}' does not support contextual execution with {ReturnTypeName()} return");
    }

    // Only a subclass outside this assembly lacks Invoke; these are the messages its calls have always thrown
    private string ReturnTypeName()
    {
      switch (ReturnType)
      {
        case Type.Integer: return "integer";
        case Type.Long: return "long";
        case Type.Float: return "float";
        case Type.Double: return "double";
        case Type.Decimal: return "decimal";
        case Type.String: return "string";
        case Type.Boolean: return "boolean";
        default:
          throw new ArgumentOutOfRangeException("ValueType", (ExtendedType)ReturnType,
            $"Unsupported return type for function: {Name}");
      }
    }

    public override string ToString()
    {
      if (ParameterTypes.Length == 0)
      {
        return $"{ReturnType.ToDebugString()} {Name}()";
      }

      var paramTypeNames = new string[ParameterTypes.Length];
      for (var i = 0; i < ParameterTypes.Length; i++)
      {
        paramTypeNames[i] = ParameterTypes[i].ToDebugString();
      }

      var parameters = string.Join(", ", paramTypeNames);
      return $"{ReturnType.ToDebugString()} {Name}({parameters})";
    }
  }
}