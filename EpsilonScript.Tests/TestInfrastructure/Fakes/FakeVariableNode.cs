using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using ValueType = EpsilonScript.AST.ValueType;

namespace EpsilonScript.Tests.TestInfrastructure.Fakes
{
  public class FakeVariableNode : Node
  {
    public FakeVariableNode(VariableValue variable)
    {
      Variable = variable;
      switch (Variable.Type)
      {
        case Type.Integer:
          ValueType = ValueType.Integer;
          IntegerValue = Variable.IntegerValue;
          FloatValue = Variable.FloatValue;
          BooleanValue = Variable.BooleanValue;
          break;
        case Type.Float:
          ValueType = ValueType.Float;
          IntegerValue = Variable.IntegerValue;
          FloatValue = Variable.FloatValue;
          break;
        case Type.Boolean:
          ValueType = ValueType.Boolean;
          IntegerValue = Variable.IntegerValue;
          BooleanValue = Variable.BooleanValue;
          break;
        case Type.String:
          ValueType = ValueType.String;
          StringValue = Variable.StringValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
      }
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN stack");
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      // Update values from variable after execution
      switch (Variable.Type)
      {
        case Type.Integer:
          ValueType = ValueType.Integer;
          IntegerValue = Variable.IntegerValue;
          FloatValue = Variable.FloatValue;
          BooleanValue = Variable.BooleanValue;
          break;
        case Type.Float:
          ValueType = ValueType.Float;
          IntegerValue = Variable.IntegerValue;
          FloatValue = Variable.FloatValue;
          break;
        case Type.Boolean:
          ValueType = ValueType.Boolean;
          IntegerValue = Variable.IntegerValue;
          BooleanValue = Variable.BooleanValue;
          break;
        case Type.String:
          ValueType = ValueType.String;
          StringValue = Variable.StringValue;
          break;
      }
    }
  }
}