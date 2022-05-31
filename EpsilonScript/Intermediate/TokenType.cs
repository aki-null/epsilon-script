namespace EpsilonScript.Intermediate
{
  public enum TokenType
  {
    None,
    Identifier,
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
    BooleanOrOperator,
    BooleanAndOperator,
    BooleanLiteralTrue,
    BooleanLiteralFalse,
    AssignmentOperator,
    AssignmentAddOperator,
    AssignmentSubtractOperator,
    AssignmentMultiplyOperator,
    AssignmentDivideOperator,
    PlusSign,
    MinusSign,
    MultiplyOperator,
    DivideOperator,
    ModuloOperator,
    Integer,
    Float,
  }
}