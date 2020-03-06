# EpsilonScript

EpsilonScript is an interpreter to evaluate a simple expression written in C#.

It targets .NET Standard 2.0.

## Features
- Can express simple mathematical expressions
- Can express conditions (i.e. boolean expression)
- Variables with read/write support
- Simple syntax without too much flexibility (yes, this is a feature)
- Supports Unity
- No heap allocation after compilation

## Project State
Documentation is still lacking, but the core features are usable. There are not enough tests for some components of the software, which is being worked on now.

## Samples

### Arithmetics

Basic arithmetic operators (`+`, `-`, `*`, `/`, `%`) are supported. Parentheses (`(`, `)`) are recognized too.

#### Code

```c#
var compiler = new Compiler();
var script = compiler.Compile("(1 + 2 + 3 * 2) * 2", Compiler.Options.Immutable);
script.Execute();
Console.WriteLine(script.IntgerValue);
```

#### Result

```
18
```

### Variables

Variables can be read, and also be assigned to (`=`). Compound assignment operators can be used too (`+=`, `-=`, `*=`, `/=`).

#### Code

```c#
var compiler = new Compiler();
var variables = new Dictionary<string, VariableValue> {["val"] = new VariableValue(43.0f)};
var script = compiler.Compile("val = val * 10.0", 
Compiler.Options.None, variables);
script.Execute();
Console.WriteLine(variables["val"].FloatValue);
```

#### Result

```
430.0
```

`Compiler.Options.Immutable` flag prevents mutating expression from compiling.

Variables cannot be defined inside the script. This is done on purpose to prevent the expression from doing "too much".

### Comparison

Basic comparison operators are supported (`==`, `!=`, `<`, `<=`, `>`, `>=`) as well as logical operators (`!`, `&&` `||`).

#### Code

```c#
var compiler = new Compiler();
var variables = new Dictionary<string, VariableValue> {["val"] = new VariableValue(43.0f)};
var script = compiler.Compile("val >= 0.0 && val < 50.0", 
Compiler.Options.Immutable, variables);
script.Execute();
Console.WriteLine(script.BooleanValue);
```

#### Result

```
True
```

### Functions

Functions are supported in EpsilonScript. There are built-in functions, but custom functions can be defined too.

#### Code

```c#
var compiler = new Compiler();
compiler.AddCustomFunction(new CustomFunction("rand", (float d) => Random.Range(0.0f, d)));
var script = compiler.Compile("rand(0, 10)", Compiler.Options.Immutable);
script.Execute();
Console.WriteLine(script.FloatValue);
```

#### Result

```
3.1
```

The list of default built-in functions can be found [here](https://github.com/aki-null/epsilon-script/blob/master/EpsilonScript/Compiler.cs).

#### Overloading

Functions can be overloaded, which means you can define a function with different signatures (including types and number of parameters) with the same name.

Few built-in functions such as `abs`, `min`, `max`, and `ifelse` use this feature.

#### Constant Functions

A custom function can be flagged as a constant function. A constant function must always return the same result given that the input parameters do not change. The execution result of a constant function with constant parameters is cached on a compilation for performance optimization.

A constant function can be created by passing `true` to the `isConstant` constructor parameter:

```c#
new CustomFunction("sin", (float v) => (float) System.Math.Sin(v), true)
```

For example, a result of the following expression is cached on a compilation, because the built-in `sin` function is marked as constant: `sin(3.141592 / 2)`

## Use Cases

### Condition Validator

In many games, game designers often want to express some kind of condition. For example, there may be a scenario where a character should act differently depending on what the player has done.

```
monsters_fought == 0 && has_key
```

## Context

It is common to use a scripting engine like Lua to let the game designers write the game logic for maximum freedom, but that is not always feasible. Many games require frequent content updates after the first release and are typically maintained for many years with minimum software regression.

Data-driven approach (e.g. Microsoft Excel, Google Spreadsheet, Unity serialization, etc) is often chosen, which trades freedom with a more controlled environment, but entering complex expression can become cumbersome or even impossible. EpsilonScript is one of my attempts to empower the game designers with a strength of expression in such an environment.

On a different note, an expression can also be difficult to express with node-based visual scripting. A complex expression often requires multiple nodes to implement, and are difficult to intuitively understand what they do. Being able to simply write an expression in text is much faster and more readable.

Finally, features that are hard to read, or can cause complication is avoided on purpose. For example, the ternary operator is not implemented (built-in `IfElse` function can be used to achieve the same result with clearer syntax).
