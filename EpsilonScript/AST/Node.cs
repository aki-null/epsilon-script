using System;
using System.Collections.Generic;
using EpsilonScript.Bytecode;
using EpsilonScript.Function;
using EpsilonScript.Intermediate;

namespace EpsilonScript.AST
{
  internal abstract class Node
  {
    public virtual bool IsConstant => true;

    public abstract void Build(Stack<Node> rpnStack, Element element, Compiler.Options options,
      CustomFunctionContainer functions);

    public abstract void Encode(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm);

    protected bool TryEncodeConstant(MutableProgram program, ref byte nextRegisterIdx,
      VirtualMachine.VirtualMachine constantVm)
    {
      if (constantVm == null)
      {
        return false;
      }

      if (!IsConstant)
      {
        return false;
      }

      var constantPartialProgram = new MutableProgram(program.Functions);
      byte partialRegisterIndex = 0;
      Encode(constantPartialProgram, ref partialRegisterIndex, null);
      if (partialRegisterIndex > 0)
      {
        constantPartialProgram.Instructions.Add(new Instruction
        {
          Type = InstructionType.Return,
          reg0 = (byte)(partialRegisterIndex - 1)
        });
      }

      var result = constantVm.Run(new Program(constantPartialProgram), null, null);

      switch (result.Type)
      {
        case Type.Void:
          throw new ArgumentException("Script execution returned void", nameof(result.Type));
        case Type.Integer:
          PushValue(program, ref nextRegisterIdx, result.IntegerValue);
          break;
        case Type.Float:
          PushValue(program, ref nextRegisterIdx, result.FloatValue);
          break;
        case Type.Boolean:
          PushValue(program, ref nextRegisterIdx, result.BooleanValue);
          break;
        case Type.String:
          PushValue(program, ref nextRegisterIdx, result.StringValue);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }

      return true;
    }

    protected static void PushValue(MutableProgram program, ref byte nextRegisterIdx, int value)
    {
      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.LoadInteger,
        IntegerValue = value,
        reg0 = nextRegisterIdx
      });
      ++nextRegisterIdx;
    }

    protected static void PushValue(MutableProgram program, ref byte nextRegisterIdx, float value)
    {
      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.LoadFloat,
        FloatValue = value,
        reg0 = nextRegisterIdx
      });
      ++nextRegisterIdx;
    }

    protected static void PushValue(MutableProgram program, ref byte nextRegisterIdx, bool value)
    {
      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.LoadBoolean,
        BooleanValue = value,
        reg0 = nextRegisterIdx
      });
      ++nextRegisterIdx;
    }

    protected static void PushValue(MutableProgram program, ref byte nextRegisterIdx, string value)
    {
      // Reuse the same string if already in string table
      var stringIndex = program.StringTable.IndexOf(value);
      if (stringIndex < 0)
      {
        stringIndex = program.StringTable.Count;
        program.StringTable.Add(value);
      }

      program.Instructions.Add(new Instruction
      {
        Type = InstructionType.LoadString,
        IntegerValue = stringIndex,
        reg0 = nextRegisterIdx
      });

      ++nextRegisterIdx;
    }
  }
}