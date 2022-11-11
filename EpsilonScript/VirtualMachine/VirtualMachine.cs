using EpsilonScript.Bytecode;

namespace EpsilonScript.VirtualMachine
{
  public partial class VirtualMachine
  {
    // Shared VM for the main thread
    internal static VirtualMachine Main { get; } = new VirtualMachine();

    internal const int MaxVariables = 32;

    private readonly RegisterValue[] _registers = new RegisterValue[32];
    private readonly string[] _stringRegisters = new string[32];
    private readonly VariableValue[] _variableRegisters = new VariableValue[MaxVariables];

    private Program _program;

    internal ConcreteValue Run(Program program, IVariableContainer globalVariables, IVariableContainer localVariables)
    {
      try
      {
        _program = program;
        var instructionPointer = 0;
        var instructions = program.Instructions;
        while (instructionPointer < instructions.Length)
        {
          var instruction = instructions[instructionPointer];
          switch (instruction.Type)
          {
            case InstructionType.LoadInteger:
              _registers[instruction.reg0] = new RegisterValue
              {
                ValueType = RegisterValueType.Integer,
                IntegerValue = instruction.IntegerValue
              };
              break;
            case InstructionType.LoadFloat:
              _registers[instruction.reg0] = new RegisterValue
              {
                ValueType = RegisterValueType.Float,
                FloatValue = instruction.FloatValue
              };
              break;
            case InstructionType.LoadBoolean:
              _registers[instruction.reg0] = new RegisterValue
              {
                ValueType = RegisterValueType.Boolean,
                BooleanValue = instruction.BooleanValue
              };
              break;
            case InstructionType.LoadString:
              _registers[instruction.reg0] = new RegisterValue
              {
                ValueType = RegisterValueType.String,
                IntegerValue = instruction.IntegerValue
              };
              break;
            case InstructionType.LoadVariableValue:
              LoadVariableValue(globalVariables, localVariables, instruction);
              break;
            case InstructionType.Add:
              Add(instruction);
              break;
            case InstructionType.Subtract:
              Subtract(instruction);
              break;
            case InstructionType.Multiply:
              Multiply(instruction);
              break;
            case InstructionType.Divide:
              Divide(instruction);
              break;
            case InstructionType.Modulo:
              Modulo(instruction);
              break;
            case InstructionType.Negate:
              Negate(instruction);
              break;
            case InstructionType.Negative:
              Negative(instruction);
              break;
            case InstructionType.ComparisonEqual:
              ComparisonEqualValue(instruction, false);
              break;
            case InstructionType.ComparisonNotEqual:
              ComparisonEqualValue(instruction, true);
              break;
            case InstructionType.ComparisonLessThan:
              ComparisonLessThanValue(instruction);
              break;
            case InstructionType.ComparisonGreaterThan:
              ComparisonGreaterThanValue(instruction);
              break;
            case InstructionType.ComparisonLessThanOrEqualTo:
              ComparisonLessThanOrEqualToValue(instruction);
              break;
            case InstructionType.ComparisonGreaterThanOrEqualTo:
              ComparisonGreaterThanOrEqualToValue(instruction);
              break;
            case InstructionType.Jump:
              instructionPointer = instruction.IntegerValue - 1;
              break;
            case InstructionType.JumpIf:
              if (_registers[instruction.reg0].BooleanValue)
              {
                instructionPointer = instruction.IntegerValue - 1;
              }

              break;
            case InstructionType.JumpNotIf:
              if (!_registers[instruction.reg0].BooleanValue)
              {
                instructionPointer = instruction.IntegerValue - 1;
              }

              break;
            case InstructionType.AssignVariable:
              SetVariableValue(globalVariables, localVariables, instruction);
              break;
            case InstructionType.PrefetchVariable:
              PrefetchVariable(globalVariables, localVariables, instruction);
              break;
            case InstructionType.CallFunction:
              Function(instruction);
              break;
            case InstructionType.Return:
              return _registers[instruction.reg0].ResolveToConcreteValue(program.StringTable, _stringRegisters);
            default:
              throw new RuntimeException("Unsupported instruction");
          }

          ++instructionPointer;
        }
      }
      finally
      {
        _program = null;
      }

      return new ConcreteValue
      {
        Type = Type.Void
      };
    }
  }
}