using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace EpsilonScript
{
  /// <summary>
  /// Extension methods for StringBuilder to append numeric values with InvariantCulture
  /// without allocations using a reusable buffer from CompilerContext.
  /// </summary>
  internal static class StringBuilderExtensions
  {
    /// <summary>
    /// Appends a float value to StringBuilder using InvariantCulture formatting.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringBuilder AppendFloatInvariant(this StringBuilder sb, float value, CompilerContext context)
    {
      if (!value.TryFormat(context.FormatBuffer, out var charsWritten, default, CultureInfo.InvariantCulture))
      {
        throw new InvalidOperationException(
          $"Float value {value} formatting exceeded buffer size of {context.FormatBuffer.Length} characters");
      }

      return sb.Append(context.FormatBuffer, 0, charsWritten);
    }

    /// <summary>
    /// Appends a double value to StringBuilder using InvariantCulture formatting.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringBuilder AppendDoubleInvariant(this StringBuilder sb, double value, CompilerContext context)
    {
      if (!value.TryFormat(context.FormatBuffer, out var charsWritten, default, CultureInfo.InvariantCulture))
      {
        throw new InvalidOperationException(
          $"Double value {value} formatting exceeded buffer size of {context.FormatBuffer.Length} characters");
      }

      return sb.Append(context.FormatBuffer, 0, charsWritten);
    }

    /// <summary>
    /// Appends a decimal value to StringBuilder using InvariantCulture formatting.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StringBuilder AppendDecimalInvariant(this StringBuilder sb, decimal value, CompilerContext context)
    {
      if (!value.TryFormat(context.FormatBuffer, out var charsWritten, default, CultureInfo.InvariantCulture))
      {
        throw new InvalidOperationException(
          $"Decimal value {value} formatting exceeded buffer size of {context.FormatBuffer.Length} characters");
      }

      return sb.Append(context.FormatBuffer, 0, charsWritten);
    }
  }
}