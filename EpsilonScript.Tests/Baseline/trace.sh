#!/bin/bash
# Usage: EpsilonScript.Tests/Baseline/trace.sh <git-ref> <output-dir>
# Builds BaselineCorpus.cs against the library at <git-ref> and writes one trace per precision configuration to
# <output-dir>/<IntegerPrecision>-<FloatPrecision>.txt. BaselineTests prints the path of the current trace when it
# fails; diff the two to see which outcomes changed. Prints the SHA-256 of each trace for BaselineTests.Expected.
set -euo pipefail
REF="$1"
OUT="$(mkdir -p "$2" && cd "$2" && pwd)"
HERE="$(cd "$(dirname "$0")" && pwd)"
REPO="$(git -C "$HERE" rev-parse --show-toplevel)"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

git -C "$REPO" archive "$REF" EpsilonScript | tar -x -C "$WORK"
mkdir "$WORK/trace"
cp "$HERE/BaselineCorpus.cs" "$WORK/trace/"
cat > "$WORK/trace/trace.csproj" <<'PROJECT'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net6.0</TargetFramework>
    <LangVersion>8</LangVersion>
    <!-- The corpus is internal to the test assembly; this name lets it see the library's internals too -->
    <AssemblyName>EpsilonScript.Tests</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../EpsilonScript/EpsilonScript.csproj" />
  </ItemGroup>
</Project>
PROJECT
cat > "$WORK/trace/Program.cs" <<'PROGRAM'
using System.IO;
using EpsilonScript;
using EpsilonScript.Tests.Baseline;

internal static class Program
{
  private static void Main(string[] args)
  {
    foreach (var integerPrecision in new[] { Compiler.IntegerPrecision.Integer, Compiler.IntegerPrecision.Long })
    foreach (var floatPrecision in new[]
               { Compiler.FloatPrecision.Float, Compiler.FloatPrecision.Double, Compiler.FloatPrecision.Decimal })
    {
      using var writer = new StreamWriter(Path.Combine(args[0], $"{integerPrecision}-{floatPrecision}.txt"))
        { NewLine = "\n" };
      BaselineCorpus.Write(writer, integerPrecision, floatPrecision);
    }
  }
}
PROGRAM
dotnet run --project "$WORK/trace" -c Release -- "$OUT"
if command -v shasum > /dev/null; then sha=(shasum -a 256); else sha=(sha256sum); fi
for f in "$OUT"/*.txt; do echo "$(basename "$f" .txt) $("${sha[@]}" "$f" | cut -d' ' -f1)"; done
