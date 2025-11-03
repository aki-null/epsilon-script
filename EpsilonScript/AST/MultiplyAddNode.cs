using System;
using System.Collections.Generic;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  /// <summary>
  /// Multiply-Add node: computes (a * b) ± c in a single operation.
  /// This optimization reduces Execute() calls and tree traversal overhead.
  /// Supports: (a*b)+c, (a*b)-c, c-(a*b)
  /// </summary>
  internal class MultiplyAddNode : Node
  {
    internal enum OperationMode
    {
      MultiplyAdd, // (a * b) + c
      AddMultiply, // c + (a * b)
      MultiplySubtract, // (a * b) - c
      SubtractMultiply // c - (a * b)
    }

    private Node _multiplier1; // a in (a * b) ± c
    private Node _multiplier2; // b in (a * b) ± c
    private Node _addend; // c in (a * b) ± c
    private readonly Type _configuredIntegerType;
    private readonly Type _configuredFloatType;
    private readonly OperationMode _mode;
    private bool _noAllocMode;

    public override bool IsPrecomputable =>
      _multiplier1.IsPrecomputable && _multiplier2.IsPrecomputable && _addend.IsPrecomputable;

    /// <summary>
    /// Constructor used during optimization phase to create multiply-add node
    /// </summary>
    public MultiplyAddNode(Node multiplier1, Node multiplier2, Node addend,
      Type configuredIntegerType, Type configuredFloatType, OperationMode mode)
    {
      _multiplier1 = multiplier1;
      _multiplier2 = multiplier2;
      _addend = addend;
      _configuredIntegerType = configuredIntegerType;
      _configuredFloatType = configuredFloatType;
      _mode = mode;
    }

    public override void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      IVariableContainer variables, IDictionary<VariableId, CustomFunctionOverload> functions,
      Compiler.IntegerPrecision intPrecision, Compiler.FloatPrecision floatPrecision)
    {
      // This node is only created during optimization, never during initial AST build
      throw new InvalidOperationException("MultiplyAddNode cannot be built from RPN stack");
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private ExtendedType PromoteType(ExtendedType left, ExtendedType right)
    {
      if (left == ExtendedType.String || right == ExtendedType.String)
        return ExtendedType.String;

      if (left == (ExtendedType)_configuredFloatType || right == (ExtendedType)_configuredFloatType)
        return (ExtendedType)_configuredFloatType;

      return (ExtendedType)_configuredIntegerType;
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      // Execute all operands once
      _multiplier1.Execute(variablesOverride);
      _multiplier2.Execute(variablesOverride);
      _addend.Execute(variablesOverride);

      // Check for boolean operands upfront
      if (_multiplier1.ValueType == ExtendedType.Boolean || _multiplier2.ValueType == ExtendedType.Boolean ||
          _addend.ValueType == ExtendedType.Boolean)
      {
        throw new RuntimeException("Boolean values cannot be used in arithmetic operations");
      }

      // Validate types - matches original ArithmeticNode behavior
      // Multiply must be numeric * numeric (strings not supported in multiply)
      if (!_multiplier1.IsNumeric || !_multiplier2.IsNumeric)
      {
        throw new RuntimeException("Multiply operation can only be performed on numeric values");
      }

      // For add/subtract: addend can be string only if mode is Add and addend is on the LEFT
      // This matches original ArithmeticNode behavior: "string + X" is allowed, "X + string" is not
      if (!_addend.IsNumeric && _addend.ValueType != ExtendedType.String)
      {
        throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
      }

      // String validation: strings only allowed in AddMultiply pattern (c + (a*b))
      if (_addend.ValueType == ExtendedType.String)
      {
        // Only AddMultiply pattern allows strings (string on left side)
        // This matches original ArithmeticNode: "string + X" is allowed, "X + string" is not
        if (_mode != OperationMode.AddMultiply)
        {
          if (_mode == OperationMode.MultiplySubtract || _mode == OperationMode.SubtractMultiply)
          {
            throw new RuntimeException("String operations only support concatenation (+), not subtraction");
          }
          else // MultiplyAdd
          {
            throw new RuntimeException("An arithmetic operation can only be performed on numeric values");
          }
        }
      }

      // Determine result type: promote through multiply first, then add
      var multiplyType = PromoteType(_multiplier1.ValueType, _multiplier2.ValueType);
      ValueType = PromoteType(multiplyType, _addend.ValueType);

      // Perform multiply-add/subtract operation
      switch (ValueType)
      {
        case ExtendedType.Integer:
          var m1Int = _multiplier1.IntegerValue;
          var m2Int = _multiplier2.IntegerValue;
          var addInt = _addend.IntegerValue;
          var productInt = m1Int * m2Int;
          IntegerValue = _mode switch
          {
            OperationMode.MultiplyAdd => productInt + addInt,
            OperationMode.AddMultiply => productInt + addInt,
            OperationMode.MultiplySubtract => productInt - addInt,
            OperationMode.SubtractMultiply => addInt - productInt,
            _ => throw new ArgumentOutOfRangeException(nameof(_mode), _mode, "Unsupported operation mode")
          };
          break;

        case ExtendedType.Long:
          var m1Long = _multiplier1.LongValue;
          var m2Long = _multiplier2.LongValue;
          var addLong = _addend.LongValue;
          var productLong = m1Long * m2Long;
          LongValue = _mode switch
          {
            OperationMode.MultiplyAdd => productLong + addLong,
            OperationMode.AddMultiply => productLong + addLong,
            OperationMode.MultiplySubtract => productLong - addLong,
            OperationMode.SubtractMultiply => addLong - productLong,
            _ => throw new ArgumentOutOfRangeException(nameof(_mode), _mode, "Unsupported operation mode")
          };
          break;

        case ExtendedType.Float:
          var m1Float = _multiplier1.FloatValue;
          var m2Float = _multiplier2.FloatValue;
          var addFloat = _addend.FloatValue;
          var productFloat = m1Float * m2Float;
          FloatValue = _mode switch
          {
            OperationMode.MultiplyAdd => productFloat + addFloat,
            OperationMode.AddMultiply => productFloat + addFloat,
            OperationMode.MultiplySubtract => productFloat - addFloat,
            OperationMode.SubtractMultiply => addFloat - productFloat,
            _ => throw new ArgumentOutOfRangeException(nameof(_mode), _mode, "Unsupported operation mode")
          };
          break;

        case ExtendedType.Double:
          var m1Double = _multiplier1.DoubleValue;
          var m2Double = _multiplier2.DoubleValue;
          var addDouble = _addend.DoubleValue;
          var productDouble = m1Double * m2Double;
          DoubleValue = _mode switch
          {
            OperationMode.MultiplyAdd => productDouble + addDouble,
            OperationMode.AddMultiply => productDouble + addDouble,
            OperationMode.MultiplySubtract => productDouble - addDouble,
            OperationMode.SubtractMultiply => addDouble - productDouble,
            _ => throw new ArgumentOutOfRangeException(nameof(_mode), _mode, "Unsupported operation mode")
          };
          break;

        case ExtendedType.Decimal:
          var m1Decimal = _multiplier1.DecimalValue;
          var m2Decimal = _multiplier2.DecimalValue;
          var addDecimal = _addend.DecimalValue;
          var productDecimal = m1Decimal * m2Decimal;
          DecimalValue = _mode switch
          {
            OperationMode.MultiplyAdd => productDecimal + addDecimal,
            OperationMode.AddMultiply => productDecimal + addDecimal,
            OperationMode.MultiplySubtract => productDecimal - addDecimal,
            OperationMode.SubtractMultiply => addDecimal - productDecimal,
            _ => throw new ArgumentOutOfRangeException(nameof(_mode), _mode, "Unsupported operation mode")
          };
          break;

        case ExtendedType.String:
          // String concatenation: addend is string, multiply result is numeric
          // Only AddMultiply pattern: string + (numeric * numeric)
          // This maintains compatibility with ArithmeticNode: only "string + X" is allowed, not "X + string"
          if (_mode == OperationMode.AddMultiply && _addend.ValueType == ExtendedType.String)
          {
            // NoAlloc validation: Block runtime string concatenation
            if (_noAllocMode)
            {
              throw new RuntimeException(
                "String concatenation is not allowed in NoAlloc mode (causes runtime heap allocation)");
            }

            // Compute product based on multiply type, then format to string
            switch (multiplyType)
            {
              case ExtendedType.Integer:
                var strProductInt = _multiplier1.IntegerValue * _multiplier2.IntegerValue;
                StringValue = $"{_addend.StringValue}{strProductInt}";
                break;

              case ExtendedType.Long:
                var strProductLong = _multiplier1.LongValue * _multiplier2.LongValue;
                StringValue = $"{_addend.StringValue}{strProductLong}";
                break;

              case ExtendedType.Float:
                var strProductFloat = _multiplier1.FloatValue * _multiplier2.FloatValue;
                StringValue = FormattableString.Invariant($"{_addend.StringValue}{strProductFloat}");
                break;

              case ExtendedType.Double:
                var strProductDouble = _multiplier1.DoubleValue * _multiplier2.DoubleValue;
                StringValue = FormattableString.Invariant($"{_addend.StringValue}{strProductDouble}");
                break;

              case ExtendedType.Decimal:
                var strProductDecimal = _multiplier1.DecimalValue * _multiplier2.DecimalValue;
                StringValue = FormattableString.Invariant($"{_addend.StringValue}{strProductDecimal}");
                break;

              default:
                throw new ArgumentOutOfRangeException(nameof(multiplyType), multiplyType, "Unsupported multiply type");
            }
          }
          else
          {
            throw new RuntimeException("String operations only support concatenation with string on left side");
          }

          break;

        default:
          throw new ArgumentOutOfRangeException(nameof(ValueType), ValueType, "Unsupported value type");
      }
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      _multiplier1 = _multiplier1.Optimize();
      _multiplier2 = _multiplier2.Optimize();
      _addend = _addend.Optimize();

      return this;
    }

    public override void ConfigureNoAlloc()
    {
      _noAllocMode = true;
      _multiplier1?.ConfigureNoAlloc();
      _multiplier2?.ConfigureNoAlloc();
      _addend?.ConfigureNoAlloc();
    }
  }
}