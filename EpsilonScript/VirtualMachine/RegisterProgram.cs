using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using EpsilonScript.AST;
using EpsilonScript.Function;

namespace EpsilonScript.VirtualMachine
{
  /// <summary>
  /// A script lowered to a flat register program.
  /// </summary>
  /// <remarks>
  /// Instructions compute values of the configured integer and float types inline. Any other operand type, and
  /// every error, goes to the generic path in RegisterProgram.Generic.cs.
  /// </remarks>
  internal sealed partial class RegisterProgram
  {
    // Reload replaces these when the program is reused for evaluation
    private Instruction[] _code;
    private SourceLocation[] _locations;
    private readonly CompilerContext _context;
    private VariableSlot[] _variables;
    private FunctionSite[] _functions;

    // Assignment targets resolved by BindVariable, indexed like _variables. Allocated by the first bind, which always
    // runs before the assignment that reads it.
    private VariableValue[] _boundVariables;
    private Register[] _registers;
    // Allocated on first use; a register typed String or Decimal implies its array exists
    private string[] _strings;
    private decimal[] _decimals;
    private int _resultRegister;
    private readonly ExtendedType _integerType;
    private readonly ExtendedType _floatType;
    private readonly bool _longIntegers;
    private readonly bool _doubleFloats;
    private readonly bool _decimalFloats; // Float values live in _decimals; arithmetic goes through System.Decimal
    private readonly bool _noAlloc;

    // Decimal mode: the float bits each LoadVariable last converted, indexed like _variables. Converting double to
    // decimal dominates decimal scripts, and variables usually keep their value between executions.
    private long[] _convertedBits;
    private bool[] _convertedValid;

    public RegisterProgram(Instruction[] code, SourceLocation[] locations, CompilerContext context,
      VariableSlot[] variables, FunctionSite[] functions, int registerCount, int resultRegister,
      ExtendedType integerType, ExtendedType floatType, bool noAlloc)
    {
      _code = code;
      _locations = locations;
      _context = context;
      _variables = variables;
      _functions = functions;
      _registers = new Register[registerCount];
      _resultRegister = resultRegister;
      _integerType = integerType;
      _floatType = floatType;
      _longIntegers = integerType == ExtendedType.Long;
      _doubleFloats = floatType == ExtendedType.Double;
      _decimalFloats = floatType == ExtendedType.Decimal;
      _noAlloc = noAlloc;
      if (_decimalFloats)
      {
        _decimals = new decimal[registerCount];
        _convertedBits = new long[variables.Length];
        _convertedValid = new bool[variables.Length];
      }

      // A NoAlloc script allocates nothing when it first runs either, so it allocates now what execution would on
      // first use. Strings come from constants, which seed _strings at lowering, variables and function results.
      // Other float precisions hold decimals only from function results and assignments to decimal variables.
      for (var i = 0; noAlloc && i < code.Length; ++i)
      {
        var op = code[i].Op;
        if (op == OpCode.BindVariable)
        {
          _boundVariables = _boundVariables ?? new VariableValue[variables.Length];
        }

        if (op == OpCode.LoadVariable || op == OpCode.Call)
        {
          _strings = _strings ?? new string[registerCount];
        }

        if (op == OpCode.Call || op == OpCode.BindVariable)
        {
          _decimals = _decimals ?? new decimal[registerCount];
        }
      }
    }

    /// <summary>
    /// Clears the strings and bound variables of a program reused for evaluation once its result is read.
    /// </summary>
    internal void ReleaseReferences(int registerCount, int variableCount)
    {
      if (_strings != null)
      {
        Array.Clear(_strings, 0, registerCount);
      }

      if (_boundVariables != null)
      {
        Array.Clear(_boundVariables, 0, variableCount);
      }
    }

    /// <summary>
    /// Replaces the code of a program that is reused for evaluation. The code array may be longer than the code,
    /// which then ends with Halt; the other arrays may be longer than the code uses.
    /// </summary>
    internal void Reload(Instruction[] code, SourceLocation[] locations, VariableSlot[] variables,
      FunctionSite[] functions, int registerCount, int resultRegister)
    {
      _code = code;
      _locations = locations;
      _variables = variables;
      _functions = functions;
      _resultRegister = resultRegister;
      if (_registers.Length < registerCount)
      {
        // Side arrays are sized like the registers
        _registers = new Register[registerCount];
        _strings = null;
        _decimals = _decimalFloats ? new decimal[registerCount] : null;
      }

      if (_boundVariables != null && _boundVariables.Length < variables.Length)
      {
        _boundVariables = null;
      }

      if (_decimalFloats)
      {
        if (_convertedValid.Length < variables.Length)
        {
          _convertedBits = new long[variables.Length];
          _convertedValid = new bool[variables.Length];
        }
        else
        {
          System.Array.Clear(_convertedValid, 0, _convertedValid.Length);
        }
      }
    }

    /// <summary>
    /// Register holding the script result. CompiledScript reads values from it with the Read methods.
    /// </summary>
    internal int ResultRegister
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _resultRegister;
    }

    internal ExtendedType ResultType
    {
      [MethodImpl(MethodImplOptions.AggressiveInlining)] get => _registers[_resultRegister].Type;
    }

    public void Execute(IVariableContainer variablesOverride)
    {
      var code = _code;
      var registers = _registers;
      var integerType = _integerType;
      var floatType = _floatType;

      var pc = 0;
      while (pc < code.Length)
      {
        ref var instruction = ref code[pc];
        ++pc;

        switch (instruction.Op)
        {
          case OpCode.LoadVariable:
          {
            ref var slot = ref _variables[instruction.Site];
            var variable = slot.Resolve(variablesOverride);
            ref var destination = ref registers[instruction.Destination];
            switch (variable.Type)
            {
              case Type.Integer:
              case Type.Long:
                destination.Integer = _longIntegers ? variable.RawInteger : (int)variable.RawInteger;
                destination.Type = integerType;
                break;
              case Type.Float:
              case Type.Double:
                if (_decimalFloats)
                {
                  LoadFloatAsDecimal(instruction.Site, instruction.Destination, variable.RawFloat);
                }
                else
                {
                  destination.Float = _doubleFloats ? variable.RawFloat : (float)variable.RawFloat;
                  destination.Type = floatType;
                }

                break;
              default:
                LoadOtherVariable(ref instruction, variable);
                break;
            }

            break;
          }

          case OpCode.MakeTuple:
            registers[instruction.Destination].Type = ExtendedType.Tuple;
            break;

          case OpCode.BindVariable:
          {
            ref var slot = ref _variables[instruction.Site];
            var variable = slot.Resolve(variablesOverride);
            // Integer through Decimal; loading any other type fails, so binding one does too
            if ((uint)variable.Type - 1 >= 7)
            {
              throw new ArgumentOutOfRangeException("Type", variable.Type, "Unsupported variable type");
            }

            // Loading a float into a decimal register fails for NaN, infinities and values out of decimal range
            if (_decimalFloats && (variable.Type == Type.Float || variable.Type == Type.Double) &&
                !(System.Math.Abs(variable.RawFloat) < DecimalLimit))
            {
              ConvertToDecimal(variable);
            }

            Bind(instruction.Site, variable);
            break;
          }

          case OpCode.Move:
            Move(instruction.Destination, instruction.A);
            break;

          case OpCode.Positive:
          case OpCode.AssignNonVariable:
            Slow(ref instruction);
            break;

          case OpCode.Call:
            Call(ref instruction, variablesOverride);
            break;

          case OpCode.AssignDirect:
          case OpCode.AssignAdd:
          case OpCode.AssignSubtract:
          case OpCode.AssignMultiply:
          case OpCode.AssignDivide:
          case OpCode.AssignModulo:
            if (!TryAssign(ref instruction))
            {
              Slow(ref instruction);
            }

            break;

          case OpCode.Add:
          {
            ref var a = ref registers[instruction.A];
            ref var b = ref registers[instruction.B];
            ref var destination = ref registers[instruction.Destination];
            if (a.Type == integerType && b.Type == integerType)
            {
              destination.Integer = _longIntegers ? a.Integer + b.Integer : (int)a.Integer + (int)b.Integer;
              destination.Type = integerType;
            }
            else if (IsFast(a.Type) && IsFast(b.Type))
            {
              if (_decimalFloats)
              {
                // Decimal before the operation can overflow, as the tree interpreter's node was
                destination.Type = ExtendedType.Decimal;
                SetDecimal(instruction.Destination, AsDecimal(instruction.A) + AsDecimal(instruction.B));
              }
              else
              {
                destination.Float = _doubleFloats ? AsDouble(ref a) + AsDouble(ref b) : AsFloat(ref a) + AsFloat(ref b);
                destination.Type = floatType;
              }
            }
            else if (a.Type != ExtendedType.String || !TryConcatenate(ref instruction))
            {
              Slow(ref instruction);
            }

            break;
          }

          case OpCode.Subtract:
          {
            ref var a = ref registers[instruction.A];
            ref var b = ref registers[instruction.B];
            ref var destination = ref registers[instruction.Destination];
            if (a.Type == integerType && b.Type == integerType)
            {
              destination.Integer = _longIntegers ? a.Integer - b.Integer : (int)a.Integer - (int)b.Integer;
              destination.Type = integerType;
            }
            else if (IsFast(a.Type) && IsFast(b.Type))
            {
              if (_decimalFloats)
              {
                // Decimal before the operation can overflow, as the tree interpreter's node was
                destination.Type = ExtendedType.Decimal;
                SetDecimal(instruction.Destination, AsDecimal(instruction.A) - AsDecimal(instruction.B));
              }
              else
              {
                destination.Float = _doubleFloats ? AsDouble(ref a) - AsDouble(ref b) : AsFloat(ref a) - AsFloat(ref b);
                destination.Type = floatType;
              }
            }
            else
            {
              Slow(ref instruction);
            }

            break;
          }

          case OpCode.Multiply:
          {
            ref var a = ref registers[instruction.A];
            ref var b = ref registers[instruction.B];
            ref var destination = ref registers[instruction.Destination];
            if (a.Type == integerType && b.Type == integerType)
            {
              destination.Integer = _longIntegers ? a.Integer * b.Integer : (int)a.Integer * (int)b.Integer;
              destination.Type = integerType;
            }
            else if (IsFast(a.Type) && IsFast(b.Type))
            {
              if (_decimalFloats)
              {
                // Decimal before the operation can overflow, as the tree interpreter's node was
                destination.Type = ExtendedType.Decimal;
                SetDecimal(instruction.Destination, AsDecimal(instruction.A) * AsDecimal(instruction.B));
              }
              else
              {
                destination.Float = _doubleFloats ? AsDouble(ref a) * AsDouble(ref b) : AsFloat(ref a) * AsFloat(ref b);
                destination.Type = floatType;
              }
            }
            else
            {
              Slow(ref instruction);
            }

            break;
          }

          case OpCode.Divide:
          case OpCode.Modulo:
            // Zero divisors, MinValue / -1 and non-numeric operands take the generic path for its exceptions
            if (!TryDivide(ref instruction))
            {
              Slow(ref instruction);
            }

            break;

          // Each form passes its own constant op, so the inlined arithmetic needs no branch on it
          case OpCode.MultiplyAdd:
          case OpCode.AddMultiply:
            if (!MultiplyAdd(ref instruction, registers, integerType, floatType, OpCode.MultiplyAdd))
            {
              Slow(ref instruction);
            }

            break;

          case OpCode.MultiplySubtract:
            if (!MultiplyAdd(ref instruction, registers, integerType, floatType, OpCode.MultiplySubtract))
            {
              Slow(ref instruction);
            }

            break;

          case OpCode.SubtractMultiply:
            if (!MultiplyAdd(ref instruction, registers, integerType, floatType, OpCode.SubtractMultiply))
            {
              Slow(ref instruction);
            }

            break;

          case OpCode.Negate:
          {
            ref var a = ref registers[instruction.A];
            ref var destination = ref registers[instruction.Destination];
            if (a.Type == integerType)
            {
              destination.Integer = _longIntegers ? -a.Integer : -(int)a.Integer;
              destination.Type = integerType;
            }
            else if (a.Type == floatType)
            {
              if (_decimalFloats)
              {
                WriteDecimal(instruction.Destination, -_decimals[instruction.A]);
              }
              else
              {
                destination.Float = _doubleFloats ? -a.Float : -(float)a.Float;
                destination.Type = floatType;
              }
            }
            else
            {
              Slow(ref instruction);
            }

            break;
          }

          case OpCode.Not:
          {
            ref var a = ref registers[instruction.A];
            if (a.Type == ExtendedType.Boolean)
            {
              ref var destination = ref registers[instruction.Destination];
              destination.Integer = a.Integer != 0 ? 0 : 1;
              destination.Type = ExtendedType.Boolean;
            }
            else
            {
              Slow(ref instruction);
            }

            break;
          }

          case OpCode.Equal:
          case OpCode.NotEqual:
          case OpCode.LessThan:
          case OpCode.LessThanOrEqual:
          case OpCode.GreaterThan:
          case OpCode.GreaterThanOrEqual:
            if (!TryCompare(ref instruction))
            {
              Slow(ref instruction);
            }

            break;

          case OpCode.AndLeft:
          case OpCode.OrLeft:
          {
            ref var a = ref registers[instruction.A];
            if (a.Type != ExtendedType.Boolean)
            {
              // The node rejects the left operand before it reads the right one
              Slow(ref instruction);
              break;
            }

            var shortCircuit = instruction.Op == OpCode.AndLeft ? a.Integer == 0 : a.Integer != 0;
            if (shortCircuit)
            {
              ref var destination = ref registers[instruction.Destination];
              destination.Integer = a.Integer != 0 ? 1 : 0;
              destination.Type = ExtendedType.Boolean;
              pc = instruction.C;
            }

            break;
          }

          case OpCode.BooleanRight:
          {
            ref var b = ref registers[instruction.B];
            if (b.Type == ExtendedType.Boolean)
            {
              ref var destination = ref registers[instruction.Destination];
              destination.Integer = b.Integer != 0 ? 1 : 0;
              destination.Type = ExtendedType.Boolean;
            }
            else
            {
              Slow(ref instruction);
            }

            break;
          }

          case OpCode.Halt:
            return;

          default:
            throw new InvalidOperationException($"Unhandled opcode {instruction.Op}");
        }
      }

    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsFast(ExtendedType type) => type == _integerType || type == _floatType;

    // Same conversions as Node.FloatValue / Node.DoubleValue for the configured integer and float types
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float AsFloat(ref Register register) =>
      register.Type == _floatType ? (float)register.Float : (float)register.Integer;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private double AsDouble(ref Register register) =>
      register.Type == _floatType ? register.Float : (double)register.Integer;

    // For registers already checked to hold the configured integer or decimal type
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private decimal AsDecimal(int register) =>
      _registers[register].Type == _floatType ? _decimals[register] : _registers[register].Integer;

    private bool TryDivide(ref Instruction instruction)
    {
      ref var a = ref _registers[instruction.A];
      ref var b = ref _registers[instruction.B];
      ref var destination = ref _registers[instruction.Destination];
      var modulo = instruction.Op == OpCode.Modulo;
      if (a.Type == _integerType && b.Type == _integerType)
      {
        var divisor = b.Integer;
        if (divisor == 0 || divisor == -1)
        {
          return false;
        }

        if (_longIntegers)
        {
          destination.Integer = modulo ? a.Integer % divisor : a.Integer / divisor;
        }
        else
        {
          destination.Integer = modulo ? (int)a.Integer % (int)divisor : (int)a.Integer / (int)divisor;
        }

        destination.Type = _integerType;
        return true;
      }

      if (!IsFast(a.Type) || !IsFast(b.Type))
      {
        return false;
      }

      if (_decimalFloats)
      {
        var divisor = AsDecimal(instruction.B);
        if (divisor == 0m)
        {
          return false;
        }

        var dividend = AsDecimal(instruction.A);
        // Decimal before the operation can overflow, as the tree interpreter's node was
        _registers[instruction.Destination].Type = ExtendedType.Decimal;
        SetDecimal(instruction.Destination, modulo ? dividend % divisor : dividend / divisor);
        return true;
      }

      if (_doubleFloats)
      {
        var divisor = AsDouble(ref b);
        if (divisor == 0.0)
        {
          return false;
        }

        destination.Float = modulo ? AsDouble(ref a) % divisor : AsDouble(ref a) / divisor;
      }
      else
      {
        var divisor = AsFloat(ref b);
        if (divisor == 0.0f)
        {
          return false;
        }

        destination.Float = modulo ? AsFloat(ref a) % divisor : AsFloat(ref a) / divisor;
      }

      destination.Type = _floatType;
      return true;
    }

    // Concatenation with a string left operand; NoAlloc and unsupported right operands take the generic path for its
    // exceptions
    private bool TryConcatenate(ref Instruction instruction)
    {
      ref var b = ref _registers[instruction.B];
      var rightType = b.Type;
      if (_noAlloc || rightType == ExtendedType.Null || rightType == ExtendedType.Undefined ||
          rightType == ExtendedType.Tuple)
      {
        return false;
      }

      var context = _context;
      var sb = context.StringBuilder;
      sb.Clear();
      sb.Append(_strings[instruction.A]);
      switch (rightType)
      {
        case ExtendedType.Integer:
          sb.Append((int)b.Integer);
          break;
        case ExtendedType.Long:
          sb.Append(b.Integer);
          break;
        case ExtendedType.Float:
          sb.AppendFloatInvariant((float)b.Float, context);
          break;
        case ExtendedType.Double:
          sb.AppendDoubleInvariant(b.Float, context);
          break;
        case ExtendedType.Decimal:
          sb.AppendDecimalInvariant(_decimals[instruction.B], context);
          break;
        case ExtendedType.String:
          sb.Append(_strings[instruction.B]);
          break;
        case ExtendedType.Boolean:
          sb.Append(b.Integer != 0 ? "true" : "false");
          break;
        default:
          return false;
      }

      WriteString(instruction.Destination, sb.ToString());
      return true;
    }

    // Assignments of numeric values to numeric variables; other cases, and zero divisors, take the generic path
    private bool TryAssign(ref Instruction instruction)
    {
      var variable = _boundVariables[instruction.C];
      var valueType = _registers[instruction.B].Type;
      if (!IsFast(valueType))
      {
        return false;
      }

      var op = instruction.Op;
      var value = instruction.B;
      switch (variable.Type)
      {
        case Type.Integer:
        {
          var right = ReadInteger(value);
          var current = (int)variable.RawInteger;
          int result;
          switch (op)
          {
            case OpCode.AssignDirect: result = right; break;
            case OpCode.AssignAdd: result = current + right; break;
            case OpCode.AssignSubtract: result = current - right; break;
            case OpCode.AssignMultiply: result = current * right; break;
            default:
              if (right == 0 || right == -1) return false;
              result = op == OpCode.AssignDivide ? current / right : current % right;
              break;
          }

          variable.StoreRawInteger(result);
          WriteInteger(instruction.Destination, result);
          return true;
        }
        case Type.Long:
        {
          var right = ReadLong(value);
          var current = variable.RawInteger;
          long result;
          switch (op)
          {
            case OpCode.AssignDirect: result = right; break;
            case OpCode.AssignAdd: result = current + right; break;
            case OpCode.AssignSubtract: result = current - right; break;
            case OpCode.AssignMultiply: result = current * right; break;
            default:
              if (right == 0 || right == -1) return false;
              result = op == OpCode.AssignDivide ? current / right : current % right;
              break;
          }

          variable.StoreRawInteger(result);
          WriteLong(instruction.Destination, result);
          return true;
        }
        case Type.Float:
        {
          var right = ReadFloat(value);
          var current = (float)variable.RawFloat;
          float result;
          switch (op)
          {
            case OpCode.AssignDirect: result = right; break;
            case OpCode.AssignAdd: result = current + right; break;
            case OpCode.AssignSubtract: result = current - right; break;
            case OpCode.AssignMultiply: result = current * right; break;
            default:
              if (right == 0.0f) return false;
              result = op == OpCode.AssignDivide ? current / right : current % right;
              break;
          }

          variable.StoreRawFloat(result);
          WriteFloat(instruction.Destination, result);
          return true;
        }
        case Type.Double:
        {
          var right = ReadDouble(value);
          var current = variable.RawFloat;
          double result;
          switch (op)
          {
            case OpCode.AssignDirect: result = right; break;
            case OpCode.AssignAdd: result = current + right; break;
            case OpCode.AssignSubtract: result = current - right; break;
            case OpCode.AssignMultiply: result = current * right; break;
            default:
              if (right == 0.0) return false;
              result = op == OpCode.AssignDivide ? current / right : current % right;
              break;
          }

          variable.StoreRawFloat(result);
          WriteDouble(instruction.Destination, result);
          return true;
        }
        case Type.Decimal:
        {
          var right = ReadDecimal(value);
          var current = variable.DecimalValue;
          decimal result;
          switch (op)
          {
            case OpCode.AssignDirect: result = right; break;
            case OpCode.AssignAdd: result = current + right; break;
            case OpCode.AssignSubtract: result = current - right; break;
            case OpCode.AssignMultiply: result = current * right; break;
            default:
              if (right == 0m) return false;
              result = op == OpCode.AssignDivide ? current / right : current % right;
              break;
          }

          variable.DecimalValue = result;
          WriteDecimal(instruction.Destination, variable.DecimalValue);
          return true;
        }
        default:
          return false;
      }
    }

    private void Move(int destination, int source)
    {
      ref var from = ref _registers[source];
      switch (from.Type)
      {
        case ExtendedType.String:
          WriteString(destination, _strings[source]);
          return;
        case ExtendedType.Decimal:
          SetDecimal(destination, _decimals[source]);
          break;
      }

      _registers[destination] = from;
    }

    // The integer and binary float forms inline into Execute, as Add and Multiply do; IL2CPP inlines only
    // AggressiveInlining methods. op is MultiplyAdd for AddMultiply too, which computes the same sum.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MultiplyAdd(ref Instruction instruction, Register[] registers, ExtendedType integerType,
      ExtendedType floatType, OpCode op)
    {
      ref var m1 = ref registers[instruction.A];
      ref var m2 = ref registers[instruction.B];
      ref var addend = ref registers[instruction.C];
      ref var destination = ref registers[instruction.Destination];
      if (m1.Type == integerType && m2.Type == integerType && addend.Type == integerType)
      {
        if (_longIntegers)
        {
          var product = m1.Integer * m2.Integer;
          destination.Integer = op == OpCode.MultiplyAdd ? product + addend.Integer
            : op == OpCode.MultiplySubtract ? product - addend.Integer
            : addend.Integer - product;
        }
        else
        {
          var product = (int)m1.Integer * (int)m2.Integer;
          destination.Integer = op == OpCode.MultiplyAdd ? product + (int)addend.Integer
            : op == OpCode.MultiplySubtract ? product - (int)addend.Integer
            : (int)addend.Integer - product;
        }

        destination.Type = integerType;
        return true;
      }

      if (_decimalFloats || !IsFast(m1.Type) || !IsFast(m2.Type) || !IsFast(addend.Type))
      {
        return TryMultiplyAdd(ref instruction);
      }

      if (_doubleFloats)
      {
        var product = AsDouble(ref m1) * AsDouble(ref m2);
        destination.Float = op == OpCode.MultiplyAdd ? product + AsDouble(ref addend)
          : op == OpCode.MultiplySubtract ? product - AsDouble(ref addend)
          : AsDouble(ref addend) - product;
      }
      else
      {
        var product = AsFloat(ref m1) * AsFloat(ref m2);
        destination.Float = op == OpCode.MultiplyAdd ? product + AsFloat(ref addend)
          : op == OpCode.MultiplySubtract ? product - AsFloat(ref addend)
          : AsFloat(ref addend) - product;
      }

      destination.Type = floatType;
      return true;
    }

    private bool TryMultiplyAdd(ref Instruction instruction)
    {
      ref var m1 = ref _registers[instruction.A];
      ref var m2 = ref _registers[instruction.B];
      ref var addend = ref _registers[instruction.C];
      ref var destination = ref _registers[instruction.Destination];
      var op = instruction.Op;
      if (m1.Type == _integerType && m2.Type == _integerType && addend.Type == _integerType)
      {
        if (_longIntegers)
        {
          var product = m1.Integer * m2.Integer;
          destination.Integer = op == OpCode.MultiplyAdd || op == OpCode.AddMultiply ? product + addend.Integer
            : op == OpCode.MultiplySubtract ? product - addend.Integer
            : addend.Integer - product;
        }
        else
        {
          var product = (int)m1.Integer * (int)m2.Integer;
          destination.Integer = op == OpCode.MultiplyAdd || op == OpCode.AddMultiply ? product + (int)addend.Integer
            : op == OpCode.MultiplySubtract ? product - (int)addend.Integer
            : (int)addend.Integer - product;
        }

        destination.Type = _integerType;
        return true;
      }

      if (!IsFast(m1.Type) || !IsFast(m2.Type) || !IsFast(addend.Type))
      {
        return false;
      }

      if (_decimalFloats)
      {
        // Decimal before the operation can overflow, as the tree interpreter's node was
        _registers[instruction.Destination].Type = ExtendedType.Decimal;
        var product = AsDecimal(instruction.A) * AsDecimal(instruction.B);
        var c = AsDecimal(instruction.C);
        SetDecimal(instruction.Destination,
          op == OpCode.MultiplyAdd || op == OpCode.AddMultiply ? product + c
          : op == OpCode.MultiplySubtract ? product - c
          : c - product);
        return true;
      }

      if (_doubleFloats)
      {
        var product = AsDouble(ref m1) * AsDouble(ref m2);
        destination.Float = op == OpCode.MultiplyAdd || op == OpCode.AddMultiply ? product + AsDouble(ref addend)
          : op == OpCode.MultiplySubtract ? product - AsDouble(ref addend)
          : AsDouble(ref addend) - product;
      }
      else
      {
        var product = AsFloat(ref m1) * AsFloat(ref m2);
        destination.Float = op == OpCode.MultiplyAdd || op == OpCode.AddMultiply ? product + AsFloat(ref addend)
          : op == OpCode.MultiplySubtract ? product - AsFloat(ref addend)
          : AsFloat(ref addend) - product;
      }

      destination.Type = _floatType;
      return true;
    }

    private bool TryCompare(ref Instruction instruction)
    {
      ref var a = ref _registers[instruction.A];
      ref var b = ref _registers[instruction.B];
      var op = instruction.Op;
      bool result;
      if (a.Type == _integerType && b.Type == _integerType)
      {
        // Integer registers hold sign-extended values in either precision, so comparing longs is exact
        var x = a.Integer;
        var y = b.Integer;
        switch (op)
        {
          case OpCode.Equal: result = x == y; break;
          case OpCode.NotEqual: result = x != y; break;
          case OpCode.LessThan: result = x < y; break;
          case OpCode.LessThanOrEqual: result = x <= y; break;
          case OpCode.GreaterThan: result = x > y; break;
          default: result = x >= y; break;
        }
      }
      else if (IsFast(a.Type) && IsFast(b.Type))
      {
        if (_decimalFloats)
        {
          var x = AsDecimal(instruction.A);
          var y = AsDecimal(instruction.B);
          switch (op)
          {
            case OpCode.Equal: result = x == y; break;
            case OpCode.NotEqual: result = x != y; break;
            case OpCode.LessThan: result = x < y; break;
            case OpCode.LessThanOrEqual: result = x <= y; break;
            case OpCode.GreaterThan: result = x > y; break;
            default: result = x >= y; break;
          }
        }
        else if (_doubleFloats)
        {
          var x = AsDouble(ref a);
          var y = AsDouble(ref b);
          switch (op)
          {
            case OpCode.Equal: result = Math.IsNearlyEqual(x, y); break;
            case OpCode.NotEqual: result = !Math.IsNearlyEqual(x, y); break;
            case OpCode.LessThan: result = x < y; break;
            case OpCode.LessThanOrEqual: result = x <= y; break;
            case OpCode.GreaterThan: result = x > y; break;
            default: result = x >= y; break;
          }
        }
        else
        {
          var x = AsFloat(ref a);
          var y = AsFloat(ref b);
          switch (op)
          {
            case OpCode.Equal: result = Math.IsNearlyEqual(x, y); break;
            case OpCode.NotEqual: result = !Math.IsNearlyEqual(x, y); break;
            case OpCode.LessThan: result = x < y; break;
            case OpCode.LessThanOrEqual: result = x <= y; break;
            case OpCode.GreaterThan: result = x > y; break;
            default: result = x >= y; break;
          }
        }
      }
      else if (a.Type == ExtendedType.String && b.Type == ExtendedType.String &&
               (op == OpCode.Equal || op == OpCode.NotEqual))
      {
        result = string.Equals(_strings[instruction.A], _strings[instruction.B], System.StringComparison.Ordinal) ==
                 (op == OpCode.Equal);
      }
      else if (a.Type == ExtendedType.Boolean && b.Type == ExtendedType.Boolean &&
               (op == OpCode.Equal || op == OpCode.NotEqual))
      {
        result = (a.Integer != 0) == (b.Integer != 0) == (op == OpCode.Equal);
      }
      else
      {
        return false;
      }

      ref var destination = ref _registers[instruction.Destination];
      destination.Integer = result ? 1 : 0;
      destination.Type = ExtendedType.Boolean;
      return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LoadFloatAsDecimal(int variableIndex, int register, double value)
    {
      // Only this method and LoadOtherVariable write a LoadVariable register as Decimal, and the latter clears
      // the flag, so a Decimal register with matching bits still holds this conversion. This relies on every
      // node owning its register: an instruction that wrote another value into a LoadVariable register would
      // make this return a stale decimal.
      var bits = BitConverter.DoubleToInt64Bits(value);
      if (_convertedValid[variableIndex] && _convertedBits[variableIndex] == bits &&
          _registers[register].Type == ExtendedType.Decimal)
      {
        return;
      }

      _convertedValid[variableIndex] = false;
      WriteDecimal(register, (decimal)value);
      _convertedBits[variableIndex] = bits;
      _convertedValid[variableIndex] = true;
    }

    // Decimal and non-numeric variables, converted to the configured types
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void LoadOtherVariable(ref Instruction instruction, VariableValue variable)
    {
      var register = instruction.Destination;
      if (_decimalFloats)
      {
        _convertedValid[instruction.Site] = false;
      }

      ref var destination = ref _registers[register];
      switch (variable.Type)
      {
        case Type.Decimal:
          if (_decimalFloats)
          {
            WriteDecimal(register, variable.DecimalValue);
          }
          else if (_doubleFloats)
          {
            destination.Float = variable.DoubleValue;
            destination.Type = _floatType;
          }
          else
          {
            destination.Float = variable.FloatValue;
            destination.Type = _floatType;
          }

          break;
        case Type.Boolean:
          destination.Integer = variable.BooleanValue ? 1 : 0;
          destination.Type = ExtendedType.Boolean;
          break;
        case Type.String:
          var value = variable.StringValue;
          SetString(register, value);

          destination.Type = ExtendedType.String;
          break;
        default:
          throw new ArgumentOutOfRangeException("Type", variable.Type, "Unsupported variable type");
      }
    }

    // Overloads are resolved by argument types and cached per call site
    private void Call(ref Instruction instruction, IVariableContainer variablesOverride)
    {
      var site = _functions[instruction.Site];
      var arguments = site.Arguments;
      var packedTypes = new PackedParameterTypes();
      for (var i = 0; i < arguments.Length; ++i)
      {
        packedTypes.AddType(_registers[arguments[i]].Type);
      }

      var function = site.Function;
      if (function == null || site.PackedTypes != packedTypes || site.Version != site.Overload.Version)
      {
        function = site.Resolve(packedTypes);
        SetReturnType(instruction.Destination, (ExtendedType)function.ReturnType);
      }

      if (site.IsContextual)
      {
        function.Invoke(this, new VariableContextAdapter(variablesOverride, site.CompileTimeVariables),
          arguments, instruction.Destination);
      }
      else
      {
        function.Invoke(this, arguments, instruction.Destination);
      }
    }

    // The tree interpreter's node took the return type when it resolved an overload, before the call, which may
    // throw. Results read after that failure read the register as that type, so its value array must exist.
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void SetReturnType(int register, ExtendedType type)
    {
      if (type == ExtendedType.String && _strings == null)
      {
        _strings = new string[_registers.Length];
      }
      else if (type == ExtendedType.Decimal && _decimals == null)
      {
        AllocateDecimals();
      }

      _registers[register].Type = type;
    }

    // Register reads with the same conversions as the Node value getters. The inlined part handles the type
    // family the caller usually asks for; everything else goes through the full switch.

    // Integer (1) and Long (5), Float (2) and Double (6) differ only in bit 2
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsIntegerFamily(ExtendedType type) => ((int)type & ~4) == 1;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsFloatFamily(ExtendedType type) => ((int)type & ~4) == 2;


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int ReadInteger(int register)
    {
      ref var source = ref _registers[register];
      return IsIntegerFamily(source.Type) ? (int)source.Integer : ReadIntegerSlow(register);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private int ReadIntegerSlow(int register)
    {
      ref var source = ref _registers[register];
      switch (source.Type)
      {
        case ExtendedType.Integer:
        case ExtendedType.Long:
          return (int)source.Integer;
        case ExtendedType.Float:
        case ExtendedType.Double:
          var value = source.Float;
          if (double.IsNaN(value) || double.IsInfinity(value))
            return 0;
          return (int)value;
        case ExtendedType.Decimal:
          return (int)_decimals[register];
        case ExtendedType.Boolean:
          return source.Integer != 0 ? 1 : 0;
        case ExtendedType.String:
        case ExtendedType.Null:
        case ExtendedType.Undefined:
          return 0;
        default:
          throw new InvalidOperationException($"Cannot convert {source.Type} to integer");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal long ReadLong(int register)
    {
      ref var source = ref _registers[register];
      return IsIntegerFamily(source.Type) ? source.Integer : ReadLongSlow(register);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private long ReadLongSlow(int register)
    {
      ref var source = ref _registers[register];
      switch (source.Type)
      {
        case ExtendedType.Integer:
        case ExtendedType.Long:
          return source.Integer;
        case ExtendedType.Float:
        case ExtendedType.Double:
          var value = source.Float;
          if (double.IsNaN(value) || double.IsInfinity(value))
            return 0;
          return (long)value;
        case ExtendedType.Decimal:
          return (long)_decimals[register];
        case ExtendedType.Boolean:
          return source.Integer;
        case ExtendedType.String:
        case ExtendedType.Null:
        case ExtendedType.Undefined:
          return 0L;
        default:
          throw new InvalidOperationException($"Cannot convert {source.Type} to long");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal float ReadFloat(int register)
    {
      ref var source = ref _registers[register];
      return IsFloatFamily(source.Type) ? (float)source.Float : ReadFloatSlow(register);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private float ReadFloatSlow(int register)
    {
      ref var source = ref _registers[register];
      switch (source.Type)
      {
        case ExtendedType.Integer:
        case ExtendedType.Long:
          return source.Integer;
        case ExtendedType.Float:
        case ExtendedType.Double:
          return (float)source.Float;
        case ExtendedType.Decimal:
          return (float)_decimals[register];
        case ExtendedType.Boolean:
          return source.Integer != 0 ? 1.0f : 0.0f;
        case ExtendedType.String:
        case ExtendedType.Null:
        case ExtendedType.Undefined:
          return 0.0f;
        default:
          throw new InvalidOperationException($"Cannot convert {source.Type} to float");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal double ReadDouble(int register)
    {
      ref var source = ref _registers[register];
      return IsFloatFamily(source.Type) ? source.Float : ReadDoubleSlow(register);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private double ReadDoubleSlow(int register)
    {
      ref var source = ref _registers[register];
      switch (source.Type)
      {
        case ExtendedType.Integer:
        case ExtendedType.Long:
          return source.Integer;
        case ExtendedType.Float:
        case ExtendedType.Double:
          return source.Float;
        case ExtendedType.Decimal:
          return (double)_decimals[register];
        case ExtendedType.Boolean:
          return source.Integer != 0 ? 1.0 : 0.0;
        case ExtendedType.String:
        case ExtendedType.Null:
        case ExtendedType.Undefined:
          return 0.0;
        default:
          throw new InvalidOperationException($"Cannot convert {source.Type} to double");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal decimal ReadDecimal(int register)
    {
      ref var source = ref _registers[register];
      return source.Type == ExtendedType.Decimal ? _decimals[register] : ReadDecimalSlow(register);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private decimal ReadDecimalSlow(int register)
    {
      ref var source = ref _registers[register];
      switch (source.Type)
      {
        case ExtendedType.Integer:
        case ExtendedType.Long:
          return source.Integer;
        case ExtendedType.Float:
        case ExtendedType.Double:
          return (decimal)source.Float;
        case ExtendedType.Decimal:
          return _decimals[register];
        case ExtendedType.Boolean:
          return source.Integer != 0 ? 1m : 0m;
        case ExtendedType.String:
        case ExtendedType.Null:
        case ExtendedType.Undefined:
          return 0m;
        default:
          throw new InvalidOperationException($"Cannot convert {source.Type} to decimal");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal bool ReadBoolean(int register)
    {
      ref var source = ref _registers[register];
      return source.Type == ExtendedType.Boolean ? source.Integer != 0 : ReadBooleanSlow(register);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool ReadBooleanSlow(int register)
    {
      ref var source = ref _registers[register];
      switch (source.Type)
      {
        case ExtendedType.Boolean:
        case ExtendedType.Integer:
        case ExtendedType.Long:
          return source.Integer != 0;
        case ExtendedType.Float:
        case ExtendedType.Double:
          var value = source.Float;
          return value != 0.0 && !double.IsNaN(value);
        case ExtendedType.Decimal:
          return _decimals[register] != 0m;
        case ExtendedType.String:
        case ExtendedType.Null:
        case ExtendedType.Undefined:
          return false;
        default:
          throw new InvalidOperationException($"Cannot convert {source.Type} to boolean");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal string ReadString(int register)
    {
      ref var source = ref _registers[register];
      return source.Type == ExtendedType.String ? _strings[register] : ReadStringSlow(register);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private string ReadStringSlow(int register)
    {
      ref var source = ref _registers[register];
      switch (source.Type)
      {
        case ExtendedType.String:
          return _strings[register];
        case ExtendedType.Integer:
        case ExtendedType.Long:
          return source.Integer.ToString(CultureInfo.InvariantCulture);
        case ExtendedType.Float:
          return ((float)source.Float).ToString(CultureInfo.InvariantCulture);
        case ExtendedType.Double:
          return source.Float.ToString(CultureInfo.InvariantCulture);
        case ExtendedType.Decimal:
          return _decimals[register].ToString(CultureInfo.InvariantCulture);
        case ExtendedType.Boolean:
          return (source.Integer != 0).ToString();
        case ExtendedType.Null:
        case ExtendedType.Undefined:
          return null;
        default:
          throw new InvalidOperationException($"Cannot convert {source.Type} to string");
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteInteger(int register, int value)
    {
      ref var destination = ref _registers[register];
      destination.Integer = value;
      destination.Type = ExtendedType.Integer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteLong(int register, long value)
    {
      ref var destination = ref _registers[register];
      destination.Integer = value;
      destination.Type = ExtendedType.Long;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteFloat(int register, float value)
    {
      ref var destination = ref _registers[register];
      destination.Float = value;
      destination.Type = ExtendedType.Float;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteDouble(int register, double value)
    {
      ref var destination = ref _registers[register];
      destination.Float = value;
      destination.Type = ExtendedType.Double;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteDecimal(int register, decimal value)
    {
      SetDecimal(register, value);
      _registers[register].Type = ExtendedType.Decimal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteBoolean(int register, bool value)
    {
      ref var destination = ref _registers[register];
      destination.Integer = value ? 1 : 0;
      destination.Type = ExtendedType.Boolean;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal void WriteString(int register, string value)
    {
      SetString(register, value);

      _registers[register].Type = ExtendedType.String;
    }

    // 2^96, above decimal.MaxValue; every double below it converts
    private const double DecimalLimit = 79228162514264337593543950336.0;

    // Throws the conversion's OverflowException, or returns when the value fits after all
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ConvertToDecimal(VariableValue variable) => _ = variable.DecimalValue;

    private void Bind(int variableIndex, VariableValue variable)
    {
      var bound = _boundVariables ?? (_boundVariables = new VariableValue[_variables.Length]);
      if (!ReferenceEquals(bound[variableIndex], variable))
      {
        bound[variableIndex] = variable;
      }
    }

    private void SetString(int register, string value)
    {
      var strings = _strings ?? (_strings = new string[_registers.Length]);
      if (!ReferenceEquals(strings[register], value))
      {
        strings[register] = value;
      }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SetDecimal(int register, decimal value)
    {
      (_decimals ?? AllocateDecimals())[register] = value;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private decimal[] AllocateDecimals() => _decimals = new decimal[_registers.Length];

    // The integer and binary float bits of a register, for copying a result back into a node
    internal long ReadRawBits(int register) => _registers[register].Integer;

    internal ExtendedType ReadType(int register) => _registers[register].Type;

    // Seeds a register from a node at lowering: constants, and the root's value before the first Execute
    internal void Store(int register, Node node)
    {
      ref var destination = ref _registers[register];
      var type = node.ValueType;
      switch (type)
      {
        case ExtendedType.String:
          SetString(register, node.StringValue);
          break;
        case ExtendedType.Decimal:
          SetDecimal(register, node.DecimalValue);
          break;
        default:
          destination.Integer = node.RawBits;
          break;
      }

      destination.Type = type;
    }
  }
}
