namespace EpsilonScript.Bytecode
{
  internal enum InstructionType : byte
  {
    // Loads an integer value
    // value = Integer value to load (Integer)
    // reg0 = Load target, reg1 = Unused, reg2 = Unused
    LoadInteger,

    // Loads a float value into
    // value = Float value to load (Float)
    // reg0 = Load target, reg1 = Unused, reg2 = Unused
    LoadFloat,

    // Loads a boolean value
    // value = Boolean value to load (Boolean)
    // reg0 = Load target, reg1 = Unused, reg2 = Unused
    LoadBoolean,

    // Loads a string in the string table
    // value = String table index to load (Integer)
    // reg0 = Load target, reg1 = Unused, reg2 = Unused
    LoadString,

    // Loads a variable value
    // value = Variable name identifier (Integer)
    // reg0 = Load target, reg1 = Unused, reg2 = Unused
    LoadVariableValue,

    // Loads a prefetched variable value
    // value = Unused
    // reg0 = Load target, reg1 = Variable prefetch ID, reg2 = Unused
    LoadPrefetchedVariableValue,

    // Adds two numbers
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    Add,

    // Subtracts a number from an another number
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    Subtract,

    // Multiplies two numbers
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    Multiply,

    // Divides a number by an another number
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    Divide,

    // Performs a modulo operation
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    Modulo,

    // Negates a boolean value
    // value = Unused
    // reg0 = Write target, reg1 = Read value, reg2 = Unused
    Negate,

    // Flips a sign of a number
    // value = Unused
    // reg0 = Write target, reg1 = Read value, reg2 = Unused
    Negative,

    // Compares two values for equality
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    ComparisonEqual,

    // Compares two values for inequality
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    ComparisonNotEqual,

    // Compares two values to see whether left value is less than the right value
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    ComparisonLessThan,

    // Compares two values to see whether left value is greater than the right value
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    ComparisonGreaterThan,

    // Compares two values to see whether left value is less than or equal to the right value
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    ComparisonLessThanOrEqualTo,

    // Compares two values to see whether left value is greater than or equal to the right value
    // value = Unused
    // reg0 = Write target, reg1 = Left value, reg2 = Right value
    ComparisonGreaterThanOrEqualTo,

    // Jumps the execution of the program to the value
    // value = Program instruction index (Integer)
    // reg0 = Unused, reg1 = Unused, reg2 = Unused
    Jump,

    // Jumps the execution of the program to the value if the value is TRUE
    // value = Program instruction index (Integer)
    // reg0 = Condition value, reg1 = Unused, reg2 = Unused
    JumpIf,

    // Jumps the execution of the program to the value if the value is FALSE
    // value = Program instruction index (Integer)
    // reg0 = Condition value, reg1 = Unused, reg2 = Unused
    JumpNotIf,

    // Assigns a value into a variable
    // value = Variable identifier (Integer) - Can be zero if variable is prefetched (see reg1)
    // reg0 = Read target, reg1 = Variable prefetch ID (only used when prefetched), reg2 = Unused
    AssignVariable,

    // Prefetches a variable for repeated access
    // Note: This instruction does not interact with registers
    // value = Variable identifier (Integer)
    // reg0 = Variable cache target, reg1 = Unused, reg2 = Unused
    PrefetchVariable,

    // Executes a C# function
    // value = Function identifier (Integer)
    // reg0 = Write target, reg1 = Number of parameters, reg2 = First function parameter
    CallFunction,

    // Returns the execution result and halts the program
    // value = Unused
    // reg0 = Result value, reg1 = Unused, reg2 = Unused
    Return
  }
}