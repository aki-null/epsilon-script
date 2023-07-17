using EpsilonScript.Bytecode;

namespace EpsilonScript.VirtualMachine
{
  public partial class VirtualMachine
  {
    // Shared VM for the main thread
    internal static VirtualMachine Main { get; } = new VirtualMachine();

    internal const int MaxVariableCache = 32;
    internal const int RegisterCount = 32;

    private readonly RegisterValue[] _registers = new RegisterValue[RegisterCount];
    private readonly string[] _stringRegisters = new string[RegisterCount];
    private readonly VariableValue[] _variableCache = new VariableValue[MaxVariableCache];

    private Program _program;


    internal unsafe ConcreteValue Run(Program program, IVariableContainer globalVariables,
      IVariableContainer localVariables)
    {
      try
      {
        fixed (RegisterValue* regPtr = _registers)
        {
          _program = program;
          var instructionPointer = 0;
          var instructions = program.Instructions;
          while (instructionPointer < instructions.Length)
          {
            var instruction = instructions[instructionPointer];
            RegisterValue* targetRegPtr = null;
            switch (instruction.Type)
            {
              case InstructionType.LoadInteger:
                targetRegPtr = regPtr + instruction.reg0;
                targetRegPtr->ValueType = RegisterValueType.Integer;
                targetRegPtr->IntegerValue = instruction.IntegerValue;
                break;
              case InstructionType.LoadFloat:
                targetRegPtr = regPtr + instruction.reg0;
                targetRegPtr->ValueType = RegisterValueType.Float;
                targetRegPtr->FloatValue = instruction.FloatValue;
                break;
              case InstructionType.LoadBoolean:
                targetRegPtr = regPtr + instruction.reg0;
                targetRegPtr->ValueType = RegisterValueType.Boolean;
                targetRegPtr->BooleanValue = instruction.BooleanValue;
                break;
              case InstructionType.LoadString:
                targetRegPtr = regPtr + instruction.reg0;
                targetRegPtr->ValueType = RegisterValueType.String;
                targetRegPtr->IntegerValue = instruction.IntegerValue;
                break;
              case InstructionType.LoadVariableValue:
                FindVariable(instruction.IntegerValue, globalVariables, localVariables)
                  .LoadToRegister(regPtr, _stringRegisters, instruction.reg0);
                break;
              case InstructionType.LoadPrefetchedVariableValue:
                _variableCache[instruction.reg1].LoadToRegister(regPtr, _stringRegisters, instruction.reg0);
                break;
              case InstructionType.Add:
                Add(instruction, regPtr);
                break;
              case InstructionType.Subtract:
                Subtract(instruction, regPtr);
                break;
              case InstructionType.Multiply:
                Multiply(instruction, regPtr);
                break;
              case InstructionType.Divide:
                Divide(instruction, regPtr);
                break;
              case InstructionType.Modulo:
                Modulo(instruction, regPtr);
                break;
              case InstructionType.Negate:
                Negate(instruction, regPtr);
                break;
              case InstructionType.Negative:
                Negative(instruction, regPtr);
                break;
              case InstructionType.ComparisonEqual:
                ComparisonEqualValue(instruction, false, regPtr);
                break;
              case InstructionType.ComparisonNotEqual:
                ComparisonEqualValue(instruction, true, regPtr);
                break;
              case InstructionType.ComparisonLessThan:
                ComparisonLessThanValue(instruction, regPtr);
                break;
              case InstructionType.ComparisonGreaterThan:
                ComparisonGreaterThanValue(instruction, regPtr);
                break;
              case InstructionType.ComparisonLessThanOrEqualTo:
                ComparisonLessThanOrEqualToValue(instruction, regPtr);
                break;
              case InstructionType.ComparisonGreaterThanOrEqualTo:
                ComparisonGreaterThanOrEqualToValue(instruction, regPtr);
                break;
              case InstructionType.Jump:
                instructionPointer = instruction.IntegerValue - 1;
                break;
              case InstructionType.JumpIf:
                if ((regPtr + instruction.reg0)->BooleanValue)
                {
                  instructionPointer = instruction.IntegerValue - 1;
                }

                break;
              case InstructionType.JumpNotIf:
                if (!(regPtr + instruction.reg0)->BooleanValue)
                {
                  instructionPointer = instruction.IntegerValue - 1;
                }

                break;
              case InstructionType.AssignVariable:
                SetVariableValue(globalVariables, localVariables, instruction, regPtr);
                break;
              // Prefetch is optimal when accessing variables multiple times in a single program.
              // This is because finding a variable usually involves a dictionary lookup.
              case InstructionType.PrefetchVariable:
                _variableCache[instruction.reg0] =
                  FindVariable(instruction.IntegerValue, globalVariables, localVariables);
                break;
              case InstructionType.CallFunction:
                Function(instruction, regPtr);
                break;
              case InstructionType.Return:
                return (regPtr + instruction.reg0)->ResolveToConcreteValue(program.StringTable, _stringRegisters);
              default:
                throw new RuntimeException("Unsupported instruction");
            }

            ++instructionPointer;
          }
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