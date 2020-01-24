using System;

namespace EpsilonScript.Parser
{
  public enum ElementType
  {
    None,
    Variable,
    Function,
    FunctionStartParenthesis,
    LeftParenthesis,
    RightParenthesis,
    Comma,
    Semicolon,
    ComparisonEqual,
    ComparisonNotEqual,
    ComparisonLessThan,
    ComparisonGreaterThan,
    ComparisonLessThanOrEqualTo,
    ComparisonGreaterThanOrEqualTo,
    NegateOperator,
    PositiveOperator,
    NegativeOperator,
    BooleanOrOperator,
    BooleanAndOperator,
    BooleanLiteralTrue,
    BooleanLiteralFalse,
    AssignmentOperator,
    AssignmentAddOperator,
    AssignmentSubtractOperator,
    AssignmentMultiplyOperator,
    AssignmentDivideOperator,
    AddOperator,
    SubtractOperator,
    MultiplyOperator,
    DivideOperator,
    Integer,
    Float,
  }

  static class ElementTypeExtensionMethods
  {
    public static bool IsValue(this ElementType type)
    {
      return type switch
      {
        ElementType.None => true,
        ElementType.Variable => true,
        ElementType.BooleanLiteralTrue => true,
        ElementType.BooleanLiteralFalse => true,
        ElementType.Integer => true,
        ElementType.Float => true,
        _ => false
      };
    }

    public static bool IsOperator(this ElementType type)
    {
      return !IsValue(type);
    }

    public static int Precedence(this ElementType type)
    {
      return type switch
      {
        ElementType.Function => 8,
        ElementType.NegateOperator => 8,
        ElementType.PositiveOperator => 8,
        ElementType.NegativeOperator => 8,
        ElementType.MultiplyOperator => 7,
        ElementType.DivideOperator => 7,
        ElementType.AddOperator => 6,
        ElementType.SubtractOperator => 6,
        ElementType.ComparisonEqual => 5,
        ElementType.ComparisonNotEqual => 5,
        ElementType.ComparisonLessThan => 5,
        ElementType.ComparisonGreaterThan => 5,
        ElementType.ComparisonLessThanOrEqualTo => 5,
        ElementType.ComparisonGreaterThanOrEqualTo => 5,
        ElementType.BooleanAndOperator => 4,
        ElementType.BooleanOrOperator => 3,
        ElementType.AssignmentOperator => 2,
        ElementType.AssignmentAddOperator => 2,
        ElementType.AssignmentSubtractOperator => 2,
        ElementType.AssignmentMultiplyOperator => 2,
        ElementType.AssignmentDivideOperator => 2,
        ElementType.Comma => 1,
        ElementType.Semicolon => 0,
        ElementType.None => -1,
        ElementType.Variable => -1,
        ElementType.FunctionStartParenthesis => -1,
        ElementType.LeftParenthesis => -1,
        ElementType.RightParenthesis => -1,
        ElementType.BooleanLiteralTrue => -1,
        ElementType.BooleanLiteralFalse => -1,
        ElementType.Integer => -1,
        ElementType.Float => -1,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };
    }

    public static Associativity Associativity(this ElementType type)
    {
      return type switch
      {
        // Operators
        ElementType.Function => Parser.Associativity.Right,
        ElementType.NegateOperator => Parser.Associativity.Right,
        ElementType.NegativeOperator => Parser.Associativity.Right,
        ElementType.PositiveOperator => Parser.Associativity.Right,
        ElementType.MultiplyOperator => Parser.Associativity.Left,
        ElementType.DivideOperator => Parser.Associativity.Left,
        ElementType.AddOperator => Parser.Associativity.Left,
        ElementType.SubtractOperator => Parser.Associativity.Left,
        ElementType.ComparisonEqual => Parser.Associativity.Left,
        ElementType.ComparisonNotEqual => Parser.Associativity.Left,
        ElementType.ComparisonLessThan => Parser.Associativity.Left,
        ElementType.ComparisonGreaterThan => Parser.Associativity.Left,
        ElementType.ComparisonLessThanOrEqualTo => Parser.Associativity.Left,
        ElementType.ComparisonGreaterThanOrEqualTo => Parser.Associativity.Left,
        ElementType.BooleanOrOperator => Parser.Associativity.Left,
        ElementType.BooleanAndOperator => Parser.Associativity.Left,
        ElementType.AssignmentOperator => Parser.Associativity.Right,
        ElementType.AssignmentAddOperator => Parser.Associativity.Right,
        ElementType.AssignmentSubtractOperator => Parser.Associativity.Right,
        ElementType.AssignmentMultiplyOperator => Parser.Associativity.Right,
        ElementType.AssignmentDivideOperator => Parser.Associativity.Right,
        ElementType.Comma => Parser.Associativity.Left,
        ElementType.Semicolon => Parser.Associativity.Left,
        // Non-operators
        ElementType.None => Parser.Associativity.None,
        ElementType.Variable => Parser.Associativity.None,
        ElementType.FunctionStartParenthesis => Parser.Associativity.None,
        ElementType.LeftParenthesis => Parser.Associativity.None,
        ElementType.RightParenthesis => Parser.Associativity.None,
        ElementType.BooleanLiteralTrue => Parser.Associativity.None,
        ElementType.BooleanLiteralFalse => Parser.Associativity.None,
        ElementType.Integer => Parser.Associativity.None,
        ElementType.Float => Parser.Associativity.None,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
      };
    }
  }
}