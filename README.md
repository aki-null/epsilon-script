# EpsilonScript

[![Readme_ja](https://img.shields.io/badge/EpsilonScript-%E6%97%A5%E6%9C%AC%E8%AA%9E-E87A90)](https://github.com/aki-null/epsilon-script/blob/master/README_ja.md)

EpsilonScript is an embeddable expression engine for C# with functions and allocation-free execution.

Supports .NET Standard 2.1. See [changelog](CHANGELOG.md) for version history.

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
- [Arithmetic](#arithmetic)
- [Variables](#variables)
- [Comparison](#comparison)
- [Functions](#functions)
- [Strings](#strings)
- [Expression Sequencing](#expression-sequencing)
- [Numeric Precision](#numeric-precision)
- [Heap Allocations](#heap-allocations)
- [Thread Safety](#thread-safety)
- [Motivation](#motivation)
- [Development](#development)

## Features
- Simple syntax
- Arithmetic (`+`, `-`, `*`, `/`, `%`) and boolean operators (`&&`, `||`, `!`, comparisons)
- Dynamic typing with assignment operators (`=`, `+=`, `-=`, `*=`, `/=`, `%=`)
- String concatenation
- Custom functions with overloading
- Configurable numeric precision (int/long, float/double/decimal)
- Zero allocations after compilation (except string operations with variables)
- Immutable mode blocks variable changes; NoAlloc mode blocks runtime allocations
- Compile-time optimization (constant folding, deterministic functions, dead code elimination)
- Swap variable containers at runtime (compile once, execute with different data)
- Unity support

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

## Arithmetic

Supports basic operators (`+`, `-`, `*`, `/`, `%`) and parentheses.

```c#
var compiler = new Compiler();
var script = compiler.Compile("(1 + 2 + 3 * 2) * 2", Compiler.Options.Immutable);
script.Execute();
Console.WriteLine(script.IntegerValue);  // 18
```

## Variables

Variables support assignment (`=`) and compound operators (`+=`, `-=`, `*=`, `/=`, `%=`).

Variables are stored in an `IVariableContainer` (use `DictionaryVariableContainer` for a basic implementation).

```c#
var compiler = new Compiler();
VariableId valId = "val"; // Implicit conversion from string
var variables = new DictionaryVariableContainer { [valId] = new VariableValue(43.0f) };
var script = compiler.Compile("val = val * 10.0", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables[valId].FloatValue);  // 430.0
```

`VariableId` provides a simple string-like interface while using integer IDs internally for fast lookups. Recommended for performance-critical code.

Variables can't be defined in scripts - this keeps expressions simple.

### String-based Variable Access

If performance isn't critical, you can use strings directly:

```c#
var compiler = new Compiler();
var variables = new DictionaryVariableContainer { ["val"] = new VariableValue(43.0f) };
var script = compiler.Compile("val = val * 10.0", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables["val"].FloatValue);
```

String lookups are slower than `VariableId` because of internal conversion overhead.

### Variables with Periods

Variable names can contain periods (`.`) for organizing related values:

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

### Immutable Mode

Two modes for variables:

Mutable Mode (Default):
- Variables can be modified

Immutable Mode:
- Blocks all variable modifications
- Compile-time error if you use assignment operators

```c#
// Immutable mode - only reads variables
var script1 = compiler.Compile("health - damage", Compiler.Options.Immutable, variables);
script1.Execute(); // Works

// Mutable mode - can modify variables
var script2 = compiler.Compile("health -= damage", Compiler.Options.None, variables);
script2.Execute(); // Works

// Immutable mode with assignment - throws exception at compile time
var script3 = compiler.Compile(
    "health -= damage", Compiler.Options.Immutable, variables); // Exception!
```

### Variable Container Override

Pass a different `IVariableContainer` to `Execute()` to override variables at runtime. It checks the override container first, then falls back to the compile-time container.

Useful pattern: compile once with globals, execute many times with per-instance data.

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

### Dynamic Typing

Variable types are determined at runtime, not compile time.

Compile once and execute with different types. The expression `a + b` adds numbers or concatenates strings depending on the data:

```c#
var script = compiler.Compile("a + b", Compiler.Options.None, null);

// Execute with floats - performs addition
var floatVars = new DictionaryVariableContainer
{
  ["a"] = new VariableValue(1.5f),
  ["b"] = new VariableValue(2.3f)
};
script.Execute(floatVars);
Console.WriteLine(script.FloatValue);  // 3.8

// Execute with strings (same compiled script) - performs concatenation
var stringVars = new DictionaryVariableContainer
{
  ["a"] = new VariableValue("Hello"),
  ["b"] = new VariableValue(" World")
};
script.Execute(stringVars);
Console.WriteLine(script.StringValue);  // "Hello World"
```

## Comparison

Supports comparison (`==`, `!=`, `<`, `<=`, `>`, `>=`) and logical operators (`!`, `&&`, `||`).

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

## Functions

Includes built-in functions and custom functions.

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

### Function Requirements

Custom functions can't mutate state. Read external data is fine, but don't modify anything.

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

Custom functions support 0-5 parameters. Contextual functions support up to 3 context variables and 3 script parameters.

### Built-in Functions

- Trigonometric: `sin`, `cos`, `tan`, `asin`, `acos`, `atan`, `atan2`, `sinh`, `cosh`, `tanh`
- Math: `sqrt`, `abs`, `floor`, `ceil`, `trunc`, `pow`, `min`, `max`
- String: `lower`, `upper`, `len`
- Utility: `ifelse` (ternary operator alternative)

See [Compiler.cs](https://github.com/aki-null/epsilon-script/blob/master/EpsilonScript/Compiler.cs) for the complete list.

### Overloading

Functions can share the same name with different parameter types or counts.

Built-in functions like `abs`, `min`, `max`, and `ifelse` use overloading.

### Deterministic Functions

Mark functions as **deterministic** (same inputs = same output) to enable compile-time optimization.

Pass `isDeterministic: true` to enable:

```c#
// Deterministic - same input always produces same output
CustomFunction.Create("sin", (float v) => MathF.Sin(v), isDeterministic: true)

CustomFunction.Create("clamp", (float val, float min, float max) =>
    Math.Max(min, Math.Min(max, val)), isDeterministic: true)
```

Deterministic functions with constant parameters get evaluated at compile time:

```c#
compiler.AddCustomFunction(
    CustomFunction.Create("sin", (float v) => MathF.Sin(v), isDeterministic: true));

// Evaluated at compile time - sin(1.5708) is cached as ~1.0
var script = compiler.Compile("sin(3.141592 / 2) * 10");
```

### Method Groups

You can use method groups instead of lambdas:

```c#
public int GetScore(string level) => CalculateScore(level);

// Method group instead of lambda
compiler.AddCustomFunction(CustomFunction.Create<string, int>("score", GetScore));
```

Method groups with parameters need explicit generic type parameters (as shown above). Zero-parameter method groups work without them:

```c#
public int GetConstant() => 42;

// Zero parameters - type inference works
compiler.AddCustomFunction(CustomFunction.Create("constant", GetConstant));
```

### Contextual Custom Functions

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

### Bulk Function Registration

Use `AddCustomFunctionRange` to register multiple functions at once:

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

## Strings

Strings work with both double (`"..."`) and single (`'...'`) quotes:

```
"Hello World"
'Hello World'
"It's working"
'He said "hello"'
```

Note: No escape sequences are supported. Backslashes and other special characters are treated as literal characters. This makes it easy to write paths.

### String Operations

Strings support concatenation, mixing with numbers, and comparison:

```c#
// Concatenation
"Hello " + "World"  // "Hello World"

// Mix strings and numbers
"Debug: " + 128  // "Debug: 128"

// Comparison
"Hello" == "Hello"  // true
```

### Using Strings in Functions

```c#
var compiler = new Compiler();
compiler.AddCustomFunction(CustomFunction.Create("read_save_data",
  (string flag) => SaveData.Instance.GetIntegerData(flag)));
var script = compiler.Compile("read_save_data('LVL00_PLAYCOUNT') > 5", Compiler.Options.Immutable);
script.Execute();
Console.WriteLine(script.BooleanValue);  // True (if GetIntegerData returns 10)
```

## Expression Sequencing

Use semicolons (`;`) to run multiple expressions. Returns the last expression's value.

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

## Numeric Precision

Set integer and floating-point precision when creating the compiler.

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

Operations use the configured precision automatically.

### Custom Functions and Precision

Match your custom function types to the compiler's precision:

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

Mismatched types throw a runtime error. Integers auto-convert to floats when needed.

## Heap Allocations

EpsilonScript avoids allocations after compilation, with a few exceptions:

String concatenation with variables allocates:
```csharp
'Debug: ' + variable  // Allocates at runtime
```

`ToString()` calls from type conversions allocate:
```csharp
stringVar = 42  // Calls ToString(), allocates
```

Custom functions allocate if their implementation allocates.

Constants get optimized away at compile time:
```csharp
'BUILD_FLAG_' + 4  // Optimized to 'BUILD_FLAG_4', no runtime allocation
```

### Enforcing Zero Allocations

`Compiler.Options.NoAlloc` throws RuntimeException if code allocates at runtime:

```csharp
// No allocations:
var script = compiler.Compile("x * 2 + 1", Compiler.Options.NoAlloc, variables);
script.Execute();

// Throws RuntimeException:
compiler.Compile("'Debug: ' + variable", Compiler.Options.NoAlloc, variables).Execute();
compiler.Compile("stringVar = 42", Compiler.Options.NoAlloc, variables).Execute();

// No exception - optimized to constant:
compiler.Compile("'BUILD_FLAG_' + 4", Compiler.Options.NoAlloc).Execute();
```

NoAlloc mode does not validate custom function internals.

## Thread Safety

Each thread needs its own `Compiler` instance.

### Recommended Pattern

Each thread creates its own `Compiler`, `CompiledScript`, and `DictionaryVariableContainer`:

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

### Unsafe Usage

**Sharing a Compiler across threads:**
```csharp
var compiler = new Compiler(); // UNSAFE: shared across threads
Parallel.For(0, 100, i =>
{
    var variables = new DictionaryVariableContainer
    {
        ["x"] = new VariableValue(10),
        ["y"] = new VariableValue(i)
    };
    var script = compiler.Compile("x + y", Compiler.Options.Immutable, variables); // Causes data races
});
```

**Sharing a CompiledScript across threads:**
```csharp
var compiler = new Compiler();
var script = compiler.Compile("x + y", Compiler.Options.Immutable); // UNSAFE: shared execution state
Parallel.For(0, 100, i =>
{
    var variables = new DictionaryVariableContainer
    {
        ["x"] = new VariableValue(10),
        ["y"] = new VariableValue(i)
    };
    script.Execute(variables); // May produce incorrect results
});
```

### Guidelines

- Create a new `Compiler` instance per thread
- Create a new `CompiledScript` per thread
- Create a new `DictionaryVariableContainer` per thread

## Motivation

Game designers need to express logic. A quest might require "the player fought no monsters and has the key". Damage calculation might depend on weapon type and armor type. These conditions are too complex for simple data tables but don't need full scripting languages.

The usual options don't fit well. Pure data (Excel, JSON, Unity serialization) means asking programmers to add new columns or special cases for each new rule. Full scripting languages (Lua, Python) give designers freedom to write anything—including infinite loops, performance problems, or code that breaks game systems in subtle ways.

EpsilonScript narrows the scope to just expressions. No loops. No variable declarations. Just evaluate an expression and return a result. This constraint is the point: designers can express complex calculations and conditions, but can't write code that spirals into maintenance problems.

Expressions need to run fast because they run often—hundreds of times per frame in game loops. Compilation happens once, producing a reusable script that executes without reparsing or allocating memory. The variable container pattern means you compile once, then execute it for every entity that needs that calculation.

The syntax deliberately omits features that hurt readability. No ternary operator—use `ifelse(condition, true_value, false_value)` instead. No implicit behaviors that require remembering special cases. Programmers control exactly what functions exist and what they do. The result is expressions that everyone on the team can read.

Within visual scripting systems, EpsilonScript handles the case where connecting nodes becomes tedious. Wiring up `base_damage * weapon_effectiveness(weapon, armor) * range_modifier` with individual nodes creates clutter. An expression node keeps the graph focused on control flow while delegating calculations to text, which is clearer for that purpose.

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
