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

    private void Function(Instruction instruction)
    {
      // reg0 = store address
      // reg1 = function parameter count
      // reg2 = first function parameter register index
      var functionParameterCount = instruction.reg1;

      // The parameter list is laid on register list in reverse order
      for (var i = 0; i < functionParameterCount; ++i)
      {
        var currentStackValue = _registers[instruction.reg2 + i];
        var funcParam = currentStackValue.ResolveToConcreteValue(_program.StringTable, _stringRegisters);
        _functionParameters[i] = funcParam;
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

      // Find actual function
      var func = funcOverload.Find(_functionParameters.AsSpan(0, functionParameterCount));

      switch (func.ReturnType)
      {
        case Type.Integer:
          _registers[instruction.reg0] = new RegisterValue
          {
            ValueType = RegisterValueType.Integer,
            IntegerValue = func.ExecuteInt(_functionParameters.AsSpan(0, functionParameterCount))
          };
          break;
        case Type.Float:
          _registers[instruction.reg0] = new RegisterValue
          {
            ValueType = RegisterValueType.Float,
            FloatValue = func.ExecuteFloat(_functionParameters.AsSpan(0, functionParameterCount))
          };
          break;
        case Type.Boolean:
          _registers[instruction.reg0] = new RegisterValue
          {
            ValueType = RegisterValueType.Boolean,
            BooleanValue = func.ExecuteBoolean(_functionParameters.AsSpan(0, functionParameterCount))
          };
          break;
        case Type.String:
          _stringRegisters[instruction.reg0] =
            func.ExecuteString(_functionParameters.AsSpan(0, functionParameterCount));
          _registers[instruction.reg0] = new RegisterValue
          {
            ValueType = RegisterValueType.StringStack
          };
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(func.ReturnType), func.ReturnType,
            "Unsupported function return type");
      }
    }
  }
}