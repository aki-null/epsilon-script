using System;
using System.Runtime.CompilerServices;
using EpsilonScript.Helper;
using EpsilonScript.Bytecode;

namespace EpsilonScript.VirtualMachine
{
  public partial class VirtualMachine
  {
    private static VariableValue FindVariable(int variableName, IVariableContainer globalVariables,
      IVariableContainer localVariables)
    {
      if (localVariables != null && localVariables.TryGetValue(variableName, out var variable))
      {
        return variable;
      }

      if (globalVariables != null && globalVariables.TryGetValue(variableName, out variable))
      {
        return variable;
      }

      throw new RuntimeException($"Undefined variable: {variableName.GetStringFromUniqueIdentifier()}");
    }

    /// <summary>
    /// Prefetch is optimal when accessing variables multiple times in a single program.
    /// This is because finding a variable usually involves a dictionary lookup.
    /// </summary>
    private void PrefetchVariable(IVariableContainer globalVariables, IVariableContainer localVariables,
      Instruction instruction)
    {
      var variable = FindVariable(instruction.IntegerValue, globalVariables, localVariables);
      _variableRegisters[instruction.reg0] = variable;
    }

    private void LoadVariableValue(IVariableContainer globalVariables, IVariableContainer localVariables,
      Instruction instruction)
    {
      var variable = instruction.IntegerValue > 0
        ? FindVariable(instruction.IntegerValue, globalVariables, localVariables) // Cached access
        : _variableRegisters[instruction.reg1]; // Uncached access
      variable.LoadToRegister(_registers, _stringRegisters, instruction.reg0);
    }

    private void SetVariableValue(IVariableContainer globalVariables, IVariableContainer localVariables,
      Instruction instruction)
    {
      var variable = instruction.IntegerValue > 0
        ? FindVariable(instruction.IntegerValue, globalVariables, localVariables) // Cached access
        : _variableRegisters[instruction.reg1]; // Uncached access
      var registerValue = _registers[instruction.reg0];
      switch (registerValue.ValueType)
      {
        case RegisterValueType.Integer:
          variable.IntegerValue = registerValue.IntegerValue;
          break;
        case RegisterValueType.Float:
          variable.FloatValue = registerValue.FloatValue;
          break;
        case RegisterValueType.Boolean:
          variable.BooleanValue = registerValue.BooleanValue;
          break;
        case RegisterValueType.String:
          variable.StringValue = _program.StringTable[registerValue.IntegerValue];
          break;
        case RegisterValueType.StringStack:
          variable.StringValue = _stringRegisters[instruction.reg0];
          break;
        case RegisterValueType.Variable:
          throw new RuntimeException(
            "Variable stack value type must be resolved first for it to be assigned to an another variable");
        default:
          throw new ArgumentOutOfRangeException(nameof(registerValue.ValueType), registerValue.ValueType,
            "VariableAssign instruction does not support such stack value type");
      }
    }
  }
}