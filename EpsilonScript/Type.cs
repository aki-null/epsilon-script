namespace EpsilonScript
{
  /// <summary>
  /// Runtime value types
  /// </summary>
  /// <remarks>
  /// IMPORTANT: Enum indices are NON-SEQUENTIAL due to serialization compatibility.
  /// These values are serialized in existing projects and CANNOT be changed.
  /// New types must be added with new indices without reordering existing ones.
  /// </remarks>
  public enum Type : byte
  {
    Undefined = 0,

    // Integer types
    Integer = 1,
    Long = 5,

    // Floating-point types
    Float = 2,
    Double = 6,
    Decimal = 7,

    // Other types
    Boolean = 3,
    String = 4,
  }

  /// <summary>
  /// Internal type enum used by AST nodes.
  /// Includes all runtime types plus AST-specific types (Null, Tuple).
  /// Values 0-7 match Type exactly for zero-cost conversion.
  /// </summary>
  /// <remarks>
  /// IMPORTANT: Values 0-7 MUST match Type enum exactly for zero-cost conversion.
  /// These indices are non-sequential due to serialization compatibility constraints
  /// inherited from the Type enum.
  /// </remarks>
  internal enum ExtendedType : byte
  {
    Undefined = 0,

    // Integer types
    Integer = 1,
    Long = 5,

    // Floating-point types
    Float = 2,
    Double = 6,
    Decimal = 7,

    // Other types
    Boolean = 3,
    String = 4,

    // Special AST-only types
    Null = 8,
    Tuple = 9,
  }
}