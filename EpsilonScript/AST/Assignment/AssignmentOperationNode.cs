using System;
using System.Collections.Generic;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST.Assignment
{
  /// <summary>
  /// Abstract base class for all assignment operation nodes.
  /// Provides common infrastructure for assignment operations (=, +=, -=, *=, /=, %=).
  /// Concrete subclasses implement operation-specific assignment logic.
  /// </summary>
  internal abstract class AssignmentOperationNode : Node
  {
    private Node _leftNode;
    protected Node RightNode;
    private bool _noAllocMode;

    public override bool IsPrecomputable => false;

    /// <summary>
    /// Returns true if this assignment operation requires numeric operands only.
    /// Simple assignment (=) returns false; compound assignments (+=, -=, etc.) return true.
    /// </summary>
    protected virtual bool RequiresNumericOperand() => false;

    protected override void BuildCore(Stack<Node> rpnStack, Element element, CompilerContext context,
      Compiler.Options options, IVariableContainer variables)
    {
      if ((options & Compiler.Options.Immutable) == Compiler.Options.Immutable)
      {
        throw new ParserException(element.Token, "An assignment operator cannot be used for an immutable script");
      }

      if (!rpnStack.TryPop(out RightNode) || !rpnStack.TryPop(out _leftNode))
      {
        throw new ParserException(element.Token, "Cannot find elements to perform assignment operation on");
      }
    }

    public override void Execute(IVariableContainer variablesOverride)
    {
      _leftNode.Execute(variablesOverride);
      RightNode.Execute(variablesOverride);

      if (_leftNode.Variable == null)
      {
        throw CreateRuntimeException("A left hand side of an assignment operator must be a variable");
      }

      var variable = _leftNode.Variable;

      // NoAlloc validation: Block assignments that cause ToString() allocation
      // Assigning non-string to string variable calls ToString() in VariableValue setter
      if (_noAllocMode && variable.Type == Type.String && RightNode.ValueType != ExtendedType.String)
      {
        throw CreateRuntimeException(
          "Assigning non-string value to string variable causes allocation (ToString()) in NoAlloc mode");
      }

      // Type validation for compound assignments (numeric only)
      if (RequiresNumericOperand() && !RightNode.IsNumeric)
      {
        throw CreateRuntimeException("An arithmetic operation can only be performed on a numeric value");
      }

      // Perform the operation-specific assignment
      PerformAssignment(variable);

      // Copy the variable's value to this node's result
      SetResultFromVariable(variable);
    }

    /// <summary>
    /// Performs the operation-specific assignment on the variable.
    /// Each concrete class implements its own assignment logic.
    /// </summary>
    protected abstract void PerformAssignment(VariableValue variable);

    /// <summary>
    /// Copies the variable's value to this node's result value.
    /// Called after assignment to ensure the node returns the assigned value.
    /// </summary>
    private void SetResultFromVariable(VariableValue variable)
    {
      switch (variable.Type)
      {
        case Type.Integer:
          IntegerValue = variable.IntegerValue;
          break;
        case Type.Long:
          LongValue = variable.LongValue;
          break;
        case Type.Float:
          FloatValue = variable.FloatValue;
          break;
        case Type.Double:
          DoubleValue = variable.DoubleValue;
          break;
        case Type.Decimal:
          DecimalValue = variable.DecimalValue;
          break;
        case Type.Boolean:
          BooleanValue = variable.BooleanValue;
          break;
        case Type.String:
          StringValue = variable.StringValue;
          break;
        default:
          throw new ArgumentOutOfRangeException(nameof(variable.Type), variable.Type, "Unsupported variable type");
      }
    }

    public override void Validate()
    {
      _leftNode?.Validate();
      RightNode?.Validate();
    }

    public override void ConfigureNoAlloc()
    {
      _noAllocMode = true;
      _leftNode?.ConfigureNoAlloc();
      RightNode?.ConfigureNoAlloc();
    }
  }
}