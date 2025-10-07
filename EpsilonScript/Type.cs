namespace EpsilonScript
{
  /// <summary>
  /// Runtime value types
  /// </summary>
  public enum Type : byte
  {
    Undefined = 0,
    // Integer types
    Integer = 1,
    Long = 2,
    // Floating-point types
    Float = 3,
    Double = 4,
    Decimal = 5,
    // Other types
    Boolean = 6,
    String = 7,
  }

  /// <summary>
  /// Internal type enum used by AST nodes.
  /// Includes all runtime types plus AST-specific types (Null, Tuple).
  /// Values 0-7 match Type exactly for zero-cost conversion.
  /// </summary>
  internal enum ExtendedType : byte
  {
    Undefined = 0,
    // Integer types
    Integer = 1,
    Long = 2,
    // Floating-point types
    Float = 3,
    Double = 4,
    Decimal = 5,
    // Other types
    Boolean = 6,
    String = 7,
    // Special AST-only types
    Null = 8,
    Tuple = 9,
  }
}