using System.Collections.Generic;

namespace EpsilonScript.VirtualMachine
{
  public static class VirtualMachinePool
  {
    private static readonly Stack<VirtualMachine> Pool = new Stack<VirtualMachine>();

    public static VirtualMachine Take()
    {
      return Pool.Count == 0 ? new VirtualMachine() : Pool.Pop();
    }

    public static void Return(VirtualMachine vm)
    {
      Pool.Push(vm);
    }
  }
}