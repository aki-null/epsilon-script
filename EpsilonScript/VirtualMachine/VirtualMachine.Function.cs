using System;
using EpsilonScript.Bytecode;
using EpsilonScript.Helper;

namespace EpsilonScript.VirtualMachine
{
  public partial class VirtualMachine
  {
    // NOTE: Virtual machine supports maximum of the following number of parameters for a function.
    //       This can be raised by changing the constant below.
    private const int MaxFunctionParameters = 16;
    private readonly ConcreteValue[] _functionParameters = new ConcreteValue[MaxFunctionParameters];

    private unsafe void Function(Instruction instruction, RegisterValue* regPtr)
    {
      // reg0 = store address
      // reg1 = function parameter count
      // reg2 = first function parameter register index
      var functionParameterCount = instruction.reg1;

      // The parameter list is laid on register list in reverse order
      for (var i = 0; i < functionParameterCount; ++i)
      {
        var currentStackValue = regPtr + instruction.reg2 + i;
        _functionParameters[i] = currentStackValue->ResolveToConcreteValue(_program.StringTable, _stringRegisters);
        // String reference has been created inside the parameter list. This should be cleared to allow for the garbage
        // collector to clean them, but the cleaning cost is not cheap, so it is left as is on purpose.
        // It is likely to be overwritten when an another function is executed later anyway.
      }

      // Find function overload
      var funcId = instruction.IntegerValue;
      if (!_program.Functions.Query(funcId, out var funcOverload))
      {
        throw new RuntimeException($"Function named {funcId.GetStringFromUniqueIdentifier()} is not defined");
      }

      var paramSpan = _functionParameters.AsSpan(0, functionParameterCount);

      // Find actual function
      var func = funcOverload.Find(paramSpan);

      var targetRegPtr = regPtr + instruction.reg0;
      switch (func.ReturnType)
      {
        case Type.Integer:
          targetRegPtr->ValueType = RegisterValueType.Integer;
          targetRegPtr->IntegerValue = func.ExecuteInt(paramSpan);
          break;
        case Type.Float:
          targetRegPtr->ValueType = RegisterValueType.Float;
          targetRegPtr->FloatValue = func.ExecuteFloat(paramSpan);
          break;
        case Type.Boolean:
          targetRegPtr->ValueType = RegisterValueType.Boolean;
          targetRegPtr->BooleanValue = func.ExecuteBoolean(paramSpan);
          break;
        case Type.String:
          targetRegPtr->ValueType = RegisterValueType.StringStack;
          targetRegPtr->IntegerValue = instruction.reg0;
          _stringRegisters[instruction.reg0] = func.ExecuteString(paramSpan);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(func.ReturnType), func.ReturnType,
            "Unsupported function return type");
      }
    }
  }
}