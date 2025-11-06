using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.AST.Arithmetic;
using EpsilonScript.AST.Assignment;
using EpsilonScript.AST.Boolean;
using EpsilonScript.AST.Comparison;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Tests.TestInfrastructure
{
  public abstract class AstTestBase
  {
    internal static Stack<Node> CreateStack(params Node[] nodes)
    {
      var stack = new Stack<Node>();
      foreach (var node in nodes)
      {
        stack.Push(node);
      }

      return stack;
    }

    /// <summary>
    /// Factory method to create arithmetic operation nodes based on element type.
    /// Used by tests to instantiate the correct node type for arithmetic operations.
    /// </summary>
    internal static ArithmeticOperationNode CreateArithmeticNode(ElementType elementType)
    {
      return elementType switch
      {
        ElementType.AddOperator => new AddNode(),
        ElementType.SubtractOperator => new SubtractNode(),
        ElementType.MultiplyOperator => new MultiplyNode(),
        ElementType.DivideOperator => new DivideNode(),
        ElementType.ModuloOperator => new ModuloNode(),
        _ => throw new ArgumentException($"ElementType {elementType} is not an arithmetic operator",
          nameof(elementType))
      };
    }

    /// <summary>
    /// Factory method to create arithmetic operation nodes based on operator symbol string.
    /// Convenience overload for tests that have operator symbols ("+", "-", "*", "/", "%").
    /// </summary>
    internal static ArithmeticOperationNode CreateArithmeticNode(string operatorSymbol)
    {
      return operatorSymbol switch
      {
        "+" => new AddNode(),
        "-" => new SubtractNode(),
        "*" => new MultiplyNode(),
        "/" => new DivideNode(),
        "%" => new ModuloNode(),
        _ => throw new ArgumentException($"Operator symbol '{operatorSymbol}' is not a recognized arithmetic operator",
          nameof(operatorSymbol))
      };
    }

    /// <summary>
    /// Factory method to create assignment operation nodes based on element type.
    /// Used by tests to instantiate the correct node type for assignment operations.
    /// </summary>
    internal static AssignmentOperationNode CreateAssignmentNode(ElementType elementType)
    {
      return elementType switch
      {
        ElementType.AssignmentOperator => new DirectAssignmentNode(),
        ElementType.AssignmentAddOperator => new AddAssignmentNode(),
        ElementType.AssignmentSubtractOperator => new SubtractAssignmentNode(),
        ElementType.AssignmentMultiplyOperator => new MultiplyAssignmentNode(),
        ElementType.AssignmentDivideOperator => new DivideAssignmentNode(),
        ElementType.AssignmentModuloOperator => new ModuloAssignmentNode(),
        _ => throw new ArgumentException($"ElementType {elementType} is not an assignment operator",
          nameof(elementType))
      };
    }

    /// <summary>
    /// Factory method to create assignment operation nodes based on operator symbol string.
    /// Convenience overload for tests that have operator symbols ("=", "+=", "-=", "*=", "/=", "%=").
    /// </summary>
    internal static AssignmentOperationNode CreateAssignmentNode(string operatorSymbol)
    {
      return operatorSymbol switch
      {
        "=" => new DirectAssignmentNode(),
        "+=" => new AddAssignmentNode(),
        "-=" => new SubtractAssignmentNode(),
        "*=" => new MultiplyAssignmentNode(),
        "/=" => new DivideAssignmentNode(),
        "%=" => new ModuloAssignmentNode(),
        _ => throw new ArgumentException($"Operator symbol '{operatorSymbol}' is not a recognized assignment operator",
          nameof(operatorSymbol))
      };
    }

    /// <summary>
    /// Factory method to create comparison operation nodes based on element type.
    /// Used by tests to instantiate the correct node type for comparison operations.
    /// </summary>
    internal static ComparisonOperationNode CreateComparisonNode(ElementType elementType)
    {
      return elementType switch
      {
        ElementType.ComparisonEqual => new EqualComparisonNode(),
        ElementType.ComparisonNotEqual => new NotEqualComparisonNode(),
        ElementType.ComparisonLessThan => new LessThanComparisonNode(),
        ElementType.ComparisonGreaterThan => new GreaterThanComparisonNode(),
        ElementType.ComparisonLessThanOrEqualTo => new LessThanOrEqualComparisonNode(),
        ElementType.ComparisonGreaterThanOrEqualTo => new GreaterThanOrEqualComparisonNode(),
        _ => throw new ArgumentException($"ElementType {elementType} is not a comparison operator",
          nameof(elementType))
      };
    }

    /// <summary>
    /// Factory method to create comparison operation nodes based on operator symbol string.
    /// Convenience overload for tests that have operator symbols ("==", "!=", "<", ">", "<=", ">=").
    /// </summary>
    internal static ComparisonOperationNode CreateComparisonNode(string operatorSymbol)
    {
      return operatorSymbol switch
      {
        "==" => new EqualComparisonNode(),
        "!=" => new NotEqualComparisonNode(),
        "<" => new LessThanComparisonNode(),
        ">" => new GreaterThanComparisonNode(),
        "<=" => new LessThanOrEqualComparisonNode(),
        ">=" => new GreaterThanOrEqualComparisonNode(),
        _ => throw new ArgumentException($"Operator symbol '{operatorSymbol}' is not a recognized comparison operator",
          nameof(operatorSymbol))
      };
    }

    /// <summary>
    /// Factory method to create boolean operation nodes based on element type.
    /// Used by tests to instantiate the correct node type for boolean operations.
    /// </summary>
    internal static BooleanOperationNode CreateBooleanOperationNode(ElementType elementType)
    {
      return elementType switch
      {
        ElementType.BooleanAndOperator => new BooleanAndNode(),
        ElementType.BooleanOrOperator => new BooleanOrNode(),
        _ => throw new ArgumentException($"ElementType {elementType} is not a boolean operator",
          nameof(elementType))
      };
    }

    /// <summary>
    /// Factory method to create boolean operation nodes based on operator symbol string.
    /// Convenience overload for tests that have operator symbols ("&&", "||").
    /// </summary>
    internal static BooleanOperationNode CreateBooleanOperationNode(string operatorSymbol)
    {
      return operatorSymbol switch
      {
        "&&" => new BooleanAndNode(),
        "||" => new BooleanOrNode(),
        _ => throw new ArgumentException($"Operator symbol '{operatorSymbol}' is not a recognized boolean operator",
          nameof(operatorSymbol))
      };
    }
  }
}