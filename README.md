# EpsilonScript

EpsilonScript is an embeddable expression engine for C# with functions and allocation-free execution.

It targets .NET Standard 2.1.

**Basic expressions:**
```c#
var compiler = new Compiler();
var script = compiler.Compile("10 + 20 * 2");
script.Execute();
Console.WriteLine(script.IntegerValue); // 50
```

**With variables:**
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

**Swapping variable containers:**
```c#
// Compile once
var script = compiler.Compile("health - damage", Compiler.Options.Immutable);

// Execute with different containers
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

**With custom functions:**
```c#
// Weapon effectiveness table - too complex to express inline
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

## Table of Contents

- [Features](#features)
- [Installation](#installation)
  - [Unity](#unity)
- [Project State](#project-state)
- [Samples](#samples)
  - [Arithmetic](#arithmetic)
  - [Variables](#variables)
  - [Comparison](#comparison)
  - [Functions](#functions)
  - [Strings](#strings)
  - [Expression Sequencing](#expression-sequencing)
- [Numeric Precision](#numeric-precision)
- [Heap Allocations](#heap-allocations)
- [Use Cases](#use-cases)
- [Context](#context)

## Features
- Mathematical expressions
- Boolean expressions
- Variables with read/write support
- Intentionally simple syntax
- Unity support
- No heap allocation after compilation (with exceptions)
- Configurable numeric precision

## Installation

### Unity

Add the package via Unity Package Manager:

1. Open **Window > Package Manager**
2. Click the **+** button in the top-left corner
3. Select **Add package from git URL**
4. Enter: `https://github.com/aki-null/epsilon-script-unity.git`

Alternatively, add it to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.akinull.epsilonscript": "https://github.com/aki-null/epsilon-script-unity.git"
  }
}
```

## Project State
All features should be stable for production use.

See [changelog](CHANGELOG.md) for release notes and version history.

## Samples

### Arithmetic

Basic arithmetic operators (`+`, `-`, `*`, `/`, `%`) and parentheses (`(`, `)`) are supported.

#### Code

```c#
var compiler = new Compiler();
var script = compiler.Compile("(1 + 2 + 3 * 2) * 2", Compiler.Options.Immutable);
script.Execute();
Console.WriteLine(script.IntegerValue);
```

#### Result

```
18
```

### Variables

Variables can be read and assigned (`=`). Compound assignment operators (`+=`, `-=`, `*=`, `/=`) are also supported.

Variables are stored in an `IVariableContainer`. Use `DictionaryVariableContainer` for a simple implementation.

#### Code

```c#
var compiler = new Compiler();
VariableId valId = "val"; // Implicit conversion from string
var variables = new DictionaryVariableContainer { [valId] = new VariableValue(43.0f) };
var script = compiler.Compile("val = val * 10.0", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables[valId].FloatValue);
```

#### Result

```
430.0
```

The `VariableId` struct provides type safety and optimal performance by using unique integer identifiers internally while maintaining a simple string-based interface. This is the recommended way to work with variables for performance-critical code.

Variables cannot be defined within scripts. This prevents expressions from becoming too complex.

#### String-based Variable Access

For simpler code where performance is less critical, you can use strings directly without `VariableId`:

```c#
var compiler = new Compiler();
var variables = new DictionaryVariableContainer { ["val"] = new VariableValue(43.0f) };
var script = compiler.Compile("val = val * 10.0", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables["val"].FloatValue);
```

Note: Direct string usage involves implicit conversion that is slower than using `VariableId`. Use `VariableId` when performance matters.

#### Immutable Mode

The compiler supports two modes for variable handling:

Mutable Mode (Default):
- Allows variable assignment and modification

Immutable Mode:
- Prevents all variable modification operations
- Throws an exception at compile time if assignment operators are used

```c#
// Immutable mode - only reads variables
var script1 = compiler.Compile("health - damage", Compiler.Options.Immutable, variables);
script1.Execute(); // Works

// Mutable mode - can modify variables
var script2 = compiler.Compile("health -= damage", Compiler.Options.None, variables);
script2.Execute(); // Works

// Immutable mode with assignment - throws exception at compile time
var script3 = compiler.Compile("health -= damage", Compiler.Options.Immutable, variables); // Exception!
```

#### Variable Container Override

Scripts can override variables at execution time by passing an `IVariableContainer` to `Execute()`. The override container is checked first; if a variable isn't found, it falls back to the compile-time container.

Compile once with global variables, execute multiple times with instance-specific overrides.

```c#
// Compile with global config
var globals = new DictionaryVariableContainer
{
  ["shipping_fee"] = new VariableValue(5.99f),
  ["tax_rate"] = new VariableValue(0.08f)
};
var script = compiler.Compile("price + shipping_fee + price * tax_rate",
                               Compiler.Options.None, globals);

// Execute for each instance
foreach (var user in users)
{
  var instanceVars = new DictionaryVariableContainer
  {
    ["price"] = new VariableValue(user.CartTotal)  // Override with instance value
    // shipping_fee and tax_rate fall back to globals
  };
  script.Execute(instanceVars);
  Console.WriteLine($"Total: ${script.FloatValue}");
}
```

#### Dynamic Typing

Variables are dynamically typed - their type is determined at runtime from the `VariableValue`, not at compile time. The same compiled script can execute with different types.

```c#
var script = compiler.Compile("a + b", Compiler.Options.None, null);

// Execute with floats
var floatVars = new DictionaryVariableContainer
{
  ["a"] = new VariableValue(1.5f),
  ["b"] = new VariableValue(2.3f)
};
script.Execute(floatVars);
Console.WriteLine(script.FloatValue);  // 3.8

// Execute with strings (same script)
var stringVars = new DictionaryVariableContainer
{
  ["a"] = new VariableValue("Hello"),
  ["b"] = new VariableValue(" World")
};
script.Execute(stringVars);
Console.WriteLine(script.StringValue);  // "Hello World"
```

### Comparison

Comparison operators (`==`, `!=`, `<`, `<=`, `>`, `>=`) and logical operators (`!`, `&&`, `||`) are supported.

#### Code

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

#### Result

```
True
```

### Functions

EpsilonScript supports built-in functions and custom functions.

```c#
var compiler = new Compiler();
compiler.AddCustomFunction(
    CustomFunction.Create("clamp", (float val, float min, float max) =>
        Math.Max(min, Math.Min(max, val))));

var variables = new DictionaryVariableContainer { ["damage"] = new VariableValue(50) };
var script = compiler.Compile("clamp(damage * 1.5, 10, 100)", Compiler.Options.Immutable, variables);
script.Execute();
Console.WriteLine(script.FloatValue); // 75
```

#### Function Requirements

Custom functions must not mutate state. Functions can read external data but cannot modify anything.

```c#
// Allowed: pure calculation
CustomFunction.Create("square", (float x) => x * x)

// Allowed: read-only external access
CustomFunction.Create("get_health", () => player.Health)

// Allowed: non-deterministic but no mutation
CustomFunction.Create("rand", (float max) => Random.Range(0.0f, max))

// Forbidden: mutates external state
CustomFunction.Create("set_score", (int score) => { gameState.Score = score; return score; })
```

### Built-in Functions

- Trigonometric: `sin`, `cos`, `tan`, `asin`, `acos`, `atan`, `atan2`, `sinh`, `cosh`, `tanh`
- Math: `sqrt`, `abs`, `floor`, `ceil`, `trunc`, `pow`, `min`, `max`
- String: `lower`, `upper`, `len`
- Utility: `ifelse` (ternary operator alternative)

The complete list can be found in [Compiler.cs](https://github.com/aki-null/epsilon-script/blob/master/EpsilonScript/Compiler.cs).

#### Overloading

Functions can be overloaded with different signatures (parameter types and counts) using the same name.

Built-in functions like `abs`, `min`, `max`, and `ifelse` use overloading.

#### Constant Functions

Custom functions can be marked as constant. Constant functions always return the same result for the same inputs. Results are cached at compilation time for performance.

A constant function can be created by passing `true` to the `isConstant` constructor parameter:

```c#
CustomFunction.Create("sin", (float v) => (float)System.Math.Sin(v), true)
```

For example, `sin(3.141592 / 2)` is cached at compilation because `sin` is marked as constant.

#### Method Groups

You can use method groups instead of lambdas:

```c#
public int GetScore(string level) => CalculateScore(level);

// Method group instead of lambda
compiler.AddCustomFunction(CustomFunction.Create<string, int>("score", GetScore));
```

**Note:** Method groups with parameters require explicit generic type parameters. However, zero-parameter method groups work without explicit generics:

```c#
public int GetConstant() => 42;

// Zero-parameter method group - no explicit generics needed
compiler.AddCustomFunction(CustomFunction.Create("constant", GetConstant));
```

#### Contextual Custom Functions

Contextual functions read variables from the execution environment without requiring them as parameters.

```c#
var compiler = new Compiler();

// Function reads 'day' from context
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

Functions can combine context variables with script parameters:

```c#
compiler.AddCustomFunction(
    CustomFunction.CreateContextual(
        "IsAfter",
        "currentDay",
        (int current, int target) => current > target));

var script = compiler.Compile("IsAfter(5)", Compiler.Options.Immutable, variables);
```

Supports up to 3 context variables and 3 script parameters.

### Strings

Strings are supported, primarily for function parameters.

#### Code

```c#
var compiler = new Compiler();
compiler.AddCustomFunction(CustomFunction.Create("read_save_data",
  (string flag) => SaveData.Instance.GetIntegerData(flag)));
var script = compiler.Compile(@"read_save_data(""LVL00_PLAYCOUNT"") > 5");
script.Execute();
Console.WriteLine(script.BooleanValue);
```

#### Result

If `SaveData.Instance.GetIntegerData("LVL00_PLAYCOUNT")` returns 10:
```
True
```

String concatenation is supported:

#### Code

```
"Hello " + "World"
```

#### Result

```
"Hello World"
```

Strings can be concatenated with numbers:

#### Code

```
"Debug: " + 128
```

#### Result

```
"Debug: 128"
```

String comparison is supported:

#### Code

```
"Hello" == "Hello"
```

#### Result

```
true
```

### Expression Sequencing

The semicolon operator (`;`) sequences multiple expressions. The result is the last expression's value.

#### Code

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
Console.WriteLine(script.IntegerValue); // 26 (x is 6, y is 20)
```

#### Result

```
26
```

## Numeric Precision

Configure numeric precision when creating the compiler to control integer and floating-point types used in expressions.

### Precision Options

**Integer Precision:**
- `Integer` (default): 32-bit int
- `Long`: 64-bit long

**Float Precision:**
- `Float` (default): 32-bit float
- `Double`: 64-bit double
- `Decimal`: 128-bit decimal

### Usage

```c#
// Default: 32-bit int and float
var compiler = new Compiler();

// High precision: 64-bit long and 128-bit decimal
var preciseCompiler = new Compiler(
    Compiler.IntegerPrecision.Long,
    Compiler.FloatPrecision.Decimal);

var script = preciseCompiler.Compile("0.1 + 0.2");
script.Execute();
Console.WriteLine(script.DecimalValue); // 0.3
```

All operations automatically promote to configured precision.

### Custom Functions and Precision

Custom function parameter types must match the compiler's configured precision:

```c#
// Compiler uses Double precision
var compiler = new Compiler(
    Compiler.IntegerPrecision.Integer,
    Compiler.FloatPrecision.Double);

// Function must use double, not float
compiler.AddCustomFunction(CustomFunction.Create("calc", (double x) => x * 2.5));

var script = compiler.Compile("calc(10.5)");
script.Execute();
Console.WriteLine(script.DoubleValue); // 26.25
```

If parameter types don't match, you'll get a runtime error. Integer arguments automatically convert to the configured float type when needed.

## Heap Allocations

Heap allocations are a concern for games.

EpsilonScript avoids allocations after compilation, with a few exceptions:

### String Concatenations

String concatenation causes heap allocations:
```
"Debug: " + i
```
where `i` is a variable.

Constant string concatenation happens at compilation and produces no garbage:
```
"Debug: " + 42 * 42
```

### Custom Functions

Custom functions that allocate memory will produce garbage when called.

## Use Cases

### Condition Validator

Game designers often need to express conditions. For example, a character might act differently based on player actions:

```
monsters_fought == 0 && has_key
```

## Development

### T4 Template Code Generation

Custom function implementations use T4 templates to reduce maintenance burden and ensure consistency.

**Generated files**:
- `EpsilonScript/Function/CustomFunction.Generated.cs`
- `EpsilonScript/Function/CustomFunction.Contextual.Generated.cs`

**Template files**:
- `EpsilonScript/Function/CustomFunction.Generated.tt`
- `EpsilonScript/Function/CustomFunction.Contextual.Generated.tt`

#### Prerequisites

Install the T4 command-line tool:
```bash
dotnet tool install -g dotnet-t4
```

#### Regenerating Code

After modifying templates, regenerate the code:
```bash
cd EpsilonScript/Function
t4 CustomFunction.Generated.tt
t4 CustomFunction.Contextual.Generated.tt
```

## Context

Scripting engines like Lua provide maximum freedom but aren't always feasible. Games need frequent updates with minimal regression risk.

Data-driven approaches (Excel, Google Sheets, Unity serialization) offer control but make complex expressions cumbersome or impossible. EpsilonScript provides expression power in a controlled environment.

Node-based visual scripting can make complex expressions difficult to read and understand. Text expressions are faster to write and easier to read.

Features that complicate reading are avoided. For example, the ternary operator isn't implemented—use the `ifelse` function for clearer syntax.
