using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.Parser
{
  /// <summary>
  /// Validates parser syntax rules.
  /// Throws ParserException for syntax and structural errors.
  /// Throws ArgumentException for programming errors (null parameters, invalid state).
  /// Note: Semantic/type errors throw RuntimeException (in AST nodes during Execute()).
  /// </summary>
  internal static class Validator
  {
    /// <summary>
    /// Validates an element placement against all applicable rules.
    /// Uses optimized dispatch based on element type.
    /// </summary>
    /// <param name="currentToken">The current token being validated</param>
    /// <param name="currentElementType">The element type of the current token</param>
    /// <param name="previousToken">The previous token (if any)</param>
    /// <param name="previousElementType">The element type of the previous token (if any)</param>
    /// <param name="parenthesisDepth">Current parenthesis nesting depth</param>
    /// <param name="parenthesisTypeStack">Stack tracking whether each open paren is for grouping or function calls</param>
    /// <exception cref="ParserException">Thrown when any validation rule fails</exception>
    public static void Validate(
      Token currentToken,
      ElementType currentElementType,
      Token? previousToken,
      ElementType? previousElementType,
      int parenthesisDepth,
      Stack<ParenthesisType> parenthesisTypeStack)
    {
      // Check value-related rules first (most common)
      if (currentElementType.IsValue())
      {
        ValidateAdjacentValue(currentToken, previousElementType);
      }

      // Check operator rules (second most common)
      if (currentElementType.IsBinaryOperator())
      {
        ValidateBinaryOperator(currentToken, previousToken, previousElementType);
      }

      // Check specific element types
      switch (currentElementType)
      {
        case ElementType.AssignmentOperator:
        case ElementType.AssignmentAddOperator:
        case ElementType.AssignmentSubtractOperator:
        case ElementType.AssignmentMultiplyOperator:
        case ElementType.AssignmentDivideOperator:
          ValidateAssignmentLValue(currentToken, previousElementType);
          break;

        case ElementType.Comma:
          ValidateCommaPlacement(currentToken, previousElementType, parenthesisTypeStack);
          break;

        case ElementType.LeftParenthesis:
          ValidateLeftParenthesis(currentToken, previousElementType);
          break;

        case ElementType.RightParenthesis:
          ValidateRightParenthesis(currentToken, parenthesisDepth, previousElementType);
          break;

        case ElementType.Function:
          ValidateFunctionPlacement(currentToken, previousElementType);
          break;
      }
    }

    #region Validation Rules

    /// <summary>
    /// Validates binary operator placement.
    ///
    /// Rule: Binary operators require a left operand and cannot follow another binary operator.
    ///
    /// Note: + and - are classified as either unary (PositiveOperator/NegativeOperator) or
    /// binary (AddOperator/SubtractOperator) in TokenParser.Process() BEFORE this validator runs.
    /// By the time this method is called, the element type has already been determined.
    /// This validator only runs when the operator was classified as binary.
    ///
    /// Valid:   "1 + 2"        (binary + has left operand)
    /// Valid:   "+ 1"          (+ classified as unary PositiveOperator, doesn't call this validator)
    /// Valid:   "1 + - 2"      (second - classified as unary NegativeOperator, parses as 1 + (-2))
    /// Invalid: "* 1"          (* has no unary form - binary operator at start)
    /// Invalid: "/ 2"          (/ has no unary form - binary operator at start)
    /// Invalid: "1 * / 2"      (consecutive operators where neither can be unary)
    /// Invalid: "x = = 5"      (consecutive binary operators)
    /// </summary>
    private static void ValidateBinaryOperator(Token currentToken, Token? previousToken,
      ElementType? previousElementType)
    {
      // Rule 1: Binary operators cannot be the first element
      if (!previousElementType.HasValue)
      {
        throw new ParserException(currentToken, $"Expression cannot start with binary operator '{currentToken.Text}'");
      }

      // Rule 2: Binary operators cannot follow other binary operators
      // This check occurs AFTER +/- signs have been classified as unary or binary.
      // If the previous element is a binary operator, we cannot have another binary operator.
      // Example: "1 + - 2" is valid because - was classified as NegativeOperator (unary), not SubtractOperator (binary)
      if (previousElementType.Value.IsBinaryOperator())
      {
        if (!previousToken.HasValue)
        {
          throw new ArgumentException("previousToken must have value when previousElementType has value",
            nameof(previousToken));
        }

        throw new ParserException(currentToken,
          $"Invalid consecutive operators: '{previousToken.Value.Text}' " + $"followed by '{currentToken.Text}'");
      }
    }

    /// <summary>
    /// Validates that values are not adjacent without operators.
    ///
    /// Rule: Values (literals, variables, expressions in parens) must be separated by operators.
    /// The grammar does not allow two values side by side, so any occurrence of adjacent values
    /// is invalid regardless of what follows.
    ///
    /// Valid:   "1 + 2"           (operator between values)
    /// Invalid: "1 2"             (adjacent literals)
    /// Invalid: "1 2 +"           (adjacent literals, even with operator after)
    /// Invalid: "x y"             (adjacent variables)
    /// Invalid: "(1) 2"           (value after parenthesized expression)
    /// Invalid: "(1)(2)"          (adjacent parenthesized expressions)
    /// </summary>
    private static void ValidateAdjacentValue(Token currentToken, ElementType? previousElementType)
    {
      if (!previousElementType.HasValue)
      {
        return; // No previous element - OK
      }

      var prev = previousElementType.Value;

      // Case 1: Value after closing parenthesis
      // Example: "(1)2" or "func()x"
      if (prev == ElementType.RightParenthesis)
      {
        throw new ParserException(currentToken, "Value cannot directly follow closing parenthesis without an operator");
      }

      // Case 2: Value after another value
      // Examples: "1 2", "x y", "true false", "1 2 +"
      // Grammar does not allow two values side by side, so always reject
      if (prev.IsValue())
      {
        throw new ParserException(currentToken,
          "Adjacent values require an operator between them (e.g., '1 + 2' not '1 2')");
      }
    }

    /// <summary>
    /// Validates assignment operator left-hand sides.
    ///
    /// Rule: Assignments can only be made to variables (lvalues), not to literals or expressions.
    ///
    /// Valid:   "x = 5"          (variable lvalue)
    /// Valid:   "count += 1"     (variable lvalue)
    /// Invalid: "1 = 2"          (literal lvalue)
    /// Invalid: "(x+1) = 2"      (expression lvalue)
    /// Invalid: "func() = x"     (function result lvalue)
    /// </summary>
    private static void ValidateAssignmentLValue(Token currentToken, ElementType? previousElementType)
    {
      // Rule 1: Assignment must have a previous element
      if (!previousElementType.HasValue)
      {
        throw new ParserException(currentToken,
          "Assignment operator requires a variable on the left side (e.g., 'x = 5' not '= 5')");
      }

      // Rule 2: Previous element must be a variable (lvalue)
      if (previousElementType != ElementType.Variable)
      {
        throw new ParserException(currentToken, "Assignment can only be to a variable, not to literals or expressions");
      }
    }

    /// <summary>
    /// Validates comma placement in function arguments.
    ///
    /// Rule: Commas can only be used to separate function parameters, not in grouping parentheses.
    ///
    /// Valid:   "func(1, 2, 3)"      (separating arguments in function call)
    /// Invalid: "(1, 2)"              (comma in grouping parentheses - creates tuple)
    /// Invalid: "x = (1, 2)"          (comma in grouping context)
    /// Invalid: "!(1, 2)"             (comma in grouping context)
    /// Invalid: ", 1"                 (leading comma)
    /// Invalid: "func(, 1)"           (comma after opening paren)
    /// Invalid: "func(1,)"            (trailing comma)
    /// Invalid: "func(1,, 2)"         (double comma)
    /// </summary>
    private static void ValidateCommaPlacement(Token currentToken, ElementType? previousElementType,
      Stack<ParenthesisType> parenthesisTypeStack)
    {
      // Rule 1: Cannot start expression with comma
      if (!previousElementType.HasValue)
      {
        throw new ParserException(currentToken,
          "Expression cannot start with a comma (commas separate function arguments)");
      }

      // Rule 2: Comma can only appear inside function calls, not grouping parentheses
      // This prevents tuples from being created in invalid contexts
      if (parenthesisTypeStack.Count > 0 && parenthesisTypeStack.Peek() == ParenthesisType.Grouping)
      {
        throw new ParserException(currentToken,
          "Comma can only be used to separate function parameters");
      }

      // Rule 3: Cannot have comma right after opening paren
      if (previousElementType == ElementType.FunctionStartParenthesis)
      {
        throw new ParserException(currentToken, "Unexpected comma - missing expression");
      }

      // Rule 4: Cannot have double comma
      if (previousElementType == ElementType.Comma)
      {
        throw new ParserException(currentToken, "Unexpected comma - missing expression");
      }
    }

    /// <summary>
    /// Validates left parenthesis placement.
    ///
    /// Rule: Opening parenthesis cannot directly follow closing parenthesis without an operator.
    ///
    /// Valid:   "(1) + (2)"       (operator between parenthesized groups)
    /// Invalid: "(1)(2)"          (adjacent parenthesized expressions)
    /// Invalid: "func()()"        (function calls cannot be chained - no function returning function)
    /// </summary>
    private static void ValidateLeftParenthesis(Token currentToken, ElementType? previousElementType)
    {
      // Check for adjacent parentheses: )(
      // This happens when we have expressions like (1)(2) - closing paren followed by opening paren
      if (previousElementType == ElementType.RightParenthesis)
      {
        throw new ParserException(currentToken,
          "Adjacent parenthesized expressions require an operator between them (e.g., '(1) + (2)' not '(1)(2)')");
      }
    }

    /// <summary>
    /// Validates right parenthesis placement and matching.
    ///
    /// Rule: Closing parenthesis must match opening parenthesis, grouping parens cannot be empty,
    /// and no trailing comma before closing paren.
    ///
    /// Valid:   "(1)"             (matched parenthesis)
    /// Valid:   "func()"          (empty function call - OK, None element inserted before this check)
    /// Invalid: "()"              (empty grouping parenthesis)
    /// Invalid: "func(1,)"        (trailing comma)
    /// Invalid: "1)"              (unmatched closing paren)
    /// </summary>
    private static void ValidateRightParenthesis(Token currentToken, int parenthesisDepth,
      ElementType? previousElementType)
    {
      // Rule 1: Check for extra closing parenthesis (no matching opening)
      if (parenthesisDepth <= 0)
      {
        throw new ParserException(currentToken, "Unopened closing parenthesis - missing opening parenthesis '('");
      }

      // Rule 2: Check for empty grouping parentheses (non-function)
      // Note: For empty function calls func(), a None element is inserted before the closing paren
      // in TokenParser, so this check only catches empty grouping parens () where previous is LeftParenthesis
      if (previousElementType == ElementType.LeftParenthesis)
      {
        throw new ParserException(currentToken,
          "Empty parentheses are not allowed (use them with functions like 'func()' or with expressions like '(1 + 2)')");
      }

      // Rule 3: Check for trailing comma before closing paren
      if (previousElementType == ElementType.Comma)
      {
        throw new ParserException(currentToken,
          "Trailing comma before closing parenthesis (e.g., use 'func(1, 2)' not 'func(1, 2,)')");
      }
    }

    /// <summary>
    /// Validates function placement - ensures a function doesn't directly follow a value without an operator.
    ///
    /// Rule: Functions must be separated from values by operators.
    ///
    /// Valid:   "x + func()"      (operator between value and function)
    /// Valid:   "func()"          (function at start)
    /// Invalid: "x func()"        (function directly after variable)
    /// Invalid: "1 func()"        (function directly after literal)
    /// Invalid: "true func()"     (function directly after boolean)
    /// </summary>
    private static void ValidateFunctionPlacement(Token currentToken, ElementType? previousElementType)
    {
      // Functions can appear after:
      // - Nothing (start of expression)
      // - Operators
      // - Opening parenthesis
      // - Comma (in function arguments)
      //
      // Functions CANNOT appear after:
      // - Values (numbers, booleans, strings, variables)
      // - Closing parenthesis

      if (!previousElementType.HasValue)
      {
        return; // Start of expression is fine
      }

      // Check if previous was a value or closing paren
      if (previousElementType.Value.IsValue() || previousElementType == ElementType.RightParenthesis)
      {
        throw new ParserException(currentToken,
          "Function call cannot directly follow an expression without an operator (e.g., 'x + func()' not 'x func()')");
      }
    }

    #endregion

    #region Expression End Validation

    /// <summary>
    /// Validates expression end state.
    /// Called when End() is invoked to ensure expression is complete.
    ///
    /// Rule: Expression must not end with operators or commas, and all parentheses must be closed.
    ///
    /// Valid:   "1 + 2"           (complete expression)
    /// Valid:   "+1"              (unary operator with operand - complete)
    /// Valid:   ""                (empty expression)
    /// Invalid: "1 +"             (trailing binary operator)
    /// Invalid: "+"               (unary operator without operand)
    /// Invalid: "1,"              (trailing comma)
    /// Invalid: "(1"              (unclosed parenthesis)
    /// </summary>
    public static void ValidateExpressionEnd(ElementType? lastElementType, Token lastToken, int parenthesisDepth)
    {
      // Check for unclosed parentheses
      if (parenthesisDepth > 0)
      {
        throw new ParserException(lastToken, "Unclosed parenthesis - missing closing parenthesis");
      }

      if (!lastElementType.HasValue)
      {
        return; // Empty expression - OK
      }

      var lastType = lastElementType.Value;

      switch (lastType)
      {
        // Check for trailing unary operators
        case ElementType.NegativeOperator:
        case ElementType.PositiveOperator:
        case ElementType.NegateOperator:
          throw new ParserException(lastToken, "Unary operator requires an operand");
        // Check for trailing comma
        case ElementType.Comma:
          throw new ParserException(lastToken, "Expression cannot end with a comma");
      }

      // Check for trailing binary operators
      if (lastType.IsBinaryOperator())
      {
        throw new ParserException(lastToken, "Expression cannot end with an operator");
      }
    }

    #endregion
  }
}