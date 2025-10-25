# Changelog

## [1.4.0] - 2025-XX-XX

### Added
- **Contextual Custom Functions**: Functions can read variables from execution environment without explicit parameters
  - `CustomFunction.CreateContextual()` factory methods for creating context-aware functions
  - Supports up to 3 context variables and 3 script parameters

### Changed
- **Function Resolution Caching**: Function overload lookups now cached per execution for improved performance
  - Cached using packed parameter types for fast comparison
  - Cache invalidated when parameter type signature changes
- **Multiply-Add Optimization**: Improved performance for expressions matching pattern `(a*b)±c`

### Fixed
- **Type Enum Serialization Compatibility**: Restored pre-1.3.0 enum indices for Integer/Float/Boolean/String
  - Version 1.3.0 broke serialization by reordering existing types

## [1.3.0] - 2025-10-07

### Added
- **Configurable Numeric Precision**: New `Compiler(IntegerPrecision, FloatPrecision)` constructor
  - Integer: `Integer` (32-bit, default) or `Long` (64-bit)
  - Float: `Float` (32-bit, default), `Double` (64-bit), or `Decimal` (128-bit)
- **CompiledScript API Expansion**: New properties for accessing values at different precisions
  - Added `LongValue`, `DoubleValue`, `DecimalValue` properties
  - Added `IntegerPrecision` and `FloatPrecision` properties to query compiler configuration
- **VariableValue API Expansion**: Support for all numeric precision types
  - New constructors: `VariableValue(long)`, `VariableValue(double)`, `VariableValue(decimal)`
  - New properties: `LongValue`, `DoubleValue`, `DecimalValue` with automatic type conversion
- **Precision-Aware Operations**: Variables and expressions automatically adapt to compiler precision
  - Variables auto-convert to match compiler precision during evaluation
  - Assignment operators correctly handle cross-precision conversions
  - Original variable types preserved; conversion happens only during expression evaluation

### Changed
- **Performance Improvements**: Float precision operations optimized
  - Built-in float functions now use `MathF` instead of casting `System.Math`
  - Aggressive inlining on hot-path value property accessors
  - Switch expressions used for type dispatch, reducing branching overhead

## [1.2.1] - 2025-10-02

This is a maintenance purpose release for Unity package. No change to a program has been made.

## [1.2.0] - 2025-10-01

### Added
- **Period characters in identifiers**: Variable names and function names can now contain periods (e.g., `user.name`, `math.square()`)
- **VariableId struct**: Strongly-typed variable identifier that replaces direct `uint` usage
  - Provides implicit conversions to/from `uint` and `string` for backwards compatibility
  - Maintains internal unique identifier mapping through extension methods
  - Improves type safety and encapsulates identifier management
- **Boolean Short-Circuit Optimization**: Compile-time optimization for boolean expressions
- String variable support with integer parsing
- Support for `CustomFunction.Create` with zero-parameter functions that have return values (`Func<TResult>`)
- `AddCustomFunctionRange(IEnumerable<CustomFunction> functions)` method for adding multiple custom functions at once
- Documentation clarification on function purity requirements

### Fixed
- **Unity Compatibility**: Added conditional compilation support for Unity's `UnsafeUtility.As` API in `TypeTraits.cs`
- **AST Optimization**: Fixed `Compiler.cs` optimization pipeline where `rootNode.Optimize()` result was being discarded instead of captured, causing some AST optimizations to be ignored
- **Constant Function Folding**: `FunctionNode.Optimize()` now evaluates constant functions with constant parameters at compile time
- **Sign Operator Optimization**: Fixed unary positive operator (`+expr`) returning unoptimized child node instead of optimized result
- **Trailing Semicolon Support**: Trailing semicolons are now allowed and treated as no-op instead of throwing "Cannot find tokens to sequence" error

### Changed
- **BREAKING CHANGE**: `IVariableContainer.TryGetValue()` now takes `VariableId` instead of `uint`
  - Existing implementations must be updated to use `VariableId` parameter
  - Most calling code continues to work due to implicit `uint` → `VariableId` conversion
- AST Node `Build()` method signatures updated to use `IDictionary<VariableId, CustomFunctionOverload>`
- Internal function dictionaries now use `VariableId` keys instead of `uint`
- Comparison operators `<=` and `>=` now use direct operations instead of `< || ==` and `> || ==`
- Eliminated direct calls to `GetUniqueIdentifier()` and `GetStringFromUniqueIdentifier()` in favor of VariableId implicit conversions

### Migration Guide
For custom `IVariableContainer` implementations:
```csharp
// Before
public bool TryGetValue(uint variableKey, out VariableValue variableValue)

// After
public bool TryGetValue(VariableId variableKey, out VariableValue variableValue)
```

Most existing code using `uint` variable identifiers will continue to work due to implicit conversions.

## [1.1.0] - 2025-09-28

### Added
- Division by zero checks for integer and float operations
- Float-to-int conversion overflow handling (NaN, infinity, out-of-range values)
- Unit testing infrastructure and test coverage

### Changed
- **BREAKING CHANGE**: CustomFunction API redesigned from fixed signatures to flexible factory pattern
  - Old: Limited to predefined function signatures (IntInt, FloatFloat, etc.) with union-based storage
  - New: `CustomFunction.Create("name", delegate)` supporting any delegate type with 1-5 parameters
  - Removes the need to match specific signature enums, allowing natural delegate usage
- FloatNode boolean conversion now handles NaN and infinity cases
- Math.IsNearlyEqual sign comparison logic fixed (aInt < 0 != bInt < 0)
- Optimized unique identifier utility

### Fixed
- ArithmeticNode now throws DivideByZeroException for division/modulo by zero
- ArithmeticNode provides specific error messages for boolean arithmetic operations
- ArithmeticNode provides specific error messages for unsupported string operations

## [1.0.0]
- Initial stable release
