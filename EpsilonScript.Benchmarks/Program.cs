using BenchmarkDotNet.Running;

namespace EpsilonScript.Benchmarks;

/// <summary>
/// EpsilonScript Performance Benchmarks
///
/// Tests the performance impact of different numeric precision configurations:
/// - Integer Precision: Integer (32-bit) vs Long (64-bit)
/// - Float Precision: Float (32-bit) vs Double (64-bit) vs Decimal (128-bit)
///
/// All benchmarks use variables to prevent AST constant folding optimization.
///
/// Usage:
///   dotnet run -c Release
///
/// For specific benchmark:
///   dotnet run -c Release --filter "*SimpleMathBenchmark*"
///   dotnet run -c Release --filter "*ComplexMathBenchmark*"
/// </summary>
public class Program
{
  public static void Main(string[] args)
  {
    var summary = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
  }
}
