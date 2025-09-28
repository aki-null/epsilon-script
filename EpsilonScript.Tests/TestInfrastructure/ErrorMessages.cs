namespace EpsilonScript.Tests.TestInfrastructure
{
  public static class ErrorMessages
  {
    // Parser errors
    public const string CannotFindValuesForArithmeticOperation = "Cannot find values to perform arithmetic operation on";
    public const string CannotFindValuesForBooleanOperation = "Cannot find values to perform a boolean operation on";
    public const string CannotFindParametersForFunction = "Cannot find parameters for calling function";
    public const string CannotFindTokensToSequence = "Cannot find tokens to sequence";
    public const string CannotFindValueForNegateOperation = "Cannot find value to perform negate operation on";
    public const string UndefinedFunction = "Undefined function";
    public const string AssignmentOperatorCannotBeUsedForImmutableScript = "assignment operator cannot be used for an immutable script";

    // Runtime errors
    public const string ArithmeticOperationOnlyOnNumericValues = "An arithmetic operation can only be performed on numeric values";
    public const string ArithmeticOperationOnlyOnNumericValue = "arithmetic operation can only be performed on a numeric value";
    public const string BooleanOperationOnlyOnBooleanValues = "Boolean operation can only be performed on boolean values";
    public const string CannotNegateNonBooleanValue = "Cannot negate a non-boolean value";
    public const string FloatValueCannotBeAssignedToBooleanVariable = "float value cannot be assigned to a boolean variable";
    public const string FunctionWithGivenTypeSignatureUndefined = "function with given type signature is undefined";
    public const string LeftHandSideMustBeVariable = "left hand side of an assignment operator must be a variable";
    public const string StringOperationsOnlySupportConcatenation = "String operations only support concatenation (+), not";
    public const string BooleanValuesCannotBeUsedInArithmeticOperations = "Boolean values cannot be used in arithmetic operations";
    public const string UndefinedVariable = "Undefined variable";
    public const string CannotFindValuesForArithmeticComparisonOnNonNumericTypes = "Cannot find values to perform arithmetic comparision on non numeric types";

    // System errors
    public const string ValueTooLargeOrSmallForInt32 = "Value was either too large or too small for an Int32";
  }
}