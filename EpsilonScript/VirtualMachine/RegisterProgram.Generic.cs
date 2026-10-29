using System;
using System.Runtime.CompilerServices;

namespace EpsilonScript.VirtualMachine
{
  /// <remarks>
  /// Full semantics for every instruction, used when the inline fast paths do not apply: operand types other than
  /// the configured integer and float types, strings, booleans, and every error. Each method is a port of the AST
  /// node it replaces, with node value getters replaced by the register Read methods, which convert identically.
  /// Error types, messages and locations match the nodes exactly.
  /// </remarks>
  internal sealed partial class RegisterProgram
  {
    private const string BooleanOperationTypeError = "Boolean operation can only be performed on boolean values";
    private const string NoAllocConcatenationError =
      "String concatenation is not allowed in NoAlloc mode (causes runtime heap allocation)";
    private const string NumericArithmeticError = "An arithmetic operation can only be performed on numeric values";

    private RuntimeException Error(ref Instruction instruction, string message) =>
      new RuntimeException(message, _locations[instruction.Site]);

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Slow(ref Instruction instruction)
    {
      switch (instruction.Op)
      {
        case OpCode.Add:
        case OpCode.Subtract:
        case OpCode.Multiply:
        case OpCode.Divide:
        case OpCode.Modulo:
          Arithmetic(ref instruction);
          break;
        case OpCode.MultiplyAdd:
        case OpCode.AddMultiply:
        case OpCode.MultiplySubtract:
        case OpCode.SubtractMultiply:
          MultiplyAdd(ref instruction);
          break;
        case OpCode.Negate:
        case OpCode.Positive:
          Sign(ref instruction);
          break;
        case OpCode.Not:
          if (_registers[instruction.A].Type != ExtendedType.Boolean)
          {
            throw Error(ref instruction, "Cannot negate a non-boolean value");
          }

          WriteBoolean(instruction.Destination, !ReadBoolean(instruction.A));
          break;
        case OpCode.Equal:
        case OpCode.NotEqual:
        case OpCode.LessThan:
        case OpCode.LessThanOrEqual:
        case OpCode.GreaterThan:
        case OpCode.GreaterThanOrEqual:
          Compare(ref instruction);
          break;
        case OpCode.AndLeft:
        case OpCode.OrLeft:
        case OpCode.BooleanRight:
          // Both sides are checked as they are evaluated; the fast paths handle boolean operands
          throw Error(ref instruction, BooleanOperationTypeError);
        case OpCode.AssignDirect:
        case OpCode.AssignAdd:
        case OpCode.AssignSubtract:
        case OpCode.AssignMultiply:
        case OpCode.AssignDivide:
        case OpCode.AssignModulo:
          Assign(ref instruction);
          break;
        case OpCode.AssignNonVariable:
          throw Error(ref instruction, "A left hand side of an assignment operator must be a variable");
        default:
          throw new InvalidOperationException($"Unhandled opcode {instruction.Op}");
      }
    }

    private ExtendedType PromoteArithmetic(ExtendedType left, ExtendedType right)
    {
      if (left == ExtendedType.String || right == ExtendedType.String)
        return ExtendedType.String;

      if (left == _floatType || right == _floatType)
        return _floatType;

      return _integerType;
    }

    private static string OperatorName(OpCode op)
    {
      switch (op)
      {
        case OpCode.Add: return "addition";
        case OpCode.Subtract: return "subtraction";
        case OpCode.Multiply: return "multiplication";
        case OpCode.Divide: return "division";
        default: return "modulo";
      }
    }

    // Binary arithmetic in the promoted type: string, then the configured float type, then the configured
    // integer type
    private void Arithmetic(ref Instruction instruction)
    {
      var op = instruction.Op;
      int a = instruction.A, b = instruction.B, destination = instruction.Destination;
      var leftType = _registers[a].Type;
      var rightType = _registers[b].Type;
      var type = PromoteArithmetic(leftType, rightType);
      // The result keeps its promoted type when the operation throws, as the tree interpreter's node did
      _registers[destination].Type = type;

      if ((!leftType.IsNumber() || !rightType.IsNumber()) && leftType != ExtendedType.String)
      {
        if (leftType == ExtendedType.Boolean || rightType == ExtendedType.Boolean)
        {
          throw Error(ref instruction,
            $"Boolean values cannot be used in arithmetic operations ({OperatorName(op)})");
        }

        throw Error(ref instruction, NumericArithmeticError);
      }

      if (_noAlloc && type == ExtendedType.String)
      {
        throw Error(ref instruction, NoAllocConcatenationError);
      }

      switch (type)
      {
        case ExtendedType.Integer:
        {
          var left = ReadInteger(a);
          var right = ReadInteger(b);
          switch (op)
          {
            case OpCode.Add: WriteInteger(destination, left + right); break;
            case OpCode.Subtract: WriteInteger(destination, left - right); break;
            case OpCode.Multiply: WriteInteger(destination, left * right); break;
            case OpCode.Divide:
              if (right == 0) throw new DivideByZeroException("Division by zero");
              WriteInteger(destination, left / right);
              break;
            default:
              if (right == 0) throw new DivideByZeroException("Modulo by zero");
              WriteInteger(destination, left % right);
              break;
          }

          break;
        }
        case ExtendedType.Long:
        {
          var left = ReadLong(a);
          var right = ReadLong(b);
          switch (op)
          {
            case OpCode.Add: WriteLong(destination, left + right); break;
            case OpCode.Subtract: WriteLong(destination, left - right); break;
            case OpCode.Multiply: WriteLong(destination, left * right); break;
            case OpCode.Divide:
              if (right == 0) throw new DivideByZeroException("Division by zero");
              WriteLong(destination, left / right);
              break;
            default:
              if (right == 0) throw new DivideByZeroException("Modulo by zero");
              WriteLong(destination, left % right);
              break;
          }

          break;
        }
        case ExtendedType.Float:
        {
          var left = ReadFloat(a);
          var right = ReadFloat(b);
          switch (op)
          {
            case OpCode.Add: WriteFloat(destination, left + right); break;
            case OpCode.Subtract: WriteFloat(destination, left - right); break;
            case OpCode.Multiply: WriteFloat(destination, left * right); break;
            case OpCode.Divide:
              if (right == 0.0f) throw new DivideByZeroException("Division by zero");
              WriteFloat(destination, left / right);
              break;
            default:
              if (right == 0.0f) throw new DivideByZeroException("Modulo by zero");
              WriteFloat(destination, left % right);
              break;
          }

          break;
        }
        case ExtendedType.Double:
        {
          var left = ReadDouble(a);
          var right = ReadDouble(b);
          switch (op)
          {
            case OpCode.Add: WriteDouble(destination, left + right); break;
            case OpCode.Subtract: WriteDouble(destination, left - right); break;
            case OpCode.Multiply: WriteDouble(destination, left * right); break;
            case OpCode.Divide:
              if (right == 0.0) throw new DivideByZeroException("Division by zero");
              WriteDouble(destination, left / right);
              break;
            default:
              if (right == 0.0) throw new DivideByZeroException("Modulo by zero");
              WriteDouble(destination, left % right);
              break;
          }

          break;
        }
        case ExtendedType.Decimal:
        {
          var left = ReadDecimal(a);
          var right = ReadDecimal(b);
          switch (op)
          {
            case OpCode.Add: WriteDecimal(destination, left + right); break;
            case OpCode.Subtract: WriteDecimal(destination, left - right); break;
            case OpCode.Multiply: WriteDecimal(destination, left * right); break;
            case OpCode.Divide:
              if (right == 0m) throw new DivideByZeroException("Division by zero");
              WriteDecimal(destination, left / right);
              break;
            default:
              if (right == 0m) throw new DivideByZeroException("Modulo by zero");
              WriteDecimal(destination, left % right);
              break;
          }

          break;
        }
        case ExtendedType.String:
          if (op != OpCode.Add)
          {
            throw Error(ref instruction,
              $"String operations only support concatenation (+), not {OperatorName(op)}");
          }

          Concatenate(ref instruction);
          break;
        default:
          throw new ArgumentOutOfRangeException("targetType", type, "Unsupported value type");
      }
    }

    // String concatenation; the left operand is a string. NoAlloc string results were rejected by the caller.
    private void Concatenate(ref Instruction instruction)
    {
      if (!TryConcatenate(ref instruction))
      {
        throw new ArgumentOutOfRangeException("ValueType", _registers[instruction.B].Type,
          "Unsupported value type for string concatenation");
      }
    }

    // (a * b) ± c and c ± (a * b): the product type promotes first, then the addend's
    private void MultiplyAdd(ref Instruction instruction)
    {
      var op = instruction.Op;
      int m1 = instruction.A, m2 = instruction.B, addend = instruction.C, destination = instruction.Destination;
      var location = _locations[instruction.Site];
      var multiplyLocation = _locations[instruction.Site + 1];
      var m1Type = _registers[m1].Type;
      var m2Type = _registers[m2].Type;
      var addendType = _registers[addend].Type;

      if (m1Type == ExtendedType.Boolean || m2Type == ExtendedType.Boolean)
      {
        throw new RuntimeException("Boolean values cannot be used in arithmetic operations", multiplyLocation);
      }

      if (addendType == ExtendedType.Boolean)
      {
        throw new RuntimeException("Boolean values cannot be used in arithmetic operations", location);
      }

      if (!m1Type.IsNumber() || !m2Type.IsNumber())
      {
        throw new RuntimeException("Multiply operation can only be performed on numeric values",
          multiplyLocation);
      }

      if (!addendType.IsNumber() && addendType != ExtendedType.String)
      {
        throw new RuntimeException(NumericArithmeticError, location);
      }

      if (addendType == ExtendedType.String && op != OpCode.AddMultiply)
      {
        throw new RuntimeException(
          op == OpCode.MultiplyAdd
            ? NumericArithmeticError
            : "String operations only support concatenation (+), not subtraction", location);
      }

      var multiplyType = PromoteArithmetic(m1Type, m2Type);
      var type = PromoteArithmetic(multiplyType, addendType);
      _registers[destination].Type = type;
      switch (type)
      {
        case ExtendedType.Integer:
        {
          switch (op)
          {
            case OpCode.MultiplySubtract: WriteInteger(destination, ReadInteger(m1) * ReadInteger(m2) - ReadInteger(addend)); break;
            case OpCode.SubtractMultiply: WriteInteger(destination, ReadInteger(addend) - ReadInteger(m1) * ReadInteger(m2)); break;
            default: WriteInteger(destination, ReadInteger(m1) * ReadInteger(m2) + ReadInteger(addend)); break;
          }

          break;
        }
        case ExtendedType.Long:
        {
          switch (op)
          {
            case OpCode.MultiplySubtract: WriteLong(destination, ReadLong(m1) * ReadLong(m2) - ReadLong(addend)); break;
            case OpCode.SubtractMultiply: WriteLong(destination, ReadLong(addend) - ReadLong(m1) * ReadLong(m2)); break;
            default: WriteLong(destination, ReadLong(m1) * ReadLong(m2) + ReadLong(addend)); break;
          }

          break;
        }
        case ExtendedType.Float:
        {
          switch (op)
          {
            case OpCode.MultiplySubtract: WriteFloat(destination, ReadFloat(m1) * ReadFloat(m2) - ReadFloat(addend)); break;
            case OpCode.SubtractMultiply: WriteFloat(destination, ReadFloat(addend) - ReadFloat(m1) * ReadFloat(m2)); break;
            default: WriteFloat(destination, ReadFloat(m1) * ReadFloat(m2) + ReadFloat(addend)); break;
          }

          break;
        }
        case ExtendedType.Double:
        {
          switch (op)
          {
            case OpCode.MultiplySubtract: WriteDouble(destination, ReadDouble(m1) * ReadDouble(m2) - ReadDouble(addend)); break;
            case OpCode.SubtractMultiply: WriteDouble(destination, ReadDouble(addend) - ReadDouble(m1) * ReadDouble(m2)); break;
            default: WriteDouble(destination, ReadDouble(m1) * ReadDouble(m2) + ReadDouble(addend)); break;
          }

          break;
        }
        case ExtendedType.Decimal:
        {
          switch (op)
          {
            case OpCode.MultiplySubtract: WriteDecimal(destination, ReadDecimal(m1) * ReadDecimal(m2) - ReadDecimal(addend)); break;
            case OpCode.SubtractMultiply: WriteDecimal(destination, ReadDecimal(addend) - ReadDecimal(m1) * ReadDecimal(m2)); break;
            default: WriteDecimal(destination, ReadDecimal(m1) * ReadDecimal(m2) + ReadDecimal(addend)); break;
          }

          break;
        }
        case ExtendedType.String:
          ConcatenateProduct(ref instruction, multiplyType);
          break;
        default:
          throw new ArgumentOutOfRangeException("ValueType", type, "Unsupported value type");
      }
    }

    // String addend: string + (numeric * numeric)
    private void ConcatenateProduct(ref Instruction instruction, ExtendedType multiplyType)
    {
      if (_noAlloc)
      {
        throw new RuntimeException(NoAllocConcatenationError, _locations[instruction.Site]);
      }

      int m1 = instruction.A, m2 = instruction.B;
      var context = _context;
      var sb = context.StringBuilder;
      sb.Clear();
      sb.Append(_strings[instruction.C]);
      switch (multiplyType)
      {
        case ExtendedType.Integer:
          sb.Append(ReadInteger(m1) * ReadInteger(m2));
          break;
        case ExtendedType.Long:
          sb.Append(ReadLong(m1) * ReadLong(m2));
          break;
        case ExtendedType.Float:
          sb.AppendFloatInvariant(ReadFloat(m1) * ReadFloat(m2), context);
          break;
        case ExtendedType.Double:
          sb.AppendDoubleInvariant(ReadDouble(m1) * ReadDouble(m2), context);
          break;
        case ExtendedType.Decimal:
          sb.AppendDecimalInvariant(ReadDecimal(m1) * ReadDecimal(m2), context);
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(multiplyType), multiplyType,
            "Unsupported value type for string concatenation");
      }

      WriteString(instruction.Destination, sb.ToString());
    }

    // Unary minus and plus; the result keeps the operand's type
    private void Sign(ref Instruction instruction)
    {
      int a = instruction.A, destination = instruction.Destination;
      var type = _registers[a].Type;
      if (!type.IsNumber())
      {
        throw Error(ref instruction, "Sign of a non-numeric value cannot be changed");
      }

      var negate = instruction.Op == OpCode.Negate;
      switch (type)
      {
        case ExtendedType.Integer:
          WriteInteger(destination, negate ? -ReadInteger(a) : ReadInteger(a));
          break;
        case ExtendedType.Long:
          WriteLong(destination, negate ? -ReadLong(a) : ReadLong(a));
          break;
        case ExtendedType.Float:
          WriteFloat(destination, negate ? -ReadFloat(a) : ReadFloat(a));
          break;
        case ExtendedType.Double:
          WriteDouble(destination, negate ? -ReadDouble(a) : ReadDouble(a));
          break;
        case ExtendedType.Decimal:
          WriteDecimal(destination, negate ? -ReadDecimal(a) : ReadDecimal(a));
          break;
        default:
          throw new ArgumentOutOfRangeException("ValueType", type, "Unsupported value type for sign change");
      }
    }

    // Comparisons: equality also accepts two values of the same non-numeric type; ordering requires numbers
    private void Compare(ref Instruction instruction)
    {
      var op = instruction.Op;
      int a = instruction.A, b = instruction.B;
      var leftType = _registers[a].Type;
      var rightType = _registers[b].Type;
      var equality = op == OpCode.Equal || op == OpCode.NotEqual;

      if (equality)
      {
        if (leftType == ExtendedType.Tuple || rightType == ExtendedType.Tuple)
        {
          throw Error(ref instruction,
            $"Cannot perform comparison on tuple types (left: {leftType}, right: {rightType})");
        }

        if (leftType != rightType)
        {
          var leftIsNumeric = leftType.IsNumber();
          var rightIsNumeric = rightType.IsNumber();
          if (!leftIsNumeric || !rightIsNumeric)
          {
            if (leftIsNumeric || rightIsNumeric)
            {
              throw Error(ref instruction,
                $"Cannot compare incompatible types: {leftType} and {rightType} (numeric types can only be compared with other numeric types)");
            }

            throw Error(ref instruction, $"Cannot compare incompatible types: {leftType} and {rightType}");
          }
        }
      }
      else if (!leftType.IsNumber() || !rightType.IsNumber())
      {
        throw Error(ref instruction,
          $"Cannot perform arithmetic comparison on non-numeric types (left: {leftType}, right: {rightType})");
      }

      ExtendedType type;
      if (leftType == ExtendedType.String || rightType == ExtendedType.String)
        type = ExtendedType.String;
      else if (leftType == ExtendedType.Boolean || rightType == ExtendedType.Boolean)
        type = ExtendedType.Boolean;
      else if (leftType == _floatType || rightType == _floatType)
        type = _floatType;
      else
        type = _integerType;

      bool result;
      switch (type)
      {
        case ExtendedType.Integer:
          result = Order(op, ReadInteger(a).CompareTo(ReadInteger(b)));
          break;
        case ExtendedType.Long:
          result = Order(op, ReadLong(a).CompareTo(ReadLong(b)));
          break;
        case ExtendedType.Float:
        {
          var left = ReadFloat(a);
          var right = ReadFloat(b);
          result = equality ? Math.IsNearlyEqual(left, right) == (op == OpCode.Equal) : Order(op, left, right);
          break;
        }
        case ExtendedType.Double:
        {
          var left = ReadDouble(a);
          var right = ReadDouble(b);
          result = equality ? Math.IsNearlyEqual(left, right) == (op == OpCode.Equal) : Order(op, left, right);
          break;
        }
        case ExtendedType.Decimal:
          result = Order(op, ReadDecimal(a).CompareTo(ReadDecimal(b)));
          break;
        case ExtendedType.Boolean:
          // Only equality reaches here: ordering rejects non-numeric operands
          result = (ReadBoolean(a) == ReadBoolean(b)) == (op == OpCode.Equal);
          break;
        case ExtendedType.String:
          result = string.Equals(ReadString(a), ReadString(b), StringComparison.Ordinal) == (op == OpCode.Equal);
          break;
        default:
          throw new ArgumentOutOfRangeException("comparisonType", type, "Unsupported type for comparison");
      }

      WriteBoolean(instruction.Destination, result);
    }

    // Integer and decimal comparisons are total orders, so the sign of CompareTo decides every operator
    private static bool Order(OpCode op, int comparison)
    {
      switch (op)
      {
        case OpCode.Equal: return comparison == 0;
        case OpCode.NotEqual: return comparison != 0;
        case OpCode.LessThan: return comparison < 0;
        case OpCode.LessThanOrEqual: return comparison <= 0;
        case OpCode.GreaterThan: return comparison > 0;
        default: return comparison >= 0;
      }
    }

    // Binary floats use the IEEE operators: NaN orders false both ways
    private static bool Order(OpCode op, double left, double right)
    {
      switch (op)
      {
        case OpCode.LessThan: return left < right;
        case OpCode.LessThanOrEqual: return left <= right;
        case OpCode.GreaterThan: return left > right;
        default: return left >= right;
      }
    }

    // Assignments through the VariableValue setters, which convert to the variable's type
    private void Assign(ref Instruction instruction)
    {
      var op = instruction.Op;
      var value = instruction.B;
      var valueType = _registers[value].Type;
      var variable = _boundVariables[instruction.C];

      if (_noAlloc && variable.Type == Type.String && valueType != ExtendedType.String)
      {
        throw Error(ref instruction,
          "Assigning non-string value to string variable causes allocation (ToString()) in NoAlloc mode");
      }

      if (op != OpCode.AssignDirect && !valueType.IsNumber())
      {
        throw Error(ref instruction, "An arithmetic operation can only be performed on a numeric value");
      }

      switch (variable.Type)
      {
        case Type.Integer:
        {
          var right = ReadInteger(value);
          switch (op)
          {
            case OpCode.AssignDirect: variable.IntegerValue = right; break;
            case OpCode.AssignAdd: variable.IntegerValue += right; break;
            case OpCode.AssignSubtract: variable.IntegerValue -= right; break;
            case OpCode.AssignMultiply: variable.IntegerValue *= right; break;
            case OpCode.AssignDivide:
              if (right == 0) throw new DivideByZeroException("Division by zero");
              variable.IntegerValue /= right;
              break;
            default:
              if (right == 0) throw new DivideByZeroException("Modulo by zero");
              variable.IntegerValue %= right;
              break;
          }

          break;
        }
        case Type.Long:
        {
          var right = ReadLong(value);
          switch (op)
          {
            case OpCode.AssignDirect: variable.LongValue = right; break;
            case OpCode.AssignAdd: variable.LongValue += right; break;
            case OpCode.AssignSubtract: variable.LongValue -= right; break;
            case OpCode.AssignMultiply: variable.LongValue *= right; break;
            case OpCode.AssignDivide:
              if (right == 0) throw new DivideByZeroException("Division by zero");
              variable.LongValue /= right;
              break;
            default:
              if (right == 0) throw new DivideByZeroException("Modulo by zero");
              variable.LongValue %= right;
              break;
          }

          break;
        }
        case Type.Float:
        {
          var right = ReadFloat(value);
          switch (op)
          {
            case OpCode.AssignDirect: variable.FloatValue = right; break;
            case OpCode.AssignAdd: variable.FloatValue += right; break;
            case OpCode.AssignSubtract: variable.FloatValue -= right; break;
            case OpCode.AssignMultiply: variable.FloatValue *= right; break;
            case OpCode.AssignDivide:
              if (right == 0.0f) throw new DivideByZeroException("Division by zero");
              variable.FloatValue /= right;
              break;
            default:
              if (right == 0.0f) throw new DivideByZeroException("Modulo by zero");
              variable.FloatValue %= right;
              break;
          }

          break;
        }
        case Type.Double:
        {
          var right = ReadDouble(value);
          switch (op)
          {
            case OpCode.AssignDirect: variable.DoubleValue = right; break;
            case OpCode.AssignAdd: variable.DoubleValue += right; break;
            case OpCode.AssignSubtract: variable.DoubleValue -= right; break;
            case OpCode.AssignMultiply: variable.DoubleValue *= right; break;
            case OpCode.AssignDivide:
              if (right == 0.0) throw new DivideByZeroException("Division by zero");
              variable.DoubleValue /= right;
              break;
            default:
              if (right == 0.0) throw new DivideByZeroException("Modulo by zero");
              variable.DoubleValue %= right;
              break;
          }

          break;
        }
        case Type.Decimal:
        {
          var right = ReadDecimal(value);
          switch (op)
          {
            case OpCode.AssignDirect: variable.DecimalValue = right; break;
            case OpCode.AssignAdd: variable.DecimalValue += right; break;
            case OpCode.AssignSubtract: variable.DecimalValue -= right; break;
            case OpCode.AssignMultiply: variable.DecimalValue *= right; break;
            case OpCode.AssignDivide:
              if (right == 0m) throw new DivideByZeroException("Division by zero");
              variable.DecimalValue /= right;
              break;
            default:
              if (right == 0m) throw new DivideByZeroException("Modulo by zero");
              variable.DecimalValue %= right;
              break;
          }

          break;
        }
        case Type.Boolean when op == OpCode.AssignDirect:
          if (valueType.IsFloat())
          {
            throw Error(ref instruction, "A float value cannot be assigned to a boolean variable");
          }

          if (valueType.IsNumber())
          {
            throw Error(ref instruction, "A numeric value cannot be assigned to a boolean variable");
          }

          variable.BooleanValue = ReadBoolean(value);
          break;
        case Type.String when op == OpCode.AssignDirect:
          variable.StringValue = ReadString(value);
          break;
        default:
          throw new ArgumentOutOfRangeException("Type", variable.Type, "Unsupported variable type");
      }

      // The result is the variable's value after the assignment
      var destination = instruction.Destination;
      switch (variable.Type)
      {
        case Type.Integer: WriteInteger(destination, variable.IntegerValue); break;
        case Type.Long: WriteLong(destination, variable.LongValue); break;
        case Type.Float: WriteFloat(destination, variable.FloatValue); break;
        case Type.Double: WriteDouble(destination, variable.DoubleValue); break;
        case Type.Decimal: WriteDecimal(destination, variable.DecimalValue); break;
        case Type.Boolean: WriteBoolean(destination, variable.BooleanValue); break;
        case Type.String: WriteString(destination, variable.StringValue); break;
        default:
          throw new ArgumentOutOfRangeException("Type", variable.Type, "Unsupported variable type");
      }
    }
  }
}
