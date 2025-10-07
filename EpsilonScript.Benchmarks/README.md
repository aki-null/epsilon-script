# EpsilonScript Performance Benchmarks

Performance benchmarks testing the impact of different numeric precision configurations.

## Benchmarks

**SimpleMathBenchmark**: `(a + b) * c - d / e`

**ComplexMathBenchmark**: `sqrt(pow(x * cos(angle) - y * sin(angle), 2.0) + pow(x * sin(angle) + y * cos(angle), 2.0)) * scale + offset`

Each tests all 6 precision permutations:
- Integer/Long (32/64-bit) × Float/Double/Decimal (32/64/128-bit)
- Uses variables to prevent AST constant folding

## Usage

```bash
# Run all benchmarks
dotnet run -c Release

# Run specific benchmark
dotnet run -c Release --filter "*SimpleMathBenchmark*"
dotnet run -c Release --filter "*ComplexMathBenchmark*"

# Run without JIT (using NativeAOT)
dotnet run -c Release -- --runtimes nativeaot9.0

# Export results
dotnet run -c Release --exporters json html csv
```
