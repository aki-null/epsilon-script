# EpsilonScript

EpsilonScript is an interpreter for evaluating simple expressions, written in C#.

It targets .NET Standard 2.0.

## Table of Contents

- [Features](#features)
- [Project State](#project-state)
- [Samples](#samples)
  - [Arithmetic](#arithmetic)
  - [Variables](#variables)
  - [Comparison](#comparison)
  - [Functions](#functions)
  - [Strings](#strings)
  - [Expression Sequencing](#expression-sequencing)
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

The `VariableId` struct provides type safety and optimal performance by using unique integer identifiers internally while maintaining a simple string-based interface.

The `Compiler.Options.Immutable` flag prevents expressions that modify variables from compiling.

Variables cannot be defined within scripts. This prevents expressions from becoming too complex.

#### Alternative: Direct String Usage

For simpler code, you can use strings directly:

```c#
var compiler = new Compiler();
var variables = new DictionaryVariableContainer { ["val"] = new VariableValue(43.0f) };
var script = compiler.Compile("val = val * 10.0", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables["val"].FloatValue);
```

### Comparison

Comparison operators (`==`, `!=`, `<`, `<=`, `>`, `>=`) and logical operators (`!`, `&&`, `||`) are supported.

#### Code

```c#
var compiler = new Compiler();
VariableId valId = "val";
var variables = new DictionaryVariableContainer { [valId] = new VariableValue(43.0f) };
var script = compiler.Compile("val >= 0.0 && val < 50.0", Compiler.Options.Immutable, variables);
script.Execute();
Console.WriteLine(script.BooleanValue);
```

#### Result

```
True
```

### Functions

EpsilonScript supports built-in functions and custom functions.

#### Code

```c#
var compiler = new Compiler();
compiler.AddCustomFunction(CustomFunction.Create("rand", (float d) => Random.Range(0.0f, d)));
var script = compiler.Compile("rand(0, 10)", Compiler.Options.Immutable);
script.Execute();
Console.WriteLine(script.FloatValue);
```

#### Result

```
3.1
```

Built-in functions include:

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

#### Upgrading Existing Code

Older releases used concrete custom function constructors. Use the factory helper instead:

```c#
// old
compiler.AddCustomFunction(new CustomFunction("foo", (float v) => v * 2));

// new
compiler.AddCustomFunction(CustomFunction.Create("foo", (float v) => v * 2));
```

This change allows any parameter types in custom functions. The factory supports one to five parameters with an optional `isConstant` flag.


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
var script = compiler.Compile("x = x + 1; y = y * 2; x + y", Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(script.IntegerValue); // 26 (x is 6, y is 20)
```

#### Result

```
26
```

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

## Context

Scripting engines like Lua provide maximum freedom but aren't always feasible. Games need frequent updates with minimal regression risk.

Data-driven approaches (Excel, Google Sheets, Unity serialization) offer control but make complex expressions cumbersome or impossible. EpsilonScript provides expression power in a controlled environment.

Node-based visual scripting can make complex expressions difficult to read and understand. Text expressions are faster to write and easier to read.

Features that complicate reading are avoided. For example, the ternary operator isn't implemented—use the `ifelse` function for clearer syntax.
