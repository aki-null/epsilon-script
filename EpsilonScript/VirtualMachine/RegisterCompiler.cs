using System;
using System.Collections.Generic;
using EpsilonScript.AST;
using EpsilonScript.AST.Assignment;

namespace EpsilonScript.VirtualMachine
{
  /// <summary>
  /// Lowers an optimized AST into a RegisterProgram. Every node gets its own register; literals are preloaded.
  /// A Compiler reuses one instance, so the working lists are allocated once.
  /// </summary>
  internal sealed class RegisterCompiler
  {
    // Working arrays with counts rather than List<T>: IL2CPP runs value-type List methods through shared generic
    // code, whose allocations and copies look up the element type at run time
    private Instruction[] _code = new Instruction[32];
    private int _codeCount;
    private SourceLocation[] _locations = new SourceLocation[32];
    private int _locationCount;
    private VariableSlot[] _variables = new VariableSlot[16];
    private int _variableCount;
    private FunctionSite[] _functions = new FunctionSite[8];
    private int _functionCount;
    private Node[] _constants = new Node[16];
    private int[] _constantRegisters = new int[16];
    private int _constantCount;
    private int _registerCount;

    private static readonly SourceLocation[] NoLocations = new SourceLocation[0];
    private static readonly VariableSlot[] NoVariables = new VariableSlot[0];
    private static readonly FunctionSite[] NoFunctions = new FunctionSite[0];

    // Registers of the arguments of the calls being lowered; a nested call's arguments sit above its caller's
    private int[] _arguments = new int[16];
    private int _argumentCount;

    // Reused by CompileForEvaluation. The program runs on the working arrays, which stay filled until
    // ReleaseEvaluation, and call sites are reused across evaluations.
    private RegisterProgram _evaluationProgram;
    private bool _evaluation;
    private FunctionSite[] _sites = new FunctionSite[8];

    // Every node a recording CompileForEvaluation lowered, with its register and the code position its lowering
    // started at, for WriteBack
    private bool _recording;
    private Node[] _evaluated = new Node[16];
    private int[] _evaluatedRegisters = new int[16];
    private int[] _evaluatedStarts = new int[16];
    private int _evaluatedCount;
    private bool[] _skipped = new bool[33];

    public RegisterProgram Compile(Node root, CompilerContext context, bool noAlloc)
    {
      try
      {
        var result = LowerRoot(root);
        var code = new Instruction[_codeCount];
        Array.Copy(_code, code, _codeCount);
        var locations = _locationCount == 0 ? NoLocations : new SourceLocation[_locationCount];
        Array.Copy(_locations, locations, _locationCount);
        var variables = _variableCount == 0 ? NoVariables : new VariableSlot[_variableCount];
        Array.Copy(_variables, variables, _variableCount);
        var functions = _functionCount == 0 ? NoFunctions : new FunctionSite[_functionCount];
        Array.Copy(_functions, functions, _functionCount);
        var program = new RegisterProgram(code, locations, context, variables, functions, _registerCount, result,
          context.ConfiguredIntegerType, context.ConfiguredFloatType, noAlloc);
        Seed(program, result, root);
        return program;
      }
      finally
      {
        Clear();
      }
    }

    /// <summary>
    /// Lowers a subtree into a program that is reused by every call, for evaluating it once: constant folding runs
    /// it for each folded subtree. The program runs on the working arrays, so it is valid until ReleaseEvaluation,
    /// which must be called after reading its result and before lowering anything else.
    /// </summary>
    public RegisterProgram CompileForEvaluation(Node root, CompilerContext context, bool record)
    {
      try
      {
        _evaluation = true;
        _recording = record;
        var result = LowerRoot(root);
        _recording = false;
        AddCode(new Instruction { Op = OpCode.Halt });
        --_codeCount;
        var program = _evaluationProgram;
        if (program == null)
        {
          program = _evaluationProgram = new RegisterProgram(_code, _locations, context, _variables, _functions,
            _registerCount, result, context.ConfiguredIntegerType, context.ConfiguredFloatType, false);
        }
        else
        {
          program.Reload(_code, _locations, _variables, _functions, _registerCount, result);
        }

        Seed(program, result, root);
        return program;
      }
      catch
      {
        // The program was not reloaded, so it holds nothing of this evaluation
        _recording = false;
        ClearEvaluation();
        throw;
      }
    }

    /// <summary>
    /// Gives every node the last recording evaluation executed the value it computed, as the tree interpreter left
    /// a value in each node it executed. Optimize reads them: a parent that folded a subtree decides on the values
    /// its children got. Nodes in an operand the run short-circuited past keep their values.
    /// </summary>
    public void WriteBack()
    {
      var code = _code;
      var program = _evaluationProgram;
      var count = _codeCount;
      if (_skipped.Length <= count)
      {
        _skipped = new bool[code.Length + 1];
      }

      // A node's lowering starts inside a skipped operand exactly when the node was skipped
      var skipped = _skipped;
      var skipUntil = 0;
      for (var i = 0; i <= count; ++i)
      {
        skipped[i] = i < skipUntil;
        if (skipped[i] || i == count)
        {
          continue;
        }

        var op = code[i].Op;
        if ((op == OpCode.AndLeft || op == OpCode.OrLeft) &&
            (program.ReadRawBits(code[i].A) != 0) == (op == OpCode.OrLeft))
        {
          skipUntil = code[i].C;
        }
      }

      for (var i = 0; i < _evaluatedCount; ++i)
      {
        if (!skipped[_evaluatedStarts[i]])
        {
          _evaluated[i].SetResult(program, _evaluatedRegisters[i]);
        }
      }
    }

    private void ClearEvaluated()
    {
      Array.Clear(_evaluated, 0, _evaluatedCount);
      _evaluatedCount = 0;
    }

    /// <summary>
    /// Drops everything the last evaluation referenced once its result is read: function sites, variable slots,
    /// strings and bound variables, so the reused program does not keep them alive.
    /// </summary>
    public void ReleaseEvaluation()
    {
      _evaluationProgram.ReleaseReferences(_registerCount, _variableCount);
      ClearEvaluation();
    }

    private void ClearEvaluation()
    {
      ClearEvaluated();
      for (var i = 0; i < _functionCount; ++i)
      {
        _sites[i].Release();
      }

      Clear();
      _evaluation = false;
    }

    private void Seed(RegisterProgram program, int result, Node root)
    {
      for (var i = 0; i < _constantCount; ++i)
      {
        program.Store(_constantRegisters[i], _constants[i]);
      }

      // Results read before the first Execute match the unexecuted tree
      program.Store(result, root);
    }

    private void Clear()
    {
      // Drop node references so the lists do not keep this script alive
      Array.Clear(_variables, 0, _variableCount);
      Array.Clear(_functions, 0, _functionCount);
      Array.Clear(_constants, 0, _constantCount);
      _codeCount = 0;
      _locationCount = 0;
      _variableCount = 0;
      _functionCount = 0;
      _constantCount = 0;
      _registerCount = 0;
      _argumentCount = 0;
    }


    private int Lower(Node node)
    {
      if (!_recording)
      {
        return node.Lower(this);
      }

      var start = _codeCount;
      var register = node.Lower(this);
      if (_evaluatedCount == _evaluated.Length)
      {
        _evaluated = Grow(_evaluated);
        _evaluatedRegisters = Grow(_evaluatedRegisters);
        _evaluatedStarts = Grow(_evaluatedStarts);
      }

      _evaluated[_evaluatedCount] = node;
      _evaluatedRegisters[_evaluatedCount] = register;
      _evaluatedStarts[_evaluatedCount++] = start;
      return register;
    }

    private int LowerRoot(Node root)
    {
      var result = Lower(root);
      if (!(root is SequenceNode))
      {
        return result;
      }

      // A sequence is the one node that returns another node's register. The result gets its own register, copied
      // once the last element succeeds: a run that throws leaves the element half written but the result as it
      // was, like the tree interpreter's sequence, and Seed's unexecuted root value does not overwrite a constant.
      var destination = _registerCount++;
      AddCode(new Instruction { Op = OpCode.Move, Destination = destination, A = result });
      return destination;
    }

    // Lowering entry points, called by Node.Lower overrides

    internal int LowerConstant(Node node)
    {
      var register = _registerCount++;
      if (_constantCount == _constants.Length)
      {
        _constants = Grow(_constants);
        _constantRegisters = Grow(_constantRegisters);
      }

      _constants[_constantCount] = node;
      _constantRegisters[_constantCount++] = register;
      return register;
    }

    internal int LowerVariable(ref VariableSlot variable)
    {
      return Emit(OpCode.LoadVariable, AddVariable(ref variable));
    }

    internal int LowerTuple(List<Node> elements)
    {
      foreach (var element in elements)
      {
        Lower(element);
      }

      var destination = _registerCount++;
      AddCode(new Instruction { Op = OpCode.MakeTuple, Destination = destination });
      return destination;
    }

    internal int LowerAssignNonVariable(Node assignment, Node target, Node value)
    {
      Lower(target);
      Lower(value);
      var destination = _registerCount++;
      AddCode(new Instruction { Op = OpCode.AssignNonVariable, Destination = destination, Site = AddSite(assignment) });
      return destination;
    }

    internal int LowerOperation(Node node, OpCode op, Node operand)
    {
      var a = Lower(operand);
      return EmitOperation(node, op, a, 0, 0);
    }

    internal int LowerOperation(Node node, OpCode op, Node left, Node right)
    {
      var a = Lower(left);
      var b = Lower(right);
      return EmitOperation(node, op, a, b, 0);
    }

    internal int LowerOperation(Node node, OpCode op, Node first, Node second, Node third)
    {
      var a = Lower(first);
      var b = Lower(second);
      var c = Lower(third);
      return EmitOperation(node, op, a, b, c);
    }

    private int EmitOperation(Node node, OpCode op, int a, int b, int c)
    {
      var destination = _registerCount++;
      AddCode(new Instruction
      {
        Op = op, Destination = destination, A = a, B = b, C = c,
        Site = IsMultiplyAdd(op) ? AddMultiplyAddSite(node) : AddSite(node),
      });
      return destination;
    }

    internal int LowerSequence(Node first, Node last)
    {
      if (first != null)
      {
        Lower(first);
      }

      return Lower(last);
    }

    internal int LowerAssignment(AssignmentOperationNode assignment, ref VariableSlot target, Node value)
    {
      var targetIndex = AddVariable(ref target);
      AddCode(new Instruction { Op = OpCode.BindVariable, Site = targetIndex });

      var valueRegister = Lower(value);
      var site = AddSite(assignment);
      var destination = _registerCount++;
      AddCode(new Instruction
      {
        Op = assignment.LoweredOp, Destination = destination, B = valueRegister, C = targetIndex, Site = site,
      });
      return destination;
    }


    internal int LowerCall(List<Node> parameters, Function.CustomFunctionOverload overload,
      IVariableContainer compileTimeVariables, SourceLocation location)
    {
      var count = parameters.Count;
      var start = _argumentCount;
      for (var i = 0; i < count; ++i)
      {
        var register = Lower(parameters[i]);
        if (_argumentCount == _arguments.Length)
        {
          _arguments = Grow(_arguments);
        }

        _arguments[_argumentCount++] = register;
      }

      _argumentCount = start;
      if (_functionCount == _functions.Length)
      {
        _functions = Grow(_functions);
      }

      // A compiled program owns its sites; evaluations reuse theirs
      FunctionSite site;
      if (_evaluation)
      {
        if (_functionCount == _sites.Length)
        {
          _sites = Grow(_sites);
        }

        site = _sites[_functionCount] ?? (_sites[_functionCount] = new FunctionSite());
      }
      else
      {
        site = new FunctionSite();
      }

      site.Reset(overload, count, compileTimeVariables, location);

      Array.Copy(_arguments, start, site.Arguments, 0, count);
      _functions[_functionCount] = site;
      return Emit(OpCode.Call, _functionCount++);
    }

    internal int LowerBoolean(Node node, OpCode leftOp, Node left, Node right)
    {
      var leftRegister = Lower(left);
      var destination = _registerCount++;
      var site = AddSite(node);
      var leftIndex = _codeCount;
      AddCode(new Instruction
        { Op = leftOp, Destination = destination, A = leftRegister, Site = site });

      var rightRegister = Lower(right);
      AddCode(new Instruction { Op = OpCode.BooleanRight, Destination = destination, B = rightRegister, Site = site });

      // Short-circuit past the right operand
      _code[leftIndex].C = _codeCount;
      return destination;
    }

    private int Emit(OpCode op, int site)
    {
      var destination = _registerCount++;
      AddCode(new Instruction { Op = op, Destination = destination, Site = site });
      return destination;
    }

    private int AddSite(Node node)
    {
      return AddLocation(node.Location);
    }

    private int AddMultiplyAddSite(Node node)
    {
      var site = AddSite(node);
      AddLocation(node.MultiplyLocation);
      return site;
    }

    private static bool IsMultiplyAdd(OpCode op) =>
      op == OpCode.MultiplyAdd || op == OpCode.AddMultiply || op == OpCode.MultiplySubtract ||
      op == OpCode.SubtractMultiply;

    private void AddCode(Instruction instruction)
    {
      if (_codeCount == _code.Length)
      {
        var grown = new Instruction[_code.Length * 2];
        Array.Copy(_code, grown, _codeCount);
        _code = grown;
      }

      _code[_codeCount++] = instruction;
    }

    private int AddLocation(SourceLocation location)
    {
      if (_locationCount == _locations.Length)
      {
        var grown = new SourceLocation[_locations.Length * 2];
        Array.Copy(_locations, grown, _locationCount);
        _locations = grown;
      }

      _locations[_locationCount] = location;
      return _locationCount++;
    }

    private int AddVariable(ref VariableSlot variable)
    {
      if (_variableCount == _variables.Length)
      {
        var grown = new VariableSlot[_variables.Length * 2];
        Array.Copy(_variables, grown, _variableCount);
        _variables = grown;
      }

      _variables[_variableCount] = variable;
      return _variableCount++;
    }

    private static FunctionSite[] Grow(FunctionSite[] array)
    {
      var grown = new FunctionSite[array.Length * 2];
      Array.Copy(array, grown, array.Length);
      return grown;
    }

    private static Node[] Grow(Node[] array)
    {
      var grown = new Node[array.Length * 2];
      Array.Copy(array, grown, array.Length);
      return grown;
    }

    private static int[] Grow(int[] array)
    {
      var grown = new int[array.Length * 2];
      Array.Copy(array, grown, array.Length);
      return grown;
    }
  }
}
