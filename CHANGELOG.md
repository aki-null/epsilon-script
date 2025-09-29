# Changelog

## [1.2.0] - 2025-09-29

### Added
- **Period characters in identifiers**: Variable names and function names can now contain periods (e.g., `user.name`, `math.square()`)
- **VariableId struct**: Strongly-typed variable identifier that replaces direct `uint` usage
  - Provides implicit conversions to/from `uint` and `string` for backwards compatibility
  - Maintains internal unique identifier mapping through extension methods
  - Improves type safety and encapsulates identifier management
- Support for `CustomFunction.Create` with zero-parameter functions that have return values (`Func<TResult>`)
- `AddCustomFunctionRange(IEnumerable<CustomFunction> functions)` method for adding multiple custom functions at once
- String variable support with integer parsing

### Changed
- **BREAKING CHANGE**: `IVariableContainer.TryGetValue()` now takes `VariableId` instead of `uint`
  - Existing implementations must be updated to use `VariableId` parameter
  - Most calling code continues to work due to implicit `uint` → `VariableId` conversion
- Internal function dictionaries now use `VariableId` keys instead of `uint`
- AST Node `Build()` method signatures updated to use `IDictionary<VariableId, CustomFunctionOverload>`
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
