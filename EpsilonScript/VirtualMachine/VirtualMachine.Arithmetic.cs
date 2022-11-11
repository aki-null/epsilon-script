using EpsilonScript.Bytecode;

namespace EpsilonScript.VirtualMachine
{
  public partial class VirtualMachine
  {
    private void Add(Instruction instruction)
    {
      var left = _registers[instruction.reg1];
      var right = _registers[instruction.reg2];

      switch ((int)right.ValueType | (int)left.ValueType << 1)
      {
        case 0b00: // int, int
          _registers[instruction.reg0] = new RegisterValue
          {
            ValueType = RegisterValueType.Integer,
            IntegerValue = left.IntegerValue + right.IntegerValue
          };
          break;
        case 0b01: // int, float
          _registers[instruction.reg0] = new RegisterValue
          {
            ValueType = RegisterValueType.Float,
            FloatValue = left.IntegerValue + right.FloatValue
          };
          break;
        case 0b10: // float, int
          _registers[instruction.reg0] = new RegisterValue
          {
            ValueType = RegisterValueType.Float,
            FloatValue = left.FloatValue + right.IntegerValue
          };
          break;
        case 0b11: // float, float
          _registers[instruction.reg0] = new RegisterValue
          {
            ValueType = RegisterValueType.Float,
            FloatValue = left.FloatValue + right.FloatValue
          };
          break;
        default:
          if (left.ValueType == RegisterValueType.String) // string, anything
          {
            var leftStr = left.ResolveString(_program.StringTable, _stringRegisters);
            var rightStr = right.ResolveString(_program.StringTable, _stringRegisters);
            _stringRegisters[instruction.reg0] = leftStr + rightStr;
            _registers[instruction.reg0] = new RegisterValue
            {
              ValueType = RegisterValueType.StringStack
            };
          }
          else
          {
            throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
          }

          break;
      }
    }

    private void Subtract(Instruction instruction)
    {
      var left = _registers[instruction.reg1];
      var right = _registers[instruction.reg2];

      _registers[instruction.reg0] = ((int)right.ValueType | (int)left.ValueType << 1) switch
      {
        0b00 => // int, int
          new RegisterValue
          {
            ValueType = RegisterValueType.Integer, IntegerValue = left.IntegerValue - right.IntegerValue
          },
        0b01 => // int, float
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.IntegerValue - right.FloatValue },
        0b10 => // float, int
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.FloatValue - right.IntegerValue },
        0b11 => // float, float
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.FloatValue - right.FloatValue },
        _ => throw new RuntimeException("An arithmetic operation can only be performed on numeric values")
      };
    }

    private void Multiply(Instruction instruction)
    {
      var left = _registers[instruction.reg1];
      var right = _registers[instruction.reg2];

      _registers[instruction.reg0] = ((int)right.ValueType | (int)left.ValueType << 1) switch
      {
        0b00 => // int, int
          new RegisterValue
          {
            ValueType = RegisterValueType.Integer, IntegerValue = left.IntegerValue * right.IntegerValue
          },
        0b01 => // int, float
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.IntegerValue * right.FloatValue },
        0b10 => // float, int
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.FloatValue * right.IntegerValue },
        0b11 => // float, float
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.FloatValue * right.FloatValue },
        _ => throw new RuntimeException("Only numeric values can be multiplied")
      };
    }

    private void Divide(Instruction instruction)
    {
      var left = _registers[instruction.reg1];
      var right = _registers[instruction.reg2];

      _registers[instruction.reg0] = ((int)right.ValueType | (int)left.ValueType << 1) switch
      {
        0b00 => // int, int
          new RegisterValue
          {
            ValueType = RegisterValueType.Integer, IntegerValue = left.IntegerValue / right.IntegerValue
          },
        0b01 => // int, float
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.IntegerValue / right.FloatValue },
        0b10 => // float, int
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.FloatValue / right.IntegerValue },
        0b11 => // float, float
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.FloatValue / right.FloatValue },
        _ => throw new RuntimeException("Only numeric values can be divided")
      };
    }

    private void Modulo(Instruction instruction)
    {
      var left = _registers[instruction.reg1];
      var right = _registers[instruction.reg2];

      _registers[instruction.reg0] = ((int)right.ValueType | (int)left.ValueType << 1) switch
      {
        0b00 => // int, int
          new RegisterValue
          {
            ValueType = RegisterValueType.Integer, IntegerValue = left.IntegerValue % right.IntegerValue
          },
        0b01 => // int, float
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.IntegerValue % right.FloatValue },
        0b10 => // float, int
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.FloatValue % right.IntegerValue },
        0b11 => // float, float
          new RegisterValue { ValueType = RegisterValueType.Float, FloatValue = left.FloatValue % right.FloatValue },
        _ => throw new RuntimeException("A modulo operation can only be executed on numeric values")
      };
    }

    private void Negative(Instruction instruction)
    {
      var value = _registers[instruction.reg1];

      _registers[instruction.reg0] = value.ValueType switch
      {
        RegisterValueType.Integer => new RegisterValue
        {
          ValueType = RegisterValueType.Integer, IntegerValue = -value.IntegerValue
        },
        RegisterValueType.Float => new RegisterValue
        {
          ValueType = RegisterValueType.Float, FloatValue = -value.FloatValue
        },
        _ => throw new RuntimeException("Only numeric value can be negative")
      };
    }

    private void Negate(Instruction instruction)
    {
      var value = _registers[instruction.reg1];
      if (value.ValueType != RegisterValueType.Boolean)
      {
        throw new RuntimeException("Only boolean value can be negated");
      }

      _registers[instruction.reg0] = new RegisterValue
      {
        ValueType = RegisterValueType.Boolean,
        BooleanValue = !value.BooleanValue
      };
    }
  }
}