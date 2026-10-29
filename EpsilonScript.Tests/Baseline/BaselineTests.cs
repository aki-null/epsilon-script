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
      ["Integer-Float"] = "a9ca5440dafbd06fb43db888345d96171fd1abafbb769c18825dffe55f79db51",
      ["Integer-Double"] = "e6d682d29c78fb81f61ba63c194ea6f86eb4180b409571c74ece9e74f10075b7",
      ["Integer-Decimal"] = "1c5f20087c6f679c333120cbac823392340a1b13a6a9e5733549524e165b7c1f",
      ["Long-Float"] = "98751d4da3e501029dcb044f68860e9f4246fe45d7998342b25e238f70f2563e",
      ["Long-Double"] = "cb1e415b9d7a958e24a5c02c47a61c636ea4a36502bc3c3237229dc2c95850db",
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
