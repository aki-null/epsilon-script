using System;
using System.Collections.Generic;
using EpsilonScript.Function;

namespace EpsilonScript.Bytecode
{
  internal class MutableProgram
  {
    public List<Instruction> Instructions = new List<Instruction>();
    public readonly List<string> StringTable = new List<string>();
    public readonly CustomFunctionContainer Functions;

    private List<Instruction> _instructionsBackBuffer = new List<Instruction>();

    private readonly Dictionary<int, int> _variableUsageCache = new Dictionary<int, int>();

    public MutableProgram(CustomFunctionContainer functions)
    {
      Functions = functions;
    }

    public void Reset()
    {
      Instructions.Clear();
      StringTable.Clear();
      _instructionsBackBuffer.Clear();
      _variableUsageCache.Clear();
    }

    public int AddString(string value)
    {
      var stringId = StringTable.IndexOf(value);
      if (stringId >= 0)
      {
        return stringId;
      }

      StringTable.Add(value);
      return StringTable.Count - 1;
    }

    /// <summary>
    /// Finds all variable load instructions and inserts pre-fetch instructions at the start of the program.
    /// Variable lookup requires a dictionary lookup, or execution of user code to fetch one, which can be slow when
    /// done multiple times in a program.
    /// </summary>
    public void OptimizeVariableLoad()
    {
      List<Instruction> variablePrefetchInstructions = null;

      var nextVariableRegisterIndex = 0;
      try
      {
        foreach (var instruction in Instructions)
        {
          if (instruction.Type != InstructionType.LoadVariableValue &&
              instruction.Type != InstructionType.AssignVariable)
          {
            continue;
          }

          var varId = instruction.IntegerValue;
          if (_variableUsageCache.ContainsKey(varId))
          {
            continue;
          }

          if (nextVariableRegisterIndex >= VirtualMachine.VirtualMachine.MaxVariableCache)
          {
            throw new ArgumentException(
              $"The current virtual machine configuration supports maximum of {VirtualMachine.VirtualMachine.MaxVariableCache} variables.");
          }

          variablePrefetchInstructions ??= new List<Instruction>();
          variablePrefetchInstructions.Add(new Instruction
          {
            Type = InstructionType.PrefetchVariable,
            IntegerValue = varId,
            reg0 = (byte)nextVariableRegisterIndex
          });
          _variableUsageCache[varId] = nextVariableRegisterIndex;

          ++nextVariableRegisterIndex;
        }

        if (variablePrefetchInstructions != null)
        {
          InsertInstructions(variablePrefetchInstructions, 0);
        }

        // Update variable instructions with pre-fetched variable register index
        for (var i = 0; i < Instructions.Count; i++)
        {
          var instruction = Instructions[i];
          switch (instruction.Type)
          {
            case InstructionType.LoadVariableValue:
            case InstructionType.AssignVariable:
            {
              instruction.reg1 = (byte)_variableUsageCache[instruction.IntegerValue];
              instruction.IntegerValue = 0;
              Instructions[i] = instruction;
              break;
            }
          }
        }
      }
      finally
      {
        _variableUsageCache.Clear();
      }
    }

    private void InsertInstructions(IList<Instruction> newInstructions, int position)
    {
      try
      {
        var instructionOffset = newInstructions.Count;
        // Instructions before the position can be copied as is
        for (var i = 0; i < position; ++i)
        {
          _instructionsBackBuffer.Add(Instructions[i]);
        }

        // Insert the new instructions
        for (var i = 0; i < newInstructions.Count; ++i)
        {
          _instructionsBackBuffer.Add(newInstructions[i]);
        }

        // Insert the rest of the instructions, which may need to be adjusted in some cases
        for (var i = position; i < Instructions.Count; ++i)
        {
          var instruction = Instructions[i];
          switch (instruction.Type)
          {
            case InstructionType.Jump:
            case InstructionType.JumpIf:
            case InstructionType.JumpNotIf:
              // Jump instructions that jumps to instructions after the insertion position needs to be adjusted
              if (instruction.IntegerValue >= position)
              {
                instruction.IntegerValue += instructionOffset;
              }

              break;
          }

          _instructionsBackBuffer.Add(instruction);
        }

        (Instructions, _instructionsBackBuffer) = (_instructionsBackBuffer, Instructions);
      }
      finally
      {
        _instructionsBackBuffer.Clear();
      }
    }
  }
}