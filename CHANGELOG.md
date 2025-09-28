# Changelog

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
