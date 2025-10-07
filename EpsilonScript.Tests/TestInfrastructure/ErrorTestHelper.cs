using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;
using Xunit;

namespace EpsilonScript.Tests.TestInfrastructure
{
  /// <summary>
  /// Shared utilities for testing error conditions across different test classes
  /// </summary>
  public static class ErrorTestHelper
  {
    /// <summary>
    /// Assert that a parser exception is thrown with the expected message fragment
    /// </summary>
    internal static void AssertParserException(Action action, string expectedMessageFragment)
    {
      var exception = Assert.Throws<ParserException>(action);
      if (!string.IsNullOrEmpty(expectedMessageFragment))
      {
        Assert.Contains(expectedMessageFragment, exception.Message);
      }
    }

    /// <summary>
    /// Assert that a runtime exception is thrown with the expected message fragment
    /// </summary>
    internal static void AssertRuntimeException(Action action, string expectedMessageFragment)
    {
      var exception = Assert.Throws<RuntimeException>(action);
      if (!string.IsNullOrEmpty(expectedMessageFragment))
      {
        Assert.Contains(expectedMessageFragment, exception.Message);
      }
    }

    /// <summary>
    /// Helper to build AST node with error handling
    /// </summary>
    internal static void BuildNodeExpectingError<TException>(Node node, Stack<Node> rpnStack, Element element,
      Compiler.Options options, IVariableContainer variables,
      IDictionary<VariableId, CustomFunctionOverload> functions, string expectedMessageFragment = null)
      where TException : Exception
    {
      var exception = Assert.Throws<TException>(() =>
        node.Build(rpnStack, element, options, variables, functions, Compiler.IntegerPrecision.Integer,
          Compiler.FloatPrecision.Float));

      if (!string.IsNullOrEmpty(expectedMessageFragment))
      {
        Assert.Contains(expectedMessageFragment, exception.Message);
      }
    }

    /// <summary>
    /// Helper to execute AST node expecting error
    /// </summary>
    internal static void ExecuteNodeExpectingError<TException>(Node node,
      IVariableContainer variablesOverride = null, string expectedMessageFragment = null)
      where TException : Exception
    {
      var exception = Assert.Throws<TException>(() => node.Execute(variablesOverride));

      if (!string.IsNullOrEmpty(expectedMessageFragment))
      {
        Assert.Contains(expectedMessageFragment, exception.Message);
      }
    }
  }
}