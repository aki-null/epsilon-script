using System;
using EpsilonScript.Bytecode;

namespace EpsilonScript.VirtualMachine
{
  public partial class VirtualMachine
  {
    private static RegisterValueType GetEqualityComparisonType(RegisterValueType left, RegisterValueType right)
    {
      switch (left)
      {
        case RegisterValueType.Integer:
          if (right.IsNumber())
          {
            return right;
          }

          break;
        case RegisterValueType.Float:
          if (right.IsNumber())
          {
            return RegisterValueType.Float;
          }

          break;
        case RegisterValueType.Boolean when right == RegisterValueType.Boolean:
          return RegisterValueType.Boolean;
        case RegisterValueType.String:
        case RegisterValueType.StringStack:
          if (right == RegisterValueType.String || right == RegisterValueType.StringStack)
          {
            return RegisterValueType.String;
          }

          break;
      }

      throw new RuntimeException($"{left} cannot be compared against a {right}");
    }

    private static RegisterValueType GetComparisonType(RegisterValueType left, RegisterValueType right)
    {
      switch (left)
      {
        case RegisterValueType.Integer:
          if (right.IsNumber())
          {
            return right;
          }

          break;
        case RegisterValueType.Float:
          if (right.IsNumber())
          {
            return RegisterValueType.Float;
          }

          break;
      }

      throw new RuntimeException($"{left} cannot be compared against a {right}");
    }

    private unsafe void ComparisonEqualValue(Instruction instruction, bool flip, RegisterValue* regPtr)
    {
      var targetRegPtr = regPtr + instruction.reg0;
      var left = regPtr + instruction.reg1;
      var right = regPtr + instruction.reg2;

      var comparisonType = GetEqualityComparisonType(left->ValueType, right->ValueType);

      bool result;
      switch (comparisonType)
      {
        case RegisterValueType.Integer:
          // We can assume that integer comparison can only be done when both sides are integers
          result = left->IntegerValue == right->IntegerValue;
          break;
        case RegisterValueType.Float:
          // ReSharper disable once CompareOfFloatsByEqualityOperator
          result = left->AsFloat() == right->AsFloat();
          break;
        case RegisterValueType.Boolean:
          result = left->BooleanValue == right->BooleanValue;
          break;
        case RegisterValueType.String:
          var leftStr = left->ResolveString(_program.StringTable, _stringRegisters);
          var rightStr = right->ResolveString(_program.StringTable, _stringRegisters);
          result = leftStr.Equals(rightStr, StringComparison.Ordinal);
          break;
        default:
          throw new RuntimeException("Unexpected virtual machine error");
      }

      if (flip)
      {
        result = !result;
      }

      targetRegPtr->ValueType = RegisterValueType.Boolean;
      targetRegPtr->BooleanValue = result;
    }

    private unsafe void ComparisonLessThanValue(Instruction instruction, RegisterValue* regPtr)
    {
      var targetRegPtr = regPtr + instruction.reg0;
      var left = regPtr + instruction.reg1;
      var right = regPtr + instruction.reg2;

      var comparisonType = GetComparisonType(left->ValueType, right->ValueType);
      bool result;
      switch (comparisonType)
      {
        case RegisterValueType.Integer:
          // We can assume that integer comparison can only be done when both sides are integers
          result = left->IntegerValue < right->IntegerValue;
          break;
        case RegisterValueType.Float:
          result = left->AsFloat() < right->AsFloat();
          break;
        default:
          throw new RuntimeException("Unexpected virtual machine error");
      }

      targetRegPtr->ValueType = RegisterValueType.Boolean;
      targetRegPtr->BooleanValue = result;
    }

    private unsafe void ComparisonGreaterThanValue(Instruction instruction, RegisterValue* regPtr)
    {
      var targetRegPtr = regPtr + instruction.reg0;
      var left = regPtr + instruction.reg1;
      var right = regPtr + instruction.reg2;

      var comparisonType = GetComparisonType(left->ValueType, right->ValueType);
      bool result;
      switch (comparisonType)
      {
        case RegisterValueType.Integer:
          // We can assume that integer comparison can only be done when both sides are integers
          result = left->IntegerValue > right->IntegerValue;
          break;
        case RegisterValueType.Float:
          result = left->AsFloat() > right->AsFloat();
          break;
        default:
          throw new RuntimeException("Unexpected virtual machine error");
      }

      targetRegPtr->ValueType = RegisterValueType.Boolean;
      targetRegPtr->BooleanValue = result;
    }

    private unsafe void ComparisonLessThanOrEqualToValue(Instruction instruction, RegisterValue* regPtr)
    {
      var targetRegPtr = regPtr + instruction.reg0;
      var left = regPtr + instruction.reg1;
      var right = regPtr + instruction.reg2;

      var comparisonType = GetComparisonType(left->ValueType, right->ValueType);
      bool result;
      switch (comparisonType)
      {
        case RegisterValueType.Integer:
          // We can assume that integer comparison can only be done when both sides are integers
          result = left->IntegerValue <= right->IntegerValue;
          break;
        case RegisterValueType.Float:
          result = left->AsFloat() <= right->AsFloat();
          break;
        default:
          throw new RuntimeException("Unexpected virtual machine error");
      }

      targetRegPtr->ValueType = RegisterValueType.Boolean;
      targetRegPtr->BooleanValue = result;
    }

    private unsafe void ComparisonGreaterThanOrEqualToValue(Instruction instruction, RegisterValue* regPtr)
    {
      var targetRegPtr = regPtr + instruction.reg0;
      var left = regPtr + instruction.reg1;
      var right = regPtr + instruction.reg2;

      var comparisonType = GetComparisonType(left->ValueType, right->ValueType);
      bool result;
      switch (comparisonType)
      {
        case RegisterValueType.Integer:
          // We can assume that integer comparison can only be done when both sides are integers
          result = left->IntegerValue >= right->IntegerValue;
          break;
        case RegisterValueType.Float:
          result = left->AsFloat() >= right->AsFloat();
          break;
        default:
          throw new RuntimeException("Unexpected virtual machine error");
      }

      targetRegPtr->ValueType = RegisterValueType.Boolean;
      targetRegPtr->BooleanValue = result;
    }
  }
}