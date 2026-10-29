using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace EpsilonScript.Tests.Baseline
{
  /// <summary>
  /// Requires the corpus trace to match the one recorded from commit 03593b8, before the register VM replaced the
  /// tree interpreter. Any change to compile errors, results, conversions or runtime errors fails here.
  /// </summary>
  [Trait("Category", "Integration")]
  [Trait("Component", "Baseline")]
  public class BaselineTests
  {
    // SHA-256 of each trace, printed by: EpsilonScript.Tests/Baseline/trace.sh 03593b8 <output-dir>
    private static readonly Dictionary<string, string> Expected = new Dictionary<string, string>
    {
      ["Integer-Float"] = "d200268081784c0ca49c26b53905982167da79d3dac56a6d1f0d565d6fbf926c",
      ["Integer-Double"] = "8f47ef5ef8b2cf7121330af61feba1b0a9e88a40b22be29055aefde25929d724",
      ["Integer-Decimal"] = "1c5f20087c6f679c333120cbac823392340a1b13a6a9e5733549524e165b7c1f",
      ["Long-Float"] = "9994207b894f9de09eca9a3a3a28a650c323b2ba1642fe6efab177a45159433c",
      ["Long-Double"] = "bd20fd2d626c48cd98e44c9bb89b05402fe27bb8ef9a4b94234103dd3056b040",
      ["Long-Decimal"] = "ea3d8c627e01059474b72036922a805543db7cd361ab12752f810925c046684f",
    };

    [Theory]
    [InlineData(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Float)]
    [InlineData(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Double)]
    [InlineData(Compiler.IntegerPrecision.Integer, Compiler.FloatPrecision.Decimal)]
    [InlineData(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Float)]
    [InlineData(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Double)]
    [InlineData(Compiler.IntegerPrecision.Long, Compiler.FloatPrecision.Decimal)]
    public void Corpus_MatchesBaseline(Compiler.IntegerPrecision integerPrecision,
      Compiler.FloatPrecision floatPrecision)
    {
      var name = $"{integerPrecision}-{floatPrecision}";
      // Same line endings as the StreamWriter trace.sh uses
      var writer = new StringWriter { NewLine = "\n" };
      BaselineCorpus.Write(writer, integerPrecision, floatPrecision);
      var trace = writer.ToString();

      string hash;
      using (var sha = SHA256.Create())
      {
        var sb = new StringBuilder();
        foreach (var b in sha.ComputeHash(new UTF8Encoding(false).GetBytes(trace)))
        {
          sb.Append(b.ToString("x2"));
        }

        hash = sb.ToString();
      }

      if (hash != Expected[name])
      {
        var path = Path.Combine(Path.GetTempPath(), $"epsilonscript-trace-{name}.txt");
        File.WriteAllText(path, trace);
        Assert.True(false,
          $"Trace differs from the baseline. Current trace: {path}. " +
          "Run EpsilonScript.Tests/Baseline/trace.sh 03593b8 <dir> and diff against <dir>/" + name + ".txt");
      }
    }
  }
}
