namespace EpsilonScript.AST
{
  public enum ValueType
  {
    Undefined,
    Null,
    Integer,
    Float,
    Boolean,
    Tuple,
    String
  }

  public static class ValueTypeExtension
  {
    public static bool IsNumber(this ValueType t)
    {
      return t == ValueType.Integer || t == ValueType.Float;
    }
  }
}