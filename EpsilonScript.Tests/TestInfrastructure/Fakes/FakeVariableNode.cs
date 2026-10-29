using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.AST.Assignment;
using EpsilonScript.Intermediate;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript.Tests.TestInfrastructure.Fakes
{
  /// <summary>
  /// Variable node bound to a single value, without building from source. The VM reads and assigns the value.
  /// </summary>
  internal class FakeVariableNode : Node
  {
    private static readonly VariableId Name = "fake";

    private readonly VariableValue _variable;
    private VariableSlot _slot;

    private VariableValue Variable => _variable;

    public FakeVariableNode(VariableValue variable)
    {
      _variable = variable;
      _slot = new VariableSlot(Name, new DictionaryVariableContainer { [Name] = variable }, Location);
      switch (Variable.Type)
      {
        case Type.Integer:
          IntegerValue = Variable.IntegerValue;
          break;
        case Type.Long:
          LongValue = Variable.LongValue;
          break;
        case Type.Float:
          FloatValue = Variable.FloatValue;
          break;
        case Type.Double:
          DoubleValue = Variable.DoubleValue;
          break;
        case Type.Decimal:
          DecimalValue = Variable.DecimalValue;
          break;
        case Type.Boolean:
          BooleanValue = Variable.BooleanValue;
          break;
        case Type.String:
          StringValue = Variable.StringValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
      }
    }

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      throw new NotImplementedException("Fake nodes cannot be built from RPN stack");
    }

    public override bool IsPrecomputable => false;

    internal override int Lower(RegisterCompiler compiler) => compiler.LowerVariable(ref _slot);

    internal override int LowerAssignmentTarget(RegisterCompiler compiler, AssignmentOperationNode assignment,
      Node value) => compiler.LowerAssignment(assignment, ref _slot, value);
  }
}