using System;

namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class DivideNode : ArithmeticOperationNode
  {
    protected override string GetOperatorName() => "division";

    protected override void CalculateInteger()
    {
      var leftInt = _leftNode.IntegerValue;
      var rightInt = _rightNode.IntegerValue;
      if (rightInt == 0)
        throw new DivideByZeroException("Division by zero");
      IntegerValue = leftInt / rightInt;
    }

    protected override void CalculateLong()
    {
      var leftLong = _leftNode.LongValue;
      var rightLong = _rightNode.LongValue;
      if (rightLong == 0)
        throw new DivideByZeroException("Division by zero");
      LongValue = leftLong / rightLong;
    }

    protected override void CalculateFloat()
    {
      var leftFloat = _leftNode.FloatValue;
      var rightFloat = _rightNode.FloatValue;
      if (rightFloat == 0.0f)
        throw new DivideByZeroException("Division by zero");
      FloatValue = leftFloat / rightFloat;
    }

    protected override void CalculateDouble()
    {
      var leftDouble = _leftNode.DoubleValue;
      var rightDouble = _rightNode.DoubleValue;
      if (rightDouble == 0.0)
        throw new DivideByZeroException("Division by zero");
      DoubleValue = leftDouble / rightDouble;
    }

    protected override void CalculateDecimal()
    {
      var leftDecimal = _leftNode.DecimalValue;
      var rightDecimal = _rightNode.DecimalValue;
      if (rightDecimal == 0m)
        throw new DivideByZeroException("Division by zero");
      DecimalValue = leftDecimal / rightDecimal;
    }

    public override Node Optimize()
    {
      if (IsPrecomputable)
      {
        Execute(null);
        return CreateValueNode();
      }

      _leftNode = _leftNode.Optimize();
      _rightNode = _rightNode.Optimize();
      return this;
    }
  }
}