using System;

namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class DivideNode : ArithmeticOperationNode
  {
    protected override string GetOperatorName() => "division";

    protected override void CalculateInteger()
    {
      var leftInt = LeftNode.IntegerValue;
      var rightInt = RightNode.IntegerValue;
      if (rightInt == 0)
        throw new DivideByZeroException("Division by zero");
      IntegerValue = leftInt / rightInt;
    }

    protected override void CalculateLong()
    {
      var leftLong = LeftNode.LongValue;
      var rightLong = RightNode.LongValue;
      if (rightLong == 0)
        throw new DivideByZeroException("Division by zero");
      LongValue = leftLong / rightLong;
    }

    protected override void CalculateFloat()
    {
      var leftFloat = LeftNode.FloatValue;
      var rightFloat = RightNode.FloatValue;
      if (rightFloat == 0.0f)
        throw new DivideByZeroException("Division by zero");
      FloatValue = leftFloat / rightFloat;
    }

    protected override void CalculateDouble()
    {
      var leftDouble = LeftNode.DoubleValue;
      var rightDouble = RightNode.DoubleValue;
      if (rightDouble == 0.0)
        throw new DivideByZeroException("Division by zero");
      DoubleValue = leftDouble / rightDouble;
    }

    protected override void CalculateDecimal()
    {
      var leftDecimal = LeftNode.DecimalValue;
      var rightDecimal = RightNode.DecimalValue;
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

      LeftNode = LeftNode.Optimize();
      RightNode = RightNode.Optimize();
      return this;
    }
  }
}