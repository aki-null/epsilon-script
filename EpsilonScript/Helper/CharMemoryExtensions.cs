using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EpsilonScript.Helper
{
  /// <summary>
  /// Faster equivalents of ReadOnlyMemory&lt;char&gt;.Span and ToString() for string-backed memory.
  /// </summary>
  /// <remarks>
  /// The BCL members begin with a typeof(T) == typeof(char) check. Under IL2CPP the generic body is shared, so that
  /// check materializes Type objects through a locked runtime metadata lookup on every call. TryGetString only tests
  /// the backing object's class.
  /// </remarks>
  internal static class CharMemoryExtensions
  {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ReadOnlySpan<char> FastSpan(this ReadOnlyMemory<char> memory)
    {
      return MemoryMarshal.TryGetString(memory, out var text, out var start, out var length)
        ? text.AsSpan(start, length)
        : memory.Span;
    }

    public static string FastToString(this ReadOnlyMemory<char> memory)
    {
      if (!MemoryMarshal.TryGetString(memory, out var text, out var start, out var length))
      {
        return memory.ToString();
      }

      return start == 0 && length == text.Length ? text : text.Substring(start, length);
    }
  }
}
