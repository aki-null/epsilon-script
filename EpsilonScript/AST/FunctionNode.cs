using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Helper;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.AST
{
  internal sealed class FunctionNode : Node
  {
    public override bool IsPrecomputable => _functionOverload.IsDeterministic && AreParametersPrecomputable;

    private CustomFunctionOverload _functionOverload;
    // The argument tuple's list, or _single for a lone argument; both are pooled with their nodes
    private List<Node> _parameters;
    private readonly List<Node> _single = new List<Node>(1);
    private IVariableContainer _variables;

    internal override int Lower(RegisterCompiler compiler) =>
      compiler.LowerCall(_parameters, _functionOverload, _variables, Location);

    private bool AreParametersPrecomputable
    {
      get
      {
        var parameters = _parameters;
        for (var i = 0; i < parameters.Count; ++i)
        {
          if (!parameters[i].IsPrecomputable)
          {
            return false;
          }
        }

        return true;
      }
    }

    internal override void Reset()
    {
      base.Reset();
      _functionOverload = null;
      _parameters = null;
      _single.Clear();
      _variables = null;
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      VariableId functionName = UniqueIdentifierExtension.GetUniqueIdentifier(element.Token.Text.FastSpan());

      // Store compile-time variables for contextual functions
      _variables = variables;

      if (!context.Functions.TryGetValue(functionName, out _functionOverload))
      {
        throw new ParserException(element.Token, $"Undefined function: {functionName}");
      }

      if (!rpnStack.TryPop(out var childNode))
      {
        throw new ParserException(element.Token,
          $"Cannot find parameters for calling function: {functionName}");
      }

      switch (childNode.ValueType)
      {
        case ExtendedType.Tuple:
          _parameters = childNode.TupleValue;
          break;
        case ExtendedType.Null:
          _parameters = _single;
          break;
        default:
          _single.Add(childNode);
          _parameters = _single;
          break;
      }

      // Validate parameter count once at build time (PackedParameterTypes supports max 7)
      if (_parameters.Count > 7)
      {
        throw new ParserException(element.Token,
          $"Function {functionName} has too many parameters (max 7): {_parameters.Count}");
      }
    }

    public override Node Optimize()
    {
      // Optimize child parameter nodes first
      for (var i = 0; i < _parameters.Count; ++i)
      {
        _parameters[i] = _parameters[i].Optimize();
      }

      // If function is constant and all parameters are constant, evaluate at compile time
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      return this;
    }

    public override void Validate()
    {
      // First, recursively validate all parameter nodes
      if (_parameters != null)
      {
        for (var i = 0; i < _parameters.Count; ++i)
        {
          _parameters[i]?.Validate();
        }
      }

      // Then validate this function's signature if possible
      ValidateSignature();
    }

    private void ValidateSignature()
    {
      // Build packed types
      var packedTypes = new PackedParameterTypes();
      for (var i = 0; i < _parameters.Count; ++i)
      {
        packedTypes.AddType(_parameters[i].ValueType);
      }

      // Returns a function (exact or candidate) if match possible, null if validation fails
      var function = _functionOverload.Find(packedTypes);

      if (function != null)
      {
        // Match found (an exact match or the first candidate for Undefined parameters): its return type is the
        // static type enclosing nodes validate against
        ValueType = (ExtendedType)function.ReturnType;
      }
      else
      {
        // No match possible - validation failed
        throw new ParserException(Location, _functionOverload.CreateMismatchMessage(packedTypes));
      }
    }
  }
}