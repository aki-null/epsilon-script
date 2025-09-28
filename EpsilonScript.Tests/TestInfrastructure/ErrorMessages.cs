namespace EpsilonScript.Tests.TestInfrastructure
{
  public static class ErrorMessages
  {
    // Parser errors
    public const string AssignmentOperatorCannotBeUsedForImmutableScript =
      "assignment operator cannot be used for an immutable script";

    // Runtime errors
    public const string ArithmeticOperationOnlyOnNumericValue =
      "arithmetic operation can only be performed on a numeric value";

    public const string FloatValueCannotBeAssignedToBooleanVariable =
      "float value cannot be assigned to a boolean variable";

    public const string LeftHandSideMustBeVariable = "left hand side of an assignment operator must be a variable";
  }
}