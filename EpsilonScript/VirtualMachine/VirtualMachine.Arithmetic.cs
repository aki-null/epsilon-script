using EpsilonScript.Bytecode;

namespace EpsilonScript.VirtualMachine
{
  public partial class VirtualMachine
  {
    private unsafe void Add(Instruction instruction, RegisterValue* regPtr)
    {
      var targetRegPtr = regPtr + instruction.reg0;
      var left = regPtr + instruction.reg1;
      var right = regPtr + instruction.reg2;

      switch ((int)right->ValueType | (int)left->ValueType << 1)
      {
        case 0b00: // int, int
          targetRegPtr->ValueType = RegisterValueType.Integer;
          targetRegPtr->IntegerValue = left->IntegerValue + right->IntegerValue;
          break;
        case 0b01: // int, float
          targetRegPtr->ValueType = RegisterValueType.Float;
          targetRegPtr->FloatValue = left->IntegerValue + right->FloatValue;
          break;
        case 0b10: // float, int
          targetRegPtr->ValueType = RegisterValueType.Float;
          targetRegPtr->FloatValue = left->FloatValue + right->IntegerValue;
          break;
        case 0b11: // float, float
          targetRegPtr->ValueType = RegisterValueType.Float;
          targetRegPtr->FloatValue = left->FloatValue + right->FloatValue;
          break;
        default:
          if (left->ValueType == RegisterValueType.String) // string, anything
          {
            var leftStr = left->ResolveString(_program.StringTable, _stringRegisters);
            var rightStr = right->ResolveString(_program.StringTable, _stringRegisters);
            _stringRegisters[instruction.reg0] = leftStr + rightStr;
            targetRegPtr->ValueType = RegisterValueType.StringStack;
          }
          else
          {
            throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
          }

          break;
      }
    }

    private unsafe void Subtract(Instruction instruction, RegisterValue* regPtr)
    {
      var targetRegPtr = regPtr + instruction.reg0;
      var left = regPtr + instruction.reg1;
      var right = regPtr + instruction.reg2;

      switch ((int)right->ValueType | (int)left->ValueType << 1)
      {
        case 0b00: // int, int
          targetRegPtr->ValueType = RegisterValueType.Integer;
          targetRegPtr->IntegerValue = left->IntegerValue - right->IntegerValue;
          break;
        case 0b01: // int, float
          targetRegPtr->ValueType = RegisterValueType.Float;
          targetRegPtr->FloatValue = left->IntegerValue - right->FloatValue;
          break;
        case 0b10: // float, int
          targetRegPtr->ValueType = RegisterValueType.Float;
          targetRegPtr->FloatValue = left->FloatValue - right->IntegerValue;
          break;
        case 0b11: // float, float
          targetRegPtr->ValueType = RegisterValueType.Float;
          targetRegPtr->FloatValue = left->FloatValue - right->FloatValue;
          break;
        default:
          throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
      }
    }

    private unsafe void Multiply(Instruction instruction, RegisterValue* regPtr)
    {
      var targetPtr = regPtr + instruction.reg0;
      var leftPtr = regPtr + instruction.reg1;
      var rightPtr = regPtr + instruction.reg2;

      switch ((int)rightPtr->ValueType | (int)leftPtr->ValueType << 1)
      {
        case 0b00: // int, int
          targetPtr->ValueType = RegisterValueType.Integer;
          targetPtr->IntegerValue = leftPtr->IntegerValue * rightPtr->IntegerValue;
          break;
        case 0b01: // int, float
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = leftPtr->IntegerValue * rightPtr->FloatValue;
          break;
        case 0b10: // float, int
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = leftPtr->FloatValue * rightPtr->IntegerValue;
          break;
        case 0b11: // float, float
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = leftPtr->FloatValue * rightPtr->FloatValue;
          break;
        default:
          throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
      }
    }

    private unsafe void Divide(Instruction instruction, RegisterValue* regPtr)
    {
      var targetPtr = regPtr + instruction.reg0;
      var leftPtr = regPtr + instruction.reg1;
      var rightPtr = regPtr + instruction.reg2;

      switch ((int)rightPtr->ValueType | (int)leftPtr->ValueType << 1)
      {
        case 0b00: // int, int
          targetPtr->ValueType = RegisterValueType.Integer;
          targetPtr->IntegerValue = leftPtr->IntegerValue / rightPtr->IntegerValue;
          break;
        case 0b01: // int, float
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = leftPtr->IntegerValue / rightPtr->FloatValue;
          break;
        case 0b10: // float, int
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = leftPtr->FloatValue / rightPtr->IntegerValue;
          break;
        case 0b11: // float, float
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = leftPtr->FloatValue / rightPtr->FloatValue;
          break;
        default:
          throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
      }
    }

    private unsafe void Modulo(Instruction instruction, RegisterValue* regPtr)
    {
      var targetPtr = regPtr + instruction.reg0;
      var left = regPtr + instruction.reg1;
      var right = regPtr + instruction.reg2;

      switch ((int)right->ValueType | (int)left->ValueType << 1)
      {
        case 0b00: // int, int
          targetPtr->ValueType = RegisterValueType.Integer;
          targetPtr->IntegerValue = left->IntegerValue % right->IntegerValue;
          break;
        case 0b01: // int, float
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = left->IntegerValue % right->FloatValue;
          break;
        case 0b10: // float, int
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = left->FloatValue % right->IntegerValue;
          break;
        case 0b11: // float, float
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = left->FloatValue % right->FloatValue;
          break;
        default:
          throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
      }
    }

    private unsafe void Negative(Instruction instruction, RegisterValue* regPtr)
    {
      var targetPtr = regPtr + instruction.reg0;
      var valuePtr = regPtr + instruction.reg1;

      switch (valuePtr->ValueType)
      {
        case RegisterValueType.Integer:
          targetPtr->ValueType = RegisterValueType.Integer;
          targetPtr->IntegerValue = -valuePtr->IntegerValue;
          break;
        case RegisterValueType.Float:
          targetPtr->ValueType = RegisterValueType.Float;
          targetPtr->FloatValue = -valuePtr->FloatValue;
          break;
        default:
          throw new RuntimeException("Only numeric value can be negative");
      }
    }

    private unsafe void Negate(Instruction instruction, RegisterValue* regPtr)
    {
      var targetPtr = regPtr + instruction.reg0;
      var valuePtr = regPtr + instruction.reg1;

      if (valuePtr->ValueType != RegisterValueType.Boolean)
      {
        throw new RuntimeException("Only boolean value can be negated");
      }

      targetPtr->ValueType = RegisterValueType.Boolean;
      targetPtr->BooleanValue = !valuePtr->BooleanValue;
    }
  }
}