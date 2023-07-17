using System;
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

    private unsafe void LoadVariableValue(IVariableContainer globalVariables, IVariableContainer localVariables,
      Instruction instruction, RegisterValue* regPtr)
    {
      var variable = instruction.IntegerValue > 0
        ? FindVariable(instruction.IntegerValue, globalVariables, localVariables) // Uncached access
        : _variableRegisters[instruction.reg1]; // Cached access
      variable.LoadToRegister(regPtr, _stringRegisters, instruction.reg0);
    }

    private unsafe void SetVariableValue(IVariableContainer globalVariables, IVariableContainer localVariables,
      Instruction instruction, RegisterValue* regPtr)
    {
      var variable = instruction.IntegerValue > 0
        ? FindVariable(instruction.IntegerValue, globalVariables, localVariables) // Uncached access
        : _variableRegisters[instruction.reg1]; // Cached access
      var targetRegPtr = regPtr + instruction.reg0;
      switch (targetRegPtr->ValueType)
      {
        case RegisterValueType.Integer:
          variable.IntegerValue = targetRegPtr->IntegerValue;
          break;
        case RegisterValueType.Float:
          variable.FloatValue = targetRegPtr->FloatValue;
          break;
        case RegisterValueType.Boolean:
          variable.BooleanValue = targetRegPtr->BooleanValue;
          break;
        case RegisterValueType.String:
          variable.StringValue = _program.StringTable[targetRegPtr->IntegerValue];
          break;
        case RegisterValueType.StringStack:
          variable.StringValue = _stringRegisters[instruction.reg0];
          break;
        case RegisterValueType.Variable:
          throw new RuntimeException(
            "Variable stack value type must be resolved first for it to be assigned to an another variable");
        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), targetRegPtr->ValueType,
            "VariableAssign instruction does not support such stack value type");
      }
    }
  }
}