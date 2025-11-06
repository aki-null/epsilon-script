using System;

namespace EpsilonScript.AST.Arithmetic
{
  internal sealed class ModuloNode : ArithmeticOperationNode
  {
    protected override string GetOperatorName() => "modulo";

    protected override void CalculateInteger()
    {
      var leftInt = LeftNode.IntegerValue;
      var rightInt = RightNode.IntegerValue;
      if (rightInt == 0)
        throw new DivideByZeroException("Modulo by zero");
      IntegerValue = leftInt % rightInt;
    }

    protected override void CalculateLong()
    {
      var leftLong = LeftNode.LongValue;
      var rightLong = RightNode.LongValue;
      if (rightLong == 0)
        throw new DivideByZeroException("Modulo by zero");
      LongValue = leftLong % rightLong;
    }

    protected override void CalculateFloat()
    {
      var leftFloat = LeftNode.FloatValue;
      var rightFloat = RightNode.FloatValue;
      if (rightFloat == 0.0f)
        throw new DivideByZeroException("Modulo by zero");
      FloatValue = leftFloat % rightFloat;
    }

    protected override void CalculateDouble()
    {
      var leftDouble = LeftNode.DoubleValue;
      var rightDouble = RightNode.DoubleValue;
      if (rightDouble == 0.0)
        throw new DivideByZeroException("Modulo by zero");
      DoubleValue = leftDouble % rightDouble;
    }

    protected override void CalculateDecimal()
    {
      var leftDecimal = LeftNode.DecimalValue;
      var rightDecimal = RightNode.DecimalValue;
      if (rightDecimal == 0m)
        throw new DivideByZeroException("Modulo by zero");
      DecimalValue = leftDecimal % rightDecimal;
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