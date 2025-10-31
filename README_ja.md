# EpsilonScript

EpsilonScriptは、C#アプリケーションに組み込んで使える式評価ライブラリです。カスタム関数の定義が可能で、コンパイル後の実行時にはメモリ割り当てが発生しません。

.NET Standard 2.1に対応しています。

**基本的な式の評価:**
```c#
var compiler = new Compiler();
var script = compiler.Compile("10 + 20 * 2");
script.Execute();
Console.WriteLine(script.IntegerValue); // 50
```

**変数を使用した例:**
```c#
var variables = new DictionaryVariableContainer
{
    ["health"] = new VariableValue(100),
    ["damage"] = new VariableValue(25)
};
var script = compiler.Compile("health - damage", Compiler.Options.Immutable, variables);
script.Execute();
Console.WriteLine(script.IntegerValue); // 75
```

**変数コンテナの切り替え:**
```c#
// 一度だけコンパイル
var script = compiler.Compile("health - damage", Compiler.Options.Immutable);

// 異なるコンテナで実行
var player1 = new DictionaryVariableContainer
{
    ["health"] = new VariableValue(100),
    ["damage"] = new VariableValue(25)
};
var player2 = new DictionaryVariableContainer
{
    ["health"] = new VariableValue(80),
    ["damage"] = new VariableValue(30)
};

script.Execute(player1);
Console.WriteLine(script.IntegerValue); // 75

script.Execute(player2);
Console.WriteLine(script.IntegerValue); // 50
```

**カスタム関数の使用:**
```c#
// 武器と防具の相性テーブル - 複雑なロジックは関数として定義すると便利
compiler.AddCustomFunction(CustomFunction.Create("weapon_effectiveness",
    (string weaponType, string armorType) => (weaponType, armorType) switch
    {
        ("hammer", "heavy") => 1.5f,
        ("hammer", "light") => 0.8f,
        ("sword", "heavy") => 0.8f,
        ("sword", "light") => 1.2f,
        _ => 1.0f
    }));

var variables = new DictionaryVariableContainer
{
    ["base_damage"] = new VariableValue(100),
    ["weapon"] = new VariableValue("hammer"),
    ["armor"] = new VariableValue("heavy")
};

var script = compiler.Compile(
    "base_damage * weapon_effectiveness(weapon, armor)",
    Compiler.Options.Immutable);
script.Execute(variables);
Console.WriteLine(script.FloatValue); // 150
```

## 目次

- [機能](#機能)
- [インストール](#インストール)
  - [Unity](#unity)
- [プロジェクトの状態](#プロジェクトの状態)
- [サンプル](#サンプル)
  - [算術演算](#算術演算)
  - [変数](#変数)
  - [比較演算](#比較演算)
  - [関数](#関数)
  - [文字列](#文字列)
  - [式の連続実行](#式の連続実行)
- [数値精度](#数値精度)
- [ヒープアロケーション](#ヒープアロケーション)
- [設計思想](#設計思想)
- [開発](#開発)

## 機能
- 数式の計算
- 論理式の計算
- 変数の読み書きに対応
- シンプルに設計された構文
- Unityに対応
- コンパイル後の実行時にヒープ割り当てが発生しない設計(一部例外あり)
- 数値の精度を設定可能

## インストール

### Unity

Unity Package Managerからパッケージを追加できます:

1. **Window > Package Manager**を開きます
2. 左上の**+**ボタンをクリックします
3. **Add package from git URL**を選択します
4. 以下のURLを入力します: `https://github.com/aki-null/epsilon-script-unity.git`

または、`Packages/manifest.json`に直接追加することもできます:

```json
{
  "dependencies": {
    "com.akinull.epsilonscript": "https://github.com/aki-null/epsilon-script-unity.git"
  }
}
```

## プロジェクトの状態
すべての機能は安定して動作します。

リリースノートとバージョン履歴は[変更履歴](CHANGELOG.md)を参照してください。

## サンプル

### 算術演算

基本的な算術演算子(`+`、`-`、`*`、`/`、`%`)と括弧(`(`、`)`)がサポートされています。

#### コード

```c#
var compiler = new Compiler();
var script = compiler.Compile("(1 + 2 + 3 * 2) * 2", Compiler.Options.Immutable);
script.Execute();
Console.WriteLine(script.IntegerValue);
```

#### 結果

```
18
```

### 変数

変数の読み取りと代入(`=`)が可能です。複合代入演算子(`+=`、`-=`、`*=`、`/=`)もサポートされています。

変数は`IVariableContainer`に格納されます。簡単に使える実装として`DictionaryVariableContainer`が用意されています。

#### コード

```c#
var compiler = new Compiler();
VariableId valId = "val"; // 文字列からの暗黙的な変換
var variables = new DictionaryVariableContainer { [valId] = new VariableValue(43.0f) };
var script = compiler.Compile("val = val * 10.0", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables[valId].FloatValue);
```

#### 結果

```
430.0
```

`VariableId`構造体は、文字列のような使いやすいインターフェースを保ちながら、内部的には整数識別子を使うことで型安全性と高いパフォーマンスを実現しています。パフォーマンスが重要な箇所では、こちらの使用を推奨します。

スクリプト内で新しい変数を定義することはできません。式が複雑になりすぎないようにするための設計です。

#### 文字列ベースの変数アクセス

パフォーマンスがそれほど重要でない場合は、`VariableId`を使わずに直接文字列を使うこともできます:

```c#
var compiler = new Compiler();
var variables = new DictionaryVariableContainer { ["val"] = new VariableValue(43.0f) };
var script = compiler.Compile("val = val * 10.0", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables["val"].FloatValue);
```

注意: 文字列を直接使用すると内部で変換処理が発生するため、`VariableId`を使用するよりも遅くなります。パフォーマンスが重要な場合は`VariableId`を使ってください。

#### イミュータブルモード

コンパイラは変数の扱いについて、2つのモードをサポートしています:

ミュータブルモード(デフォルト):
- 変数への代入や変更が可能

イミュータブルモード:
- 変数の変更を一切禁止
- 代入演算子を使うとコンパイル時にエラーになる

```c#
// イミュータブルモード - 変数の読み取りのみ
var script1 = compiler.Compile("health - damage", Compiler.Options.Immutable, variables);
script1.Execute(); // OK

// ミュータブルモード - 変数の変更が可能
var script2 = compiler.Compile("health -= damage", Compiler.Options.None, variables);
script2.Execute(); // OK

// イミュータブルモードで代入 - コンパイル時にエラー
var script3 = compiler.Compile(
    "health -= damage", Compiler.Options.Immutable, variables); // エラー!
```

#### 変数コンテナのオーバーライド

実行時に`Execute()`へ`IVariableContainer`を渡すことで、変数の値を上書きできます。実行時のコンテナが優先的に参照され、変数が見つからない場合はコンパイル時のコンテナを参照します。

グローバルな変数でコンパイルしておき、インスタンスごとの変数を渡して実行する、といった使い方ができます。

```c#
// グローバル設定でコンパイル
var globals = new DictionaryVariableContainer
{
  ["shipping_fee"] = new VariableValue(5.99f),
  ["tax_rate"] = new VariableValue(0.08f)
};
var script = compiler.Compile("price + shipping_fee + price * tax_rate",
                               Compiler.Options.None, globals);

// 各インスタンスで実行
foreach (var user in users)
{
  var instanceVars = new DictionaryVariableContainer
  {
    ["price"] = new VariableValue(user.CartTotal)  // インスタンス固有の値で上書き
    // shipping_feeとtax_rateはグローバルの値を使用
  };
  script.Execute(instanceVars);
  Console.WriteLine($"Total: ${script.FloatValue}");
}
```

#### 動的型付け

変数は動的に型付けされます。型はコンパイル時ではなく、実行時に`VariableValue`から決まります。同じコンパイル済みスクリプトを、異なる型の変数で実行することもできます。

```c#
var script = compiler.Compile("a + b", Compiler.Options.None, null);

// 浮動小数点数で実行
var floatVars = new DictionaryVariableContainer
{
  ["a"] = new VariableValue(1.5f),
  ["b"] = new VariableValue(2.3f)
};
script.Execute(floatVars);
Console.WriteLine(script.FloatValue);  // 3.8

// 文字列で実行(同じスクリプト)
var stringVars = new DictionaryVariableContainer
{
  ["a"] = new VariableValue("Hello"),
  ["b"] = new VariableValue(" World")
};
script.Execute(stringVars);
Console.WriteLine(script.StringValue);  // "Hello World"
```

### 比較演算

比較演算子(`==`、`!=`、`<`、`<=`、`>`、`>=`)と論理演算子(`!`、`&&`、`||`)がサポートされています。

#### コード

```c#
var compiler = new Compiler();
VariableId valId = "val";
var variables = new DictionaryVariableContainer { [valId] = new VariableValue(43.0f) };
var script = compiler.Compile(
    "val >= 0.0 && val < 50.0",
    Compiler.Options.Immutable,
    variables);
script.Execute();
Console.WriteLine(script.BooleanValue);
```

#### 結果

```
True
```

### 関数

EpsilonScriptは組み込み関数とカスタム関数をサポートしています。

```c#
var compiler = new Compiler();
compiler.AddCustomFunction(
    CustomFunction.Create("clamp", (float val, float min, float max) =>
        Math.Max(min, Math.Min(max, val))));

var variables = new DictionaryVariableContainer { ["damage"] = new VariableValue(50) };
var script = compiler.Compile(
    "clamp(damage * 1.5, 10, 100)", Compiler.Options.Immutable, variables);
script.Execute();
Console.WriteLine(script.FloatValue); // 75
```

#### 関数の要件

カスタム関数は状態を変更してはいけません。外部データの読み取りは可能ですが、何かを変更することはできません。

```c#
// OK: 純粋な計算
CustomFunction.Create("square", (float x) => x * x)

// OK: 外部データの読み取り
CustomFunction.Create("get_health", () => player.Health)

// OK: 非決定的だが変更はしない
CustomFunction.Create("rand", (float max) => Random.Range(0.0f, max))

// NG: 外部の状態を変更
CustomFunction.Create("set_score", (int score) => { gameState.Score = score; return score; })
```

#### 組み込み関数

- 三角関数: `sin`、`cos`、`tan`、`asin`、`acos`、`atan`、`atan2`、`sinh`、`cosh`、`tanh`
- 数学関数: `sqrt`、`abs`、`floor`、`ceil`、`trunc`、`pow`、`min`、`max`
- 文字列関数: `lower`、`upper`、`len`
- ユーティリティ: `ifelse`(三項演算子の代替)

完全なリストは[Compiler.cs](https://github.com/aki-null/epsilon-script/blob/master/EpsilonScript/Compiler.cs)で確認できます。

#### オーバーロード

関数は同じ名前で異なるシグネチャ(パラメータの型と数)を持つようにオーバーロードできます。

`abs`、`min`、`max`、`ifelse`などの組み込み関数はオーバーロードを使用しています。

#### 決定的関数

コンパイル時の最適化のために、関数を**決定的**としてマークできます。決定的な関数とは、同じ入力に対して常に同じ結果を返す関数です。

関数を決定的としてマークするには、`isDeterministic: true`を渡します:

```c#
// 決定的関数 - 同じ入力には常に同じ出力
CustomFunction.Create("sin", (float v) => MathF.Sin(v), isDeterministic: true)

CustomFunction.Create("clamp", (float val, float min, float max) =>
    Math.Max(min, Math.Min(max, val)), isDeterministic: true)
```

すべてのパラメータが定数の場合、決定的関数はコンパイル時に評価されます:

```c#
compiler.AddCustomFunction(
    CustomFunction.Create("sin", (float v) => MathF.Sin(v), isDeterministic: true));

// コンパイル時に評価される - sin(1.5708)の結果(約1.0)がキャッシュされる
var script = compiler.Compile("sin(3.141592 / 2) * 10");
```

#### メソッドグループ

ラムダの代わりにメソッドグループを使用できます:

```c#
public int GetScore(string level) => CalculateScore(level);

// ラムダの代わりにメソッドグループ
compiler.AddCustomFunction(CustomFunction.Create<string, int>("score", GetScore));
```

**注意:** パラメータを持つメソッドグループを使う場合は、ジェネリック型パラメータを明示する必要があります。ただし、パラメータなしのメソッドグループは明示不要です:

```c#
public int GetConstant() => 42;

// パラメータなしのメソッドグループ - ジェネリクスの明示は不要
compiler.AddCustomFunction(CustomFunction.Create("constant", GetConstant));
```

#### コンテキスト型カスタム関数

コンテキスト型関数は、変数をパラメータとして受け取らずに、実行環境から直接読み取ることができます。

```c#
var compiler = new Compiler();

// 関数はコンテキストから'day'を読み取る
compiler.AddCustomFunction(
    CustomFunction.CreateContextual(
        "IsMon",
        "day",
        (int day) => day % 7 == 1));

var variables = new DictionaryVariableContainer
{
    ["day"] = new VariableValue(1)
};

var script = compiler.Compile("IsMon()", Compiler.Options.Immutable, variables);
script.Execute();
Console.WriteLine(script.BooleanValue); // True
```

コンテキスト変数とスクリプトのパラメータを組み合わせることもできます:

```c#
compiler.AddCustomFunction(
    CustomFunction.CreateContextual(
        "IsAfter",
        "currentDay",
        (int current, int target) => current > target));

var script = compiler.Compile("IsAfter(5)", Compiler.Options.Immutable, variables);
```

コンテキスト変数を最大3つ、スクリプトパラメータを最大3つまでサポートします。

### 文字列

文字列もサポートされており、主に関数の引数として使用します。

#### コード

```c#
var compiler = new Compiler();
compiler.AddCustomFunction(CustomFunction.Create("read_save_data",
  (string flag) => SaveData.Instance.GetIntegerData(flag)));
var script = compiler.Compile(@"read_save_data(""LVL00_PLAYCOUNT"") > 5");
script.Execute();
Console.WriteLine(script.BooleanValue);
```

#### 結果

`SaveData.Instance.GetIntegerData("LVL00_PLAYCOUNT")`が10を返す場合:
```
True
```

文字列の連結がサポートされています:

#### コード

```
"Hello " + "World"
```

#### 結果

```
"Hello World"
```

文字列は数値と連結できます:

#### コード

```
"Debug: " + 128
```

#### 結果

```
"Debug: 128"
```

文字列の比較がサポートされています:

#### コード

```
"Hello" == "Hello"
```

#### 結果

```
true
```

### 式の連続実行

セミコロン(`;`)で複数の式を並べて実行できます。結果は最後の式の値になります。

#### コード

```c#
var compiler = new Compiler();
VariableId xId = "x";
VariableId yId = "y";
var variables = new DictionaryVariableContainer
{
  [xId] = new VariableValue(5),
  [yId] = new VariableValue(10)
};
var script = compiler.Compile(
    "x = x + 1; y = y * 2; x + y",
    Compiler.Options.None,
    variables);
script.Execute();
Console.WriteLine(script.IntegerValue); // 26 (xは6、yは20)
```

#### 結果

```
26
```

## 数値精度

コンパイラの作成時に数値精度を指定することで、式で使用する整数型と浮動小数点型を選択できます。

### 精度のオプション

**整数精度:**
- `Integer`(デフォルト): 32ビットint
- `Long`: 64ビットlong

**浮動小数点精度:**
- `Float`(デフォルト): 32ビットfloat
- `Double`: 64ビットdouble
- `Decimal`: 128ビットdecimal

### 使用方法

```c#
// デフォルト: 32ビットintとfloat
var compiler = new Compiler();

// 高精度: 64ビットlongと128ビットdecimal
var preciseCompiler = new Compiler(
    Compiler.IntegerPrecision.Long,
    Compiler.FloatPrecision.Decimal);

var script = preciseCompiler.Compile("0.1 + 0.2");
script.Execute();
Console.WriteLine(script.DecimalValue); // 0.3
```

すべての演算は、設定した精度に自動的に変換されます。

### カスタム関数と精度

カスタム関数のパラメータ型は、コンパイラで設定した精度に合わせる必要があります:

```c#
// コンパイラはDouble精度を使用
var compiler = new Compiler(
    Compiler.IntegerPrecision.Integer,
    Compiler.FloatPrecision.Double);

// 関数はfloatではなくdoubleを使う必要がある
compiler.AddCustomFunction(CustomFunction.Create("calc", (double x) => x * 2.5));

var script = compiler.Compile("calc(10.5)");
script.Execute();
Console.WriteLine(script.DoubleValue); // 26.25
```

パラメータ型が一致しない場合、実行時エラーになります。なお、整数の引数は必要に応じて浮動小数点型に自動変換されます。

## ヒープアロケーション

ゲーム開発では、ヒープへのメモリ割り当てがパフォーマンス上の懸念事項になります。

EpsilonScriptは、コンパイル後の実行時にはメモリ割り当てが発生しないよう設計されていますが、いくつか例外があります:

### 文字列の連結

変数を含む文字列の連結では、ヒープ割り当てが発生します:
```
"Debug: " + i
```
ここで`i`は変数です。

定数同士の連結はコンパイル時に処理されるため、メモリ割り当ては発生しません:
```
"Debug: " + 42 * 42
```

### カスタム関数

カスタム関数内でメモリ割り当てを行う場合、呼び出し時にメモリ割り当てが発生します。

## 設計思想

ゲームデザイナーはロジックを表現する必要があります。例えば、クエストで「プレイヤーがモンスターと戦わず、鍵を持っている」という条件を設定したり、武器の種類と防具の種類に応じてダメージ計算を変えたりします。こうした条件は、単純なデータテーブルで表現するには複雑すぎますが、完全なスクリプト言語が必要なほどではありません。

しかし、通常の選択肢はどれもしっくりきません。純粋なデータ(Excel、JSON、Unityのシリアライゼーション)では、新しいルールを追加するたびにプログラマーに列の追加や特殊なケースの実装を依頼することになります。一方、完全なスクリプト言語(LuaやPython)は自由度が高すぎて、デザイナーが無限ループを書いてしまったり、パフォーマンス問題を引き起こしたり、ゲームシステムを予期しない形で壊してしまう可能性があります。

EpsilonScriptは、スコープを「式」だけに絞り込みます。ループはなし、変数宣言もなし。式を評価して結果を返すだけです。この制約が重要なポイントです。デザイナーは複雑な計算や条件を表現できますが、メンテナンス上の問題を引き起こすようなコードは書けません。

式はゲームループ内で頻繁に実行されます(フレームあたり数百回)。そのため、高速に動作する必要があります。EpsilonScriptは一度コンパイルすれば、パースやメモリ割り当てなしで何度でも実行できる再利用可能なスクリプトを生成します。変数コンテナのパターンを使えば、一度コンパイルして、この計算が必要なすべてのエンティティで使い回せます。

構文は、可読性を損なう機能を意図的に省いています。例えば三項演算子はなく、代わりに`ifelse(condition, true_value, false_value)`という関数を使います。暗黙的な振る舞いや特殊ケースを覚える必要もありません。プログラマーは、どの関数が存在し何をするかを正確にコントロールできます。結果として、チーム全員が読める式になります。

ビジュアルスクリプティングシステムの中で使う場合、EpsilonScriptはノードの接続が煩雑になるケースを解決します。例えば`base_damage * weapon_effectiveness(weapon, armor) * range_modifier`のような計算を個々のノードで配線すると、グラフが散らかります。式をテキストで書くノードを使えば、グラフはコントロールフローに集中でき、計算の詳細はテキストに任せられます。こうした用途では、テキストの方が明確です。

## 開発

### T4テンプレートコード生成

カスタム関数の実装には、メンテナンスの負担を減らし一貫性を保つために、T4テンプレートを使用しています。

**生成されるファイル**:
- `EpsilonScript/Function/CustomFunction.Generated.cs`
- `EpsilonScript/Function/CustomFunction.Contextual.Generated.cs`

**テンプレートファイル**:
- `EpsilonScript/Function/CustomFunction.Generated.tt`
- `EpsilonScript/Function/CustomFunction.Contextual.Generated.tt`

#### 前提条件

T4コマンドラインツールをインストールします:
```bash
dotnet tool install -g dotnet-t4
```

#### コードの再生成

テンプレートを変更した後は、コードを再生成します:
```bash
cd EpsilonScript/Function
t4 CustomFunction.Generated.tt
t4 CustomFunction.Contextual.Generated.tt
```
