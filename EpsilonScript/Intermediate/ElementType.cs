using System;

namespace EpsilonScript.Intermediate
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
    ModuloOperator,
    Integer,
    Float,
    String,
  }

  static class ElementTypeExtensionMethods
  {
    public static bool IsValue(this ElementType type)
    {
      switch (type)
      {
        case ElementType.None:
        case ElementType.Variable:
        case ElementType.BooleanLiteralTrue:
        case ElementType.BooleanLiteralFalse:
        case ElementType.Integer:
        case ElementType.Float:
        case ElementType.String:
          return true;
        default:
          return false;
      }
    }

    public static bool IsOperator(this ElementType type)
    {
      return !IsValue(type);
    }

    public static int Precedence(this ElementType type)
    {
      switch (type)
      {
        case ElementType.Function:
        case ElementType.NegateOperator:
        case ElementType.PositiveOperator:
        case ElementType.NegativeOperator:
          return 8;
        case ElementType.MultiplyOperator:
        case ElementType.DivideOperator:
        case ElementType.ModuloOperator:
          return 7;
        case ElementType.AddOperator:
        case ElementType.SubtractOperator:
          return 6;
        case ElementType.ComparisonEqual:
        case ElementType.ComparisonNotEqual:
        case ElementType.ComparisonLessThan:
        case ElementType.ComparisonGreaterThan:
        case ElementType.ComparisonLessThanOrEqualTo:
        case ElementType.ComparisonGreaterThanOrEqualTo:
          return 5;
        case ElementType.BooleanAndOperator:
          return 4;
        case ElementType.BooleanOrOperator:
          return 3;
        case ElementType.AssignmentOperator:
        case ElementType.AssignmentAddOperator:
        case ElementType.AssignmentSubtractOperator:
        case ElementType.AssignmentMultiplyOperator:
        case ElementType.AssignmentDivideOperator:
          return 2;
        case ElementType.Comma:
          return 1;
        case ElementType.Semicolon:
          return 0;
        case ElementType.None:
        case ElementType.Variable:
        case ElementType.FunctionStartParenthesis:
        case ElementType.LeftParenthesis:
        case ElementType.RightParenthesis:
        case ElementType.BooleanLiteralTrue:
        case ElementType.BooleanLiteralFalse:
        case ElementType.Integer:
        case ElementType.Float:
        case ElementType.String:
          return -1;
        default:
          throw new ArgumentOutOfRangeException(nameof(type), type, null);
      }
    }

    public static Associativity Associativity(this ElementType type)
    {
      switch (type)
      {
        case ElementType.Function:
        case ElementType.NegateOperator:
        case ElementType.NegativeOperator:
        case ElementType.PositiveOperator:
          return Intermediate.Associativity.Right;
        case ElementType.MultiplyOperator:
        case ElementType.DivideOperator:
        case ElementType.ModuloOperator:
        case ElementType.AddOperator:
        case ElementType.SubtractOperator:
        case ElementType.ComparisonEqual:
        case ElementType.ComparisonNotEqual:
        case ElementType.ComparisonLessThan:
        case ElementType.ComparisonGreaterThan:
        case ElementType.ComparisonLessThanOrEqualTo:
        case ElementType.ComparisonGreaterThanOrEqualTo:
        case ElementType.BooleanOrOperator:
        case ElementType.BooleanAndOperator:
          return Intermediate.Associativity.Left;
        case ElementType.AssignmentOperator:
        case ElementType.AssignmentAddOperator:
        case ElementType.AssignmentSubtractOperator:
        case ElementType.AssignmentMultiplyOperator:
        case ElementType.AssignmentDivideOperator:
          return Intermediate.Associativity.Right;
        case ElementType.Comma:
        case ElementType.Semicolon:
          return Intermediate.Associativity.Left;
        case ElementType.None:
        case ElementType.Variable:
        case ElementType.FunctionStartParenthesis:
        case ElementType.LeftParenthesis:
        case ElementType.RightParenthesis:
        case ElementType.BooleanLiteralTrue:
        case ElementType.BooleanLiteralFalse:
        case ElementType.Integer:
        case ElementType.Float:
          return Intermediate.Associativity.None;
        default:
          throw new ArgumentOutOfRangeException(nameof(type), type, null);
      }
    }
  }
}