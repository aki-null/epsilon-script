# EpsilonScript Specification

## Overview
This document provides a specification for the EpsilonScript language.

## Lexical Grammar

### Character Classes
```bnf
<letter>           ::= "a".."z" | "A".."Z"
<digit>            ::= "0".."9"
<whitespace>       ::= " " | "\t" | "\n" | "\r"
```

### Identifiers and Keywords
```bnf
<identifier_start> ::= <letter> | "_"
<identifier_body>  ::= <identifier_start> | <digit> | "."
<identifier>       ::= <identifier_start> {<identifier_body>}

<boolean_literal>  ::= "true" | "false"
```

### Numeric Literals
```bnf
<integer>          ::= <digit> {<digit>}
<exponent_marker>  ::= "e" | "E"
<sign>             ::= "+" | "-"
<exponent>         ::= <exponent_marker> [<sign>] <integer>
<float>            ::= <integer> "." [<integer>] [<exponent>]
<number>           ::= <integer> | <float>
```
**Important Notes**:
- Integer literals cannot contain exponent notation during tokenization (e.g., "2e10" is invalid)
- Floats MUST start with digits (e.g., ".5" is invalid, must be "0.5")
- Float fractional part is optional (e.g., "10." is valid)

### String Literals
```bnf
<string>           ::= '"' {<any_char_except_quote>} '"'
<any_char_except_quote> ::= <any character except '"' or EOF>
```
**Important**: No escape sequences are supported. You cannot include quotes inside strings.

### Operators and Punctuation
```bnf
<arithmetic_op>    ::= "+" | "-" | "*" | "/" | "%"
<comparison_op>    ::= "==" | "!=" | "<" | "<=" | ">" | ">="
<logical_op>       ::= "&&" | "||"
<assignment_op>    ::= "=" | "+=" | "-=" | "*=" | "/="
<unary_op>         ::= "!" | "+" | "-"
<punctuation>      ::= "(" | ")" | "," | ";"
```
**Note**: There is no `%=` (modulo assignment) operator in the language.

## Syntactic Grammar

### Primary Expressions
```bnf
<primary>          ::= <identifier>
                     | <number>
                     | <string>
                     | <boolean_literal>
                     | "(" <expression> ")"
```

### Function Calls
```bnf
<function_call>    ::= <identifier> "(" [<argument_list>] ")"
<argument_list>    ::= <assignment_expr> {"," <assignment_expr>}
```

### Unary Expressions
```bnf
<unary_expr>       ::= <primary>
                     | <function_call>
                     | <unary_op> <unary_expr>
```

### Binary Expressions (Operator Precedence)
```bnf
<multiplicative>   ::= <unary_expr> {("*" | "/" | "%") <unary_expr>}
<additive>         ::= <multiplicative> {("+" | "-") <multiplicative>}
<comparison>       ::= <additive> {<comparison_op> <additive>}
<logical_and>      ::= <comparison> {"&&" <comparison>}
<logical_or>       ::= <logical_and> {"||" <logical_and>}
<assignment_expr>  ::= <logical_or> [<assignment_op> <assignment_expr>]
```

### Sequence Expressions
```bnf
<sequence_expr>    ::= <assignment_expr> {";" <assignment_expr>}
```

### Top-Level Expression
```bnf
<expression>       ::= <sequence_expr>
<program>          ::= <expression> EOF
```

## Operator Precedence and Associativity

| Precedence | Operators | Associativity | Description |
|------------|-----------|---------------|-------------|
| 8 (highest)| function(), !, unary +, unary - | Right | Function calls, negation, unary operators |
| 7 | *, /, % | Left | Multiplicative |
| 6 | +, - | Left | Additive |
| 5 | ==, !=, <, <=, >, >= | Left | Comparison |
| 4 | && | Left | Logical AND |
| 3 | \|\| | Left | Logical OR |
| 2 | =, +=, -=, *=, /= | Right | Assignment |
| 1 | , | Left | Comma (tuple creation) |
| 0 (lowest) | ; | Left | Semicolon (sequence) |

## Type System

### Value Types
```bnf
<type>             ::= "Integer" | "Float" | "Boolean" | "String"
```

**Note**: Tuples exist as expression constructs for multi-value results (comma-separated expressions) but are not distinct value types in the type system.

### Type Coercion Rules
1. **Arithmetic Operations**:
   - Integer ⊕ Integer → Integer
   - Float ⊕ Float → Float
   - Integer ⊕ Float → Float (implicit conversion)
   - String + String → String (concatenation)
   - String + Number → String (string conversion via ToString)
   - String + Boolean → String (converts to "true"/"false")
   - String with -, *, /, % → Runtime error (only + supported for strings)
   - Integer arithmetic operations allow overflow (wraparound behavior)

2. **Comparison Operations**:
   - == and !=: Work with all types but cannot mix different types
     - Numbers can be compared with numbers (int/float mixing allowed)
     - Float equality uses ULP-based comparison for precision handling
     - Strings can only be compared with strings
     - Booleans can only be compared with booleans
   - <, <=, >, >=: ONLY work with numeric types (int/float)
     - String ordering comparisons are NOT supported
   - Mixed type comparisons always throw runtime errors

3. **Logical Operations**:
   - Only operate on boolean values
   - Short-circuit evaluation for && and ||

4. **Assignment Operations**:
   - Simple assignment (=): Type coercion follows variable's declared type
   - Compound assignments (+=, -=, *=, /=): Only work with numeric types
     - String += String is NOT supported (use simple concatenation)
   - No %= operator exists

## Semantic Rules

### Variable Rules
1. Variables must be declared in the container before use
2. Variables cannot be defined within expressions
3. Assignment operators require left-hand side to be a variable
4. In immutable mode (Compiler.Options.Immutable), assignments are forbidden

### Function Rules
1. Functions are identified by name and parameter type signature
2. Function overloading is supported based on parameter types
3. Functions can be marked as constant for optimization
4. Built-in functions are pre-registered in the compiler
5. Custom functions support 1-5 parameters

### Expression Evaluation
1. Expressions are evaluated left-to-right with operator precedence
2. Parentheses override default precedence
3. Sequences (semicolon-separated) return the last expression's value
4. Tuples (comma-separated) create multi-value results

### Short-Circuit Evaluation
- `&&` operator: if left operand is false, right is not evaluated
- `||` operator: if left operand is true, right is not evaluated

## Built-in Functions

### Mathematical Functions
```bnf
sin(float) → float
cos(float) → float
tan(float) → float
asin(float) → float
acos(float) → float
atan(float) → float
sinh(float) → float
cosh(float) → float
tanh(float) → float
atan2(float, float) → float
sqrt(float) → float
abs(int) → int
abs(float) → float
floor(float) → float
ceil(float) → float
trunc(float) → float
min(int, int) → int
min(float, float) → float
max(int, int) → int
max(float, float) → float
pow(float, float) → float
```

### Conditional Function
```bnf
ifelse(bool, int, int) → int
ifelse(bool, float, float) → float
ifelse(bool, string, string) → string
```

### String Functions
```bnf
lower(string) → string
upper(string) → string
len(string) → int
```

## Grammar Ambiguities and Resolutions

1. **Unary vs Binary Operators**: Plus and minus can be both unary and binary
   - Resolution: Context-dependent parsing based on previous token

2. **Function vs Variable**: Identifiers can be both
   - Resolution: Lookahead for '(' to distinguish

3. **Empty Function Arguments**: `func()` vs no arguments
   - Resolution: Null element inserted for empty parameter list
