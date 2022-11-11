using System;
using EpsilonScript.Bytecode;
using EpsilonScript.VirtualMachine;

namespace EpsilonScript
{
  public class CompiledScript
  {
    private readonly Program _program;
    public Type ValueType => _result.Type;

    private ConcreteValue _result;

    public int IntegerValue
    {
      get
      {
        switch (_result.Type)
        {
          case Type.Integer:
            return _result.IntegerValue;
          case Type.Float:
            return (int)_result.FloatValue;
          case Type.String:
          default:
            throw new InvalidCastException($"Value type {_result.Type} cannot be cast to integer");
        }
      }
    }

    public float FloatValue
    {
      get
      {
        switch (_result.Type)
        {
          case Type.Integer:
            return _result.IntegerValue;
          case Type.Float:
            return _result.FloatValue;
          case Type.String:
          default:
            throw new InvalidCastException($"Value type {_result.Type} cannot be cast to float");
        }
      }
    }

    public bool BooleanValue
    {
      get
      {
        switch (_result.Type)
        {
          case Type.Boolean:
            return _result.BooleanValue;
          default:
            throw new InvalidCastException($"Value type {_result.Type} cannot be cast to boolean");
        }
      }
    }

    public string StringValue
    {
      get
      {
        switch (_result.Type)
        {
          case Type.String:
            return _result.StringValue;
          default:
            throw new InvalidCastException($"Value type {_result.Type} cannot be cast to string");
        }
      }
    }

    internal CompiledScript(Program program)
    {
      _program = program;
    }

    /// <summary>
    /// Executes this script using given VM. This is useful for multi-threaded applications.
    /// </summary>
    /// <param name="vm">Virtual machine to execute the script on.</param>
    /// <param name="globalVariables">Global variables container.</param>
    /// <param name="localVariables">Local variables container. This has higher precedence.</param>
    public void Execute(VirtualMachine.VirtualMachine vm, IVariableContainer globalVariables = null,
      IVariableContainer localVariables = null)
    {
      _result = vm.Run(_program, globalVariables, localVariables);
    }

    /// <summary>
    /// Executes this script using the default VM.
    /// The script executed this way is not thread safe.
    /// Please use the another override which lets you specify the VM.
    /// There should be a single VM per thread.
    /// <see cref="VirtualMachinePool"/> can be used to manage pools of VMs.
    /// </summary>
    /// <param name="globalVariables">Global variables container.</param>
    /// <param name="localVariables">Local variables container. This has higher precedence.</param>
    public void Execute(IVariableContainer globalVariables = null, IVariableContainer localVariables = null)
    {
      Execute(VirtualMachine.VirtualMachine.Main, globalVariables, localVariables);
    }
  }
}