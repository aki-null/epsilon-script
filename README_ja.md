# EpsilonScript

EpsilonScriptは、C#アプリケーションに組み込んで使える式評価ライブラリです。カスタム関数の定義が可能で、コンパイル後の実行時にはメモリ割り当てが発生しません。

.NET Standard 2.1に対応しています。バージョン履歴は[変更履歴](CHANGELOG.md)を参照してください。

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
- [算術演算](#算術演算)
- [変数](#変数)
- [比較演算](#比較演算)
- [関数](#関数)
- [文字列](#文字列)
- [式の連続実行](#式の連続実行)
- [数値精度](#数値精度)
- [ヒープアロケーション](#ヒープアロケーション)
- [スレッドセーフティ](#スレッドセーフティ)
- [設計思想](#設計思想)
- [開発](#開発)

## 機能
- シンプルな構文
- 算術演算子（`+`、`-`、`*`、`/`、`%`）と論理演算子（`&&`、`||`、`!`、比較演算子）
- 動的型付けと代入演算子（`=`、`+=`、`-=`、`*=`、`/=`、`%=`）
- 文字列連結
- オーバーロード可能なカスタム関数
- 数値精度の設定が可能（int/long、float/double/decimal）
- コンパイル後はゼロアロケーションで実行（変数を含む文字列操作を除く）
- Immutableモードで変数の変更を禁止、NoAllocモードで実行時のメモリ割り当てを禁止
- コンパイル時の最適化（定数畳み込み、決定的な関数、デッドコード削除）
- 実行時に変数コンテナを切り替え可能（一度コンパイルして、異なるデータで実行）
- Unityに対応

## インストール

### Unity

Unity Package Managerからパッケージを追加できます:

1. **Window > Package Manager**を開きます
2. 左上の **+** ボタンをクリックします
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

## 算術演算

基本的な演算子(`+`、`-`、`*`、`/`、`%`)と括弧をサポートしています。

```c#
var compiler = new Compiler();
var script = compiler.Compile("(1 + 2 + 3 * 2) * 2", Compiler.Options.Immutable);
script.Execute();
Console.WriteLine(script.IntegerValue);  // 18
```

## 変数

変数は代入(`=`)と複合演算子(`+=`、`-=`、`*=`、`/=`、`%=`)をサポートしています。

変数は`IVariableContainer`に格納されます（基本的な実装として`DictionaryVariableContainer`を使用）。

```c#
var compiler = new Compiler();
VariableId valId = "val"; // 文字列からの暗黙的な変換
var variables = new DictionaryVariableContainer { [valId] = new VariableValue(43.0f) };
var script = compiler.Compile("val = val * 10.0", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables[valId].FloatValue);  // 430.0
```

`VariableId`は文字列のようなシンプルなインターフェースを提供しながら、内部では整数IDによる高速な検索を行います。パフォーマンスが重要な箇所で推奨されます。

スクリプト内で新しい変数を定義することはできません - これにより式がシンプルに保たれます。

### 文字列ベースの変数アクセス

パフォーマンスが重要でない場合は、直接文字列を使用できます:

```c#
var compiler = new Compiler();
var variables = new DictionaryVariableContainer { ["val"] = new VariableValue(43.0f) };
var script = compiler.Compile("val = val * 10.0", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables["val"].FloatValue);
```

文字列での検索は内部的な変換のオーバーヘッドにより、`VariableId`よりも遅くなります。

### ピリオドを含む変数名

変数名にはピリオド(`.`)を含めることができます。関連する値をグループ化するのに便利です:

```c#
var compiler = new Compiler();
var variables = new DictionaryVariableContainer
{
    ["user.name"] = new VariableValue("John"),
    ["user.level"] = new VariableValue(5),
    ["config.server.port"] = new VariableValue(8080),
    ["config.server.host"] = new VariableValue("localhost")
};

var script = compiler.Compile("user.name + ':' + config.server.host", Compiler.Options.Immutable, variables);
script.Execute();
Console.WriteLine(script.StringValue);  // "John:localhost"
```

### イミュータブルモード

コンパイラは変数の扱いについて、2つのモードをサポートしています。

ミュータブルモード(デフォルト):
- 変数を変更できます

イミュータブルモード:
- すべての変数の変更を禁止します
- 代入演算子を使うとコンパイル時にエラーになります

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

### 変数コンテナのオーバーライド

実行時に`Execute()`へ異なる`IVariableContainer`を渡すと、変数の値を上書きできます。渡されたコンテナを最初に参照し、見つからない場合はコンパイル時のコンテナにフォールバックします。

グローバルな変数でコンパイルしておき、インスタンスごとのデータで何度も実行する、といった使い方ができます。

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

### 動的型付け

変数は動的に型付けされます。型はコンパイル時ではなく、実行時に決まります。

同じコンパイル済みスクリプトを、異なる型の変数で実行できます。たとえば、式`a + b`はデータに応じて数値の加算または文字列の連結を実行します:

```c#
var script = compiler.Compile("a + b", Compiler.Options.None, null);

// 浮動小数点数で実行 - 加算を実行
var floatVars = new DictionaryVariableContainer
{
  ["a"] = new VariableValue(1.5f),
  ["b"] = new VariableValue(2.3f)
};
script.Execute(floatVars);
Console.WriteLine(script.FloatValue);  // 3.8

// 文字列で実行(同じコンパイル済みスクリプト) - 連結を実行
var stringVars = new DictionaryVariableContainer
{
  ["a"] = new VariableValue("Hello"),
  ["b"] = new VariableValue(" World")
};
script.Execute(stringVars);
Console.WriteLine(script.StringValue);  // "Hello World"
```

## 比較演算

比較演算子(`==`、`!=`、`<`、`<=`、`>`、`>=`)と論理演算子(`!`、`&&`、`||`)をサポートしています。

```c#
var compiler = new Compiler();
VariableId valId = "val";
var variables = new DictionaryVariableContainer { [valId] = new VariableValue(43.0f) };
var script = compiler.Compile(
    "val >= 0.0 && val < 50.0",
    Compiler.Options.Immutable,
    variables);
script.Execute();
Console.WriteLine(script.BooleanValue);  // True
```

## 関数

組み込み関数とカスタム関数を含みます。

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

### 関数の要件

カスタム関数は状態を変更できません。外部データの読み取りは許可されていますが、何かを変更することはできません。

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

カスタム関数は0〜5個のパラメータをサポートします。コンテキスト型関数はコンテキスト変数を最大3つ、スクリプトパラメータを最大3つまでサポートします。

### 組み込み関数

- 三角関数: `sin`、`cos`、`tan`、`asin`、`acos`、`atan`、`atan2`、`sinh`、`cosh`、`tanh`
- 数学関数: `sqrt`、`abs`、`floor`、`ceil`、`trunc`、`pow`、`min`、`max`
- 文字列関数: `lower`、`upper`、`len`
- ユーティリティ: `ifelse`(三項演算子の代替)

完全なリストは[Compiler.cs](https://github.com/aki-null/epsilon-script/blob/master/EpsilonScript/Compiler.cs)を参照してください。

### オーバーロード

関数は同じ名前で異なるパラメータの型や数を持つことができます。

`abs`、`min`、`max`、`ifelse`などの組み込み関数はオーバーロードを使用しています。

### 決定的関数

関数を**決定的**（同じ入力=同じ出力）としてマークすることで、コンパイル時最適化を有効にできます。

`isDeterministic: true`を渡すことで有効になります:

```c#
// 決定的関数 - 同じ入力には常に同じ出力
CustomFunction.Create("sin", (float v) => MathF.Sin(v), isDeterministic: true)

CustomFunction.Create("clamp", (float val, float min, float max) =>
    Math.Max(min, Math.Min(max, val)), isDeterministic: true)
```

定数パラメータを持つ決定的関数は、コンパイル時に評価されます:

```c#
compiler.AddCustomFunction(
    CustomFunction.Create("sin", (float v) => MathF.Sin(v), isDeterministic: true));

// コンパイル時に評価される - sin(1.5708)の結果(約1.0)がキャッシュされる
var script = compiler.Compile("sin(3.141592 / 2) * 10");
```

### メソッドグループ

ラムダの代わりにメソッドグループを使用できます:

```c#
public int GetScore(string level) => CalculateScore(level);

// ラムダの代わりにメソッドグループ
compiler.AddCustomFunction(CustomFunction.Create<string, int>("score", GetScore));
```

パラメータを持つメソッドグループには、明示的なジェネリック型パラメータが必要です（上記参照）。パラメータなしのメソッドグループの場合は不要です:

```c#
public int GetConstant() => 42;

// パラメータなし - 型推論が機能
compiler.AddCustomFunction(CustomFunction.Create("constant", GetConstant));
```

### コンテキスト型カスタム関数

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

### 一括関数登録

`AddCustomFunctionRange`を使用すると、複数の関数を一度に登録できます:

```c#
var compiler = new Compiler();

var functions = new[]
{
    CustomFunction.Create("double", (int x) => x * 2),
    CustomFunction.Create("triple", (int x) => x * 3),
    CustomFunction.Create("square", (int x) => x * x)
};

compiler.AddCustomFunctionRange(functions);

var script = compiler.Compile("double(5) + triple(3) + square(2)");
script.Execute();
Console.WriteLine(script.IntegerValue); // 10 + 9 + 4 = 23
```

## 文字列

ダブルクォート（`"..."`）とシングルクォート（`'...'`）の両方で文字列を扱えます:

```
"Hello World"
'Hello World'
"It's working"
'He said "hello"'
```

注意: エスケープシーケンスはサポートされていません。バックスラッシュやその他の特殊文字はそのまま扱われます。これにより、パスを簡単に記述できます。

### 文字列の操作

文字列は連結、数値との組み合わせ、比較をサポートしています:

```c#
// 連結
"Hello " + "World"  // "Hello World"

// 文字列と数値の組み合わせ
"Debug: " + 128  // "Debug: 128"

// 比較
"Hello" == "Hello"  // true
```

### 関数での文字列の使用

```c#
var compiler = new Compiler();
compiler.AddCustomFunction(CustomFunction.Create("read_save_data",
  (string flag) => SaveData.Instance.GetIntegerData(flag)));
var script = compiler.Compile("read_save_data('LVL00_PLAYCOUNT') > 5", Compiler.Options.Immutable);
script.Execute();
Console.WriteLine(script.BooleanValue);  // True (GetIntegerDataが10を返す場合)
```

## 式の連続実行

セミコロン(`;`)を使うと、複数の式を順番に実行できます。結果は最後の式の値になります。

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

## 数値精度

コンパイラを作成する際に、整数と浮動小数点の精度を指定できます。

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

すべての演算は、設定した精度で自動的に実行されます。

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

型が一致しない場合、実行時エラーになります。なお、整数の引数は必要に応じて浮動小数点型に自動変換されます。

## ヒープアロケーション

EpsilonScriptは、コンパイル後の実行時にはメモリ割り当てが発生しないよう設計されていますが、いくつか例外があります:

文字列連結で変数を含む場合はメモリを割り当てます:
```csharp
'Debug: ' + variable  // 実行時にメモリ割り当て
```

`ToString()`呼び出しによる型変換はメモリを割り当てます:
```csharp
stringVar = 42  // ToString()を呼び出し、メモリ割り当て
```

カスタム関数は、実装次第でメモリを割り当てます。

定数はコンパイル時に最適化されます:
```csharp
'BUILD_FLAG_' + 4  // 'BUILD_FLAG_4'に最適化、実行時にメモリ割り当てなし
```

### ゼロアロケーションの強制

`Compiler.Options.NoAlloc`を使用すると、実行時にメモリを割り当てる操作がある場合にRuntimeExceptionをスローします:

```csharp
// メモリ割り当てなし:
var script = compiler.Compile("x * 2 + 1", Compiler.Options.NoAlloc, variables);
script.Execute();

// RuntimeExceptionをスロー:
compiler.Compile("'Debug: ' + variable", Compiler.Options.NoAlloc, variables).Execute();
compiler.Compile("stringVar = 42", Compiler.Options.NoAlloc, variables).Execute();

// 例外なし - 定数に最適化される:
compiler.Compile("'BUILD_FLAG_' + 4", Compiler.Options.NoAlloc).Execute();
```

NoAllocモードはカスタム関数の内部を検証しません。

## スレッドセーフティ

各スレッドごとに`Compiler`インスタンスを作成してください。

### 推奨パターン

各スレッドで独自の`Compiler`、`CompiledScript`、および`DictionaryVariableContainer`を作成します:

```csharp
Parallel.For(0, 100, i =>
{
    var compiler = new Compiler();
    var variables = new DictionaryVariableContainer
    {
        ["x"] = new VariableValue(10),
        ["y"] = new VariableValue(i)
    };
    var script = compiler.Compile("x + y", Compiler.Options.Immutable, variables);
    script.Execute();
    Console.WriteLine(script.IntegerValue);
});
```

### 危険なパターン

**Compilerを複数スレッドで共有:**
```csharp
var compiler = new Compiler(); // 危険: 複数スレッドで共有されています
Parallel.For(0, 100, i =>
{
    var variables = new DictionaryVariableContainer
    {
        ["x"] = new VariableValue(10),
        ["y"] = new VariableValue(i)
    };
    var script = compiler.Compile("x + y", Compiler.Options.Immutable, variables); // データ競合が発生します
});
```

**CompiledScriptを複数スレッドで実行:**
```csharp
var compiler = new Compiler();
var script = compiler.Compile("x + y", Compiler.Options.Immutable); // 危険: 実行状態が共有されます
Parallel.For(0, 100, i =>
{
    var variables = new DictionaryVariableContainer
    {
        ["x"] = new VariableValue(10),
        ["y"] = new VariableValue(i)
    };
    script.Execute(variables); // 予期しない結果になります
});
```

### ガイドライン

- 各スレッドで新しい`Compiler`インスタンスを作成してください
- 各スレッドで新しい`CompiledScript`を作成してください
- 各スレッドで新しい`DictionaryVariableContainer`を作成してください

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

#### 前提

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
