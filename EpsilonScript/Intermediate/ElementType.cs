using System;

namespace EpsilonScript.Intermediate
{
  internal enum ElementType
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
    private const ulong ValueMask =
      (1UL << (int)ElementType.Variable) |
      (1UL << (int)ElementType.BooleanLiteralTrue) |
      (1UL << (int)ElementType.BooleanLiteralFalse) |
      (1UL << (int)ElementType.Integer) |
      (1UL << (int)ElementType.Float) |
      (1UL << (int)ElementType.String);

    private const ulong OperatorMask =
      (1UL << (int)ElementType.Function) |
      (1UL << (int)ElementType.NegateOperator) |
      (1UL << (int)ElementType.PositiveOperator) |
      (1UL << (int)ElementType.NegativeOperator) |
      (1UL << (int)ElementType.AddOperator) |
      (1UL << (int)ElementType.SubtractOperator) |
      (1UL << (int)ElementType.MultiplyOperator) |
      (1UL << (int)ElementType.DivideOperator) |
      (1UL << (int)ElementType.ModuloOperator) |
      (1UL << (int)ElementType.ComparisonEqual) |
      (1UL << (int)ElementType.ComparisonNotEqual) |
      (1UL << (int)ElementType.ComparisonLessThan) |
      (1UL << (int)ElementType.ComparisonGreaterThan) |
      (1UL << (int)ElementType.ComparisonLessThanOrEqualTo) |
      (1UL << (int)ElementType.ComparisonGreaterThanOrEqualTo) |
      (1UL << (int)ElementType.BooleanOrOperator) |
      (1UL << (int)ElementType.BooleanAndOperator) |
      (1UL << (int)ElementType.AssignmentOperator) |
      (1UL << (int)ElementType.AssignmentAddOperator) |
      (1UL << (int)ElementType.AssignmentSubtractOperator) |
      (1UL << (int)ElementType.AssignmentMultiplyOperator) |
      (1UL << (int)ElementType.AssignmentDivideOperator) |
      (1UL << (int)ElementType.Comma) |
      (1UL << (int)ElementType.Semicolon);

    // Binary operators: excludes unary operators (Negate, Positive, Negative),
    // function calls, and sequence operators (Comma, Semicolon)
    private const ulong BinaryOperatorMask =
      (1UL << (int)ElementType.AddOperator) |
      (1UL << (int)ElementType.SubtractOperator) |
      (1UL << (int)ElementType.MultiplyOperator) |
      (1UL << (int)ElementType.DivideOperator) |
      (1UL << (int)ElementType.ModuloOperator) |
      (1UL << (int)ElementType.ComparisonEqual) |
      (1UL << (int)ElementType.ComparisonNotEqual) |
      (1UL << (int)ElementType.ComparisonLessThan) |
      (1UL << (int)ElementType.ComparisonGreaterThan) |
      (1UL << (int)ElementType.ComparisonLessThanOrEqualTo) |
      (1UL << (int)ElementType.ComparisonGreaterThanOrEqualTo) |
      (1UL << (int)ElementType.BooleanOrOperator) |
      (1UL << (int)ElementType.BooleanAndOperator) |
      (1UL << (int)ElementType.AssignmentOperator) |
      (1UL << (int)ElementType.AssignmentAddOperator) |
      (1UL << (int)ElementType.AssignmentSubtractOperator) |
      (1UL << (int)ElementType.AssignmentMultiplyOperator) |
      (1UL << (int)ElementType.AssignmentDivideOperator);

    /// <summary>
    /// Static constructor to validate that all ElementType enum values fit within 64-bit masks.
    /// This prevents silent failures if enum values exceed 63 (the maximum bit position for ulong).
    /// </summary>
    static ElementTypeExtensionMethods()
    {
      // Verify all enum values fit in ulong bit mask (0-63)
      foreach (ElementType value in Enum.GetValues(typeof(ElementType)))
      {
        int intValue = (int)value;
        if (intValue < 0 || intValue >= 64)
        {
          throw new InvalidOperationException(
            $"ElementType.{value} has value {intValue}, which exceeds bit mask capacity (0-63). " +
            "The bit mask optimization in ElementTypeExtensionMethods requires all enum values to be in range 0-63.");
        }
      }
    }

    /// <summary>
    /// Returns true if the element type represents an actual value (literal or variable).
    /// Structural elements like None, parentheses, and commas are not considered values.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsValue(this ElementType type)
    {
      return ((ValueMask >> (int)type) & 1) != 0;
    }

    /// <summary>
    /// Returns true if the element type represents an operator.
    /// This includes unary operators, binary operators, assignment operators, function calls,
    /// and sequence operators (comma, semicolon).
    /// All these elements have defined precedence and are treated as operators during expression parsing.
    /// Only parentheses, values, and None are not operators.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsOperator(this ElementType type)
    {
      return ((OperatorMask >> (int)type) & 1) != 0;
    }

    /// <summary>
    /// Returns true if the element type represents a binary operator.
    /// Excludes unary operators (Negate, Positive, Negative), function calls,
    /// and sequence operators (Comma, Semicolon).
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    public static bool IsBinaryOperator(this ElementType type)
    {
      return ((BinaryOperatorMask >> (int)type) & 1) != 0;
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