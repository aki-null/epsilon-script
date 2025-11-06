using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EpsilonScript.Function;
using Xunit;

namespace EpsilonScript.Tests.Threading
{
  [Trait("Category", "Threading")]
  [Trait("Component", "Compiler")]
  public class MultithreadingTests
  {
    /// <summary>
    /// Tests that multiple threads can each create their own compiler instance
    /// and compile scripts without interference.
    /// </summary>
    [Fact]
    public void ParallelCompilation_IndependentCompilers_AllSucceed()
    {
      const int threadCount = 20;
      var results = new ConcurrentBag<CompiledScript>();
      var exceptions = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var compiler = new Compiler();
          var script = compiler.Compile($"10 + {i}");
          results.Add(script);
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, results.Count);

      // Verify each script produces the correct result
      var scriptArray = results.ToArray();
      for (var i = 0; i < threadCount; i++)
      {
        scriptArray[i].Execute();
        Assert.True(scriptArray[i].IntegerValue >= 10 && scriptArray[i].IntegerValue < 10 + threadCount);
      }
    }

    /// <summary>
    /// Tests that the same script compiled and executed on multiple threads
    /// produces deterministic, identical results.
    /// </summary>
    [Fact]
    public void ParallelExecution_SameScript_DeterministicResults()
    {
      const int threadCount = 50;
      const string script = "(10 + 20) * 3 - 5";
      var results = new ConcurrentBag<int>();
      var exceptions = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var compiler = new Compiler();
          var compiled = compiler.Compile(script);
          compiled.Execute();
          results.Add(compiled.IntegerValue);
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, results.Count);

      // All results should be identical
      const int expected = 85;
      Assert.All(results, result => Assert.Equal(expected, result));
    }

    /// <summary>
    /// Tests that different threads compiling different scripts simultaneously
    /// don't interfere with each other.
    /// </summary>
    [Fact]
    public void ParallelCompilation_DifferentScripts_NoInterference()
    {
      const int threadCount = 30;
      var scripts = new[]
      {
        ("10 + 20", 30),
        ("5 * 8", 40),
        ("100 - 25", 75),
        ("50 / 2", 25),
        ("7 % 3", 1),
        ("2 + 3 * 4", 14)
      };

      var results = new ConcurrentDictionary<int, List<int>>();
      var exceptions = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var (source, expected) = scripts[i % scripts.Length];
          var compiler = new Compiler();
          var compiled = compiler.Compile(source);
          compiled.Execute();

          results.AddOrUpdate(
            expected,
            _ => new List<int> { compiled.IntegerValue },
            (_, list) =>
            {
              lock (list)
              {
                list.Add(compiled.IntegerValue);
              }

              return list;
            }
          );
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);

      // Verify each expected value was produced the correct number of times
      foreach (var (_, expected) in scripts)
      {
        Assert.True(results.ContainsKey(expected));
        Assert.All(results[expected], result => Assert.Equal(expected, result));
      }
    }

    /// <summary>
    /// Stress test with many threads compiling and executing complex scripts.
    /// </summary>
    [Fact]
    public void StressTest_ManyThreads_ComplexScripts()
    {
      const int threadCount = 100;
      var exceptions = new ConcurrentBag<Exception>();
      var successCount = 0;

      var complexScripts = new[]
      {
        "(10 + 20) * 3 - 5",
        "100 / 2 - 25 + 10",
        "3.14159 * 5 * 5",
        "(7 + 3) * (15 - 5) / 2",
        "50 + 25 * 2 - 10 / 5"
      };

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var script = complexScripts[i % complexScripts.Length];
          var compiler = new Compiler();
          var compiled = compiler.Compile(script);
          compiled.Execute();

          Interlocked.Increment(ref successCount);
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, successCount);
    }

    /// <summary>
    /// Tests that each thread should compile its own script for thread safety.
    /// CompiledScript instances are NOT thread-safe for concurrent execution
    /// because they store execution state (ValueType, _isResultCached).
    /// Each thread needs its own CompiledScript instance.
    /// </summary>
    [Fact]
    public void ParallelCompilation_EachThreadOwnScript_ThreadSafe()
    {
      const int threadCount = 50;
      const string scriptSource = "x * 2 + y";

      var results = new ConcurrentBag<(int threadId, int result)>();
      var exceptions = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          // Each thread creates its own compiler and compiles its own script
          var compiler = new Compiler();
          var variables = new DictionaryVariableContainer
          {
            ["x"] = new VariableValue(i),
            ["y"] = new VariableValue(i * 10)
          };

          var script = compiler.Compile(scriptSource, Compiler.Options.None, variables);
          script.Execute();
          results.Add((i, script.IntegerValue));
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, results.Count);

      // Verify each thread got its own correct result: x * 2 + y = i * 2 + i * 10 = i * 12
      foreach (var (threadId, result) in results)
      {
        Assert.Equal(threadId * 12, result);
      }
    }

    /// <summary>
    /// Tests that custom functions work correctly when each thread has its own compiler.
    /// </summary>
    [Fact]
    public void ParallelCompilation_CustomFunctions_Isolated()
    {
      const int threadCount = 20;
      var results = new ConcurrentBag<int>();
      var exceptions = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var compiler = new Compiler();

          // Each thread adds a custom function with a thread-specific multiplier
          var multiplier = i + 1;
          compiler.AddCustomFunction(
            CustomFunction.Create("multiply", (int v) => v * multiplier, isDeterministic: true)
          );

          var script = compiler.Compile("multiply(10)");
          script.Execute();
          results.Add(script.IntegerValue);
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, results.Count);

      // Verify we got all expected results (10, 20, 30, ..., 200)
      var expected = Enumerable.Range(1, threadCount).Select(i => i * 10).OrderBy(x => x).ToList();
      var actual = results.OrderBy(x => x).ToList();
      Assert.Equal(expected, actual);
    }

    /// <summary>
    /// Tests parallel compilation with different variable containers to ensure no leakage.
    /// </summary>
    [Fact]
    public void ParallelCompilation_DifferentVariables_NoLeakage()
    {
      const int threadCount = 30;
      var results = new ConcurrentBag<(int threadId, int result)>();
      var exceptions = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var compiler = new Compiler();
          var variables = new DictionaryVariableContainer
          {
            ["threadId"] = new VariableValue(i),
            ["multiplier"] = new VariableValue(i * 2)
          };

          var script = compiler.Compile("threadId * multiplier", Compiler.Options.None, variables);
          script.Execute();
          results.Add((i, script.IntegerValue));
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, results.Count);

      // Verify each thread got its own correct result
      foreach (var (threadId, result) in results)
      {
        Assert.Equal(threadId * threadId * 2, result);
      }
    }

    /// <summary>
    /// Tests that different threads can use different precision settings without interference.
    /// </summary>
    [Fact]
    public void ParallelCompilation_DifferentPrecisions_Isolated()
    {
      const int iterations = 20;
      var results = new ConcurrentBag<(string precision, object value)>();
      var exceptions = new ConcurrentBag<Exception>();

      var precisionConfigs = new[]
      {
        (Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float, "int-float"),
        (Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double, "long-double"),
        (Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal, "int-decimal"),
        (Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float, "long-float")
      };

      Parallel.For(0, iterations, i =>
      {
        try
        {
          var config = precisionConfigs[i % precisionConfigs.Length];
          var compiler = new Compiler(config.Item1, config.Item2);

          var script = compiler.Compile("10 + 20");
          script.Execute();
          results.Add((config.Item3, script.IntegerValue));
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(iterations, results.Count);
      Assert.All(results, r => Assert.Equal(30, (int)r.value));
    }

    /// <summary>
    /// Tests mixed operations: some threads compiling, others executing pre-compiled scripts.
    /// </summary>
    [Fact]
    public void MixedOperations_CompilationAndExecution_NoInterference()
    {
      const int threadCount = 40;

      // Pre-compile some scripts
      var compiler = new Compiler();
      var precompiledScripts = new[]
      {
        compiler.Compile("10 + 20"),
        compiler.Compile("5 * 8"),
        compiler.Compile("100 - 25")
      };

      var results = new ConcurrentBag<int>();
      var exceptions = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          if (i % 2 == 0)
          {
            // Even threads: compile new scripts
            var localCompiler = new Compiler();
            var script = localCompiler.Compile($"50 + {i}");
            script.Execute();
            results.Add(script.IntegerValue);
          }
          else
          {
            // Odd threads: execute pre-compiled scripts
            var script = precompiledScripts[i % precompiledScripts.Length];
            script.Execute();
            results.Add(script.IntegerValue);
          }
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, results.Count);
    }

    /// <summary>
    /// Tests that exceptions during parallel compilation are properly isolated.
    /// </summary>
    [Fact]
    public void ParallelCompilation_InvalidScripts_ExceptionsIsolated()
    {
      const int threadCount = 30;

      var scripts = new[]
      {
        ("10 + 20", true), // Valid
        ("10 +", false), // Invalid: incomplete expression
        ("5 * 8", true), // Valid
        (")", false), // Invalid: unmatched parenthesis
        ("x = 10; x", true), // Valid
        ("unknown_func(10)", false), // Invalid: unknown function
        ("(((10 + 5))", false) // Invalid: unmatched parentheses
      };

      var validResults = new ConcurrentBag<CompiledScript>();
      var invalidResults = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        var (script, shouldSucceed) = scripts[i % scripts.Length];
        var compiler = new Compiler();

        try
        {
          var compiled = compiler.Compile(script);
          if (shouldSucceed)
          {
            validResults.Add(compiled);
          }
          else
          {
            // This shouldn't happen for invalid scripts
            throw new Exception($"Expected compilation to fail for: {script}");
          }
        }
        catch (Exception ex)
        {
          if (!shouldSucceed)
          {
            invalidResults.Add(ex);
          }
          else
          {
            throw; // Re-throw unexpected exceptions
          }
        }
      });

      Assert.True(!validResults.IsEmpty, "Should have some valid compilations");
      Assert.True(!invalidResults.IsEmpty, "Should have some invalid compilations");
    }

    /// <summary>
    /// Rapid compilation/execution cycles to try to expose race conditions.
    /// </summary>
    [Fact]
    public void RaceConditionTest_RapidCycles_NoDataCorruption()
    {
      const int threadCount = 50;
      const int cyclesPerThread = 100;

      var exceptions = new ConcurrentBag<Exception>();
      var totalExecutions = 0;

      Parallel.For(0, threadCount, threadIndex =>
      {
        try
        {
          for (var cycle = 0; cycle < cyclesPerThread; cycle++)
          {
            var compiler = new Compiler();
            var variables = new DictionaryVariableContainer
            {
              ["a"] = new VariableValue(threadIndex),
              ["b"] = new VariableValue(cycle)
            };

            var script = compiler.Compile("a * 100 + b", Compiler.Options.None, variables);
            script.Execute();

            var expected = threadIndex * 100 + cycle;
            if (script.IntegerValue != expected)
            {
              throw new Exception($"Data corruption detected: expected {expected}, got {script.IntegerValue}");
            }

            Interlocked.Increment(ref totalExecutions);
          }
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount * cyclesPerThread, totalExecutions);
    }

    /// <summary>
    /// Tests that compiler options work correctly in parallel scenarios.
    /// </summary>
    [Fact]
    public void ParallelCompilation_DifferentOptions_Isolated()
    {
      const int threadCount = 20;

      var successfulConfigs = new[]
      {
        (Compiler.Options.None, "10 + 20 * 3"),
        (Compiler.Options.Immutable, "10 + 20"),
        (Compiler.Options.NoAlloc, "10 + 20")
      };

      // For testing Immutable option with assignments, we need variables pre-defined
      const string failureScript = "x = 10; x"; // Should fail with Immutable option

      var successCount = 0;
      var failureCount = 0;
      var exceptions = new ConcurrentBag<Exception>();

      // Test successful configurations
      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var (options, script) = successfulConfigs[i % successfulConfigs.Length];
          var compiler = new Compiler();
          var compiled = compiler.Compile(script, options);
          compiled.Execute();
          Interlocked.Increment(ref successCount);
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      // Test failure configurations - Immutable option should reject assignments
      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var compiler = new Compiler();
          var variables = new DictionaryVariableContainer
          {
            ["x"] = new VariableValue(0) // Pre-define variable
          };

          compiler.Compile(failureScript, Compiler.Options.Immutable, variables);

          // If we get here, compilation succeeded when it should have failed
          exceptions.Add(new Exception($"Expected compilation to fail for: {failureScript} with Immutable option"));
        }
        catch (ParserException)
        {
          // Expected - compilation should fail with Immutable option for assignments
          Interlocked.Increment(ref failureCount);
        }
        catch (Exception ex)
        {
          // Unexpected exception type
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, successCount);
      Assert.Equal(threadCount, failureCount);
    }

    /// <summary>
    /// Tests that string operations work correctly in parallel scenarios.
    /// </summary>
    [Fact]
    public void ParallelCompilation_StringOperations_NoInterference()
    {
      const int threadCount = 30;
      var results = new ConcurrentBag<string>();
      var exceptions = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var compiler = new Compiler();
          var script = compiler.Compile($"'Thread' + ' ' + '{i}'");
          script.Execute();
          results.Add(script.StringValue);
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, results.Count);

      // Verify each thread produced a unique string
      for (var i = 0; i < threadCount; i++)
      {
        Assert.Contains($"Thread {i}", results);
      }
    }

    /// <summary>
    /// Tests that boolean operations and comparisons work correctly in parallel.
    /// </summary>
    [Fact]
    public void ParallelCompilation_BooleanOperations_Deterministic()
    {
      const int threadCount = 40;

      var scripts = new[]
      {
        ("10 > 5", true),
        ("3 < 2", false),
        ("5 == 5", true),
        ("10 != 10", false),
        ("true && true", true),
        ("true || false", true),
        ("!false", true)
      };

      var results = new ConcurrentBag<(string script, bool result)>();
      var exceptions = new ConcurrentBag<Exception>();

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var (script, _) = scripts[i % scripts.Length];
          var compiler = new Compiler();
          var compiled = compiler.Compile(script);
          compiled.Execute();
          results.Add((script, compiled.BooleanValue));
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, results.Count);

      // Verify all results match expectations
      foreach (var (script, result) in results)
      {
        var expected = scripts.First(s => s.Item1 == script).Item2;
        Assert.Equal(expected, result);
      }
    }

    /// <summary>
    /// Tests compilation and execution with built-in math functions in parallel.
    /// </summary>
    [Fact]
    public void ParallelCompilation_BuiltInFunctions_Isolated()
    {
      const int threadCount = 30;
      var results = new ConcurrentBag<double>();
      var exceptions = new ConcurrentBag<Exception>();

      var functions = new[]
      {
        "abs(-10)",
        "min(5, 10)",
        "max(5, 10)",
        "sqrt(16.0)",
        "floor(3.7)",
        "ceil(3.2)"
      };

      Parallel.For(0, threadCount, i =>
      {
        try
        {
          var script = functions[i % functions.Length];
          var compiler = new Compiler();
          var compiled = compiler.Compile(script);
          compiled.Execute();

          // Convert result to double for uniform comparison
          double value = compiled.ValueType switch
          {
            Type.Integer => compiled.IntegerValue,
            Type.Float => compiled.FloatValue,
            _ => throw new Exception("Unexpected type")
          };

          results.Add(value);
        }
        catch (Exception ex)
        {
          exceptions.Add(ex);
        }
      });

      Assert.Empty(exceptions);
      Assert.Equal(threadCount, results.Count);
    }
  }
}